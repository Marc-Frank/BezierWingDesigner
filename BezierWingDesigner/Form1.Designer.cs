namespace BezierWingDesigner
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
            editor3d1 = new Plot3D.Editor3D();
            SuspendLayout();
            // 
            // editor3d1
            // 
            editor3d1.AxisX_Color = Color.DarkBlue;
            editor3d1.AxisY_Color = Color.DarkGreen;
            editor3d1.AxisZ_Color = Color.DarkRed;
            editor3d1.BackColor = Color.White;
            editor3d1.BorderColorFocus = Color.FromArgb(51, 153, 255);
            editor3d1.BorderColorNormal = Color.FromArgb(180, 180, 180);
            editor3d1.Location = new Point(0, 0);
            editor3d1.MouseControl = Plot3D.Editor3D.eMouseCtrl.L_Theta_R_Phi;
            editor3d1.Name = "editor3d1";
            editor3d1.Normalize = Plot3D.Editor3D.eNormalize.Separate;
            editor3d1.Raster = Plot3D.Editor3D.eRaster.Labels;
            editor3d1.Size = new Size(800, 451);
            editor3d1.TabIndex = 0;
            editor3d1.TooltipMode = Plot3D.Editor3D.eTooltip.UserText | Plot3D.Editor3D.eTooltip.Coord;
            editor3d1.TopLegendColor = Color.FromArgb(200, 200, 150);
            editor3d1.Load += editor3d1_Load;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(editor3d1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            SizeChanged += Form1_SizeChanged;
            ResumeLayout(false);
        }

        #endregion

        private Plot3D.Editor3D editor3d1;
    }
}