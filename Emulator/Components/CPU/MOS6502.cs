using Chameleon.Emulator.Core;
using Chameleon.Emulator.Core.CPU;
using Chameleon.Emulator.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Display.Core;
using Windows.Media.Protection.PlayReady;

namespace Chameleon.Emulator.Components.CPU
{
    class MOS6502 : Component, ICPU
    {
        public MOS6502()
        {
            InitializeInstructionSet();
            AddRegister(ProgramCounter = new Register16("PC"));
            AddRegister(StackPointer = new Register8("SP"));
            AddRegister(Status = new Register8("NV--DIZC"));
            Status.DisplayAsBits = true;
            AddRegister(A = new Register8("A"));
            AddRegister(X = new Register8("X"));
            AddRegister(Y = new Register8("Y"));
            AddSignalLine(Ready = new SignalLine("READY", this, false));
            AddSignalLine(Reset = new SignalLine("RESET", this, true));
            AddSignalLine(NMI = new SignalLine("NMI", this, true));
            AddSignalLine(IRQ = new SignalLine("IRQ", this, true));
        }

        public override void PowerOn()
        {
            Ready.SetSignalHigh();
            IRQ.SetSignalHigh();
            NMI.SetSignalHigh();
            Reset.SetSignalHigh();

            A.Data = 0;
            X.Data = 0;
            Y.Data = 0;
            Status.Data = 0;
            StackPointer.Data = 0xFD;
            ProgramCounter.Data = MemoryBus.Get16LE(Vectors.Reset);
        }

        public override void PowerOff()
        {
        }

        private void Interrupt(bool software, UInt16 vector)
        {
            Push16(ProgramCounter);
            // Software interrupts require additional bits to be set in the pushed status
            Push8(software ? (byte)(Status | 0x110000) : Status);
            Status.SetBits(StatusFlags.Interrupt);
            ProgramCounter.Data = MemoryBus.Get16LE(vector);
            InstructionCyclesToSkip = 6;
        }

        // Clock
        public void TickHigh()
        {
            if (InstructionCyclesToSkip-- > 0 || Ready.IsSignalLow())
                return;

            // Handle interrupts first
            if (Reset.IsSignalLow())
            {
                Interrupt(false, Vectors.Reset);
                StackPointer.Data = 0xFD;
            }
            else if (NMI.SignalEdgeLow(this))
                Interrupt(false, Vectors.NMI);
            else if (IRQ.IsSignalLow() && !Status.TestBits(StatusFlags.Interrupt))
                Interrupt(false, Vectors.IRQBRK);
            else
            {
                // Execute next instruction
                byte OpCode = PCGet8();
                Instruction = InstructionSet[OpCode];
                if (Instruction != null)
                {
                    InstructionCyclesToSkip = Instruction.Cycles - 1;
                    InstructionCyclesToSkip += Instruction.AddressingMode();
                    InstructionCyclesToSkip += Instruction.Operation();
                }
            }

        }

        public void TickLow()
        {
        }

        public void Pause()
        {
            Ready.SetSignalLow();
        }

        public void Resume()
        {
            Ready.SetSignalHigh();
        }

        // Addressing mode functions, return number of 'extra' clock cycles needed

        protected int ACC()
        {
            return 0;
        }
        protected int ABS()
        {
            EffectiveAddress = PCGet16();
            return 0;
        }
        protected int ABSX()
        {
            UInt16 baseAddress = PCGet16();
            EffectiveAddress = (UInt16)(baseAddress + X);
            return PageBoundaryCrossed(baseAddress, EffectiveAddress) ? 1 : 0;
        }
        protected int ABSY()
        {
            uint baseAddress = PCGet16();
            EffectiveAddress = (UInt16)(baseAddress + Y);
            return PageBoundaryCrossed(baseAddress, EffectiveAddress) ? 1 : 0;
        }
        protected int IMM()
        {
            EffectiveAddress = ProgramCounter.Data++;
            return 0;
        }
        protected int IMP()
        {
            return 0;
        }
        protected int IND()
        {
            EffectiveAddress = MemoryBus.Get16LE(PCGet16());
            return 0;
        }
        protected int XIND()
        {
            // Need to handle zero page wrap
            EffectiveAddress = MemoryBus.Get16LE((UInt16)((PCGet8() + X) & 0xFF));
            return 0;
        }
        protected int INDY()
        {
            UInt16 baseAddress = MemoryBus.Get16LE(PCGet8());
            EffectiveAddress = (UInt16)(baseAddress + Y);
            return PageBoundaryCrossed(baseAddress, EffectiveAddress) ? 1 : 0;
        }
        protected int REL()
        {
            UInt16 baseAddress = ProgramCounter.Data;
            EffectiveAddress = (UInt16)((int)PCGet8() + ProgramCounter.Data);
            return PageBoundaryCrossed(baseAddress, EffectiveAddress) ? 2 : 1;
        }
        protected int ZPG()
        {
            EffectiveAddress = PCGet8();
            return 0;
        }
        protected int ZPGX()
        {
            uint baseAddress = PCGet8();
            EffectiveAddress = (UInt16)(baseAddress + X);
            return PageBoundaryCrossed(baseAddress, EffectiveAddress) ? 1 : 0;
        }
        protected int ZPGY()
        {
            uint baseAddress = PCGet8();
            EffectiveAddress = (UInt16)(baseAddress + Y);
            return PageBoundaryCrossed(baseAddress, EffectiveAddress) ? 1 : 0;
        }

