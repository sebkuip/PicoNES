namespace PicoNES
{
    partial class tracelogger
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            listView1 = new ListView();
            PC = new ColumnHeader();
            OP = new ColumnHeader();
            Instruction = new ColumnHeader();
            Arguments = new ColumnHeader();
            X = new ColumnHeader();
            Y = new ColumnHeader();
            A = new ColumnHeader();
            SP = new ColumnHeader();
            Flags = new ColumnHeader();
            SuspendLayout();
            // 
            // listView1
            // 
            listView1.AutoArrange = false;
            listView1.Columns.AddRange(new ColumnHeader[] { PC, OP, Instruction, Arguments, X, Y, A, SP, Flags });
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Location = new Point(12, 12);
            listView1.Name = "listView1";
            listView1.Size = new Size(740, 417);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // PC
            // 
            PC.Tag = "PC";
            // 
            // OP
            // 
            OP.Tag = "OP";
            // 
            // Instruction
            // 
            Instruction.Tag = "Instruction";
            // 
            // Arguments
            // 
            Arguments.Tag = "Arguments";
            // 
            // X
            // 
            X.Tag = "X";
            // 
            // Y
            // 
            Y.Tag = "Y";
            // 
            // A
            // 
            A.Tag = "A";
            // 
            // SP
            // 
            SP.Tag = "SP";
            // 
            // Flags
            // 
            Flags.Tag = "Flags";
            // 
            // tracelogger
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(764, 441);
            Controls.Add(listView1);
            Name = "tracelogger";
            Text = "tracelogger";
            ResumeLayout(false);
        }

        #endregion

        private ListView listView1;
        private ColumnHeader PC;
        private ColumnHeader OP;
        private ColumnHeader Instruction;
        private ColumnHeader Arguments;
        private ColumnHeader X;
        private ColumnHeader Y;
        private ColumnHeader A;
        private ColumnHeader SP;
        private ColumnHeader Flags;
    }
}