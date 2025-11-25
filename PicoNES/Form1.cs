namespace PicoNES
{
    public partial class Form1 : Form
    {
        tracelogger tracelogger = new tracelogger();
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
            MessageBox.Show("Done");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tracelogger.Show();
            tracelogger.enableLogging = true;
        }
    }
}
