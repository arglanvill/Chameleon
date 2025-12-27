using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core
{
    interface IClockedComponent : IComponent
    {
        void TickHigh();
        void TickLow();
    }
}
