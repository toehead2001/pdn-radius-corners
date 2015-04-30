using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using PaintDotNet.Effects;
using PaintDotNet;

namespace RadiusFillCorners
{
    public class EffectPluginConfigDialog : PaintDotNet.Effects.EffectConfigDialog
    {
        private Button buttonOK;
        private NumericUpDown numRadius;
        private Label label1;
        private Button buttonCancel;
        private Button btnPreview;
        private Panel panel;
        private GroupBox groupBox1;
        private RadioButton radSecondary;
        private RadioButton radPrimary;
        private RadioButton radTransparent;
        private Panel pnlPrimary;
        private Panel pnlSecondary;
        
        private int radius = 50;
        private FillType fillType = 0;

        public EffectPluginConfigDialog(Color primary, Color secondary)
        {
            InitializeComponent();
            this.UpdateColorPanels(primary, secondary);
        }

        protected override void InitialInitToken()
        {
            theEffectToken = new EffectPluginConfigToken();
        }

        protected override void InitTokenFromDialog()
        {
            //  set token values
            ((EffectPluginConfigToken)EffectToken).radius = this.radius;
            ((EffectPluginConfigToken)EffectToken).fillType = this.fillType;
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            // initialize effect token
            EffectPluginConfigToken token = (EffectPluginConfigToken)effectToken;
            
            // set form's values from token
            this.radius = token.radius;
            this.fillType = token.fillType;

            // update the form's radius control value
            this.numRadius.Value = this.radius;
               
            // select the current filltype
            this.SelectFillType();
        }

        private void UpdateColorPanels(Color primary, Color secondary)
        {
            // set the color of the panels' background
            this.pnlPrimary.BackColor = primary;
            this.pnlSecondary.BackColor = secondary;
        }

        private void InitializeComponent()
        {
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.numRadius = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnPreview = new System.Windows.Forms.Button();
            this.panel = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pnlSecondary = new System.Windows.Forms.Panel();
            this.pnlPrimary = new System.Windows.Forms.Panel();
            this.radSecondary = new System.Windows.Forms.RadioButton();
            this.radPrimary = new System.Windows.Forms.RadioButton();
            this.radTransparent = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.numRadius)).BeginInit();
            this.panel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(275, 16);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(194, 16);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // numRadius
            // 
            this.numRadius.Location = new System.Drawing.Point(263, 23);
            this.numRadius.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numRadius.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRadius.Name = "numRadius";
            this.numRadius.Size = new System.Drawing.Size(64, 20);
            this.numRadius.TabIndex = 0;
            this.numRadius.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numRadius.ValueChanged += new System.EventHandler(this.numRadius_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(214, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Radius:";
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(252, 89);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(75, 23);
            this.btnPreview.TabIndex = 2;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // panel
            // 
            this.panel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel.Controls.Add(this.buttonOK);
            this.panel.Controls.Add(this.buttonCancel);
            this.panel.Location = new System.Drawing.Point(-12, 142);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(376, 88);
            this.panel.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pnlSecondary);
            this.groupBox1.Controls.Add(this.pnlPrimary);
            this.groupBox1.Controls.Add(this.radSecondary);
            this.groupBox1.Controls.Add(this.radPrimary);
            this.groupBox1.Controls.Add(this.radTransparent);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(196, 111);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Fill Type";
            // 
            // pnlSecondary
            // 
            this.pnlSecondary.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pnlSecondary.Location = new System.Drawing.Point(127, 77);
            this.pnlSecondary.Name = "pnlSecondary";
            this.pnlSecondary.Size = new System.Drawing.Size(40, 17);
            this.pnlSecondary.TabIndex = 3;
            // 
            // pnlPrimary
            // 
            this.pnlPrimary.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pnlPrimary.Location = new System.Drawing.Point(127, 53);
            this.pnlPrimary.Name = "pnlPrimary";
            this.pnlPrimary.Size = new System.Drawing.Size(40, 17);
            this.pnlPrimary.TabIndex = 3;
            // 
            // radSecondary
            // 
            this.radSecondary.AutoSize = true;
            this.radSecondary.Location = new System.Drawing.Point(18, 77);
            this.radSecondary.Name = "radSecondary";
            this.radSecondary.Size = new System.Drawing.Size(103, 17);
            this.radSecondary.TabIndex = 2;
            this.radSecondary.Text = "Secondary Color";
            this.radSecondary.UseVisualStyleBackColor = true;
            this.radSecondary.CheckedChanged += new System.EventHandler(this.radSecondary_CheckedChanged);
            // 
            // radPrimary
            // 
            this.radPrimary.AutoSize = true;
            this.radPrimary.Location = new System.Drawing.Point(18, 53);
            this.radPrimary.Name = "radPrimary";
            this.radPrimary.Size = new System.Drawing.Size(86, 17);
            this.radPrimary.TabIndex = 1;
            this.radPrimary.Text = "Primary Color";
            this.radPrimary.UseVisualStyleBackColor = true;
            this.radPrimary.CheckedChanged += new System.EventHandler(this.radPrimary_CheckedChanged);
            // 
            // radTransparent
            // 
            this.radTransparent.AutoSize = true;
            this.radTransparent.Checked = true;
            this.radTransparent.Location = new System.Drawing.Point(18, 29);
            this.radTransparent.Name = "radTransparent";
            this.radTransparent.Size = new System.Drawing.Size(82, 17);
            this.radTransparent.TabIndex = 0;
            this.radTransparent.TabStop = true;
            this.radTransparent.Text = "Transparent";
            this.radTransparent.UseVisualStyleBackColor = true;
            this.radTransparent.CheckedChanged += new System.EventHandler(this.radTransparent_CheckedChanged);
            // 
            // EffectPluginConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.ClientSize = new System.Drawing.Size(344, 188);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numRadius);
            this.Controls.Add(this.panel);
            this.Name = "EffectPluginConfigDialog";
            this.Text = "Radius Fill Corners";
            this.Controls.SetChildIndex(this.panel, 0);
            this.Controls.SetChildIndex(this.numRadius, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.btnPreview, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            ((System.ComponentModel.ISupportInitialize)(this.numRadius)).EndInit();
            this.panel.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // update the token's values
            FinishTokenUpdate();

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void numRadius_ValueChanged(object sender, EventArgs e)
        {
            this.radius = (int)numRadius.Value;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            FinishTokenUpdate();
        }

        private void SelectFillType()
        {
            switch (this.fillType)
            {
                case FillType.Primary:
                    this.radPrimary.Checked = true;
                    break;
                case FillType.Secondary:
                    this.radSecondary.Checked = true;
                    break;
                default:
                    this.radTransparent.Checked = true;
                    break;
            }
        }

        private void radTransparent_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radTransparent.Checked)
            {
                if (this.fillType != FillType.Transparent) this.fillType = FillType.Transparent;
            }
        }

        private void radPrimary_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radPrimary.Checked)
            {
                if (this.fillType != FillType.Primary) this.fillType = FillType.Primary;
            }
        }

        private void radSecondary_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radSecondary.Checked)
            {
                if (this.fillType != FillType.Secondary) this.fillType = FillType.Secondary;
            }
        }
    }
}