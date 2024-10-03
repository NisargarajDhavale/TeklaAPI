namespace ColToBeamConnection
{
    partial class Form1
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
            this.btn_insertBeam = new System.Windows.Forms.Button();
            this.btn_insertCol = new System.Windows.Forms.Button();
            this.btn_Plate = new System.Windows.Forms.Button();
            this.btn_Connection = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_insertBeam
            // 
            this.btn_insertBeam.Location = new System.Drawing.Point(35, 72);
            this.btn_insertBeam.Name = "btn_insertBeam";
            this.btn_insertBeam.Size = new System.Drawing.Size(279, 42);
            this.btn_insertBeam.TabIndex = 0;
            this.btn_insertBeam.Text = "Insert Beam";
            this.btn_insertBeam.UseVisualStyleBackColor = true;
            this.btn_insertBeam.Click += new System.EventHandler(this.btn_insertBeam_Click);
            // 
            // btn_insertCol
            // 
            this.btn_insertCol.Location = new System.Drawing.Point(35, 24);
            this.btn_insertCol.Name = "btn_insertCol";
            this.btn_insertCol.Size = new System.Drawing.Size(279, 42);
            this.btn_insertCol.TabIndex = 1;
            this.btn_insertCol.Text = "Insert Column";
            this.btn_insertCol.UseVisualStyleBackColor = true;
            this.btn_insertCol.Click += new System.EventHandler(this.btn_insertCol_Click);
            // 
            // btn_Plate
            // 
            this.btn_Plate.Location = new System.Drawing.Point(35, 120);
            this.btn_Plate.Name = "btn_Plate";
            this.btn_Plate.Size = new System.Drawing.Size(279, 42);
            this.btn_Plate.TabIndex = 3;
            this.btn_Plate.Text = "Insert Plate";
            this.btn_Plate.UseVisualStyleBackColor = true;
            this.btn_Plate.Click += new System.EventHandler(this.btn_Plate_Click);
            // 
            // btn_Connection
            // 
            this.btn_Connection.Location = new System.Drawing.Point(35, 287);
            this.btn_Connection.Name = "btn_Connection";
            this.btn_Connection.Size = new System.Drawing.Size(279, 42);
            this.btn_Connection.TabIndex = 4;
            this.btn_Connection.Text = "Insert Connection";
            this.btn_Connection.UseVisualStyleBackColor = true;
            this.btn_Connection.Click += new System.EventHandler(this.btn_Connection_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 391);
            this.Controls.Add(this.btn_Connection);
            this.Controls.Add(this.btn_Plate);
            this.Controls.Add(this.btn_insertCol);
            this.Controls.Add(this.btn_insertBeam);
            this.Name = "Form1";
            this.Text = "ColToBeam Connection";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_insertBeam;
        private System.Windows.Forms.Button btn_insertCol;
        private System.Windows.Forms.Button btn_Plate;
        private System.Windows.Forms.Button btn_Connection;
    }
}

