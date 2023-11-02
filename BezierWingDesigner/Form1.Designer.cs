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
            tabControl1 = new TabControl();
            tabPlanformView = new TabPage();
            formsPlot1 = new ScottPlot.FormsPlot();
            tab3DView = new TabPage();
            Editor3d = new Plot3D.Editor3D();
            tabControl1.SuspendLayout();
            tabPlanformView.SuspendLayout();
            tab3DView.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPlanformView);
            tabControl1.Controls.Add(tab3DView);
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1139, 609);
            tabControl1.TabIndex = 1;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            // 
            // tabPlanformView
            // 
            tabPlanformView.BackColor = Color.Transparent;
            tabPlanformView.Controls.Add(formsPlot1);
            tabPlanformView.Location = new Point(4, 24);
            tabPlanformView.Name = "tabPlanformView";
            tabPlanformView.Padding = new Padding(3);
            tabPlanformView.Size = new Size(1131, 581);
            tabPlanformView.TabIndex = 0;
            tabPlanformView.Text = "planform view";
            // 
            // formsPlot1
            // 
            formsPlot1.Location = new Point(0, 0);
            formsPlot1.Margin = new Padding(4, 3, 4, 3);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(1131, 581);
            formsPlot1.TabIndex = 2;
            formsPlot1.Load += formsPlot1_Load;
            // 
            // tab3DView
            // 
            tab3DView.BackColor = Color.Transparent;
            tab3DView.Controls.Add(Editor3d);
            tab3DView.Location = new Point(4, 24);
            tab3DView.Name = "tab3DView";
            tab3DView.Padding = new Padding(3);
            tab3DView.Size = new Size(1131, 581);
            tab3DView.TabIndex = 1;
            tab3DView.Text = "3D View";
            // 
            // Editor3d
            // 
            Editor3d.AxisX_Color = Color.DarkBlue;
            Editor3d.AxisY_Color = Color.DarkGreen;
            Editor3d.AxisZ_Color = Color.DarkRed;
            Editor3d.BackColor = Color.White;
            Editor3d.BorderColorFocus = Color.FromArgb(51, 153, 255);
            Editor3d.BorderColorNormal = Color.FromArgb(180, 180, 180);
            Editor3d.Location = new Point(0, 0);
            Editor3d.MouseControl = Plot3D.Editor3D.eMouseCtrl.L_Theta_R_Phi;
            Editor3d.Name = "Editor3d";
            Editor3d.Normalize = Plot3D.Editor3D.eNormalize.Separate;
            Editor3d.Raster = Plot3D.Editor3D.eRaster.Labels;
            Editor3d.Size = new Size(1131, 581);
            Editor3d.TabIndex = 1;
            Editor3d.TooltipMode = Plot3D.Editor3D.eTooltip.UserText | Plot3D.Editor3D.eTooltip.Coord;
            Editor3d.TopLegendColor = Color.FromArgb(200, 200, 150);
            Editor3d.Load += Editor3d_Load;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1139, 611);
            Controls.Add(tabControl1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            SizeChanged += Form1_SizeChanged;
            tabControl1.ResumeLayout(false);
            tabPlanformView.ResumeLayout(false);
            tab3DView.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TabControl tabControl1;
        private TabPage tabPlanformView;
        private TabPage tab3DView;
        private Plot3D.Editor3D Editor3d;
        private ScottPlot.FormsPlot formsPlot1;
    }
}