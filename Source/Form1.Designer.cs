namespace S3AP
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            outputTextbox = new TextBox();
            button1 = new Button();
            passwordTextbox = new TextBox();
            label3 = new Label();
            slotTextbox = new TextBox();
            label2 = new Label();
            hostTextbox = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // outputTextbox
            // 
            outputTextbox.Location = new Point(24, 18);
            outputTextbox.Multiline = true;
            outputTextbox.Name = "outputTextbox";
            outputTextbox.ReadOnly = true;
            outputTextbox.ScrollBars = ScrollBars.Both;
            outputTextbox.Size = new Size(470, 363);
            outputTextbox.TabIndex = 0;
            // 
            // button1
            // 
            button1.BackColor = Color.DarkSlateBlue;
            button1.FlatStyle = FlatStyle.Popup;
            button1.ForeColor = Color.FromArgb(192, 192, 0);
            button1.Location = new Point(598, 105);
            button1.Name = "button1";
            button1.Size = new Size(158, 23);
            button1.TabIndex = 1;
            button1.Text = "Connect";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // passwordTextbox
            // 
            passwordTextbox.Location = new Point(598, 76);
            passwordTextbox.Name = "passwordTextbox";
            passwordTextbox.Size = new Size(158, 23);
            passwordTextbox.TabIndex = 13;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(523, 79);
            label3.Name = "label3";
            label3.Size = new Size(60, 15);
            label3.TabIndex = 12;
            label3.Text = "Password:";
            // 
            // slotTextbox
            // 
            slotTextbox.Location = new Point(598, 47);
            slotTextbox.Name = "slotTextbox";
            slotTextbox.Size = new Size(158, 23);
            slotTextbox.TabIndex = 11;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(523, 50);
            label2.Name = "label2";
            label2.Size = new Size(30, 15);
            label2.TabIndex = 10;
            label2.Text = "Slot:";
            // 
            // hostTextbox
            // 
            hostTextbox.Location = new Point(598, 18);
            hostTextbox.Name = "hostTextbox";
            hostTextbox.Size = new Size(158, 23);
            hostTextbox.TabIndex = 9;
            hostTextbox.Text = "archipelago.gg:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(523, 21);
            label1.Name = "label1";
            label1.Size = new Size(35, 15);
            label1.TabIndex = 8;
            label1.Text = "Host:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Purple;
            ClientSize = new Size(800, 450);
            Controls.Add(passwordTextbox);
            Controls.Add(label3);
            Controls.Add(slotTextbox);
            Controls.Add(label2);
            Controls.Add(hostTextbox);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(outputTextbox);
            MaximumSize = new Size(816, 489);
            MinimumSize = new Size(816, 489);
            Name = "Form1";
            Text = "Spyro 3 Archipelago Randomiser";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox outputTextbox;
        private Button button1;
        private TextBox passwordTextbox;
        private Label label3;
        private TextBox slotTextbox;
        private Label label2;
        private TextBox hostTextbox;
        private Label label1;
    }
}
