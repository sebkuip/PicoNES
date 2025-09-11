namespace PicoNES
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CPU cpu = new CPU();
            cpu.LoadROM(@"C:\Users\Sebas\Downloads\NES_roms\4_TheStack.nes");
            cpu.Reset();
            cpu.Run();
        }
    }
}
