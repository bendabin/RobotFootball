namespace ImageProcessingCapture
{
    partial class Image_Process
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
            this.pictureBoxVideo = new System.Windows.Forms.PictureBox();
            this.Stop_button = new System.Windows.Forms.Button();
            this.Start_button = new System.Windows.Forms.Button();
            this.pictureBoxMod = new System.Windows.Forms.PictureBox();
            this.Update_Butt = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVideo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMod)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxVideo
            // 
            this.pictureBoxVideo.Location = new System.Drawing.Point(23, 12);
            this.pictureBoxVideo.Name = "pictureBoxVideo";
            this.pictureBoxVideo.Size = new System.Drawing.Size(640, 480);
            this.pictureBoxVideo.TabIndex = 0;
            this.pictureBoxVideo.TabStop = false;
            // 
            // Stop_button
            // 
            this.Stop_button.Location = new System.Drawing.Point(429, 533);
            this.Stop_button.Name = "Stop_button";
            this.Stop_button.Size = new System.Drawing.Size(154, 49);
            this.Stop_button.TabIndex = 2;
            this.Stop_button.Text = "STOP VIDEO";
            this.Stop_button.UseVisualStyleBackColor = true;
            this.Stop_button.Click += new System.EventHandler(this.Stop_button_Click);
            // 
            // Start_button
            // 
            this.Start_button.Location = new System.Drawing.Point(23, 533);
            this.Start_button.Name = "Start_button";
            this.Start_button.Size = new System.Drawing.Size(154, 49);
            this.Start_button.TabIndex = 3;
            this.Start_button.Text = "START VIDEO";
            this.Start_button.UseVisualStyleBackColor = true;
            this.Start_button.Click += new System.EventHandler(this.Start_button_Click);
            // 
            // pictureBoxMod
            // 
            this.pictureBoxMod.Location = new System.Drawing.Point(679, 12);
            this.pictureBoxMod.Name = "pictureBoxMod";
            this.pictureBoxMod.Size = new System.Drawing.Size(640, 480);
            this.pictureBoxMod.TabIndex = 4;
            this.pictureBoxMod.TabStop = false;
            // 
            // Update_Butt
            // 
            this.Update_Butt.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Update_Butt.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Update_Butt.Location = new System.Drawing.Point(235, 533);
            this.Update_Butt.Name = "Update_Butt";
            this.Update_Butt.Size = new System.Drawing.Size(108, 49);
            this.Update_Butt.TabIndex = 5;
            this.Update_Butt.Text = "UPDATE IMAGE";
            this.Update_Butt.UseVisualStyleBackColor = false;
            this.Update_Butt.Click += new System.EventHandler(this.Update_Butt_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(809, 506);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(221, 25);
            this.label1.TabIndex = 6;
            this.label1.Text = "PROCESSED IMAGE";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(216, 505);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 25);
            this.label2.TabIndex = 7;
            this.label2.Text = "LIVE CAM";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(679, 548);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(640, 20);
            this.textBox1.TabIndex = 8;
            // 
            // Image_Process
            // 
            this.ClientSize = new System.Drawing.Size(1354, 634);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Update_Butt);
            this.Controls.Add(this.pictureBoxMod);
            this.Controls.Add(this.Start_button);
            this.Controls.Add(this.Stop_button);
            this.Controls.Add(this.pictureBoxVideo);
            this.Name = "Image_Process";
            this.Text = "Image Process";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVideo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMod)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxVideo;
        private System.Windows.Forms.Button Stop_button;
        private System.Windows.Forms.Button Start_button;
        private System.Windows.Forms.PictureBox pictureBoxMod;
        private System.Windows.Forms.Button Update_Butt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;




    }
}