        // Instruction functions, return number of 'extra' clock cycles needed

        // Add Memory to Accumulator with Carry
        // A + M + C -> A, C
        protected int ADC()
        {
            if (Status.TestBits(StatusFlags.Decimal))
                AddWithCarryDecimal(MemoryBus.Get8(EffectiveAddress));
            else
                AddWithCarryBinary(MemoryBus.Get8(EffectiveAddress));
            return 0;
        }

        // AND Memory with Accumulator
        // A AND M -> A
        protected int AND()
        {
            A.BitwiseAnd(MemoryBus.Get8(EffectiveAddress));
            SetNZStatus(A);
            return 0;
        }
        // Shift left one bit (Memory or Accumulator)
        // C <- [76543210] <- 0
        protected int ASL()
        {
            if (Instruction.AddressingMode == ACC)
                ArithmeticShiftLeft(ref A.Data);
            else
            {
                byte data = MemoryBus.Get8(EffectiveAddress);
                ArithmeticShiftLeft(ref data);
                MemoryBus.Put8(EffectiveAddress, data);
            }
            return 0;
        }

        // Branch on Carry clear
        protected int BCC()
        {
            if (!Status.TestBits(StatusFlags.Carry))
            {
                ProgramCounter.Data = EffectiveAddress;
                return 1;
            }
            return 0;
        }
        // Branch on Carry set
        protected int BCS()
        {
            if (Status.TestBits(StatusFlags.Carry))
            {
                ProgramCounter.Data = EffectiveAddress;
                return 1;
            }
            return 0;
        }
        // Branch on result Zero
        protected int BEQ()
        {
            if (Status.TestBits(StatusFlags.Zero))
            {
                ProgramCounter.Data = EffectiveAddress;
                return 1;
            }
            return 0;
        }
        // Test bits in Memory with Accumulator
        // Bits 7,6 of operand are transferred to SR bits 7,6 (N,V)
        // Z is set to result of operand AND Accumulator
        // A AND M -> Z, M7 -> N, M6 -> V
        protected int BIT()
        {
            byte data = MemoryBus.Get8(EffectiveAddress);
            Status.SetBits(StatusFlags.Zero, (A & data) == 0);
            Status.SetBits(StatusFlags.Negative | StatusFlags.Overflow, (byte)(data & (StatusFlags.Negative | StatusFlags.Overflow)));
            return 0;
        }
        // Branch on result Minus
        protected int BMI()
        {
            if (Status.TestBits(StatusFlags.Negative))
            {
                ProgramCounter.Data = EffectiveAddress;
                return 1;
            }
            return 0;
        }
        // Branch on result not Zero
        protected int BNE()
        {
            if (!Status.TestBits(StatusFlags.Zero))
            {
                ProgramCounter.Data = EffectiveAddress;
                return 1;
            }
            return 0;
        }
        // Branch on result Plus
        protected int BPL()
        {
            if (!Status.TestBits(StatusFlags.Negative))
            {
                ProgramCounter.Data = EffectiveAddress;
                return 1;
            }
            return 0;
        }

