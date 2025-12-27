using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chameleon.Emulator.Core;
using Chameleon.Emulator.Core.Memory;
using Chameleon.Emulator.Core.Video;

namespace Chameleon.Emulator.Components.Video
{
    class VICII : RasterVideoController
    {
        public VICII(IDisplay display) : base(display)
        {

        }
        public void SetMemoryBus(MemoryBus memoryBus)
        {
            MemoryBus = memoryBus;
        }

        public void SetColorRAM(RAM colorRAM)
        {
            ColorRAM = colorRAM;
        }
        public void MapRegisters(MemoryBus MemoryBus, uint address)
        {
            MemoryRegisters.AddRegister(0x0011, RegisterControlRegister1);
            MemoryRegisters.AddRegister(0x0012, RegisterRaster);
            MemoryRegisters.AddRegister(0x0018, RegisterMemoryPointers);
            MemoryRegisters.AddRegister(0x0020, RegisterBorderColor);
            MemoryRegisters.AddRegister(0x0021, RegisterBackgroundColor0);
            MemoryBus.EnableReadableMemory(MemoryRegisters, address);
            MemoryBus.EnableWritableMemory(MemoryRegisters, address);
        }
        public override void TickLow()
        {
            if (!HBlankEnabled && !VBlankEnabled)
                DrawByte();
        }

        public override void TickHigh()
        {
            NextCycle();
            // Handle character code access
            if (RasterY >= BadLineRangeStart && RasterY <= BadLineRangeEnd && (RasterY & YSCROLL) == 0 && RegisterControlRegister1.TestBits(DEN))
            {
                // TODO: Stun the CPU in cycle 12
                if (RasterLineCycle == StunCPUCycle)
                {
                    TextLineCounter = 0;
                }
                // Read character codes in cycles 15-54
                if (RasterLineCycle >= CharacterCycleStart && RasterLineCycle <= CharacterCycleEnd)
                {
                    // Read character code from video matrix
                    UInt16 address = (UInt16)((RegisterMemoryPointers.GetBits(VIDEOMATRIX) << 6) | VideoMatrixCounter);
                    CharacterCodeBuffer[CharacterBufferCounter] = MemoryBus.Get8(address);
                    // Read character color from color ram
                    CharacterColorBuffer[CharacterBufferCounter] = ColorRAM.Get8(VideoMatrixCounter);
                    CharacterBufferCounter++;
                    VideoMatrixCounter++;
                }
                // TODO: Wake the CPU when extra cycles are no longer needed
                // if (some condition)
                // 

            }
        }

        public void ConnectSignalLines(SignalLine ready, SignalLine irq)
        {
            CPUReady = ready;
            CPUIRQ = irq;
            CPUReady.Connect(this);
            CPUIRQ.Connect(this);
        }

        public void DrawByte()
        {
            for (int i = 0x80; i != 0; i >>= 1)
            {
                UpdateBorderUnit();
                DrawBit(i);
                NextPixel();
            }
            if (!MainBorderEnabled)
                TextColumnCounter++;
            // Send to screen buffer
            DisplayBuffer.SetPixels(DisplayBufferBytePosition, DrawCycleBuffer);
            DisplayBufferBytePosition += DrawCycleBufferSize;
        }

        public void DrawBit(int bit)
        {
            UInt32 Color;
            if (MainBorderEnabled)
                Color = ColorsBGRA[RegisterBorderColor.GetBits(0x0F)];
            else
            {
                byte code = CharacterCodeBuffer[TextColumnCounter];
                UInt16 address = (UInt16)((RegisterMemoryPointers.GetBits(CHARACTERBANK) << 10) | code << 3 | (UInt16)TextLineCounter);
                byte data = MemoryBus.Get8(address);
                if ((data & bit) == bit)
                    Color = ColorsBGRA[CharacterColorBuffer[TextColumnCounter] & COLOR];
                else
                    Color = ColorsBGRA[RegisterBackgroundColor0.GetBits(COLOR)];
            }

            DrawCycleBuffer[DrawCycleBufferBytePosition++] = (byte)((Color >> 24) & 0xFF);
            DrawCycleBuffer[DrawCycleBufferBytePosition++] = (byte)((Color >> 16) & 0xFF);
            DrawCycleBuffer[DrawCycleBufferBytePosition++] = (byte)((Color >> 8) & 0xFF);
            DrawCycleBuffer[DrawCycleBufferBytePosition++] = (byte)(Color & 0xFF);
        }

        public void NextPixel()
        {
            if (RasterX < RasterWidth)
            {
                RasterX++;
                X++;
             }
        }

        public override void NextCycle()
        {
            base.NextCycle();
            DrawCycleBufferBytePosition = 0;
            if (RasterLineCycle == XOriginCycle)
                X = 0;
        }
        public override void NextLine()
        {
            base.NextLine();
            CharacterBufferCounter = 0;
            TextColumnCounter = 0;
            TextLineCounter++;
            if (RasterY <= 255)
                RegisterRaster.SetBits((byte)RasterY);
        }

