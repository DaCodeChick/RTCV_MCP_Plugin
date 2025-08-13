using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace MemoryVisualizer.UI
{
    //Made by KSHDO, used with permission https://github.com/ksHDO
    public class FastBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        private bool Disposed { get; set; }
        
        private readonly int[] _bits;
        private GCHandle _bitHandle;

        public FastBitmap(int width, int height)
        {
            Width = width;
            Height = height;

            _bits = new int[width * height];
            _bitHandle = GCHandle.Alloc(_bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, _bitHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int index, Color color)
        {
            _bits[index] = color.ToArgb();
        }

        public void SetPixel(int index, int color)
        {
            _bits[index] = color;
        }

        public void SetPixel(int index, ColorARGB color)
        {
            _bits[index] = color;
        }

        public void SetPixel(int x, int y, ColorARGB color)
        {
            int i = x + (y * Width);
            _bits[i] = color;
        }

        public void SetPixel(int x, int y, Color color)
        {
            int i = x + (y * Width);
            _bits[i] = color.ToArgb();
        }

        public void SetPixel(int x, int y, int color)
        {
            int i = x + (y * Width);
            _bits[i] = color;
        }

        public Color GetPixel(int x, int y)
        {
            int i = x + (y * Width);
            return Color.FromArgb(_bits[i]);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (Disposed)
                return;

            if (dispose)
            {
                Bitmap.Dispose();
                _bitHandle.Free();
            }

            Disposed = true;
        }
    }

    //From KSHDO, used with permission https://github.com/ksHDO
    public struct ColorARGB
    {
        private int Value;

        public ColorARGB(int argb)
        {
            Value = argb;
        }

        public ColorARGB(byte alpha, byte red, byte green, byte blue)
        {
            Value = (alpha << 24) | (red << 16) | (green << 8) | (blue);
        }

        public ColorARGB(int alpha, int red, int green, int blue) : this((byte)alpha, (byte)red, (byte)green, (byte)blue)
        {

        }

        public static implicit operator int(ColorARGB color)
        {
            return color.Value;
        }
    }

}