        // Force break
        // Interrupt, Push PC, SR
        protected int BRK()
        {
            Interrupt(true, Vectors.IRQBRK);
            return 0;
        }
        // Branch on Overflow clear
        protected int BVC()
        {
            if (!Status.TestBits(StatusFlags.Overflow))
            {
                ProgramCounter.Data = EffectiveAddress;
                return 1;
            }
            return 0;
        }
        // Branch on Overflow set
        protected int BVS()
        {
            if (Status.TestBits(StatusFlags.Overflow))
            {
                ProgramCounter.Data = EffectiveAddress;
                return 1;
            }
            return 0;
        }
        // Clear Carry flag
        // 0 -> C
        protected int CLC()
        {
            Status.ClearBits(StatusFlags.Carry);
            return 0;
        }
        // Clear Decimal mode
        // 0 -> D
        protected int CLD()
        {
            Status.ClearBits(StatusFlags.Decimal);
            return 0;
        }
        // Clear Interrupt disable flag
        // 0 -> I
        protected int CLI()
        {
            Status.ClearBits(StatusFlags.Interrupt);
            return 0;
        }
        // Clear Overflow flag
        // 0 -> V
        protected int CLV()
        {
            Status.ClearBits(StatusFlags.Overflow);
            return 0;
        }
        // Compare memory with register A
        protected int CMP()
        {
            CompareMemory(A);
            return 0;
        }
        // Compare memory with register X
        protected int CPX()
        {
            CompareMemory(X);
            return 0;
        }
        // Compare memory with register Y
        protected int CPY()
        {
            CompareMemory(Y);
            return 0;
        }
        // Decrement memory by 1
        // M-1 -> M,Z,N
        protected int DEC()
        {
            byte data = MemoryBus.Get8(EffectiveAddress);
            MemoryBus.Put8(EffectiveAddress, --data);
            SetNZStatus(data);
            return 0;
        }
        // Decrement X by 1
        // X-1 -> X,Z,N
        protected int DEX()
        {
            X.Data--;
            SetNZStatus(X);
            return 0;
        }
        // Decrement Y by 1
        // Y-1 -> Y,Z,N
        protected int DEY()
        {
            Y.Data--;
            SetNZStatus(Y);
            return 0;
        }
        // Exclusive-OR Memory with Accumulator
        // A XOR M -> A,Z,N
        protected int EOR()
        {
            A.BitwiseXor(MemoryBus.Get8(EffectiveAddress));
            SetNZStatus(A);
            return 0;
        }
        // Increment Memory by 1
        // M+1 -> M,Z,N
        protected int INC()
        {
            byte data = MemoryBus.Get8(EffectiveAddress);
            MemoryBus.Put8(EffectiveAddress, ++data);
            SetNZStatus(data);
            return 0;
        }
        // Increment X by 1
        // X+1 -> X,Z,N
        protected int INX()
        {
            X.Data++;
            SetNZStatus(X);
            return 0;
        }
        // Increment Y by 1
        // Y+1 -> Y,Z,N
        protected int INY()
        {
            Y.Data++;
            SetNZStatus(Y);
            return 0;
        }
        // Jump to new instruction
        // EffectiveAddress -> PC
        protected int JMP()
        {
            ProgramCounter.Data = EffectiveAddress;
            return 0;
        }
        // Jump to subroutine saving return address
        // Push PC-1
        // EffectiveAddress -> PC
        protected int JSR()
        {
            Push16((UInt16)(ProgramCounter - 1));
            ProgramCounter.Data = EffectiveAddress;
            return 0;
        }
        // Load Accumulator with Memory
        // M -> A,N,Z
        protected int LDA()
        {
            LoadMemory(A);
            return 0;
        }
        // Load X with Memory
        // M -> X,N,Z
        protected int LDX()
        {
            LoadMemory(X);
            return 0;
        }
        // Load X with Memory
        // M -> Y,N,Z
        protected int LDY()
        {
            LoadMemory(Y);
            return 0;
        }
        // Logical shift right 1 bit (Memory or Accumulator)
        // 0 -> [76543210] -> C
        protected int LSR()
        {
            if (Instruction.AddressingMode == ACC)
                LogicalShiftRight(ref A.Data);
            else
            {
                byte data = MemoryBus.Get8(EffectiveAddress);
                LogicalShiftRight(ref data);
                MemoryBus.Put8(EffectiveAddress, data);
            }
            return 0;
        }
        protected int NOP()
        {
            return 0;
        }
        // OR Memory with Accumulator
        // A OR M -> A
        protected int ORA()
        {
            A.BitwiseOr(MemoryBus.Get8(EffectiveAddress));
            SetNZStatus(A);
            return 0;
        }
        // Push Accumulator on Stack
        protected int PHA()
        {
            Push8(A);
            return 0;
        }
        // Push Status on Stack
        protected int PHP()
        {
            Push8(Status);
            return 0;
        }
        // Pull Accumulator from Stack
        protected int PLA()
        {
            A.Data = Pull8();
            SetNZStatus(A);
            return 0;
        }
        // Pull Status from Stack
        protected int PLP()
        {
            Status.Data = Pull8();
            return 0;
        }
        // Rotate left 1 bit (Memory or Accumulator)
        // C <- [76543210] <- C
        protected int ROL()
        {
            if (Instruction.AddressingMode == ACC)
                RotateLeft(ref A.Data);
            else
            {
                byte data = MemoryBus.Get8(EffectiveAddress);
                RotateLeft(ref data);
                MemoryBus.Put8(EffectiveAddress, data);
            }
            return 0;
        }
        // Rotate right 1 bit (Memory or Accumulator)
        // C -> [76543210] -> C
        protected int ROR()
        {
            if (Instruction.AddressingMode == ACC)
                RotateRight(ref A.Data);
            else
            {
                byte data = MemoryBus.Get8(EffectiveAddress);
                RotateRight(ref data);
                MemoryBus.Put8(EffectiveAddress, data);
            }
            return 0;
        }
        // Return from Interrupt
        // Pull Status
        // Pull PC
        protected int RTI()
        {
            Status.Data = Pull8();
            ProgramCounter.Data = Pull16();
            return 0;
        }
        // Return from Subroutine
        // Pull PC-1
        protected int RTS()
        {
            ProgramCounter.Data = (UInt16)(Pull16() + 1);
            return 0;
        }
        // Subtract Memory from Accumulator with Borrow
        // A - M - C -> A,C,V
        protected int SBC()
        {
            // Subtraction is addition with inverted operand
            if (Status.TestBits(StatusFlags.Decimal))
                AddWithCarryDecimal(Decimal9sComplement(MemoryBus.Get8(EffectiveAddress)));
            else
                AddWithCarryBinary((byte)~MemoryBus.Get8(EffectiveAddress));
            return 0;
        }
        // Set Carry flag
        // 1 -> C
        protected int SEC()
        {
            Status.SetBits(StatusFlags.Carry);
            return 0;
        }
        // Set Decimal mode
        // 1 -> D
        protected int SED()
        {
            Status.SetBits(StatusFlags.Decimal);
            return 0;
        }
        // Set Interrupt disable flag
        // 1 -> I
        protected int SEI()
        {
            Status.SetBits(StatusFlags.Interrupt);
            return 0;
        }
        // Store Accumulator in Memory
        // A -> M
        protected int STA()
        {
            MemoryBus.Put8(EffectiveAddress, A);
            return 0;
        }
        // Store X in Memory
        // X -> M
        protected int STX()
        {
            MemoryBus.Put8(EffectiveAddress, X);
            return 0;
        }
        // Store Y in Memory
        // Y -> M
        protected int STY()
        {
            MemoryBus.Put8(EffectiveAddress, Y);
            return 0;
        }
        // Transfer Accumulator to X
        // A -> X,N,Z
        protected int TAX()
        {
            X.Data = A;
            SetNZStatus(X);
            return 0;
        }
        // Transfer Accumulator to Y
        // A -> Y,N,Z
        protected int TAY()
        {
            Y.Data = A;
            SetNZStatus(Y);
            return 0;
        }
        // Transfer SP to X
        // SP -> X,N,Z
        protected int TSX()
        {
            X.Data = StackPointer;
            SetNZStatus(X);
            return 0;
        }
        // Transfer X to Accumulator
        // X -> A,N,Z
        protected int TXA()
        {
            A.Data = X;
            SetNZStatus(A);
            return 0;
        }
        // Transfer X to SP
        // X -> SP
        protected int TXS()
        {
            StackPointer.Data = X;
            return 0;
        }
        // Transfer Y to Accumulator
        // Y -> A,N,Z
        protected int TYA()
        {
            A.Data = Y;
            SetNZStatus(A);
            return 0;
        }

