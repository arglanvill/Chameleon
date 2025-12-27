using Chameleon.Emulator.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.CPU
{
    interface ICPU : IClockedComponent, IPausable
    {
        MemoryBus MemoryBus { get; set; }
    }
}
