using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core
{
    class Clock : Component, IClockedComponent
    {
        public Clock(ulong frequency)
        {
            Frequency = frequency;
        }
        public ulong Frequency { get; set; }

        public override string Name => throw new NotImplementedException();

        public void Connect(IClockedComponent component)
        {
            Outputs.Add(component);
        }

        public void TickLow()
        {
            foreach (IClockedComponent output in Outputs)
                output.TickLow();
        }
        public void TickHigh()
        {
            foreach (IClockedComponent output in Outputs)
                output.TickHigh();
        }

        public override void PowerOn()
        {
        }

        public override void PowerOff()
        {
        }


        private readonly List<IClockedComponent> Outputs = new List<IClockedComponent>();
    }
}