        // Utility functions
        private bool PageBoundaryCrossed(uint address1, uint address2)
        {
            return (address1 & 0xFF00) != (address2 & 0xFF00);
        }

        private byte PCGet8()
        {
            return MemoryBus.Get8(ProgramCounter.Data++);
        }
        private UInt16 PCGet16()
        {
            UInt16 data = MemoryBus.Get16LE(ProgramCounter.Data);
            ProgramCounter.Data += 2;
            return data;
        }

        public void Push8(byte data)
        {
            MemoryBus.Put8(StackAddress + StackPointer, data);
        }
        public void Push16(UInt16 data)
        {
            MemoryBus.Put16LE(StackAddress + StackPointer, data);
        }

        public byte Pull8()
        {
            return 0;
        }
        public UInt16 Pull16()
        {
            return 0;
        }

        // Add data to Accumulator with Carry
        // A + data + C -> A,C,V
        private void AddWithCarryBinary(byte data)
        {
            int result = A + data + Status.GetBits(StatusFlags.Carry);
            Status.SetBits(StatusFlags.Carry, (result & 0x100) != 0);
            Status.SetBits(StatusFlags.Overflow, result < -128 || result > 127);
            A.Data = (byte)result;
            SetNZStatus(A);
        }
        // Add data to Accumulator with Carry
        // A + data + C -> A,C,V
        // Note, signed BCD numbers are not supported by the 6502, but the N flag is still set to bit 7 of the result
        private void AddWithCarryDecimal(byte data)
        {
            // Add low digits and carry
            int lo = (A & 0x0F) + (data & 0x0F) + Status.GetBits(StatusFlags.Carry);
            // Add high digits and bit 5 from low result
            int hi = (A & 0xF0) + (data & 0xF0) + (lo & 0x10);
            // New carry is the 9th bit from high result
            Status.SetBits(StatusFlags.Carry, (hi & 0x100) != 0);
            A.Data = (byte)((byte)hi | (byte)lo);
            SetNZStatus(A);
        }
        // Return the 9s complement of a decimal mode number
        // Subtract each digit from 9
        private byte Decimal9sComplement(byte data)
        {
            return (byte)((0x90 - (data & 0xF0)) | (0x09 - (data & 0x0F)));
        }
        // C <- [76543210] <- 0
        private void ArithmeticShiftLeft(ref byte data)
        {
            data <<= 1;
            Status.SetBits(StatusFlags.Carry, (data & 0x100) != 0);
            SetNZStatus(A);
        }
        // 0 -> [76543210] -> C
        private void LogicalShiftRight(ref byte data)
        {
            Status.SetBits(StatusFlags.Carry, (byte)(data & StatusFlags.Carry));
            data >>= 1;
        }
        // C <- [76543210] <- C
        private void RotateLeft(ref byte data)
        {
            data <<= 1;
            if (Status.TestBits(StatusFlags.Carry))
                data |= 0x01;
            Status.SetBits(StatusFlags.Carry, (data & 0x100) != 0);
            SetNZStatus(A);
        }
        // C -> [76543210] -> C
        private void RotateRight(ref byte data)
        {
            bool carry = (data & StatusFlags.Carry) != 0;
            Status.SetBits(StatusFlags.Carry, (byte)(data & StatusFlags.Carry));
            data >>= 1;
            if (carry)
                data |= 0x80;
            SetNZStatus(A);
        }
        // Compare memory with register R
        // Z,C,N = R-M
        // (R >= M) -> C
        private void CompareMemory(Register8 r)
        {
            int result = r - MemoryBus.Get8(EffectiveAddress);
            Status.SetBits(StatusFlags.Carry, result >= 0);
            SetNZStatus((byte)result);
        }
        // Load Register R with Memory
        // M -> R,N,Z
        private void LoadMemory(Register8 r)
        {
            r.Data = MemoryBus.Get8(EffectiveAddress);
            SetNZStatus(r);
        }
        private void SetNZStatus(byte data)
        {
            Status.SetBits(StatusFlags.Zero, data == 0);
            Status.SetBits(StatusFlags.Negative, (byte)(data & StatusFlags.Negative));
        }

