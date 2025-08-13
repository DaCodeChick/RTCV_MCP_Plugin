using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MemoryVisualizer.UI
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using NLog;
    using RTCV.CorruptCore;
    using RTCV.NetCore;
    using Formats;
    using static RTCV.CorruptCore.RtcCore;
    using System.IO;
    using RTCV.UI.Modular;

    public partial class PluginForm : ComponentForm
    {
        // ughhhh https://stackoverflow.com/questions/4305800/using-custom-colored-cursors-in-a-c-sharp-windows-application
        // ughhhhhhhh https://stackoverflow.com/questions/18425044/c-sharp-embed-custom-cursor
        public static Cursor LoadCustomCursor(string resourceName)
        {
            string path = Path.GetTempFileName();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Resource '{resourceName}' not found.");
                }
                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
            }
            IntPtr hCurs = LoadCursorFromFile(path);
            if (hCurs == IntPtr.Zero) return Cursors.Default;
            var curs = new Cursor(hCurs);
            // Note: force the cursor to own the handle so it gets released properly
            var fi = typeof(Cursor).GetField("ownHandle", BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(curs, true);
            return curs;
        }
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string path);
        
        public volatile bool HideOnClose = true;
        
        private long _pageSize = 256 * 256;

        private volatile int _framesToNextExecute = 1;
        private volatile int _delayFrames = 1;

        private readonly object _executeLock = new object();

        private long _offset;
        private long _offsetIncr;
        private long _align;
        
        private bool _mouseDown;
        private Point _lastMousePosition = new Point(int.MaxValue, int.MaxValue);

        private volatile bool _running;
        private volatile bool _gettingBytes;

        private long _rangeStartAddress;
        private long _rangeEndAddress;

        private string _domain;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private bool _updatingDomains;

        public static PluginForm Instance;

        public PluginForm()
        {
            this.InitializeComponent();
            Instance = this;
            
            this.cbDomains.SelectedIndexChanged += this.CbDomains_SelectedIndexChanged;
            this.sliderOffset.ValueChanged += this.SliderOffset_ValueChanged;
            this.sliderDelay.ValueChanged += this.SliderDelay_ValueChanged;
            this.sliderOffset.ValueCorrection = this.SliderOffsetCorrection;
            this.FormClosing += this.MemoryVisualizer_FormClosing;

            //Pixel formats
            string[] names = PixFormats.GetNames();
            this.display.CurFormat = PixFormats.Get(names[0]);
            this.cbFormat.Items.AddRange(names);
            this.cbFormat.SelectedIndex = 0;
            this._offsetIncr = display.CurFormat.BytesWide;
            this.cbFormat.SelectedIndexChanged += this.CbFormat_SelectedIndexChanged;

            //Context menu
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(new MenuItem("Copy Image", (o, e2) =>
            {
                lock (_executeLock)
                {
                    ImageClipboard.SetClipboardImage(display.Bitmap.Bitmap, null, null);
                }
            }));
            contextMenu.MenuItems.Add(new MenuItem("Copy Range", (o, e2) =>
            {
                lock (_executeLock)
                {
                    Clipboard.SetText(this._rangeStartAddress.ToString("X") + "-" + this._rangeEndAddress.ToString("X"));
                }
            }));
            
            this.display.ContextMenu = contextMenu;
            this.display.disp.MouseDown += (o, e) =>
            {
                _mouseDown = true;
                disp_MouseMove(o, e);
            };
            this.display.disp.MouseUp += (o, e) => _mouseDown = false;
            this.display.disp.MouseMove += disp_MouseMove;

            StepActions.StepEnd += this.StepActions_StepEnd;

            this.Load += MemoryVisualizer_Load;

            this.labelVersion.Text = PluginCore.Ver.ToString(); //automatic window title

            this.cbBrushType.SelectedIndex = 0;

            this.display.Cursor = LoadCustomCursor("MemoryVisualizer.paintbrush.cur");
        }

        private void MemoryVisualizer_Load(object sender, EventArgs e)
        {
            try
            {
                bRefreshDomains_Click(null, null);
            }
            catch
            {
                //No domains loaded
            }
        }

        private void SliderDelay_ValueChanged(object arg1, EventArgs arg2)
        {
            lock (_executeLock)
            {
                this._delayFrames = (int)this.sliderDelay.Value;
                this._framesToNextExecute = (int)this.sliderDelay.Value;
            }
        }

        //Gives illusion of smoothness, skipping frames where it is still getting the previous frame
        //executes on a task

        private bool _inStep; //dont let steps be counted if still in operation. lag like an old machine.
        private void StepActions_StepEnd(object sender, EventArgs e)
        {
            if (_running && !_inStep)
            {
                _inStep = true;
                
                try
                {
                    _framesToNextExecute--;
                    if (_framesToNextExecute <= 0)
                    {
                        if (!_gettingBytes)
                        {
                            LoopMethod();
                        }
                        _framesToNextExecute = _delayFrames;
                    }
                }
                finally
                {
                    _inStep = false;
                }
            }
        }

        private void StartRunning()
        {
            this._running = true;
            this._framesToNextExecute = this._delayFrames;
            this.bLoop.Text = "Stop Updating";
        }

        private void StopRunning()
        {
            this._running = false;
            this._framesToNextExecute = this._delayFrames;
            this.bLoop.Text = "Start Updating";
        }

        private void CbDomains_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingDomains) { return; }
            lock (_executeLock)
            {
                this._domain = cbDomains.SelectedItem?.ToString();
                UpdateAllSizes();
                UpdateImage();
            }
        }

        private void LoopMethod()
        {
            this._gettingBytes = true; //Outside the lock
            lock (this._executeLock)
            {
                try
                {
                    this.UpdateImage();
                }
                catch
                {
                    _logger.Error("Failed to UpdateImage()");
                    this.StopRunning();
                    throw;
                }
            }
            this._gettingBytes = false; //Outside the lock
        }

        private void UpdateAllSizes()
        {
            var mi = MemoryDomains.GetInterface(_domain);
            if (mi == null) { return; }

            this._pageSize = (long)(display.W * display.H) * display.CurFormat.BytesWide;
            this._offsetIncr = this.display.CurFormat.BytesWide;
            //if (this.domainSize <= 0L)
            //{
            //    return;
            //}
            var dSize = mi.Size;
            long max = dSize - this._pageSize - 1L - this._align % this.display.CurFormat.BytesWide;
            if (max < 0L)
            {
                return;
            }
            this.sliderOffset.Maximum = max;
            this.sliderOffset.Value = Math.Min(this.sliderOffset.Maximum, Math.Max(this.sliderOffset.Minimum, this.sliderOffset.Value - this.sliderOffset.Value % this._offsetIncr));
        }

        private void CbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            lock (_executeLock)
            {
                //PixFormat curFormat = this.display.curFormat;
                this.display.CurFormat = PixFormats.Get(this.cbFormat.SelectedItem.ToString());
                this.nAlignment.Maximum = this.display.CurFormat.BytesWide - 1;

                if ((int)this.nWidth.Value % display.CurFormat.Pixels > 0)
                {
                    this.nWidth.Value += (int)this.nWidth.Value % display.CurFormat.Pixels;
                }
                this.UpdateAllSizes();
                this.UpdateImage();
            }

            tbColor.MaxLength = display.CurFormat.BytesWide * 2;
        }

        private void MemoryVisualizer_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.StopRunning();
            StepActions.StepEnd -= this.StepActions_StepEnd;
        }

        private long SliderOffsetCorrection(long val)
        {
            if (val % this._offsetIncr == 0L)
            {
                return val;
            }
            long curVal = val;
            if (curVal > this._offset && this._offsetIncr > 1L)
            {
                curVal += this._offsetIncr;
            }
            long correctedVal = curVal - curVal % this._offsetIncr;
            if (correctedVal > this.sliderOffset.Maximum)
                correctedVal = this.sliderOffset.Maximum;
            if (correctedVal < 0L)
                correctedVal = 0L;
            return correctedVal;
        }

        private void SliderOffset_ValueChanged(object sender, EventArgs e)
        {
            lock (_executeLock)
            {
                this._offset = this.sliderOffset.Value;
                UpdateImage();
            }
        }

        private void UpdateImage()
        {
            try
            {

                //if (this.Domain == null) { return; } included in get interface
                var mi = MemoryDomains.GetInterface(_domain);
                if (mi == null) { return; }

                long start = this._offset + this._align;
                long end = start + (long)(display.W * display.H) * this.display.CurFormat.BytesWide;
                //long numBytesToGet = this.w * this.h * this.display.curFormat.BytesWide;
                if (end >= mi.Size)
                {
                    end = mi.Size - 1;
                }

                byte[] byteArr = mi.PeekBytes(start, end, true);
                //byte[] byteArr = this.GetByteArr(start, start + (long)numBytesToGet);
                this._rangeStartAddress = start;
                this._rangeEndAddress = end + 1L; //+1 because vmds are exclusive
                if (byteArr == null)
                {
                    this._rangeStartAddress = start;
                    this._rangeEndAddress = end + 1L;
                    return;
                }

                SyncObjectSingleton.SyncObjectExecute(Instance, (o, e) =>
                { //this forces that part of code to execute on the main thread
                    this.display.SetBytes(byteArr);
                    this.display.Refresh();
                });
            }
            catch
            {
                StopRunning(); //failsafe
                throw;
            }
        }

        //private byte[] GetByteArr(long start, long end)
        //{
        //    return end >= this.memoryInterface.Size ? (byte[])null : this.memoryInterface.PeekBytes(start, end, true);
        //}

        //TODO: test with citra
        private async Task LegacyLoop()
        {
            while (this._running)
            {
                try
                {
                    this.UpdateImage();
                }
                catch
                {
                    this.StopRunning();
                    throw;
                }
                await Task.Delay((int)this.sliderDelay.Value);
            }
        }

        private void UpdateImageSize()
        {
            lock (_executeLock)
            {
                this.display.SetSize((int)this.nWidth.Value, (int)this.nHeight.Value);
                this.UpdateAllSizes();
                this.UpdateImage();
            }
        }

        private int _lastWidth = 256;
        private void nWidth_ValueChanged(object sender, EventArgs e)
        {

            if ((int)this.nWidth.Value % display.CurFormat.Pixels > 0)
            {
                this.nWidth.Value = Math.Max(1, this.nWidth.Value + (((int)nWidth.Value > _lastWidth ?  1 : -1) * ((int)this.nWidth.Value % display.CurFormat.Pixels)));
            }
            else
            {
                _lastWidth = (int)nWidth.Value;
                this.UpdateImageSize();
            }
            ////TODO: FIX
            //if (display.curFormat is PixYCbYCr)
            //{

            //}
            //else
            //{
            //    this.UpdateImageSize();
            //}
        }

        private void nHeight_ValueChanged(object sender, EventArgs e)
        {
            this.UpdateImageSize();
        }

        private void bPullOnce_Click(object sender, EventArgs e)
        {
            lock (_executeLock)
            {
                UpdateImage();
            }
        }

        private async void bLoop_Click(object sender, EventArgs e)
        {
            lock (_executeLock)
            {
                if (!this._running)
                {
                    this.bPullOnce.Enabled = false;
                    this.StartRunning();
                    //if (!this.legacyLoop)
                    //    return;
                    //await this.LegacyLoop();


                    //todo, remove this when we've fixed the cross-thread issues
                    //gbSettings.Enabled = false;

                }
                else
                {
                    this.bPullOnce.Enabled = true;
                    this.StopRunning();

                    //todo, remove this when we've fixed the cross-thread issues
                    //gbSettings.Enabled = true;
                }
            }
            await Task.Delay(1);
        }

        private void nAlignment_ValueChanged(object sender, EventArgs e)
        {
            lock (_executeLock)
            {
                this._align = (long)this.nAlignment.Value;
                this.UpdateAllSizes();
                this.UpdateImage();
            }
        }
        private void bRefreshDomains_Click(object sender, EventArgs e)
        {
            this.StopRunning();
            lock (_executeLock)
            {
                this.sliderOffset.Value = 0L;
                string previousDomain = this._domain;
                this._domain = null;

                this._updatingDomains = true; //Prevent updates
                this.cbDomains.Items.Clear();
                try
                {
                    //Get memory domain names
                    string[] strArray = (AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES] as MemoryDomainProxy[])?
                        .Select(x => x.Name)
                        .ToArray();
                    //strArray = (AllSpec.VanguardSpec[VSPEC.MEMORYDOMAINS_INTERFACES] as MemoryDomainProxy[]).Select(x => x.Name).ToArray();
                    if (strArray != null && strArray.Length > 0)
                    {
                        this.cbDomains.Items.AddRange(strArray);
                        this._updatingDomains = false;

                        if (previousDomain != null && this.cbDomains.Items.Contains(previousDomain))
                        {
                            this._domain = previousDomain;
                            this.cbDomains.SelectedItem = previousDomain; // cbDomains.Items.Cast<string>().Where(x => x == previousDomain);//IDK
                        }
                        else
                        {
                            this.cbDomains.SelectedIndex = 0;
                            this._domain = cbDomains.SelectedItem.ToString(); //Unneeded?
                        }

                        this.UpdateAllSizes();
                        this.UpdateImage();
                    }
                    else
                    {
                        this._domain = null;
                    }
                }
                catch
                {
                    StopRunning();
                    this._domain = null;
                    throw;
                }
            }
        }

        private async void bBackFull_Click(object sender, EventArgs e)
        {
            lock (_executeLock)
            {
                this.sliderOffset.Value -= this._pageSize;
                this.UpdateImage();
            }
            await Task.Delay(1);
        }

        private async void bForwardPage_Click(object sender, EventArgs e)
        {
            lock (_executeLock)
            {
                this.sliderOffset.Value += this._pageSize;
                this.UpdateImage();
            }
            await Task.Delay(1);
        }

        private void bMinusRow_Click(object sender, EventArgs e)
        {
            lock (_executeLock)
            {
                this.sliderOffset.Value -= this._pageSize / (long)this.nHeight.Value * (long)this.nRowAmt.Value;
                this.UpdateImage();
            }
        }

        private void bPlusRow_Click(object sender, EventArgs e)
        {
            lock (_executeLock)
            {
                this.sliderOffset.Value += this._pageSize / (long)this.nHeight.Value * (long)this.nRowAmt.Value;
                this.UpdateImage();
            }
        }

        private void cbLegacyLoop_CheckedChanged(object sender, EventArgs e)
        {
            //this.legacyLoop = this.cbLegacyLoop.Checked;
        }

        private void nRowAmt_ValueChanged(object sender, EventArgs e)
        {

        }

        private void bCopyRange_Click(object sender, EventArgs e)
        {
            lock (_executeLock)
            {
                Clipboard.SetText(this._rangeStartAddress.ToString("X") + "-" + this._rangeEndAddress.ToString("X"));
            }
        }

        private void bSaveImage_Click(object sender, EventArgs e)
        {
            lock (_executeLock)
            {
                //byte[] result = null;
                using (MemoryStream stream = new MemoryStream())
                {
                    //Bitmap b = new Bitmap(display.bitmap);
                    display.Bitmap.Bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    if (stream.Length <= 0)
                    {
                        return;
                    }

                    SaveFileDialog s = new SaveFileDialog();
                    s.Filter = "png files (*.png)|*.png";
                    var r = s.ShowDialog();
                    if (r == DialogResult.OK)
                    {
                        File.WriteAllBytes(s.FileName, stream.ToArray());
                    }
                    //using (MemoryStream ms = new MemoryStream(stream.ToArray()))
                    //{
                    //    IDataObject dataObj = new DataObject();
                    //    dataObj.SetData("PNG", false, stream);
                    //    Clipboard.SetDataObject(dataObj, true);
                    //    //Clipboard.SetData(DataFormats.Bitmap, Image.FromStream(ms));
                    //}
                    //result = stream.ToArray();
                }
            }
        }

        private void disp_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseDown)
            {
                _lastMousePosition = new Point(int.MaxValue, int.MaxValue);
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (e.X < 0 || e.Y < 0 || e.X >= display.disp.Width || e.Y >= display.disp.Height)
                    return;
                
                if (_lastMousePosition.X == int.MaxValue && _lastMousePosition.Y == int.MaxValue)
                {
                    _lastMousePosition = new Point(e.X, e.Y);
                }

                int realWidth;
                int realHeight;
                int offsetX = 0;
                int offsetY = 0;

                if (display.W < display.H)
                {
                    realWidth = (int)(display.W / (float)display.H * display.disp.Height);
                    realHeight = display.disp.Height;
                }
                else
                {
                    realWidth = display.disp.Width;
                    realHeight = (int)(display.H / (float)display.W * display.disp.Width);
                }
                
                if (display.disp.Width > realWidth)
                {
                    offsetX = (display.disp.Width - realWidth) / 2;
                }
                if (display.disp.Height > realHeight)
                {
                    offsetY = (display.disp.Height - realHeight) / 2;
                }
                
                PointF startPixel = new PointF
                (
                    (e.X - offsetX) * display.W / realWidth,
                    (e.Y - offsetY) * display.H / realHeight
                );
                PointF endPixel = new PointF
                (
                    (_lastMousePosition.X - offsetX) * display.W / realWidth,
                    (_lastMousePosition.Y - offsetY) * display.H / realHeight
                );
                
                int length = (int)
                    Math.Ceiling(
                        Math.Sqrt(
                            Math.Pow(startPixel.X - endPixel.X, 2) +
                            Math.Pow(startPixel.Y - endPixel.Y, 2)
                        )
                    );

                Brush(startPixel);
                for (int i = 0; i < length; i++)
                {
                    float t = (float)i / length;
                    PointF pixel = new PointF
                    (
                        endPixel.X + t * (startPixel.X - endPixel.X),
                        endPixel.Y + t * (startPixel.Y - endPixel.Y)
                    );

                    Brush(pixel);
                }

                _lastMousePosition = e.Location;
            }
            else
            {
                _lastMousePosition = new Point(int.MaxValue, int.MaxValue);
            }
        }
        
        private void Brush(PointF pixel)
        {
            switch (cbBrushType.SelectedIndex)
            {
                // Square
                case 0:
                {
                    for (int x = 0; x < nmBrushSize.Value; x++)
                    {
                        for (int y = 0; y < nmBrushSize.Value; y++)
                        {
                            Point pixelPoint = new Point((int)pixel.X + (x - ((int)nmBrushSize.Value / 2)), (int)pixel.Y + (y - ((int)nmBrushSize.Value / 2)));
                            if (pixelPoint.X < 0 || pixelPoint.Y < 0 || pixelPoint.X >= display.W || pixelPoint.Y >= display.H)
                                continue;

                            DrawPixel(pixelPoint);
                        }
                    }

                    break;
                }
                // Circle
                case 1:
                {
                    int radius = (int)nmBrushSize.Value / 2;
                    for (int x = -radius; x <= radius; x++)
                    {
                        for (int y = -radius; y <= radius; y++)
                        {
                            if (x * x + y * y <= radius * radius)
                            {
                                Point pixelPoint = new Point((int)pixel.X + x, (int)pixel.Y + y);
                                if (pixelPoint.X < 0 || pixelPoint.Y < 0 || pixelPoint.X >= display.W || pixelPoint.Y >= display.H)
                                    continue;

                                DrawPixel(pixelPoint);
                            }
                        }
                    }

                    break;
                }
                case 2:
                {
                    int radius = (int)nmBrushSize.Value / 2;
                    for (int x = -radius; x <= radius; x++)
                    {
                        for (int y = -radius; y <= radius; y++)
                        {
                            if (x * x + y * y <= radius * radius)
                            {
                                Point pixelPoint = new Point((int)pixel.X + x, (int)pixel.Y + y);
                                if (RND.NextDouble() < 0.03)
                                {
                                    DrawPixel(pixelPoint);
                                }
                            }
                        }
                    }

                    break;
                }
            }
        }

        private bool DrawPixel(Point pixel)
        {
            long address = this._rangeStartAddress + (long)(pixel.Y * display.W + pixel.X) * this.display.CurFormat.BytesWide;
                
            if (address < this._rangeStartAddress || address >= this._rangeEndAddress)
            {
                return false;
            }
                
            MemoryInterface mi = MemoryDomains.GetInterface(_domain);
            if (mi == null)
                return false;

            byte[] data = new byte[this.display.CurFormat.BytesWide];
            byte[] colorBytes = BitConverter.GetBytes(tbColor.ToRawInt().Value);
            Array.Copy(colorBytes, data, Math.Min(data.Length, colorBytes.Length));
            Array.Reverse(data);
            mi.PokeBytes(address, data);
            
            return true;
        }
    }
}
