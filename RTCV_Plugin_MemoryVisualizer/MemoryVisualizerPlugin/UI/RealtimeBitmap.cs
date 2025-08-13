namespace MemoryVisualizer.UI
{
    using System.Windows.Forms;
    using System.Drawing;
    using Formats;
    public partial class RealtimeBitmap : UserControl
    {
        public FastBitmap Bitmap;
        public PixFormat CurFormat = null;
        public int W = 256;
        public int H = 256;

        public RealtimeBitmap()
        {
            InitializeComponent();
            Bitmap = new FastBitmap(W, H);
            disp.Image = Bitmap.Bitmap;
        }

        public void SetSize(int w, int h)
        {
            var oldBmp = Bitmap;
            Bitmap = new FastBitmap(w, h);
            disp.Image = Bitmap.Bitmap;
            W = w;
            H = h;
            oldBmp.Dispose();
        }

        public void SetBytes(byte[] bytes)
        {
            //disp.Visible = false;
            SetPixels(bytes);
            //disp.Visible = true;
        }

        //protected override di

        //Does not refresh
        private void SetPixels(byte[] bts)
        {
            int pw = CurFormat.BytesWide;
            int mx = bts.Length;
            int curOffset = 0;

            if (!CurFormat.CustomParsing)
            {
                for (int y = 0; y < H; y++)
                {
                    for (int x = 0; x < W; x++)
                    {
                        if (curOffset + pw > mx)
                        {
                            //Missing textures yay
                            if ((y / 4) % 2 == 0)
                            {
                                if ((x / 4) % 2 == 0) Bitmap.SetPixel(x, y, Color.Magenta);
                                else Bitmap.SetPixel(x, y, Color.Black);
                            }
                            else
                            {
                                if ((x / 4) % 2 == 0) Bitmap.SetPixel(x, y, Color.Black);
                                else Bitmap.SetPixel(x, y, Color.Magenta);
                            }
                            //return;
                        }
                        else
                        {
                            Bitmap.SetPixel(x, y, CurFormat.Parse(bts, curOffset));
                            curOffset += pw;
                        }
                    }
                }
            }
            else
            {
                if (W % CurFormat.Pixels != 0) return;
                CurFormat.CustomParse(Bitmap, bts);
            }
        }
    }
}
