using Chameleon.Debugger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core
{
    class MasterClock : Component, IUnclockedComponent
    {
        class ManagedClock : Component, IClockedComponent
        {
            public ManagedClock(Clock clock)
            {
                Clock = clock;
            }
            public void Reset()
            {
                SkipTicks = ResetTicks;
            }

            public ulong SkipTicks { get; set; }
            public ulong ResetTicks { get; set; }
            public Clock Clock { get; set; }

            public override string Name => throw new NotImplementedException();

            public void TickLow()
            {
                if (--SkipTicks == 0)
                    Clock.TickLow();
            }

            public void TickHigh()
            {
                if (SkipTicks == 0)
                {
                    Clock.TickHigh();
                    Reset();
                }
            }

            public override void PowerOn()
            {
                Clock.PowerOn();
            }

            public override void PowerOff()
            {
                Clock.PowerOff();
            }
        }

        public void AddClock(Clock clock)
        {
            Clocks.Add(new ManagedClock(clock));
            ClockMultiplier *= clock.Frequency;
        }

        private void CalculateClocks()
        {
            foreach (ManagedClock clock in Clocks)
            {
                clock.ResetTicks = ClockMultiplier / clock.Clock.Frequency;
                clock.Reset();
            }
            // Master ticks per frame
            FrameTicks = ClockMultiplier / 60;
        }

        public void UpdateFrame()
        {
            for (ulong i = 0; i < FrameTicks; i++)
                Tick();
        }
        public void Tick()
        {
            foreach (ManagedClock clock in Clocks)
            {
                clock.TickLow();
                clock.TickHigh();
            }
        }

        public override void PowerOn()
        {
            CalculateClocks();
        }

        public override void PowerOff()
        {
        }

        public SystemDebugger Debugger;
        private readonly List<ManagedClock> Clocks = new List<ManagedClock>();
        private ulong ClockMultiplier = 1;
        private ulong FrameTicks;

        public override string Name => "Master Clock";
    }
}
