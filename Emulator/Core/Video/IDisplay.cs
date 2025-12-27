using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.Video
{
    public interface IDisplay
    {
        IBitmap GetDisplayBuffer(int width, int height);
        void DrawFrame(IBitmap bitmap, params Object[] drawParams);
    }
}
