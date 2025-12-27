using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.Memory
{
    class RAM : Memory, IWritableMemory
    {
        public override string Name => "RAM";

        public RAM(uint size) : base(size)
        {

        }
        public void Put8(uint address, byte data)
        {
            Data[address] = data;
        }
        public void Put16LE(uint address, UInt16 data)
        {
            if (BitConverter.IsLittleEndian)
                PutRaw16(address, data);
            else
                PutByteOrderSwapped16(address, data);
        }
        public void Put32LE(uint address, UInt32 data)
        {
            if (BitConverter.IsLittleEndian)
                PutRaw32(address, data);
            else
                PutByteOrderSwapped32(address, data);
        }
        public void Put16BE(uint address, UInt16 data)
        {
            if (!BitConverter.IsLittleEndian)
                PutRaw16(address, data);
            else
                PutByteOrderSwapped16(address, data);
        }
        public void Put32BE(uint address, UInt32 data)
        {
            if (!BitConverter.IsLittleEndian)
                PutRaw32(address, data);
            else
                PutByteOrderSwapped32(address, data);
        }
        private void PutRaw16(uint address, UInt16 data)
        {
            Data[address] = (byte)(data & 0xFF);
            Data[address + 1] = (byte)((data >> 8) & 0xFF);
        }
        private void PutRaw32(uint address, UInt32 data)
        {
            Data[address] = (byte)(data & 0xFF);
            Data[address + 1] = (byte)((data >> 8) & 0xFF);
            Data[address + 2] = (byte)((data >> 16) & 0xFF);
            Data[address + 3] = (byte)((data >> 24) & 0xFF);
        }
        private void PutByteOrderSwapped16(uint address, UInt16 data)
        {
            Data[address + 1] = (byte)(data & 0xFF);
            Data[address] = (byte)((data >> 8) & 0xFF);
        }
        private void PutByteOrderSwapped32(uint address, UInt32 data)
        {
            Data[address + 3] = (byte)(data & 0xFF);
            Data[address + 2] = (byte)((data >> 8) & 0xFF);
            Data[address + 1] = (byte)((data >> 16) & 0xFF);
            Data[address] = (byte)((data >> 24) & 0xFF);
        }
        public void Put(uint address, byte[] data)
        {
            Put(address, data, 0);
        }
        public void Put(uint address, byte[] data, uint start)
        {
            int length = data.Length;
            // Array copy is allegedly best if the length is > 100
            if (length > 100)
                Array.Copy(data, start, Data, address, length);
            else
            {
                // Simple loop copy if length is small
                for (uint i = 0, j = start; i < length; i++, j++)
                    Data[address + i] = data[j];
            }
        }

    }
}
