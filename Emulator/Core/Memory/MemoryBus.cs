using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.Memory
{
    class MemoryBus : IReadableMemory, IWritableMemory
    {
        public MemoryBus(uint size)
        {
            Size = size;
        }
        public void EnableReadableMemory(IReadableMemory memory, uint baseAddress)
        {
            EnableReadableMemory(memory, baseAddress, memory.Size);
        }
        public void EnableReadableMemory(IReadableMemory memory, uint baseAddress, uint size)
        {
            ReadMap.InsertRegion(baseAddress, baseAddress + size - 1, memory);
        }
        public void DisableReadableMemory(IReadableMemory memory)
        {
            ReadMap.RemoveRegion(memory);
        }
        public void EnableWritableMemory(IWritableMemory memory, uint baseAddress)
        {
            EnableWritableMemory(memory, baseAddress, memory.Size);
        }
        public void EnableWritableMemory(IWritableMemory memory, uint baseAddress, uint size)
        {
            WriteMap.InsertRegion(baseAddress, baseAddress + size - 1, memory);
        }
        public void DisableWritableMemory(IWritableMemory memory)
        {
            WriteMap.RemoveRegion(memory);
        }
        public byte Get8(uint address)
        {
            MemoryMapRegion<IReadableMemory> region = ReadMap.GetMemoryMapRegion(address);
            return region.Memory.Get8(address - region.Start);
        }

        public UInt16 Get16LE(uint address)
        {
            MemoryMapRegion<IReadableMemory> region = ReadMap.GetMemoryMapRegion(address);
            return region.Memory.Get16LE(address - region.Start);
        }

        public UInt32 Get32LE(uint address)
        {
            MemoryMapRegion<IReadableMemory> region = ReadMap.GetMemoryMapRegion(address);
            return region.Memory.Get32LE(address - region.Start);
        }

        public UInt16 Get16BE(uint address)
        {
            MemoryMapRegion<IReadableMemory> region = ReadMap.GetMemoryMapRegion(address);
            return region.Memory.Get16BE(address - region.Start);
        }

        public UInt32 Get32BE(uint address)
        {
            MemoryMapRegion<IReadableMemory> region = ReadMap.GetMemoryMapRegion(address);
            return region.Memory.Get32BE(address - region.Start);
        }

        public void Get(uint address, byte[] data)
        {
            Get(address, data, 0);
        }

        public void Get(uint address, byte[] data, uint start)
        {
            uint dataRemaining = (uint)data.Length - start;
            uint dataIndex = start;
            while (dataRemaining > 0)
            {
                MemoryMapRegion<IReadableMemory> region = ReadMap.GetMemoryMapRegion(address);
                uint regionRemaining = region.End - address + 1;
                uint toCopy = (dataRemaining > regionRemaining) ? regionRemaining : dataRemaining;
                region.Memory.Get(address - region.Start, data, dataIndex);
                dataRemaining -= toCopy;
                dataIndex += toCopy;
                address += toCopy;
            }
        }

        public void Put8(uint address, byte data)
        {
            MemoryMapRegion<IWritableMemory> region = WriteMap.GetMemoryMapRegion(address);
            region.Memory.Put8(address - region.Start, data);
        }

        public void Put16LE(uint address, UInt16 data)
        {
            MemoryMapRegion<IWritableMemory> region = WriteMap.GetMemoryMapRegion(address);
            region.Memory.Put16LE(address - region.Start, data);
        }

        public void Put32LE(uint address, UInt32 data)
        {
            MemoryMapRegion<IWritableMemory> region = WriteMap.GetMemoryMapRegion(address);
            region.Memory.Put32LE(address - region.Start, data);
        }

        public void Put16BE(uint address, UInt16 data)
        {
            MemoryMapRegion<IWritableMemory> region = WriteMap.GetMemoryMapRegion(address);
            region.Memory.Put16BE(address - region.Start, data);
        }

        public void Put32BE(uint address, UInt32 data)
        {
            MemoryMapRegion<IWritableMemory> region = WriteMap.GetMemoryMapRegion(address);
            region.Memory.Put32BE(address - region.Start, data);
        }

        public void Put(uint address, byte[] data)
        {
            Put(address, data, 0);
        }
        public void Put(uint address, byte[] data, uint start)
        {
            uint dataRemaining = (uint)data.Length - start;
            uint dataIndex = start;
            while (dataRemaining > 0)
            {
                MemoryMapRegion<IWritableMemory> region = WriteMap.GetMemoryMapRegion(address);
                uint regionRemaining = region.End - address + 1;
                uint toCopy = (dataRemaining > regionRemaining) ? regionRemaining : dataRemaining;
                region.Memory.Put(address - region.Start, data, dataIndex);
                dataRemaining -= toCopy;
                dataIndex += toCopy;
                address += toCopy;
            }
        }
        private readonly MemoryMap<IReadableMemory> ReadMap = new MemoryMap<IReadableMemory>();
        private readonly MemoryMap<IWritableMemory> WriteMap = new MemoryMap<IWritableMemory>();

        public uint Size { get; }
    }
}
