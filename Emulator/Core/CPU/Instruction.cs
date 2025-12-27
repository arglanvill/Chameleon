using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chameleon.Emulator.Core.CPU
{
    public class Instruction
    {
        public delegate int AddressingModeDelegate();
        public delegate int OperationDelegate();

        public Instruction(int opcode, string mnemonic, OperationDelegate operation, string addressingModeMnemonic, AddressingModeDelegate addressingMode, int cycles)
        {
            Opcode = opcode;
            Mnemonic = mnemonic;
            AddressingModeMnemonic = addressingModeMnemonic;
            AddressingMode = addressingMode;
            Operation = operation;
            Cycles = cycles;
        }

        public int Opcode;
        public string Mnemonic;
        public OperationDelegate Operation;
        public string AddressingModeMnemonic;
        public AddressingModeDelegate AddressingMode;
        public int Cycles;

        public static implicit operator Instruction((int opcode, string mnemonic, OperationDelegate operation, string addressingModeMnemonic, AddressingModeDelegate addressingMode, int cycles) v)
        {
            return new Instruction(v.opcode, v.mnemonic, v.operation, v.addressingModeMnemonic,v.addressingMode, v.cycles);
        }
    }
}
