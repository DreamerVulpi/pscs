using System;
using PsCs.Disasm;

namespace PsCs.Hardware
{
    public class InvalidCodeException : Exception
    {
        public InvalidCodeException(string what) : base(what)
        {
        }
    }

    internal class Registers
    {
        private uint[] input;
        private uint[] output;

        public Registers()
        {
            input = new uint[32];
            output = new uint[32];
        }

        public uint this[int index]
        {
            get => input[index];
            set => output[index] = value;
        }

        public void Cycle()
        {
            Array.Copy(output, input, 32);
        }
    }

    public class Cpu
    {
        private Registers GPR;
        private uint[] COP0R;

        private int LoadDelaySlot;
        private uint LoadDelayValue;

        private uint Pc, PcNext;
        private uint Lo, Hi;

        public Cpu()
        {
            GPR = new Registers();
            COP0R = new uint[32];
            Pc = 0xBFC00000;
            PcNext = 0xBFC00004;
        }

        void SLL(Instruction instruction)
        {
            GPR[instruction.Rd] = GPR[instruction.Rt] << instruction.ShiftAmount;
        }

        void SRL(Instruction instruction)
        {
            GPR[instruction.Rd] = GPR[instruction.Rt] >> instruction.ShiftAmount;
        }

        void SRA(Instruction instruction)
        {
            GPR[instruction.Rd] = (uint) ((int) GPR[instruction.Rt] >> instruction.ShiftAmount);
        }

        void SLLV(Instruction instruction)
        {
            var s = GPR[instruction.Rs] & 0x1F;
            GPR[instruction.Rd] = GPR[instruction.Rt] << (int) s;
        }

        void SRLV(Instruction instruction)
        {
            var s = GPR[instruction.Rs] & 0x1F;
            GPR[instruction.Rd] = GPR[instruction.Rt] >> (int) s;
        }

        void SRAV(Instruction instruction)
        {
            var s = GPR[instruction.Rs] & 0x1F;
            GPR[instruction.Rd] = (uint) (int) GPR[instruction.Rt] >> (int) s;
        }

        void JR(Instruction instruction)
        {
            PcNext = GPR[instruction.Rs];
        }

        void JALR(Instruction instruction)
        {
            GPR[instruction.Rd] = PcNext + 4;
            PcNext = GPR[instruction.Rs];
        }

        void SYSCALL(Instruction instruction, Bus bus)
        {
            //TODO SystemCallException
        }

        void BREAK(Instruction instruction, Bus bus)
        {
            //TODO BreakpointException
        }

        void MFHI(Instruction instruction)
        {
            GPR[instruction.Rd] = Hi;
        }

        void MTHI(Instruction instruction)
        {
            Hi = GPR[instruction.Rs];
        }

        void MFLO(Instruction instruction)
        {
            GPR[instruction.Rd] = Lo;
        }

        void MTLO(Instruction instruction)
        {
            Lo = GPR[instruction.Rs];
        }

        void MULT(Instruction instruction)
        {
            var temp = (long) GPR[instruction.Rs] * GPR[instruction.Rt];
            Lo = (uint) (temp & 0xFFFFFFFF);
            Hi = (uint) (temp >> 32) & 0xFFFFFFFF;
        }

        void MULTU(Instruction instruction)
        {
            var temp = (long) GPR[instruction.Rs] * GPR[instruction.Rt];
            Lo = (uint) (temp & 0xFFFFFFFF);
            Hi = (uint) (temp >> 32) & 0xFFFFFFFF;
        }

        void DIV(Instruction instruction)
        {
            Lo = GPR[instruction.Rs] / GPR[instruction.Rt];
            Hi = GPR[instruction.Rs] % GPR[instruction.Rt];
        }

        void DIVU(Instruction instruction)
        {
            Lo = GPR[instruction.Rs] / GPR[instruction.Rt];
            Hi = GPR[instruction.Rs] % GPR[instruction.Rt];
        }

        void ADD(Instruction instruction)
        {
            GPR[instruction.Rd] = GPR[instruction.Rs] + GPR[instruction.Rt];
        }

