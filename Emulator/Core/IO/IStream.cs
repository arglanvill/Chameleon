using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.IO
{
    public interface IStream : IDisposable
    {
        int Read(byte[] buffer, int offset, uint count);
    }
}
