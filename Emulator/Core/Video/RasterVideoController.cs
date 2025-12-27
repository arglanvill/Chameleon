using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.Video
{
    abstract class RasterVideoController : Component, IClockedComponent
    {
        public RasterVideoController(IDisplay display)
        {
            Display = display;
        }

        public virtual void NextCycle()
        {
            if (++RasterLineCycle == CyclesPerRasterLine)
                NextLine();
            if (RasterLineCycle == HBlankOnCycle)
                HBlankEnabled = true;
            if (RasterLineCycle == HBlankOffCycle)
                HBlankEnabled = false;
        }
        public virtual void NextLine()
        {
            if (++RasterY == RasterHeight)
                RasterY = 0;
            if (RasterY == VBlankOnLine)
            {
                VBlankEnabled = true;
                NextFrame();
            }
            if (RasterY == VBlankOffLine)
                VBlankEnabled = false;

            RasterLineCycle = 0;
            RasterX = -1;
        }
        public virtual void NextFrame()
        {
            DisplayBufferBytePosition = 0;
        }

        public int DisplayWidth { get; set; }
        public int DisplayHeight { get; set; }
        public int RasterHeight { get; set; }
        public int RasterWidth { get; set; }
        public int CyclesPerRasterLine { get; set; }
        public int HBlankOnCycle { get; set; }
        public int HBlankOffCycle { get; set; }
        public int VBlankOnLine { get; set; }
        public int VBlankOffLine { get; set; }

        public int RasterLineCycle { get; set; }
        public int RasterX { get; set; }
        public int RasterY { get; set; }
        public int X { get; set; }

        public bool HBlankEnabled;
        public bool VBlankEnabled;
        public int DisplayBufferBytePosition;

        public bool HBlank { get; set; }
        public bool VBlank { get; set; }

        protected IBitmap DisplayBuffer;
        protected IDisplay Display;

        public abstract void TickHigh();
        public abstract void TickLow();

        public void GetDisplayBuffer()
        {
            DisplayBuffer = Display.GetDisplayBuffer(DisplayWidth, DisplayHeight);
        }

        public void DrawFrame(params Object[] drawParams)
        {
            Display.DrawFrame(DisplayBuffer, drawParams);
        }

    }
}