        void ADDU(Instruction instruction)
        {
            GPR[instruction.Rd] = GPR[instruction.Rs] + GPR[instruction.Rt];
        }

        void SUB(Instruction instruction)
        {
            GPR[instruction.Rd] = GPR[instruction.Rs] - GPR[instruction.Rt];
        }

        void SUBU(Instruction instruction)
        {
            GPR[instruction.Rd] = GPR[instruction.Rs] - GPR[instruction.Rt];
        }

        void AND(Instruction instruction)
        {
            GPR[instruction.Rd] = GPR[instruction.Rs] & GPR[instruction.Rt];
        }

        void OR(Instruction instruction)
        {
            GPR[instruction.Rd] = GPR[instruction.Rs] | GPR[instruction.Rt];
        }

        void XOR(Instruction instruction)
        {
            GPR[instruction.Rd] = GPR[instruction.Rs] ^ GPR[instruction.Rt];
        }

        void NOR(Instruction instruction)
        {
            GPR[instruction.Rd] = 0xFFFFFFFF ^ (GPR[instruction.Rs] | GPR[instruction.Rt]);
        }

        void SLT(Instruction instruction)
        {
            if (GPR[instruction.Rs] < GPR[instruction.Rt])
            {
                GPR[instruction.Rd] = 1;
            }
            else
            {
                GPR[instruction.Rd] = 0;
            }
        }

        void SLTU(Instruction instruction)
        {
            // FIXME:
            if (GPR[instruction.Rs] >> 1 < GPR[instruction.Rt] >> 1)
            {
                GPR[instruction.Rd] = 1;
            }
            else
            {
                GPR[instruction.Rd] = 0;
            }
        }

        void BLTZ(Instruction instruction)
        {
            var address = PcNext + (instruction.Imm16Sx << 2);
            if (GPR[instruction.Rs] >> 31 == 1)
            {
                PcNext = address;
            }
        }

        void BGEZ(Instruction instruction)
        {
            var address = PcNext + (instruction.Imm16Sx << 2);
            if (GPR[instruction.Rs] >> 31 == 0)
            {
                PcNext = address;
            }
        }

        void BLTZAL(Instruction instruction)
        {
            var address = PcNext + (instruction.Imm16Sx << 2);
            if (GPR[instruction.Rs] >> 31 == 1)
            {
                PcNext = address;
            }

            GPR[31] = PcNext + 4;
        }

        void BGEZAL(Instruction instruction)
        {
            var address = PcNext + (instruction.Imm16Sx << 2);
            if ((GPR[instruction.Rs] >> 31) == 0)
            {
                PcNext = address;
            }

            GPR[31] = PcNext + 4;
        }

        void J(Instruction instruction)
        {
            PcNext = PcNext & 0xF0000000 | (instruction.Address << 2);
        }

        void JAL(Instruction instruction)
        {
            GPR[31] = PcNext + 4;
            PcNext = PcNext & 0xF0000000 | (instruction.Address << 2);
        }

        void BEQ(Instruction instruction)
        {
            var address = PcNext + (instruction.Imm16Sx << 2);
            if (GPR[instruction.Rs] == GPR[instruction.Rt])
            {
                PcNext = address;
            }
        }

        void BNE(Instruction instruction)
        {
            var address = PcNext + (instruction.Imm16Sx << 2);
            if (GPR[instruction.Rs] != GPR[instruction.Rs])
            {
                PcNext = address;
            }
        }

        void BLEZ(Instruction instruction)
        {
            var address = PcNext + (instruction.Imm16Sx << 2);
            if ((GPR[instruction.Rs] >> 31) == 1 || GPR[instruction.Rs] == 0)
            {
                PcNext = address;
            }
        }

        void BGTZ(Instruction instruction)
        {
            var address = PcNext + (instruction.Imm16Sx << 2);
            if ((GPR[instruction.Rs] >> 31) == 0 && GPR[instruction.Rs] != 0)
            {
                PcNext = address;
            }
        }

        void ADDI(Instruction instruction)
        {
            GPR[instruction.Rt] = GPR[instruction.Rs] + instruction.Imm16Sx;
        }