        public MemoryBus MemoryBus { get; set; }

        protected struct StatusFlags
        {
            public const byte Negative = 128;
            public const byte Overflow = 64;
            public const byte Unused1 = 32;
            public const byte Unused2 = 16;
            public const byte Decimal = 8;
            public const byte Interrupt = 4;
            public const byte Zero = 2;
            public const byte Carry = 1;
        }

        protected struct Vectors
        {
            public const UInt16 NMI = 0xFFFA;
            public const UInt16 Reset = 0xFFFC;
            public const UInt16 IRQBRK = 0xFFFE;
        }

        protected enum AddressingModes
        {
            Accumulator,
            Absolute,
            AbsoluteXIndexed,
            AbsoluteYIndexed,
            Immediate,
            Implied,
            Indirect,
            XIndexedIndirect,
            IndirectYIndexed,
            Relative,
            ZeroPage,
            ZeroPageXIndexed,
            ZeroPageYIndexed
        }

        const uint StackAddress = 0x0100;

        public Register16 ProgramCounter;
        public Register8 StackPointer;
        public Register8 Status;
        public Register8 A;
        public Register8 X;
        public Register8 Y;

        public SignalLine Ready;
        public SignalLine Reset;
        public SignalLine IRQ;
        public SignalLine NMI;

        private int InstructionCyclesToSkip;
        private Instruction Instruction;
        private UInt16 EffectiveAddress;