        public override void NextFrame()
        {
            base.NextFrame();
            VideoMatrixCounter = 0;
        }
        public void UpdateBorderUnit()
        {
            if (X == HorizontalBorderFlip40Columns)
                MainBorderEnabled = true;
            if (RasterLineCycle == 63)
            {
                if (RasterY == VerticalBorderFlip25Rows)
                    VerticalBorderEnabled = true;
                if (RasterY == VerticalBorderFlop25Rows && RegisterControlRegister1.TestBits(DEN))
                    VerticalBorderEnabled = false;
            }
            if (X == HorizontalBorderFlop40Columns)
            {
                if (RasterY == VerticalBorderFlip25Rows)
                    VerticalBorderEnabled = true;
                if (RasterY == VerticalBorderFlop25Rows && RegisterControlRegister1.TestBits(DEN))
                    VerticalBorderEnabled = false;
                if (VerticalBorderEnabled == false)
                    MainBorderEnabled = false;
            }
        }

        public override void PowerOn()
        {
            // Raster line and cycle numbers (0-based)
            RasterHeight = 263;
            VBlankOnLine = 6;
            VBlankOffLine = 33;
            CyclesPerRasterLine = 65;
            RasterWidth = 520;
            HBlankOnCycle = 61;
            HBlankOffCycle = 11;

            RasterX = RasterWidth - 1;
            RasterY = VBlankOffLine - 1;
            RasterLineCycle = CyclesPerRasterLine - 1;

            HBlankEnabled = true;
            MainBorderEnabled = true;
            VerticalBorderEnabled = true;

            DisplayWidth = (HBlankOnCycle - HBlankOffCycle) * 8;
            DisplayHeight = RasterHeight - VBlankOffLine + VBlankOnLine;

            RegisterControlRegister1.SetBits(DEN);
            RegisterBorderColor.SetBits(COLOR, LightBlue);
            RegisterBackgroundColor0.SetBits(COLOR, Blue);
            RegisterMemoryPointers.SetBits(VIDEOMATRIX, 0x10);
            RegisterMemoryPointers.SetBits(CHARACTERBANK, 0x04);

            GetDisplayBuffer();
        }

        public override void PowerOff()
        {
        }

        const int Black = 0;
        const int White = 1;
        const int Red = 2;
        const int Cyan = 3;
        const int Purple = 4;
        const int Green = 5;
        const int Blue = 6;
        const int Yellow = 7;
        const int Orange = 8;
        const int Brown = 9;
        const int LightRed = 10;
        const int DarkGray = 11;
        const int MediumGray = 12;
        const int LightGreen = 13;
        const int LightBlue = 14;
        const int LightGray = 15;

        readonly UInt32[] ColorsBGRA =
        {
            0x000000FF,     // Black
            0xFFFFFFFF,     // White
            0x000088FF,     // Red
            0xEEFFAAFF,     // Cyan
            0xCC44CCFF,     // Purple
            0x55CC00FF,     // Green
            0xAA0000FF,     // Blue
            0x77EEEEFF,     // Yellow
            0x5588DDFF,     // Orange
            0x004466FF,     // Brown
            0x7777FFFF,     // Light Red
            0x333333FF,     // Dark Gray
            0x777777FF,     // Medium Gray
            0x66FFAAFF,     // Light Green
            0xFF8800FF,     // Light Blue
            0xBBBBBBFF      // Light Gray
        };
        
        // Registers
        private Register8 RegisterBorderColor = new Register8("EC");
        private Register8 RegisterBackgroundColor0 = new Register8("B0C");
        private Register8 RegisterControlRegister1 = new Register8("CR1");
        private Register8 RegisterMemoryPointers = new Register8("MP");
        private Register8 RegisterRaster = new Register8("RASTER");

        // Register mask bits
        const int DEN = 0x10;
        const int COLOR = 0x0F;
        const int VIDEOMATRIX = 0xF0;
        const int CHARACTERBANK = 0x0E;
        const int YSCROLL = 0x07;

        private RAM ColorRAM;

        private MemoryBus MemoryBus;
        private MemoryMappedRegisters MemoryRegisters = new MemoryMappedRegisters(0x2f);

        const int VerticalBorderFlip25Rows = 251;
        const int VerticalBorderFlop25Rows = 51;
        const int VerticalBorderFlip24Rows = 247;
        const int VerticalBorderFlop24Rows = 55;

        // Line pixel numbers (0-based)
        const int HorizontalBorderFlip40Columns = 344;
        const int HorizontalBorderFlop40Columns = 24;
        const int HorizontalBorderFlip38Columns = 335;
        const int HorizontalBorderFlop38Columns = 31;

        // First displayed pixels
        const int XOriginCycle = 13;

        public bool MainBorderEnabled;
        public bool VerticalBorderEnabled;

        const int DrawCycleBufferSize = 32; // 8 pixels * 4 color bytes
        private byte[] DrawCycleBuffer = new byte[DrawCycleBufferSize];
        private int DrawCycleBufferBytePosition;

        // Character codes for current row
        private byte[] CharacterCodeBuffer = new byte[40];
        private byte[] CharacterColorBuffer = new byte[40];
        private int CharacterBufferCounter;
        const int BadLineRangeStart = 48;
        const int BadLineRangeEnd = 247;
        const int StunCPUCycle = 12;
        const int CharacterCycleStart = 15;
        const int CharacterCycleEnd = 54;
        private UInt16 VideoMatrixCounter;
        private byte TextColumnCounter;
        private byte TextLineCounter;

        // Signal lines to the CPU
        SignalLine CPUReady;
        SignalLine CPUIRQ;

        public override string Name => "MOS VICII";
    }
}