        void ADDIU(Instruction instruction)
        {
            GPR[instruction.Rt] = GPR[instruction.Rs] + instruction.Imm16Sx;
        }

        void SLTI(Instruction instruction)
        {
            if (GPR[instruction.Rs] < instruction.Imm16Sx)
            {
                GPR[instruction.Rt] = 1;
            }
            else
            {
                GPR[instruction.Rt] = 0;
            }
        }

        void SLTIU(Instruction instruction)
        {
            if (GPR[instruction.Rs] >> 1 < instruction.Imm16Sx)
            {
                GPR[instruction.Rt] = 1;
            }
            else
            {
                GPR[instruction.Rt] = 0;
            }
        }

        void ANDI(Instruction instruction)
        {
            GPR[instruction.Rt] = GPR[instruction.Rs] & instruction.Imm16;
        }

        void ORI(Instruction instruction)
        {
            GPR[instruction.Rt] = GPR[instruction.Rs] | instruction.Imm16;
        }

        void XORI(Instruction instruction)
        {
            GPR[instruction.Rt] = GPR[instruction.Rs] ^ instruction.Imm16;
        }

        void LUI(Instruction instruction)
        {
            GPR[instruction.Rt] = instruction.Imm16 << 16;
        }

        void MFC(Instruction instruction, Bus bus, uint z)
        {
            LoadDelaySlot = instruction.Rt;
            LoadDelayValue = COP0R[instruction.Rd];

            if (z == 2)
            {
                throw new NotImplementedException();
            }
        }

        void CFC(Instruction instruction, Bus bus, uint z)
        {
            if (z == 0)
            {
                throw new InvalidCodeException("COP0 has no control registers");
            }

            if (z == 2)
            {
                throw new NotImplementedException();
            }
        }

        void MTC(Instruction instruction, Bus bus, uint z)
        {
            COP0R[instruction.Rd] = GPR[instruction.Rs];

            if (z == 2)
            {
                throw new NotImplementedException();
            }
        }

        void CTC(Instruction instruction, Bus bus, uint z)
        {
            if (z == 0)
            {
                throw new InvalidCodeException("COP0 has no control registers");
            }

            if (z == 2)
            {
                throw new NotImplementedException();
            }
        }

        void LB(Instruction instruction, Bus bus)
        {
            var address = instruction.Imm16Sx + GPR[instruction.Rs];
            var value = bus.LoadByte(address);
            LoadDelaySlot = instruction.Rt;
            LoadDelayValue = (uint) (sbyte) value;
        }

        void LH(Instruction instruction, Bus bus)
        {
            var address = instruction.Imm16Sx + GPR[instruction.Rs];
            var value = bus.LoadHalfword(address);
            LoadDelaySlot = instruction.Rt;
            LoadDelayValue = (uint) (short) value;
        }

        void LW(Instruction instruction, Bus bus)
        {
            var address = instruction.Imm16Sx + GPR[instruction.Rs];
            var value = bus.LoadWord(address);
            LoadDelaySlot = instruction.Rt;
            LoadDelayValue = value;
        }

        void LWL(Instruction instruction, Bus bus)
        {
            var address = instruction.Imm16Sx + GPR[instruction.Rs];
            uint temp = 0;

            switch (address % 4)
            {
                case 0:
                    GPR[instruction.Rt] = bus.LoadWord(address / 4);
                    break;
                case 1:
                    temp = bus.LoadWord(address / 4) & (uint) 0xFFFFFF << 8;
                    GPR[instruction.Rt] = GPR[instruction.Rs] & 0xFF | temp;
                    break;
                case 2:
                    temp = bus.LoadWord(address / 4) & (uint) 0xFFFF << 16;
                    GPR[instruction.Rt] = GPR[instruction.Rs] & 0xFFFF | temp;
                    break;
                case 3:
                    temp = bus.LoadWord(address / 4) & (uint) 0xFF << 24;
                    GPR[instruction.Rt] = GPR[instruction.Rs] & 0xFFFFFF | temp;
                    break;
            }
        }