        public List<Instruction> InstructionSet;

        private void InitializeInstructionSet()
        {
            InstructionSet = new List<Instruction> {
                (0x00, "BRK", BRK, "IMP", IMP, 7), (0x01, "ORA", ORA, "XIND", XIND, 6), null, null,
                null, (0x05, "ORA", ORA, "ZPG", ZPG, 3), (0x06, "ASL", ASL, "ZPG", ZPG, 5), null,
                (0x08, "PHP", PHP, "IMP", IMP, 3), (0x09, "ORA", ORA, "IMM", IMM, 2), (0x0A, "ASL", ASL, "ACC", ACC, 2), null,
                null, (0x0D, "ORA", ORA, "ABS", ABS, 4), (0x0E, "ASL", ASL, "ABS", ABS, 6), null,
                (0x10, "BPL", BPL, "REL", REL, 2), (0x11, "ORA", ORA, "INDY", INDY, 5), null, null,
                null, (0x15, "ORA", ORA, "ZPGX", ZPGX, 4), (0x16, "ASL", ASL, "ZPGX", ZPGX, 6), null,
                (0x18, "CLC", CLC, "IMP", IMP, 2), (0x19, "ORA", ORA, "ABSY", ABSY, 4), null, null,
                null, (0x1D, "ORA", ORA, "ABSX", ABSX, 4), (0x1E, "ASL", ASL, "ABSX", ABSX, 7), null,
                (0x20, "JSR", JSR, "ABS", ABS, 6), (0x21, "AND", AND, "XIND", XIND, 6), null, null,
                (0x24, "BIT", BIT, "ZPG", ZPG, 3), (0x25, "AND", AND, "ZPG", ZPG, 3), (0x26, "ROL", ROL, "ZPG", ZPG, 5), null,
                (0x28, "PLP", PLP, "IMP", IMP, 4), (0x29, "AND", AND, "IMM", IMM, 2), (0x2A, "ROL", ROL, "ACC", ACC, 2), null,
                (0x2C, "BIT", BIT, "ABS", ABS, 4), (0x2D, "AND", AND, "ABS", ABS, 4), (0x2E, "ROL", ROL, "ABS", ABS, 6), null,
                (0x30, "BMI", BMI, "REL", REL, 2), (0x31, "AND", AND, "INDY", INDY, 5), null, null,
                null, (0x35, "AND", AND, "ZPGX", ZPGX, 4), (0x36, "ROL", ROL, "ZPGX", ZPGX, 6), null,
                (0x38, "SEC", SEC, "IMP", IMP, 2), (0x39, "AND", AND, "ABSY", ABSY, 4), null, null,
                null, (0x3D, "AND", AND, "ABSX", ABSX, 4), (0x3E, "ROL", ROL, "ABSX", ABSX, 7), null,
                (0x40, "RTI", RTI, "IMP", IMP, 6), (0x41, "EOR", EOR, "XIND", XIND, 6), null, null,
                null, (0x45, "EOR", EOR, "ZPG", ZPG, 3), (0x46, "LSR", LSR, "ZPG", ZPG, 5), null,
                (0x48, "PHA", PHA, "IMP", IMP, 3), (0x49, "EOR", EOR, "IMM", IMM, 2), (0x4A, "LSR", LSR, "ACC", ACC, 2), null,
                (0x4C, "JMP", JMP, "ABS", ABS, 3), (0x4D, "EOR", EOR, "ABS", ABS, 4), (0x4E, "LSR", LSR, "ABS", ABS, 6), null,
                (0x50, "BVC", BVC, "REL", REL, 2), (0x51, "EOR", EOR, "INDY", INDY, 5), null, null,
                null, (0x55, "EOR", EOR, "ZPGX", ZPGX, 4), (0x56, "LSR", LSR, "ZPGX", ZPGX, 6), null,
                (0x58, "CLI", CLI, "IMP", IMP, 2), (0x59, "EOR", EOR, "ABSY", ABSY, 4), null, null,
                null, (0x5D, "EOR", EOR, "ABSX", ABSX, 4), (0x5E, "LSR", LSR, "ABSX", ABSX, 7), null,
                (0x60, "RTS", RTS, "IMP", IMP, 6), (0x61, "ADC", ADC, "XIND", XIND, 6), null, null,
                null, (0x65, "ADC", ADC, "ZPG", ZPG, 3), (0x66, "ROR", ROR, "ZPG", ZPG, 5), null,
                (0x68, "PLA", PLA, "IMP", IMP, 4), (0x69, "ADC", ADC, "IMM", IMM, 2), (0x6A, "ROR", ROR, "ACC", ACC, 2), null,
                (0x6C, "JMP", JMP, "IND", IND, 5), (0x6D, "ADC", ADC, "ABS", ABS, 4), (0x6E, "ROR", ROR, "ABS", ABS, 6), null,
                (0x70, "BVS", BVS, "REL", REL, 2), (0x71, "ADC", ADC, "INDY", INDY, 5), null, null,
                null, (0x75, "ADC", ADC, "ZPGX", ZPGX, 4), (0x76, "ROR", ROR, "ZPGX", ZPGX, 6), null,
                (0x78, "SEI", SEI, "IMP", IMP, 2), (0x79, "ADC", ADC, "ABSY", ABSY, 4), null, null,
                null, (0x7D, "ADC", ADC, "ABSX", ABSX, 4), (0x7E, "ROR", ROR, "ABSX", ABSX, 7), null,
                null, (0x81, "STA", STA, "XIND", XIND, 6), null, null,
                (0x84, "STY", STY, "ZPG", ZPG, 3), (0x85, "STA", STA, "ZPG", ZPG, 3), (0x86, "STX", STX, "ZPG", ZPG, 3), null,
                (0x88, "DEY", DEY, "IMP", IMP, 2), null, (0x8A, "TXA", TXA, "IMP", IMP, 2), null,
                (0x8C, "STY", STY, "ABS", ABS, 4), (0x8D, "STA", STA, "ABS", ABS, 4), (0x8E, "STX", STX, "ABS", ABS, 4), null,
                (0x90, "BCC", BCC, "REL", REL, 2), (0x91, "STA", STA, "INDY", INDY, 6), null, null,
                (0x94, "STY", STY, "ZPGX", ZPGX, 4), (0x95, "STA", STA, "ZPGX", ZPGX, 4), (0x96, "STX", STX, "ZPGY", ZPGY, 4), null,
                (0x98, "TYA", TYA, "IMP", IMP, 2), (0x99, "STA", STA, "ABSY", ABSY, 5), (0x9A, "TXS", TXS, "IMP", IMP, 2), null,
                null, (0x9D, "STA", STA, "ABSX", ABSX, 5), null, null,
                (0xA0, "LDY", LDY, "IMM", IMM, 2), (0xA1, "LDA", LDA, "XIND", XIND, 6), (0xA2, "LDX", LDX, "IMM", IMM, 2), null,
                (0xA4, "LDY", LDY, "ZPG", ZPG, 3), (0xA5, "LDA", LDA, "ZPG", ZPG, 3), (0xA6, "LDX", LDX, "ZPG", ZPG, 3), null,
                (0xA8, "TAY", TAY, "IMP", IMP, 2), (0xA9, "LDA", LDA, "IMM", IMM, 2), (0xAA, "TAX", TAX, "IMP", IMP, 2), null,
                (0xAC, "LDY", LDY, "ABS", ABS, 4), (0xAD, "LDA", LDA, "ABS", ABS, 4), (0xAE, "LDX", LDX, "ABS", ABS, 4), null,
                (0xB0, "BCS", BCS, "REL", REL, 2), (0xB1, "LDA", LDA, "INDY", INDY, 5), null, null,
                (0xB4, "LDY", LDY, "ZPGX", ZPGX, 4), (0xB5, "LDA", LDA, "ZPGX", ZPGX, 4), (0xB6, "LDX", LDX, "ZPGY", ZPGY, 4), null,
                (0xB8, "CLV", CLV, "IMP", IMP, 2), (0xB9, "LDA", LDA, "ABSY", ABSY, 4), (0xBA, "TSX", TSX, "IMP", IMP, 2), null,
                (0xBC, "LDY", LDY, "ABSX", ABSX, 4), (0xBD, "LDA", LDA, "ABSX", ABSX, 4), (0xBE, "LDX", LDX, "ABSY", ABSY, 4), null,
                (0xC0, "CPY", CPY, "IMM", IMM, 2), (0xC1, "CMP", CMP, "XIND", XIND, 6), null, null,
                (0xC4, "CPY", CPY, "ZPG", ZPG, 3), (0xC5, "CMP", CMP, "ZPG", ZPG, 3), (0xC6, "DEC", DEC, "ZPG", ZPG, 5), null,
                (0xC8, "INY", INY, "IMP", IMP, 2), (0xC9, "CMP", CMP, "IMM", IMM, 2), (0xCA, "DEX", DEX, "IMP", IMP, 2), null,
                (0xCC, "CPY", CPY, "ABS", ABS, 4), (0xCD, "CMP", CMP, "ABS", ABS, 4), (0xCE, "DEC", DEC, "ABS", ABS, 6), null,
                (0xD0, "BNE", BNE, "REL", REL, 2), (0xD1, "CMP", CMP, "INDY", INDY, 5), null, null,
                null, (0xD5, "CMP", CMP, "ZPGX", ZPGX, 4), (0xD6, "DEC", DEC, "ZPGX", ZPGX, 6), null,
                (0xD8, "CLD", CLD, "IMP", IMP, 2), (0xD9, "CMP", CMP, "ABSY", ABSY, 4), null, null,
                null, (0xDD, "CMP", CMP, "ABSX", ABSX, 4), (0xDE, "DEC", DEC, "ABSX", ABSX, 7), null,
                (0xE0, "CPX", CPX, "IMM", IMM, 2), (0xE1, "SBC", SBC, "XIND", XIND, 6), null, null,
                (0xE4, "CPX", CPX, "ZPG", ZPG, 3), (0xE5, "SBC", SBC, "ZPG", ZPG, 3), (0xE6, "INC", INC, "ZPG", ZPG, 5), null,
                (0xE8, "INX", INX, "IMP", IMP, 2), (0xE9, "SBC", SBC, "IMM", IMM, 2), (0xEA, "NOP", NOP, "IMP", IMP, 2), null,
                (0xEC, "CPX", CPX, "ABS", ABS, 4), (0xED, "SBC", SBC, "ABS", ABS, 4), (0xEE, "INC", INC, "ABS", ABS, 6), null,
                (0xF0, "BEQ", BEQ, "REL", REL, 2), (0xF1, "SBC", SBC, "INDY", INDY, 5), null, null,
                null, (0xF5, "SBC", SBC, "ZPGX", ZPGX, 4), (0xF6, "INC", INC, "ZPGX", ZPGX, 6), null,
                (0xF8, "SED", SED, "IMP", IMP, 2), (0xF9, "SBC", SBC, "ABSY", ABSY, 4), null, null,
                null, (0xFD, "SBC", SBC, "ABSX", ABSX, 4), (0xFE, "INC", INC, "ABSX", ABSX, 7), null
            };
        }

