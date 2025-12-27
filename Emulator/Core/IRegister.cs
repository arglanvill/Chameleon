using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core
{
    public interface IRegister
    {
        string Name { get; }
        int Size { get; }
        UInt32 GetData();
        void SetData(UInt32 data);
        UInt32 GetBits(UInt32 mask);
        void SetBits(UInt32 mask, UInt32 data);
        string DisplayValue { get; }
    }
    public interface IRegister<T> : IRegister
    {
        bool TestBits(T mask);
        T GetBits(T mask);
        void SetBits(T mask);
        void SetBits(T mask, bool flag);
        void SetBits(T mask, T data);
        void ClearBits(T mask);
        void BitwiseAnd(T data);
        void BitwiseOr(T data);
        void BitwiseXor(T data);
    }
}