        void LBU(Instruction instruction, Bus bus)
        {
            var address = instruction.Imm16Sx + GPR[instruction.Rs];
            var value = bus.LoadByte(address);
            LoadDelaySlot = instruction.Rt;
            LoadDelayValue = value;
        }

        void LHU(Instruction instruction, Bus bus)
        {
            var address = instruction.Imm16Sx + GPR[instruction.Rs];
            var value = bus.LoadHalfword(address);
            LoadDelaySlot = instruction.Rt;
            LoadDelayValue = value;
        }

        void LWR(Instruction instruction, Bus bus)
        {
            var address = instruction.Imm16Sx + GPR[instruction.Rs];
            uint temp = 0;

            switch (address % 4)
            {
                case 0:
                    GPR[instruction.Rt] = bus.LoadWord(address / 4);
                    break;
                case 1:
                    temp = (bus.LoadWord(address / 4) & 0xFF000000 >> 24) & 0xFF;
                    GPR[instruction.Rt] = GPR[instruction.Rs] & 0xFFFFFF00 | temp;
                    break;
                case 2:
                    temp = (bus.LoadWord(address / 4) & 0xFFFF0000 >> 16) & 0xFFF;
                    GPR[instruction.Rt] = GPR[instruction.Rs] & 0xFFFF0000 | temp;
                    break;
                case 3:
                    temp = (bus.LoadWord(address / 4) & 0xFFFFFF00 >> 8) & 0xFFFFFF;
                    GPR[instruction.Rt] = GPR[instruction.Rs] & 0xFF000000 | temp;
                    break;
            }
        }

        void SB(Instruction instruction, Bus bus)
        {
            if ((COP0R[12] & 0x10000) != 0)
            {
                // log.Printf("Ignored store to cache")
                return;
            }

            var address = instruction.Imm16Sx + GPR[instruction.Rs];
            bus.StoreByte(address, (byte) (GPR[instruction.Rs] & 0xFF));
        }

        void SH(Instruction instruction, Bus bus)
        {
            if ((COP0R[12] & 0x10000) != 0)
            {
                return;
            }

            var address = instruction.Imm16Sx + GPR[instruction.Rs];
            bus.StoreHalfword(address, (ushort) (GPR[instruction.Rs] & 0xFFFF));
        }

        void SWL(Instruction instruction, Bus bus)
        {
            //TODO Later
        }

        void SW(Instruction instruction, Bus bus)
        {
            if ((COP0R[12] & 0x10000) != 0)
            {
                return;
            }

            var address = instruction.Imm16Sx + GPR[instruction.Rs];
            bus.StoreWord(address, GPR[instruction.Rs]);
        }

        void SWR(Instruction instruction, Bus bus)
        {
            //TODO Later
        }

