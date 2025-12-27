using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chameleon.Emulator.Core.Video;
using Microsoft.Graphics.Canvas;
using Windows.Graphics.DirectX;

namespace Chameleon.Host.Display
{
    class Win2DBitmap : IBitmap
    {
        public Win2DBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Data = new byte[Width * Height * sizeof(UInt32)];
        }
        public int Width { get; }

        public int Height { get; }

        public void SetPixels(int p, byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
                Data[p++] = data[i];
        }

        public CanvasBitmap GetCanvasBitmap(CanvasDrawingSession session)
        {
            return CanvasBitmap.CreateFromBytes(session, Data, Width, Height, DirectXPixelFormat.B8G8R8A8UIntNormalized);
        }
        private byte[] Data;
    }
}
