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
                        byte address = Read(ProgramCounter);
                        ProgramCounter++;
                        Write(address, A);
                        cycles = 3;
                        break;
                    }
                case 0x8D: // STA Absolute
                    {
                        byte low = Read(ProgramCounter);
                        ProgramCounter++;
                        byte high = Read(ProgramCounter);
                        ProgramCounter++;
                        Write((ushort)((high << 8) | low), A);
                        cycles = 4;
                        break;
                    }
                case 0xA5: // LDA Zero Page
                    {
                        byte address = Read(ProgramCounter);
                        ProgramCounter++;
                        A = Read(address);
                        flag_zero = A == 0;
                        flag_negative = A > 127;
                        cycles = 3;
                        break;
                    }
                case 0xAD: // LDA Absolute
                    {
                        byte low = Read(ProgramCounter);
                        ProgramCounter++;
                        byte high = Read(ProgramCounter);
                        ProgramCounter++;
                        A = Read((ushort)((high << 8) | low));
                        flag_zero = A == 0;
                        flag_negative = A > 127;
                        cycles = 4;
                        break;
                    }

                // Store X and Y registers
                case 0x86: // STX Zero Page
                    {
                        byte address = Read(ProgramCounter);
                        ProgramCounter++;
                        Write(address, X);
                        cycles = 3;
                        break;
                    }
                case 0x8E: // STX Absolute
                    {
                        byte low = Read(ProgramCounter);
                        ProgramCounter++;
                        byte high = Read(ProgramCounter);
                        ProgramCounter++;
                        Write((ushort)((high << 8) | low), X);
                        cycles = 4;
                        break;
                    }
                case 0x84: // STY Zero Page
                    {
                        byte address = Read(ProgramCounter);
                        ProgramCounter++;
                        Write(address, Y);
                        cycles = 3;
                        break;
                    }
                case 0x8C: // STY Absolute
                    {
                        byte low = Read(ProgramCounter);
                        ProgramCounter++;
                        byte high = Read(ProgramCounter);
                        ProgramCounter++;
                        Write((ushort)((high << 8) | low), Y);
                        cycles = 4;
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

                // Subroutines
                case 0x20: // JSR
                    {
                        byte low = Read(ProgramCounter);
                        ProgramCounter++;
                        byte high = Read(ProgramCounter);
                        ProgramCounter++;
                        ushort returnAddress = (ushort)(ProgramCounter - 1);
                        PushStack((byte)((returnAddress >> 8) & 0xFF)); // Push high byte
                        PushStack((byte)(returnAddress & 0xFF)); // Push low byte
                        ProgramCounter = (ushort)((high << 8) | low);
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
                        byte low = Read(ProgramCounter);
                        byte high = Read((ushort)(ProgramCounter + 1));
                        ProgramCounter = (ushort)((high << 8) | low);
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
