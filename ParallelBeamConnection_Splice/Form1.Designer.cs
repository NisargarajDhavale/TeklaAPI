namespace ParallelBeamConnection_Splice
{
    partial class ParallelBeamConnection_Splice
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
            this.btn_SpliceConnection = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_SpliceConnection
            // 
            this.btn_SpliceConnection.Location = new System.Drawing.Point(12, 12);
            this.btn_SpliceConnection.Name = "btn_SpliceConnection";
            this.btn_SpliceConnection.Size = new System.Drawing.Size(294, 63);
            this.btn_SpliceConnection.TabIndex = 0;
            this.btn_SpliceConnection.Text = "Insert Connection";
            this.btn_SpliceConnection.UseVisualStyleBackColor = true;
            this.btn_SpliceConnection.Click += new System.EventHandler(this.btn_SpliceConnection_Click);
            // 
            // ParallelBeamConnection_Splice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(318, 98);
            this.Controls.Add(this.btn_SpliceConnection);
            this.Name = "ParallelBeamConnection_Splice";
            this.Text = "Parallel Beam Splice";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_SpliceConnection;
    }
}

