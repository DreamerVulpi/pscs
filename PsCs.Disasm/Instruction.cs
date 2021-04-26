using System;

namespace PsCs.Disasm
{
    public class Instruction
    {
        public uint Raw;

        public Instruction(uint raw)
        {
            Raw = raw;
        }

        public uint Opcode => (Raw >> 26) & 0x3F;
        public int Rs => (int) (Raw >> 21) & 0x1F;
        public int Rt => (int) (Raw >> 16) & 0x1F;
        public int Rd => (int) (Raw >> 11) & 0x1F;
        public int ShiftAmount => (int) (Raw >> 6) & 0x1F;
        public uint Function => Raw & 0x3F;
        public uint Imm16 => Raw & 0xFFFF;
        public uint Imm16Sx => (uint) (short) (Raw & 0xFFFF);
        public uint Address => Raw & 0x3FFFFFF;

        public override string ToString()
        {
            return Opcode switch
            {
                0x00 => Function switch
                {
                    0x00 => $"SLL     ${Rd}, ${Rt}, {ShiftAmount}",
                    0x02 => $"SRL     ${Rd}, ${Rt}, {ShiftAmount}",
                    0x03 => $"SRA     ${Rd}, ${Rt}, {ShiftAmount}",
                    0x04 => $"SLLV    ${Rd}, ${Rt}, ${Rs}",
                    0x06 => $"SRLV    ${Rd}, ${Rt}, ${Rs}",
                    0x07 => $"SRAV    ${Rd}, ${Rt}, ${Rs}",
                    0x08 => $"JR      ${Rs}",
                    0x09 => $"JALR    ${Rd}, ${Rs}",
                    0x0C => "SYSCALL",
                    0x0D => "BREAK",
                    0x10 => $"MFHI    ${Rd}",
                    0x11 => $"MTHI    ${Rs}",
                    0x12 => $"MFLO    ${Rd}",
                    0x13 => $"MTLO    ${Rs}",
                    0x18 => $"MULT    ${Rt}, ${Rs}",
                    0x19 => $"MULTU   ${Rt}, ${Rs}",
                    0x1A => $"DIV     ${Rt}, ${Rs}",
                    0x1B => $"DIVU    ${Rt}, ${Rs}",
                    0x20 => $"ADD     ${Rd}, ${Rs}, ${Rt}",
                    0x21 => $"ADDU    ${Rd}, ${Rs}, ${Rt}",
                    0x22 => $"SUB     ${Rd}, ${Rs}, ${Rt}",
                    0x23 => $"SUBU    ${Rd}, ${Rs}, ${Rt}",
                    0x24 => $"AND     ${Rd}, ${Rs}, ${Rt}",
                    0x25 => $"OR      ${Rd}, ${Rs}, ${Rt}",
                    0x26 => $"XOR     ${Rd}, ${Rs}, ${Rt}",
                    0x27 => $"NOR     ${Rd}, ${Rs}, ${Rt}",
                    0x2A => $"SLT     ${Rd}, ${Rs}, ${Rt}",
                    0x2B => $"SLTU    ${Rd}, ${Rs}, ${Rt}",
                    _ => $"Invalid SPECIAL opcode: {Function:X2}h",
                },
                0x01 => $"Invalid BCOND opcode: {Rs:X2}h",
                0x02 => $"J       {Address:X8}h",
                0x03 => $"JAL     {Address:X8}h",
                0x04 => $"BEQ     ${Rs}, ${Rt}, {Imm16:X}h",
                0x05 => $"BNE     ${Rs}, ${Rt}, {Imm16:X}h",
                0x06 => $"BLEZ    ${Rs}, {Imm16:X}h",
                0x07 => $"BGTZ    ${Rs}, {Imm16:X}h",
                0x08 => $"ADDI    ${Rt}, ${Rs}, {Imm16Sx:X}h",
                0x09 => $"ADDIU   ${Rt}, ${Rs}, {Imm16Sx:X}h",
                0x0A => $"SLTI    ${Rt}, ${Rs}, {Imm16Sx:X}h",
                0x0B => $"SLTIU   ${Rt}, ${Rs}, {Imm16Sx:X}h",
                0x0C => $"ANDI    ${Rt}, ${Rs}, {Imm16:X}h",
                0x0D => $"ORI     ${Rt}, ${Rs}, {Imm16:X}h",
                0x0E => $"XORI    ${Rt}, ${Rs}, {Imm16:X}h",
                0x0F => $"LUI     ${Rt}, {Imm16:X}h",
                0x10 => "COP0",
                0x12 => "COP2",
                0x20 => $"LB      ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x21 => $"LH      ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x22 => $"LWL     ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x23 => $"LW      ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x24 => $"LBU     ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x25 => $"LHU     ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x26 => $"LWR     ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x28 => $"SB      ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x29 => $"SH      ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x2A => $"SWL     ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x2B => $"SW      ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x2E => $"SWR     ${Rt}, {Imm16Sx:X}h(${Rs})",
                0x30 => "LWC0",
                0x31 => "LWC1",
                0x32 => "LWC2",
                0x33 => "LWC3",
                0x38 => "SWC0",
                0x39 => "SWC1",
                0x3A => "SWC2",
                0x3B => "SWC3",
                _ => $"unknown opcode: {Opcode:X2}",
            };
        }
    }
}