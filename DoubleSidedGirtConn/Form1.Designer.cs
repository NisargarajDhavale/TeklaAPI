namespace DoubleSidedGirtConn
{
    partial class GirtConnection
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
            this.btn_GirtConn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_GirtConn
            // 
            this.btn_GirtConn.Location = new System.Drawing.Point(49, 87);
            this.btn_GirtConn.Name = "btn_GirtConn";
            this.btn_GirtConn.Size = new System.Drawing.Size(244, 86);
            this.btn_GirtConn.TabIndex = 0;
            this.btn_GirtConn.Text = "Insert Double Sided Girt Connection";
            this.btn_GirtConn.UseVisualStyleBackColor = true;
            this.btn_GirtConn.Click += new System.EventHandler(this.btn_GirtConn_Click);
            // 
            // GirtConnection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 272);
            this.Controls.Add(this.btn_GirtConn);
            this.Name = "GirtConnection";
            this.Text = "Girt Connection";
            this.Load += new System.EventHandler(this.GirtConnection_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_GirtConn;
    }
}

