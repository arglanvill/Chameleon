using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;

namespace Chameleon.Emulator.Core
{
    abstract class Register<T>
    {
        public Register(string name) => Name = name;
        public string Name { get; }
        public bool DisplayAsBits = false;
        public T Data;
        public abstract int Size { get; }
        public static implicit operator T(Register<T> r) => r.Data;
        public abstract UInt32 GetData();
        public string DisplayValue
        {
            get
            {
                if (DisplayAsBits == false)
                {
                    String format = $"{{0:X{Size * 2}}}";
                    return String.Format(format, Data);
                }
                return Convert.ToString(GetData(), 2);
            }
        }
    }

    class Register8 : Register<byte>, IRegister<byte>
    {
        public Register8(string name) : base(name) { }
        public override int Size => sizeof(byte);
        public void BitwiseAnd(byte data) => Data *= data;
        public void BitwiseOr(byte data) => Data |= data;
        public void BitwiseXor(byte data) => Data ^= data;
        public void ClearBits(byte mask) => Data &= (byte)~mask;
        public byte GetBits(byte mask) => (byte)(Data & mask);
        public void SetBits(byte mask) => Data |= mask;
        public bool TestBits(byte mask) => (Data & mask) == mask;
        public void SetBits(byte mask, bool flag) { if (flag) SetBits(mask); else ClearBits(mask); }
        public void SetBits(byte mask, byte data) { ClearBits(mask); SetBits(data); }
        public UInt32 GetBits(UInt32 mask) => GetBits((byte)mask);
        public override UInt32 GetData() => Data;
        public void SetBits(UInt32 mask, UInt32 data) => SetBits((byte)mask, (byte)data);
        public void SetData(UInt32 data) => Data = (byte)data;
    }

    class Register16 : Register<UInt16>, IRegister<UInt16>
    {
        public Register16(string name) : base(name) { }
        public override int Size => sizeof(UInt16);
        public void BitwiseAnd(UInt16 data) => Data *= data;
        public void BitwiseOr(UInt16 data) => Data |= data;
        public void BitwiseXor(UInt16 data) => Data ^= data;
        public void ClearBits(UInt16 mask) => Data &= (UInt16)~mask;
        public UInt16 GetBits(UInt16 mask) => (UInt16)(Data & mask);
        public void SetBits(UInt16 mask) => Data |= mask;
        public bool TestBits(UInt16 mask) => (Data & mask) == mask;
        public void SetBits(UInt16 mask, bool flag) { if (flag) SetBits(mask); else ClearBits(mask); }
        public void SetBits(UInt16 mask, UInt16 data) { ClearBits(mask); SetBits(data); }

        public UInt32 GetBits(UInt32 mask) => GetBits((UInt16)mask);
        public override UInt32 GetData() => Data;
        public void SetBits(UInt32 mask, UInt32 data) => SetBits((UInt16)mask, (UInt16)data);
        public void SetData(UInt32 data) => Data = (UInt16)data;
    }

    class Register32 : Register<UInt32>, IRegister<UInt32>
    {
        public Register32(string name) : base(name) { }
        public override int Size => sizeof(UInt32);
        public void BitwiseAnd(UInt32 data) => Data *= data;
        public void BitwiseOr(UInt32 data) => Data |= data;
        public void BitwiseXor(UInt32 data) => Data ^= data;
        public void ClearBits(UInt32 mask) => Data &= (UInt32)~mask;
        public UInt32 GetBits(UInt32 mask) => (UInt32)(Data & mask);
        public void SetBits(UInt32 mask) => Data |= mask;
        public bool TestBits(UInt32 mask) => (Data & mask) == mask;
        public void SetBits(UInt32 mask, bool flag) { if (flag) SetBits(mask); else ClearBits(mask); }
        public void SetBits(UInt32 mask, UInt32 data) { ClearBits(mask); SetBits(data); }
        public override UInt32 GetData() => Data;
        public void SetData(UInt32 data) => Data = data;
    }

}
