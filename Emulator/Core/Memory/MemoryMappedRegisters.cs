using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.Memory
{
    class MemoryMappedRegisters : IReadableMemory, IWritableMemory
    {
        public MemoryMappedRegisters(uint size)
        {
            Size = size;
        }
        struct UnalignedRegisterAccess
        {
            public UnalignedRegisterAccess(IRegister register, int misalignment)
            {
                Register = register;
                Misalignment = misalignment;
            }

            public IRegister Register { get; }
            public int Misalignment { get; }
        }
        public void AddRegister(uint address, IRegister register)
        {
            RegisterMap.Add(address, register);
            RegisterList.Add(address, register);
        }

        public byte Get8(uint address)
        {
            return (byte)GetLE(address, 1);
        }

        public UInt16 Get16LE(uint address)
        {
            return (UInt16)GetLE(address, 2);
        }

        public UInt32 Get32LE(uint address)
        {
            return (UInt32)GetLE(address, 4);
        }

        public UInt16 Get16BE(uint address)
        {
            return (UInt16)GetBE(address, 2);
        }

        public UInt32 Get32BE(uint address)
        {
            return (UInt32)GetBE(address, 4);
        }

        public void Get(uint address, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Get(uint address, byte[] data, uint start)
        {
            throw new NotImplementedException();
        }

        public void Put8(uint address, byte data)
        {
            PutLE(address, 1, data);
        }

        public void Put16LE(uint address, UInt16 data)
        {
            PutLE(address, 2, data);
        }

        public void Put32LE(uint address, UInt32 data)
        {
            PutLE(address, 4, data);
        }

        public void Put16BE(uint address, UInt16 data)
        {
            PutBE(address, 2, data);
        }

        public void Put32BE(uint address, UInt32 data)
        {
            PutBE(address, 4, data);
        }

        public void Put(uint address, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Put(uint address, byte[] data, uint start)
        {
            throw new NotImplementedException();
        }

        private UInt32 GetLE(uint address, uint size)
        {
            if (TryGetAlignedRegister(address, size, out IRegister register))
                return register.GetData();
            else if (TryGetUnalignedRegisters(address, size, out List<UnalignedRegisterAccess> unalignedRegisters))
            {
                UInt32 data = 0;
                foreach (UnalignedRegisterAccess unalignedRegister in unalignedRegisters)
                {
                    UInt32 mask = 0xFFFFFFFF;
                    if (unalignedRegister.Register.Size > size)
                        mask >>= (int)(unalignedRegister.Register.Size - size) << 3;
                    if (unalignedRegister.Misalignment < 0)
                    {
                        mask >>= (unalignedRegister.Misalignment << 3);
                        data |= unalignedRegister.Register.GetBits(mask) << (-unalignedRegister.Misalignment << 3);
                    }
                    else
                    {
                        mask <<= (unalignedRegister.Misalignment << 3);
                        data |= unalignedRegister.Register.GetBits(mask) >> (unalignedRegister.Misalignment << 3);
                    }
                }
                return data;
            }
            throw new NotSupportedException();
        }
        private UInt32 GetBE(uint address, uint size)
        {
            if (TryGetAlignedRegister(address, size, out IRegister register))
                return register.GetData();
            else if (TryGetUnalignedRegisters(address, size, out List<UnalignedRegisterAccess> unalignedRegisters))
            {
                UInt32 data = 0;
                foreach (UnalignedRegisterAccess unalignedRegister in unalignedRegisters)
                {
                    UInt32 mask = 0xFFFFFFFF;
                    if (unalignedRegister.Register.Size > size)
                        mask <<= (int)(unalignedRegister.Register.Size - size) << 3;
                    if (unalignedRegister.Misalignment < 0)
                    {
                        mask <<= (unalignedRegister.Misalignment << 3);
                        data |= unalignedRegister.Register.GetBits(mask) >> (-unalignedRegister.Misalignment << 3);
                    }
                    else
                    {
                        mask >>= (unalignedRegister.Misalignment << 3);
                        data |= unalignedRegister.Register.GetBits(mask) << (unalignedRegister.Misalignment << 3);
                    }
                }
                return data;
            }
            throw new NotSupportedException();
        }
        private void PutLE(uint address, uint size, UInt32 data)
        {
            if (TryGetAlignedRegister(address, size, out IRegister register))
                register.SetData(data);
            else if (TryGetUnalignedRegisters(address, size, out List<UnalignedRegisterAccess> unalignedRegisters))
            {
                foreach (UnalignedRegisterAccess unalignedRegister in unalignedRegisters)
                {
                    UInt32 mask = 0xFFFFFFFF;
                    if (unalignedRegister.Register.Size < size)
                        mask >>= (int)(size - unalignedRegister.Register.Size) << 3;
                    if (unalignedRegister.Misalignment < 0)
                    {
                        int shift = -unalignedRegister.Misalignment << 3;
                        mask <<= shift;
                        unalignedRegister.Register.SetBits(mask >> shift, (data & mask) >> shift);
                    }
                    else
                    {
                        int shift = unalignedRegister.Misalignment << 3;
                        mask >>= shift;
                        unalignedRegister.Register.SetBits(mask << shift, (data & mask) << shift);
                    }
                }
            }
            throw new NotSupportedException();
        }
        private void PutBE(uint address, uint size, UInt32 data)
        {
            if (TryGetAlignedRegister(address, size, out IRegister register))
                register.SetData(data);
            else if (TryGetUnalignedRegisters(address, size, out List<UnalignedRegisterAccess> unalignedRegisters))
            {
                foreach (UnalignedRegisterAccess unalignedRegister in unalignedRegisters)
                {
                    UInt32 mask = 0xFFFFFFFF;
                    if (unalignedRegister.Register.Size < size)
                        mask <<= (int)(size - unalignedRegister.Register.Size) << 3;
                    if (unalignedRegister.Misalignment < 0)
                    {
                        int shift = -unalignedRegister.Misalignment << 3;
                        mask >>= shift;
                        unalignedRegister.Register.SetBits(mask << shift, (data & mask) << shift);
                    }
                    else
                    {
                        int shift = unalignedRegister.Misalignment << 3;
                        mask <<= shift;
                        unalignedRegister.Register.SetBits(mask >> shift, (data & mask) >> shift);
                    }
                }
            }
            throw new NotSupportedException();
        }
        private bool TryGetAlignedRegister(uint address, uint size, out IRegister register)
        {
            return RegisterMap.TryGetValue(address, out register) && register.Size == size;
        }

        private bool TryGetUnalignedRegisters(uint address, uint size, out List<UnalignedRegisterAccess> unalignedRegisters)
        {
            bool found = false;
            unalignedRegisters = null;
            IEnumerator<KeyValuePair<uint, IRegister>> e = RegisterList.GetEnumerator();
            while (e.MoveNext())
            {
                uint registerAddress = e.Current.Key;
                if (registerAddress >= address + size)
                    break;
                if ((registerAddress >= address - size) && (registerAddress < address + size))
                {
                    if (found == false)
                        unalignedRegisters = new List<UnalignedRegisterAccess>();
                    UnalignedRegisterAccess unalignedRegister = new UnalignedRegisterAccess(e.Current.Value, (int)(address - registerAddress));
                    unalignedRegisters.Add(unalignedRegister);
                    found = true;
                }
            }
            return found;
        }
        // For aligned lookup (fastest)
        private readonly Dictionary<uint, IRegister> RegisterMap = new Dictionary<uint, IRegister>();
        // For unaligned lookup up (fast)
        private readonly SortedList<uint, IRegister> RegisterList = new SortedList<uint, IRegister>();

        public uint Size { get; }
    }
}
