using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.Video
{
    public interface IBitmap
    {
        void SetPixels(int p, byte[] data);
        int Width { get; }
        int Height { get; }
    }
}
