namespace PicoNES
{
    public partial class tracelogger : Form
    {
        public bool enableLogging = false;
        public tracelogger()
        {
            InitializeComponent();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            enableLogging = false;
            base.OnFormClosing(e);
        }

        public void pushData(int PC, string OP, string Arguments, int X, int Y, int A, int SP, byte Flags)
        {
            if (!enableLogging) return;
            ListViewItem item = new ListViewItem("$" + PC.ToString("X4"));
            item.SubItems.Add(OP);
            item.SubItems.Add(Arguments);
            item.SubItems.Add(X.ToString("X2"));
            item.SubItems.Add(Y.ToString("X2"));
            item.SubItems.Add(A.ToString("X2"));
            item.SubItems.Add(SP.ToString("X2"));
            item.SubItems.Add(Convert.ToString(Flags, 2).PadLeft(8, '0'));
            listView1.Items.Add(item);
        }
    }
}
