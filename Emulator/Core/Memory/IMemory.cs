using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.Memory
{
    interface IMemory
    {
        uint Size { get; }
    }
    interface IReadableMemory : IMemory
    {
        byte Get8(uint address);
        UInt16 Get16LE(uint address);
        UInt32 Get32LE(uint address);
        UInt16 Get16BE(uint address);
        UInt32 Get32BE(uint address);
        void Get(uint address, byte[] data);
        void Get(uint address, byte[] data, uint start);
    }

    interface IWritableMemory : IMemory
    {
        void Put8(uint address, byte data);
        void Put16LE(uint address, UInt16 data);
        void Put32LE(uint address, UInt32 data);
        void Put16BE(uint address, UInt16 data);
        void Put32BE(uint address, UInt32 data);
        void Put(uint address, byte[] data);
        void Put(uint address, byte[] data, uint start);
    }
}
