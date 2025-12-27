using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chameleon.Emulator.Core.Video;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Chameleon.Host.Display
{
    class Win2DDisplay : IDisplay
    {
        public void DrawFrame(IBitmap bitmap, params object[] drawParams)
        {
            Win2DBitmap win2DBitmap = (Win2DBitmap)bitmap;
            // drawParams[0] = ICanvasAnimatedControl
            // drawParams[1] = CanvasAnimatedDrawEventArgs
            CanvasAnimatedDrawEventArgs args = (CanvasAnimatedDrawEventArgs)drawParams[1];
            args.DrawingSession.DrawImage(win2DBitmap.GetCanvasBitmap(args.DrawingSession));
        }

        public IBitmap GetDisplayBuffer(int width, int height)
        {
            return new Win2DBitmap(width, height);
        }
    }
}