        public string Disassemble()
        {
            UInt32 address = DisassemblyAddress;
            byte opcode = MemoryBus.Get8(DisassemblyAddress++);
            Instruction instruction = InstructionSet[opcode];
            if (instruction == null)
                return "???";
            string format = "{0} {1}";
            UInt16 operand = 0;
            switch (instruction.AddressingModeMnemonic)
            {
                case "IMP":
                    break;
                case "ACC":
                    format += " A";
                    break;
                case "IMM":
                    format += " #{2,2:X}";
                    operand = MemoryBus.Get8(DisassemblyAddress++);
                    break;
                case "ABS":
                    format += " ${2,4:X}";
                    operand = MemoryBus.Get16LE(DisassemblyAddress += 2);
                    break;
                case "ABSX":
                    format += " ${2,4:X}, X";
                    operand = MemoryBus.Get16LE(DisassemblyAddress += 2);
                    break;
                case "ABSY":
                    format += " ${2,4:X}, Y";
                    operand = MemoryBus.Get16LE(DisassemblyAddress += 2);
                    break;
                case "ZPG":
                    format += " ${2,2:X}";
                    operand = MemoryBus.Get8(DisassemblyAddress++);
                    break;
                case "ZPGX":
                    format += " ${2,2:X}, X";
                    operand = MemoryBus.Get8(DisassemblyAddress++);
                    break;
                case "ZPGY":
                    format += " ${2,2:X}, Y";
                    operand = MemoryBus.Get8(DisassemblyAddress++);
                    break;
                case "XIND":
                    format += " ({$2,2,X}, X)";
                    operand = MemoryBus.Get8(DisassemblyAddress++);
                    break;
                case "INDY":
                    format += " ({$2,2,X}), Y";
                    operand = MemoryBus.Get8(DisassemblyAddress++);
                    break;
                case "IND":
                    format += " ({$2,4,X})";
                    operand = MemoryBus.Get16LE(DisassemblyAddress += 2);
                    break;
                case "REL":
                    format += " #{$2,2,X} {$3,4,X}";
                    operand = MemoryBus.Get8(DisassemblyAddress++);
                    break;
            }
            return string.Format(format, address, instruction.Mnemonic, operand, DisassemblyAddress + (Int16)operand);
        }
        public UInt32 DisassemblyAddress { get; set; }

        public override string Name => "MOS 6502";
    }
}