        public void Execute(Instruction instruction, Bus bus)
        {
            switch (instruction.Opcode)
            {
                case 0x00:
                    switch (instruction.Function)
                    {
                        case 0x00:
                            SLL(instruction);
                            break;
                        case 0x02:
                            SRL(instruction);
                            break;
                        case 0x03:
                            SRA(instruction);
                            break;
                        case 0x04:
                            SLLV(instruction);
                            break;
                        case 0x06:
                            SRLV(instruction);
                            break;
                        case 0x07:
                            SRAV(instruction);
                            break;
                        case 0x08:
                            JR(instruction);
                            break;
                        case 0x09:
                            JALR(instruction);
                            break;
                        //case 0x0C:
                        //	SYSCALL(instruction, bus)
                        //case 0x0D:
                        //	BREAK(instruction, bus)
                        case 0x10:
                            MFHI(instruction);
                            break;
                        case 0x11:
                            MTHI(instruction);
                            break;
                        case 0x12:
                            MFLO(instruction);
                            break;
                        case 0x13:
                            MTLO(instruction);
                            break;
                        case 0x18:
                            MULT(instruction);
                            break;
                        case 0x19:
                            MULTU(instruction);
                            break;
                        case 0x1A:
                            DIV(instruction);
                            break;
                        case 0x1B:
                            DIVU(instruction);
                            break;
                        case 0x20:
                            ADD(instruction);
                            break;
                        case 0x21:
                            ADDU(instruction);
                            break;
                        case 0x22:
                            SUB(instruction);
                            break;
                        case 0x23:
                            SUBU(instruction);
                            break;
                        case 0x24:
                            AND(instruction);
                            break;
                        case 0x25:
                            OR(instruction);
                            break;
                        case 0x26:
                            XOR(instruction);
                            break;
                        case 0x27:
                            NOR(instruction);
                            break;
                        case 0x2A:
                            SLT(instruction);
                            break;
                        case 0x2B:
                            SLTU(instruction);
                            break;
                        default:
                            throw new InvalidCodeException($"unknown special instruction: {instruction.Function:X2}");
                    }

                    break;
                case 0x01:
                    switch (instruction.Rt)
                    {
                        case 0x00:
                            BLTZ(instruction);
                            break;
                        case 0x01:
                            BGEZ(instruction);
                            break;
                        case 0x0A:
                            BLTZAL(instruction);
                            break;
                        case 0x0B:
                            BGEZAL(instruction);
                            break;
                        default:
                            throw new InvalidCodeException($"unknown bcondz instruction: {instruction.Function:X2}");
                    }

                    break;
                case 0x02:
                    J(instruction);
                    break;
                case 0x03:
                    JAL(instruction);
                    break;
                case 0x04:
                    BEQ(instruction);
                    break;
                case 0x05:
                    BNE(instruction);
                    break;
                case 0x06:
                    BLEZ(instruction);
                    break;
                case 0x07:
                    BGTZ(instruction);
                    break;
                case 0x08:
                    ADDI(instruction);
                    break;
                case 0x09:
                    ADDIU(instruction);
                    break;
                case 0x0A:
                    SLTI(instruction);
                    break;
                case 0x0B:
                    SLTIU(instruction);
                    break;
                case 0x0C:
                    ANDI(instruction);
                    break;
                case 0x0D:
                    ORI(instruction);
                    break;
                case 0x0E:
                    XORI(instruction);
                    break;
                case 0x0F:
                    LUI(instruction);
                    break;
                case 0x10:
                case 0x12:
                    var z = instruction.Opcode - 0x10;
                    switch (instruction.Rs)
                    {
                        case 0x0:
                            MFC(instruction, bus, z);
                            break;
                        case 0x2:
                            CFC(instruction, bus, z);
                            break;
                        case 0x4:
                            MTC(instruction, bus, z);
                            break;
                        case 0x6:
                            CTC(instruction, bus, z);
                            break;
                        default:
                            throw new InvalidCodeException(
                                $"unknown coprocessor opcode instruction: {instruction.Rs:X2}");
                    }

                    break;
                case 0x20:
                    LB(instruction, bus);
                    break;
                case 0x21:
                    LH(instruction, bus);
                    break;
                case 0x23:
                    LW(instruction, bus);
                    break;
                case 0x22:
                    LWL(instruction, bus);
                    break;
                case 0x24:
                    LBU(instruction, bus);
                    break;
                case 0x25:
                    LHU(instruction, bus);
                    break;
                case 0x26:
                    LWR(instruction, bus);
                    break;
                case 0x28:
                    SB(instruction, bus);
                    break;
                case 0x29:
                    SH(instruction, bus);
                    break;
                //case 0x2A:
                //	SWL(instruction, bus)
                case 0x2B:
                    SW(instruction, bus);
                    break;
                //case 0x2E:
                //	SWR(instruction, bus)
                default:
                    throw new InvalidCodeException($"unknown primary instruction: {instruction.Opcode:X2}");
            }
        }

        public void Cycle(Bus bus)
        {
            var instruction = new Instruction(bus.LoadWord(Pc));
            Console.WriteLine(instruction);
            Pc = PcNext;
            PcNext += 4;
            GPR[LoadDelaySlot] = LoadDelayValue;
            LoadDelaySlot = 0;
            LoadDelayValue = 0;
            Execute(instruction, bus);
            GPR.Cycle();
        }
    }
}