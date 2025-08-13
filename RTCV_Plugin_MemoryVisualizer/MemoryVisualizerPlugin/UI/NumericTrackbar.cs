namespace MemoryVisualizer.UI
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    public partial class NumericTrackbar : UserControl
    {
        private long _maximum = ushort.MaxValue;
        private string _name = "Name";
        private volatile bool updatingFromCode;
        private long _value;
        private long _minimum;
        public Func<long, long> ValueCorrection;

        public event Action<object, EventArgs> ValueChanged;

        public virtual void OnValueChanged(EventArgs e)
        {
            Action<object, EventArgs> valueChanged = this.ValueChanged;
            valueChanged?.Invoke(this, e);
        }

        [Description("Value the control holds")]
        [Category("Data")]
        public long Value
        {
            get => this._value;
            set
            {
                if (this.updatingFromCode)
                    return;
                
                if (value > this.Maximum)
                    value = this.Maximum;
                else if (value < this.Minimum)
                    value = this.Minimum;
                
                if (this.ValueCorrection != null)
                    value = this.ValueCorrection(value);
                
                this._value = value;
                this.GeneralUpdate();
                this.OnValueChanged(null);
            }
        }

        [Description("Whether or not the NumericUpDown should use hex")]
        [Category("Data")]
        public bool Hexadecimal
        {
            get => this.nmBox.Hexadecimal;
            set => this.nmBox.Hexadecimal = value;
        }

        [Description("Minimum value of the control")]
        [Category("Data")]
        public long Minimum
        {
            get => this._minimum;
            set
            {
                this.updatingFromCode = true;
                this._minimum = value;
                this.nmBox.Minimum = value;
                this.tbSlider.Minimum = Convert.ToInt32(value);
                this.updatingFromCode = false;
            }
        }

        [Description("Maximum value of the control")]
        [Category("Data")]
        public long Maximum
        {
            get => this._maximum;
            set
            {
                this._maximum = value;
                this.nmBox.Maximum = value;
            }
        }

        [Description("Displayed label text")]
        [Category("Data")]
        public string Label
        {
            get => this._name;
            set
            {
                this._name = value;
                this.lbName.Text = value;
            }
        }

        public NumericTrackbar()
        {
            this.InitializeComponent();
        }

        private void GeneralUpdate()
        {
            this.updatingFromCode = true;
            this.nmBox.Value = this._value;
            this.tbSlider.Value = (int)((double)this.Value / this.Maximum * this.tbSlider.Maximum);
            this.updatingFromCode = false;
        }

        private double SliderPercentage() => (double)this.tbSlider.Value / this.tbSlider.Maximum;

        private double BoxPercent() => (double)this.nmBox.Value / (double)this.nmBox.Maximum;

        private void NumericTrackbar_Load(object sender, EventArgs e)
        {
            this.updatingFromCode = true;
            this.tbSlider.Maximum = 65536;
            this.tbSlider.TickFrequency = 6553;
            this.updatingFromCode = false;
            this.tbSlider.ValueChanged += new EventHandler(this.TbSlider_ValueChanged);
            this.nmBox.ValueChanged += new EventHandler(this.NmBox_ValueChanged);
        }

        private void NmBox_ValueChanged(object sender, EventArgs e)
        {
            if (this.updatingFromCode)
                return;
            this.updatingFromCode = true;
            
            if (this.ValueCorrection != null)
                this.nmBox.Value = this.ValueCorrection((long)this.nmBox.Value);
            
            this.tbSlider.Value = Math.Min(this.tbSlider.Maximum, Math.Max(this.tbSlider.Minimum, (int)((double)this.Value / (double)this.Maximum * (double)this.tbSlider.Maximum)));
            this._value = (long)this.nmBox.Value;
            this.updatingFromCode = false;
            this.OnValueChanged((EventArgs)null);
        }

        private void TbSlider_ValueChanged(object sender, EventArgs e)
        {
            if (this.updatingFromCode)
                return;
            this.updatingFromCode = true;
            
            long val2 = (long)(this.SliderPercentage() * (double)this.Maximum);
            
            if (this.ValueCorrection != null)
                val2 = this.ValueCorrection(val2);
            this.nmBox.Value = Math.Min((long)this.nmBox.Maximum, Math.Max((long)this.nmBox.Minimum, val2));
            
            this._value = (long)this.nmBox.Value;
            this.updatingFromCode = false;
            this.OnValueChanged((EventArgs)null);
        }
    }
    internal class NoFocusTrackBar : TrackBar
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        private static int MakeParam(int loWord, int hiWord)
        {
            return (hiWord << 16) | (loWord & 0xffff);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            SendMessage(this.Handle, 0x0128, MakeParam(1, 0x1), 0);
        }
    }
}
