using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.Memory
{
    abstract class Memory : Component, IReadableMemory, IComponent
    {
        public Memory(uint size)
        {
            Size = size;
            Data = new byte[Size];

        }
        public byte Get8(uint address)
        {
            return Data[address];
        }
        public UInt16 Get16LE(uint address)
        {
            return BitConverter.IsLittleEndian ? GetRaw16(address) : GetByteOrderSwapped16(address);
        }
        public UInt32 Get32LE(uint address)
        {
            return BitConverter.IsLittleEndian ? GetRaw32(address) : GetByteOrderSwapped32(address);
        }
        public UInt16 Get16BE(uint address)
        {
            return !BitConverter.IsLittleEndian ? GetRaw16(address) : GetByteOrderSwapped16(address);
        }
        public UInt32 Get32BE(uint address)
        {
            return !BitConverter.IsLittleEndian ? GetRaw32(address) : GetByteOrderSwapped32(address);
        }
        private UInt16 GetRaw16(uint address)
        {
            return (UInt16)(Data[address] | (Data[address + 1] << 8));
        }
        private UInt32 GetRaw32(uint address)
        {
            return (UInt32)(Data[address] | (Data[address + 1] << 8) | (Data[address + 2] << 16) | (Data[address + 3] << 24));
        }
        private UInt16 GetByteOrderSwapped16(uint address)
        {
            return (UInt16)(Data[address + 1] | (Data[address] << 8));
        }
        private UInt32 GetByteOrderSwapped32(uint address)
        {
            return (UInt32)(Data[address + 3] | (Data[address + 2] << 8) | (Data[address + 1] << 16) | (Data[address] << 24));
        }

        public void Get(uint address, byte[] data)
        {
            Get(address, data, 0);
        }
        public void Get(uint address, byte[] data, uint start)
        {
            int length = data.Length - (int)start;
            // Array copy is allegedly best if the length is > 100
            if (length > 100)
                Array.Copy(Data, address, data, start, length);
            else
            {
                // Simple loop copy if length is small
                for (uint i = 0, j = start; i < length; i++, j++)
                    data[j] = Data[address + i];
            }
        }

        public override void PowerOn()
        {
        }

        public override void PowerOff()
        {
        }

        public uint Size { get; }
        protected byte[] Data;
    }
}
