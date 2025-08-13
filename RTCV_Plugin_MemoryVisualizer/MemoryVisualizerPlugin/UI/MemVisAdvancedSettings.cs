using System;
using System.Windows.Forms;

namespace MemoryVisualizer.UI
{
    public partial class MemVisAdvancedSettings : Form
    {
        public static int PixelsPerThread { get; private set; } = 256;
        public MemVisAdvancedSettings()
        {
            InitializeComponent();
            PixelsPerThread = (int)nPixPerThread.Value;
            nPixPerThread.ValueChanged += nPixPerThread_ValueChanged;
        }

        private void nPixPerThread_ValueChanged(object sender, EventArgs e)
        {
            PixelsPerThread = (int)nPixPerThread.Value;
        }
    }
}
