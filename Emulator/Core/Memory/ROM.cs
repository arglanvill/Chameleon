using Chameleon.Emulator.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.Memory
{
    class ROM : Memory
    {
        public ROM(uint size) : base(size)
        {

        }

        public override string Name => "ROM";

        public void Initialize(byte[] data)
        {
            Array.Copy(data, Data, data.Length);
        }
        public void Initialize(IStream stream)
        {
            stream.Read(Data, 0, Size);
        }
    }
}
