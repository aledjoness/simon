namespace Simon
{
    partial class SimonWindow
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
            this.left = new System.Windows.Forms.Button();
            this.right = new System.Windows.Forms.Button();
            this.top = new System.Windows.Forms.Button();
            this.bottom = new System.Windows.Forms.Button();
            this.middle = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // left
            // 
            this.left.BackColor = System.Drawing.Color.Firebrick;
            this.left.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.left.Location = new System.Drawing.Point(31, 77);
            this.left.Name = "left";
            this.left.Size = new System.Drawing.Size(45, 125);
            this.left.TabIndex = 0;
            this.left.UseVisualStyleBackColor = false;
            this.left.Click += new System.EventHandler(this.left_Click);
            // 
            // right
            // 
            this.right.BackColor = System.Drawing.Color.RoyalBlue;
            this.right.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.right.Location = new System.Drawing.Point(217, 77);
            this.right.Name = "right";
            this.right.Size = new System.Drawing.Size(45, 125);
            this.right.TabIndex = 1;
            this.right.UseVisualStyleBackColor = false;
            this.right.Click += new System.EventHandler(this.right_Click);
            // 
            // top
            // 
            this.top.BackColor = System.Drawing.Color.Goldenrod;
            this.top.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.top.Location = new System.Drawing.Point(84, 24);
            this.top.Name = "top";
            this.top.Size = new System.Drawing.Size(125, 45);
            this.top.TabIndex = 2;
            this.top.UseVisualStyleBackColor = false;
            this.top.Click += new System.EventHandler(this.top_Click);
            // 
            // bottom
            // 
            this.bottom.BackColor = System.Drawing.Color.Green;
            this.bottom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bottom.Location = new System.Drawing.Point(84, 209);
            this.bottom.Name = "bottom";
            this.bottom.Size = new System.Drawing.Size(125, 45);
            this.bottom.TabIndex = 3;
            this.bottom.UseVisualStyleBackColor = false;
            this.bottom.Click += new System.EventHandler(this.bottom_Click);
            // 
            // middle
            // 
            this.middle.BackColor = System.Drawing.Color.DarkMagenta;
            this.middle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.middle.Location = new System.Drawing.Point(124, 117);
            this.middle.Name = "middle";
            this.middle.Size = new System.Drawing.Size(45, 45);
            this.middle.TabIndex = 4;
            this.middle.UseVisualStyleBackColor = false;
            // 
            // SimonWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Desktop;
            this.ClientSize = new System.Drawing.Size(293, 278);
            this.Controls.Add(this.middle);
            this.Controls.Add(this.bottom);
            this.Controls.Add(this.top);
            this.Controls.Add(this.right);
            this.Controls.Add(this.left);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(309, 316);
            this.MinimumSize = new System.Drawing.Size(309, 316);
            this.Name = "SimonWindow";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Simon - High Score: 0";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button left;
        private System.Windows.Forms.Button right;
        private System.Windows.Forms.Button top;
        private System.Windows.Forms.Button bottom;
        private System.Windows.Forms.Button middle;
    }
}

