namespace BallSort
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
            canvasPB = new PictureBox();
            status = new TextBox();
            ((System.ComponentModel.ISupportInitialize)canvasPB).BeginInit();
            SuspendLayout();
            // 
            // canvasPB
            // 
            canvasPB.BackColor = Color.Black;
            canvasPB.Location = new Point(39, 32);
            canvasPB.Name = "canvasPB";
            canvasPB.Size = new Size(100, 50);
            canvasPB.TabIndex = 0;
            canvasPB.TabStop = false;
            canvasPB.MouseClick += BS_MouseClick;
            // 
            // status
            // 
            status.Location = new Point(29, 397);
            status.Name = "status";
            status.ReadOnly = true;
            status.Size = new Size(100, 27);
            status.TabIndex = 1;
            status.TabStop = false;
            status.WordWrap = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(status);
            Controls.Add(canvasPB);
            Name = "Form1";
            Text = "BallSort";
            Load += BS_Load;
            ((System.ComponentModel.ISupportInitialize)canvasPB).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox canvasPB;
        private TextBox status;
    }
}
