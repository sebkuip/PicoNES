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
                    ProgramCounter++;
                    cycles = 2;
                    break;
                case 0xA2: // LDX Immediate
                    X = Read(ProgramCounter);
                    ProgramCounter++;
                    cycles = 2;
                    break;
                case 0xA0: // LDY Immediate
                    Y = Read(ProgramCounter);
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

                // Jump and Branch
                case 0x4C: // JMP Absolute
                    {
                        byte low = Read(ProgramCounter);
                        byte high = Read((ushort)(ProgramCounter + 1));
                        ProgramCounter = (ushort)((high << 8) | low);
                        cycles = 3;
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
