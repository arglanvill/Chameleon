using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chameleon.Emulator.Core;
using Chameleon.Emulator.Core.Memory;
using Chameleon.Emulator.Components.CPU;
using Chameleon.Emulator.Components.Video;
using Chameleon.Emulator.Core.Video;
using Chameleon.Emulator.Core.IO;
using Windows.UI.WebUI;
using Chameleon.Debugger;

namespace Chameleon.Emulator.Systems.Computers
{
    class C64 : System
    {
        public C64(Chameleon.Host.Host host) : base(host)
        {
            Assemble();
        }

        public override void PowerOff()
        {
            MasterClock.PowerOff();
            VICII.PowerOff();
            CPU.PowerOff();
            _IsPoweredOn = false;
        }

        public override void PowerOn()
        {
            CPU.PowerOn();
            VICII.PowerOn();
            MasterClock.PowerOn();
            _IsPoweredOn = true;
        }

        public override void UpdateFrame()
        {
            if (_IsPoweredOn)
                MasterClock.UpdateFrame();
        }

        public override void DrawFrame(params Object[] drawParams)
        {
            if (_IsPoweredOn)
                VICII.DrawFrame(drawParams);
        }

        public override SystemDebugger Debugger
        {
            get => MasterClock.Debugger;
            set => MasterClock.Debugger = value;
        }
        private void Assemble()
        {
            using (IStream stream = Host.GetResourceStream("Chameleon.Emulator.ROMS.C64.characters.901225-01.bin"))
                CharacterROM.Initialize(stream);
            using (IStream stream = Host.GetResourceStream("Chameleon.Emulator.ROMS.C64.kernal.901227-03.bin"))
                KernalROM.Initialize(stream);
            using (IStream stream = Host.GetResourceStream("Chameleon.Emulator.ROMS.C64.basic.901226-01.bin"))
                BASICROM.Initialize(stream);
            AddComponent(CPU = new MOS6510());
            CPUMemoryBus.EnableReadableMemory(MainRAM, 0x0000);
            CPUMemoryBus.EnableWritableMemory(MainRAM, 0x0000);
            CPUMemoryBus.EnableReadableMemory(KernalROM, 0xE000);
            CPUMemoryBus.EnableReadableMemory(BASICROM, 0xA000);
            CPUMemoryBus.EnableReadableMemory(ColorRAM, 0xD800);
            CPUMemoryBus.EnableWritableMemory(ColorRAM, 0xD800);
            CPU.MemoryBus = CPUMemoryBus;
            VICII = new VICII(Host.GetDisplay());
            VICIIMemoryBus.EnableReadableMemory(MainRAM, 0x0000, 0x4000);
            VICIIMemoryBus.EnableWritableMemory(MainRAM, 0x0000, 0x4000);
            VICIIMemoryBus.EnableReadableMemory(CharacterROM, 0x1000);
            VICII.SetMemoryBus(VICIIMemoryBus);
            VICII.MapRegisters(CPUMemoryBus, 0xD000);
            VICII.SetColorRAM(ColorRAM);
            VICII.ConnectSignalLines(CPU.Ready, CPU.IRQ);
            MasterClock.AddClock(SystemClock);
            SystemClock.Connect(CPU);
            SystemClock.Connect(VICII);
            MainRAM.Put8(0x0428, 0x01);
            MainRAM.Put8(0x0429, 0x02);
            MainRAM.Put8(0x042A, 0x03);
            ColorRAM.Put8(0x028, 0x01);
            ColorRAM.Put8(0x029, 0x01);
            ColorRAM.Put8(0x02A, 0x01);
        }

        public override void Step()
        {
            if (_IsPoweredOn)
                MasterClock.Tick();
        }

        MOS6510 CPU;
        VICII VICII;
        MasterClock MasterClock = new MasterClock();
        Clock SystemClock = new Clock(1022727);
        MemoryBus CPUMemoryBus = new MemoryBus(0x10000);
        MemoryBus VICIIMemoryBus = new MemoryBus(0x4000);
        RAM MainRAM = new RAM(0x10000);      // 64K
        ROM BASICROM = new ROM(0x2000);      // 8K
        ROM KernalROM = new ROM(0x2000);     // 8K
        ROM CharacterROM = new ROM(0x1000);  // 4K
        RAM ColorRAM = new RAM(0x400);       // 1K

        bool _IsPoweredOn;
        public override bool IsPoweredOn => _IsPoweredOn;

        public override string Name => "C64";
    }
}
