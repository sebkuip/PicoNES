namespace PicoNES
{
    public class CPU
    {
        bool halted = false;
        uint cycles = 1;

        ushort ProgramCounter;
        byte A;
        byte X;
        byte Y;
        byte SP;

        // flags
        bool flag_carry = false;
        bool flag_zero = false;
        bool flag_interrupt_disable = false;
        bool flag_decimal = false;
        bool flag_overflow = false;
        bool flag_negative = false;

        byte[] RAM = new byte[0x0800];
        byte[] ROM = new byte[0x8000];
        byte[] Header = new byte[0x10];

        string? filepath;

        byte Read(ushort address)
        {
            if (address < 0x8000)
            {
                return RAM[address & 0x07FF]; // The NES only has 2KB of RAM, mirrored every 2KB.
            }
            else if (address >= 0x8000)
            {
                return ROM[address - 0x8000];
            }
            else
            {
                // Handle other memory-mapped I/O here
                return 0;
            }

        }
        ushort ReadAbsolute()
        {
            byte low = Read(ProgramCounter);
            ProgramCounter++;
            byte high = Read((ushort)(ProgramCounter));
            ProgramCounter++;
            return (ushort)((high << 8) | low);
        }

        ushort ReadZeroPage()
        {
            ushort address = ReadZeroPage();
            ProgramCounter++;
            return address;
        }

        void Write(ushort address, byte value)
        {
            if (address < 0x8000)
            {
                RAM[address & 0x07FF] = value; // The NES only has 2KB of RAM, mirrored every 2KB.
            }
            else if (address >= 0x8000)
            {
                // ROM is read-only
            }
            else
            {
                // Handle other memory-mapped I/O here
            }
        }

        void PushStack(byte value)
                    {
            Write((ushort)(0x0100 + SP), value);
            SP--;
            if (SP < 0x00) SP = 0xFF; // Wrap around if stack pointer goes below 0
        }

        byte PullStack()
        {
            SP++;
            if (SP > 0xFF) SP = 0x00; // Wrap around if stack pointer goes above 0xFF
            return Read((ushort)(0x0100 + SP));
        }

        public void Reset()
        {
            if (filepath == null)
            {
                throw new Exception("No ROM loaded.");
            }
            byte[] HeaderedROM = File.ReadAllBytes(filepath);
            Array.Copy(HeaderedROM, 0x10, ROM, 0, 0x8000); // Skip the 16-byte header
            Array.Copy(HeaderedROM, 0, Header, 0, 0x10); // Copy the header

            byte low = Read(0xFFFC);
            byte high = Read(0xFFFD);
            ProgramCounter = (ushort)((high << 8) | low);
            flag_interrupt_disable = true; // Disable interrupts on reset
            SP = 0xFD; // Stack Pointer starts at 0xFD
        }

        public void LoadROM(string path)
        {
            filepath = path;
        }

        public void Run()
        {
            while (!halted)
            {
                execute();
            }
        }

        private void execute()
                    {
            byte opcode = Read(ProgramCounter);
            cycles = 1; // Default cycle count
            ProgramCounter++;
            switch (opcode)
            {
                case 0x02: // BRK - Force Interrupt
                    halted = true; // For simplicity, we'll just halt the CPU
                    break;
            
                // Load immediates

                case 0xA9: // LDA Immediate
                    A = Read(ProgramCounter);
                    flag_zero = A == 0;
                    flag_negative = A > 127;
                    ProgramCounter++;
                    cycles = 2;
                    break;
                case 0xA2: // LDX Immediate
                    X = Read(ProgramCounter);
                    flag_zero = X == 0;
                    flag_negative = X > 127;
                    ProgramCounter++;
                    cycles = 2;
                    break;
                case 0xA0: // LDY Immediate
                    Y = Read(ProgramCounter);
                    flag_zero = Y == 0;
                    flag_negative = Y > 127;
                    ProgramCounter++;
                    cycles = 2;
                    break;

                // Store and load accumulator
                case 0x85: // STA Zero Page 
                    {
                        ushort address = ReadZeroPage();
                        ProgramCounter++;
                        Write(address, A);
                        cycles = 3;
                        break;
                    }
                case 0x8D: // STA Absolute
                    {
                        ushort address = ReadAbsolute();
                        Write(address, A);
                        cycles = 4;
                        break;
                    }
                case 0xA5: // LDA Zero Page
                    {
                        ushort address = ReadZeroPage();
                        ProgramCounter++;
                        A = Read(address);
                        flag_zero = A == 0;
                        flag_negative = A > 127;
                        cycles = 3;
                        break;
                    }
                case 0xAD: // LDA Absolute
                    {
                        ushort address = ReadAbsolute();
                        A = Read(address);
                        flag_zero = A == 0;
                        flag_negative = A > 127;
                        cycles = 4;
                        break;
                    }

                // Store X and Y registers
                case 0x86: // STX Zero Page
                    {
                        ushort address = ReadZeroPage();
                        ProgramCounter++;
                        Write(address, X);
                        cycles = 3;
                        break;
                    }
                case 0x8E: // STX Absolute
                    {
                        ushort address = ReadAbsolute();
                        Write(address, X);
                        cycles = 4;
                        break;
                    }
                case 0x84: // STY Zero Page
                    {
                        ushort address = ReadZeroPage();
                        ProgramCounter++;
                        Write(address, Y);
                        cycles = 3;
                        break;
                    }
                case 0x8C: // STY Absolute
                    {
                        ushort address = ReadAbsolute();
                        Write(address, Y);
                        cycles = 4;
                        break;
                    }

                // Arithmetic
                case 0x0A: // ASL Accumulator
                    {
                        flag_carry = (A & 0x80) != 0; // Set carry if bit 7 is set
                        A <<= 1;
                        flag_zero = A == 0;
                        flag_negative = A > 127;
                        ProgramCounter++;
                        cycles = 2;
                        break;
                    }
                case 0x06: // ASL Zero Page
                    {
                        ushort address = ReadZeroPage();
                        ProgramCounter++;
                        byte value = Read(address);
                        flag_carry = (value & 0x80) != 0; // Set carry if bit 7 is set
                        value <<= 1;
                        Write(address, value);
                        flag_zero = value == 0;
                        flag_negative = value > 127;
                        cycles = 5;
                        break;
                    }
                case 0x0E: // ASL Absolute
                    {
                        ushort address = ReadAbsolute();
                        byte value = Read(address);
                        flag_carry = (value & 0x80) != 0; // Set carry if bit 7 is set
                        value <<= 1;
                        Write(address, value);
                        flag_zero = value == 0;
                        flag_negative = value > 127;
                        cycles = 6;
                        break;
                    }

                // Stack manipulation
                case 0x48: // PHA
                    {
                        PushStack(A);
                        cycles = 3;
                        break;
                    }
                case 0x68: // PLA
                    {
                        A = PullStack();
                        flag_zero = A == 0;
                        flag_negative = A > 127;
                        cycles = 4;
                        break;
                    }
                case 0x08: // PHP
                    {
                        byte status = 0;
                        if (flag_carry) status |= 0x01;
                        if (flag_zero) status |= 0x02;
                        if (flag_interrupt_disable) status |= 0x04;
                        if (flag_decimal) status |= 0x08;
                        status |= 0x10; // Unused bit, always set
                        status |= 0x20; // Unused bit, always set
                        if (flag_overflow) status |= 0x40;
                        if (flag_negative) status |= 0x80;
                        PushStack(status);
                        cycles = 3;
                        break;
                    }
                case 0x28: // PLP
                    {
                        byte status = PullStack();
                        flag_carry = (status & 0x01) != 0;
                        flag_zero = (status & 0x02) != 0;
                        flag_interrupt_disable = (status & 0x04) != 0;
                        flag_decimal = (status & 0x08) != 0;
                        // Bit 4 is ignored
                        // Bit 5 is ignored
                        flag_overflow = (status & 0x40) != 0;
                        flag_negative = (status & 0x80) != 0;
                        cycles = 3;
                        break;
                    }

                // Subroutines
                case 0x20: // JSR
                    {
                        ushort address = ReadAbsolute();
                        ushort returnAddress = (ushort)(ProgramCounter - 1);
                        PushStack((byte)((returnAddress >> 8) & 0xFF)); // Push high byte
                        PushStack((byte)(returnAddress & 0xFF)); // Push low byte
                        ProgramCounter = address;
                        cycles = 6;
                        break;
                    }
                case 0x60: // RTS
                    {
                        byte low = PullStack();
                        byte high = PullStack();
                        ProgramCounter = (ushort)((high << 8) | low);
                        ProgramCounter++; // Increment to the next instruction
                        cycles = 6;
                        break;
                    }

                // Jump and Branch
                case 0x4C: // JMP Absolute
                    {
                        ushort address = ReadAbsolute();
                        ProgramCounter = address);
                        cycles = 3;
                        break;
                    }
                case 0x10: // BPL
                    {                         
                        sbyte offset = (sbyte)Read(ProgramCounter);
                        ProgramCounter++;
                        if (!flag_negative)
                        {
                            byte high = (byte)(ProgramCounter >> 8);
                            ProgramCounter = (ushort)(ProgramCounter + offset);
                            if (high != (byte)(ProgramCounter >> 8)) cycles++; // Add a cycle if page is crossed
                            cycles = 3; // Branch taken
                        }
                        else
                        {
                            cycles = 2; // Branch not taken
                        }
                        break;
                    }
                case 0x30: // BMI
                    {
                        sbyte offset = (sbyte)Read(ProgramCounter);
                        ProgramCounter++;
                        if (flag_negative)
                        {
                            byte high = (byte)(ProgramCounter >> 8);
                            ProgramCounter = (ushort)(ProgramCounter + offset);
                            if (high != (byte)(ProgramCounter >> 8)) cycles++; // Add a cycle if page is crossed
                            cycles = 3; // Branch taken
                        }
                        else
                        {
                            cycles = 2; // Branch not taken
                        }
                        break;
                    }
                case 0x50: // BVC
                    {
                        sbyte offset = (sbyte)Read(ProgramCounter);
                        ProgramCounter++;
                        if (!flag_overflow)
                        {
                            byte high = (byte)(ProgramCounter >> 8);
                            ProgramCounter = (ushort)(ProgramCounter + offset);
                            if (high != (byte)(ProgramCounter >> 8)) cycles++; // Add a cycle if page is crossed
                            cycles = 3; // Branch taken
                        }
                        else
                        {
                            cycles = 2; // Branch not taken
                        }
                        break;
                    }
                case 0x70: // BVS
                    {
                        sbyte offset = (sbyte)Read(ProgramCounter);
                        ProgramCounter++;
                        if (flag_overflow)
                        {
                            byte high = (byte)(ProgramCounter >> 8);
                            ProgramCounter = (ushort)(ProgramCounter + offset);
                            if (high != (byte)(ProgramCounter >> 8)) cycles++; // Add a cycle if page is crossed
                            cycles = 3; // Branch taken
                        }
                        else
                        {
                            cycles = 2; // Branch not taken
                        }
                        break;
                    }
                case 0x90: // BCC
                    {
                        sbyte offset = (sbyte)Read(ProgramCounter);
                        ProgramCounter++;
                        if (!flag_carry)
                        {
                            byte high = (byte)(ProgramCounter >> 8);
                            ProgramCounter = (ushort)(ProgramCounter + offset);
                            if (high != (byte)(ProgramCounter >> 8)) cycles++; // Add a cycle if page is crossed
                            cycles = 3; // Branch taken
                        }
                        else
                        {
                            cycles = 2; // Branch not taken
                        }
                        break;
                    }
                case 0xB0: // BCS
                    {
                        sbyte offset = (sbyte)Read(ProgramCounter);
                        ProgramCounter++;
                        if (flag_carry)
                        {
                            byte high = (byte)(ProgramCounter >> 8);
                            ProgramCounter = (ushort)(ProgramCounter + offset);
                            if (high != (byte)(ProgramCounter >> 8)) cycles++; // Add a cycle if page is crossed
                            cycles = 3; // Branch taken
                        }
                        else
                        {
                            cycles = 2; // Branch not taken
                        }
                        break;
                    }
                case 0xD0: // BNE
                    {
                        sbyte offset = (sbyte)Read(ProgramCounter);
                        ProgramCounter++;
                        if (!flag_zero)
                        {
                            byte high = (byte)(ProgramCounter >> 8);
                            ProgramCounter = (ushort)(ProgramCounter + offset);
                            if (high != (byte)(ProgramCounter >> 8)) cycles++; // Add a cycle if page is crossed
                            cycles = 3; // Branch taken
                        }
                        else
                        {
                            cycles = 2; // Branch not taken
                        }
                        break;
                    }
                case 0xF0: // BEQ
                    {
                        sbyte offset = (sbyte)Read(ProgramCounter);
                        ProgramCounter++;
                        if (flag_zero)
                        {
                            byte high = (byte)(ProgramCounter >> 8);
                            ProgramCounter = (ushort)(ProgramCounter + offset);
                            if (high != (byte)(ProgramCounter >> 8)) cycles++; // Add a cycle if page is crossed
                            cycles = 3; // Branch taken
                        }
                        else
                        {
                            cycles = 2; // Branch not taken
                        }
                        break;
                    }

                // Increment and Decrement
                case 0xE8: // INX
                    {
                        X++;
                        flag_zero = X == 0;
                        flag_negative = X > 127;
                        cycles = 2;
                        break;
                    }
                case 0xCA: // DEX
                    {
                        X--;
                        flag_zero = X == 0;
                        flag_negative = X > 127;
                        cycles = 2;
                        break;
                    }
                case 0xC8: // INY
                    {
                        Y++;
                        flag_zero = Y == 0;
                        flag_negative = Y > 127;
                        cycles = 2;
                        break;
                    }
                case 0x88: // DEY
                    {
                        Y--;
                        flag_zero = Y == 0;
                        flag_negative = Y > 127;
                        cycles = 2;
                        break;
                    }

                // transfers
                case 0xAA: // TAX
                    {
                        X = A;
                        flag_zero = X == 0;
                        flag_negative = X > 127;
                        cycles = 2;
                        break;
                    }
                case 0x8A: // TXA
                    {
                        A = X;
                        flag_zero = A == 0;
                        flag_negative = A > 127;
                        cycles = 2;
                        break;
                    }
                case 0xA8: // TAY
                    {
                        Y = A;
                        flag_zero = Y == 0;
                        flag_negative = Y > 127;
                        cycles = 2;
                        break;
                    }
                case 0x98: // TYA
                    {
                        A = Y;
                        flag_zero = A == 0;
                        flag_negative = A > 127;
                        cycles = 2;
                        break;
                    }
                case 0x9A: // TXS
                    {
                        SP = X;
                        cycles = 2;
                        break;
                    }
                case 0xBA: // TSX
                    {
                        X = SP;
                        flag_zero = X == 0;
                        flag_negative = X > 127;
                        cycles = 2;
                        break;
                    }

                // Flag setting
                case 0x38: // SEC
                    {
                        flag_carry = true;
                        cycles = 2;
                        break;
                    }
                case 0x18: // CLC
                    {
                        flag_carry = false;
                        cycles = 2;
                        break;
                    }
                case 0xB8: // CLV
                    {
                        flag_overflow = false;
                        cycles = 2;
                        break;
                    }
                case 0x78: // SEI
                    {
                        flag_interrupt_disable = true;
                        cycles = 2;
                        break;
                    }
                case 0x58: // CLI
                    {
                        flag_interrupt_disable = false;
                        cycles = 2;
                        break;
                    }
                case 0xF8: // SED
                    {
                        flag_decimal = true;
                        cycles = 2;
                        break;
                    }
                case 0xD8: // CLD
                    {
                        flag_decimal = false;
                        cycles = 2;
                        break;
                    }

                // No Operation
                case 0xEA: // NOP
                    {
                        cycles = 2;
                        break;
                    }

                default:
                    throw new NotImplementedException($"Opcode {opcode:X2} not implemented.");
            }

            executePPU(cycles * 3); // PPU runs at 3 times the CPU speed
        }

        private void executePPU(uint ppuCycles)
        {
            // PPU execution logic goes here
        }
    }
}
