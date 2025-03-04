namespace Contra
{
    partial class OptionsForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
            this.MinBtnSm = new System.Windows.Forms.Button();
            this.ExitBtnSm = new System.Windows.Forms.Button();
            this.LangFilterCheckBox = new System.Windows.Forms.CheckBox();
            this.FogCheckBox = new System.Windows.Forms.CheckBox();
            this.resolutionComboBox = new System.Windows.Forms.ComboBox();
            this.labelResolution = new System.Windows.Forms.Label();
            this.resOkButton = new System.Windows.Forms.Button();
            this.toolTip3 = new System.Windows.Forms.ToolTip(this.components);
            this.HeatEffectsCheckBox = new System.Windows.Forms.CheckBox();
            this.camOkButton = new System.Windows.Forms.Button();
            this.camHeightLabel = new System.Windows.Forms.Label();
            this.WaterEffectsCheckBox = new System.Windows.Forms.CheckBox();
            this.DisableDynamicLODCheckBox = new System.Windows.Forms.CheckBox();
            this.ExtraAnimationsCheckBox = new System.Windows.Forms.CheckBox();
            this.ShowPropsCheckBox = new System.Windows.Forms.CheckBox();
            this.BehindBuildingsCheckBox = new System.Windows.Forms.CheckBox();
            this.Shadows3DCheckBox = new System.Windows.Forms.CheckBox();
            this.Shadows2DCheckBox = new System.Windows.Forms.CheckBox();
            this.CloudShadowsCheckBox = new System.Windows.Forms.CheckBox();
            this.ExtraGroundLightingCheckBox = new System.Windows.Forms.CheckBox();
            this.SmoothWaterBordersCheckBox = new System.Windows.Forms.CheckBox();
            this.HotkeyStyleGroupBox = new System.Windows.Forms.GroupBox();
            this.LegacyHotkeysRadioButton = new System.Windows.Forms.RadioButton();
            this.LeikezeHotkeysRadioButton = new System.Windows.Forms.RadioButton();
            this.camTrackBar = new Contra.TrackBar();
            this.HotkeyStyleGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MinBtnSm
            // 
            this.MinBtnSm.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.MinBtnSm.BackgroundImage = global::Contra.Properties.Resources._button_sm_min;
            resources.ApplyResources(this.MinBtnSm, "MinBtnSm");
            this.MinBtnSm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MinBtnSm.FlatAppearance.BorderSize = 0;
            this.MinBtnSm.Name = "MinBtnSm";
            this.MinBtnSm.UseVisualStyleBackColor = false;
            this.MinBtnSm.Click += new System.EventHandler(this.MinBtnSm_Click);
            this.MinBtnSm.MouseEnter += new System.EventHandler(this.MinBtnSm_MouseEnter);
            this.MinBtnSm.MouseLeave += new System.EventHandler(this.MinBtnSm_MouseLeave);
            // 
            // ExitBtnSm
            // 
            this.ExitBtnSm.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ExitBtnSm.BackgroundImage = global::Contra.Properties.Resources._button_sm_exit;
            resources.ApplyResources(this.ExitBtnSm, "ExitBtnSm");
            this.ExitBtnSm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ExitBtnSm.FlatAppearance.BorderSize = 0;
            this.ExitBtnSm.Name = "ExitBtnSm";
            this.ExitBtnSm.UseVisualStyleBackColor = false;
            this.ExitBtnSm.Click += new System.EventHandler(this.ExitBtnSm_Click);
            this.ExitBtnSm.MouseEnter += new System.EventHandler(this.ExitBtnSm_MouseEnter);
            this.ExitBtnSm.MouseLeave += new System.EventHandler(this.ExitBtnSm_MouseLeave);
            // 
            // LangFilterCheckBox
            // 
            resources.ApplyResources(this.LangFilterCheckBox, "LangFilterCheckBox");
            this.LangFilterCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.LangFilterCheckBox.Checked = true;
            this.LangFilterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.LangFilterCheckBox.ForeColor = System.Drawing.Color.White;
            this.LangFilterCheckBox.Name = "LangFilterCheckBox";
            this.LangFilterCheckBox.UseVisualStyleBackColor = false;
            this.LangFilterCheckBox.CheckedChanged += new System.EventHandler(this.LangFilterCheckBox_CheckedChanged);
            // 
            // FogCheckBox
            // 
            resources.ApplyResources(this.FogCheckBox, "FogCheckBox");
            this.FogCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.FogCheckBox.Checked = true;
            this.FogCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.FogCheckBox.ForeColor = System.Drawing.Color.White;
            this.FogCheckBox.Name = "FogCheckBox";
            this.FogCheckBox.UseVisualStyleBackColor = false;
            this.FogCheckBox.CheckedChanged += new System.EventHandler(this.FogCheckBox_CheckedChanged);
            // 
            // resolutionComboBox
            // 
            resources.ApplyResources(this.resolutionComboBox, "resolutionComboBox");
            this.resolutionComboBox.FormattingEnabled = true;
            this.resolutionComboBox.Name = "resolutionComboBox";
            // 
            // labelResolution
            // 
            resources.ApplyResources(this.labelResolution, "labelResolution");
            this.labelResolution.BackColor = System.Drawing.Color.Transparent;
            this.labelResolution.ForeColor = System.Drawing.Color.White;
            this.labelResolution.Name = "labelResolution";
            // 
            // resOkButton
            // 
            this.resOkButton.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.resOkButton, "resOkButton");
            this.resOkButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.resOkButton.FlatAppearance.BorderSize = 0;
            this.resOkButton.ForeColor = System.Drawing.Color.White;
            this.resOkButton.Name = "resOkButton";
            this.resOkButton.UseVisualStyleBackColor = false;
            this.resOkButton.Click += new System.EventHandler(this.resOkButton_Click);
            this.resOkButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.resOkButton_MouseDown);
            this.resOkButton.MouseLeave += new System.EventHandler(this.resOkButton_MouseLeave);
            // 
            // toolTip3
            // 
            this.toolTip3.AutoPopDelay = 5000;
            this.toolTip3.InitialDelay = 50;
            this.toolTip3.ReshowDelay = 100;
            // 
            // HeatEffectsCheckBox
            // 
            resources.ApplyResources(this.HeatEffectsCheckBox, "HeatEffectsCheckBox");
            this.HeatEffectsCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.HeatEffectsCheckBox.ForeColor = System.Drawing.Color.White;
            this.HeatEffectsCheckBox.Name = "HeatEffectsCheckBox";
            this.HeatEffectsCheckBox.UseVisualStyleBackColor = false;
            this.HeatEffectsCheckBox.CheckedChanged += new System.EventHandler(this.HeatEffectsCheckBox_CheckedChanged);
            this.HeatEffectsCheckBox.Click += new System.EventHandler(this.HeatEffectsCheckBox_Click);
            // 
            // camOkButton
            // 
            this.camOkButton.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.camOkButton, "camOkButton");
            this.camOkButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.camOkButton.FlatAppearance.BorderSize = 0;
            this.camOkButton.ForeColor = System.Drawing.Color.White;
            this.camOkButton.Name = "camOkButton";
            this.camOkButton.UseVisualStyleBackColor = false;
            this.camOkButton.Click += new System.EventHandler(this.camOkButton_Click);
            // 
            // camHeightLabel
            // 
            resources.ApplyResources(this.camHeightLabel, "camHeightLabel");
            this.camHeightLabel.BackColor = System.Drawing.Color.Transparent;
            this.camHeightLabel.ForeColor = System.Drawing.Color.White;
            this.camHeightLabel.Name = "camHeightLabel";
            // 
            // WaterEffectsCheckBox
            // 
            resources.ApplyResources(this.WaterEffectsCheckBox, "WaterEffectsCheckBox");
            this.WaterEffectsCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.WaterEffectsCheckBox.Checked = true;
            this.WaterEffectsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.WaterEffectsCheckBox.ForeColor = System.Drawing.Color.White;
            this.WaterEffectsCheckBox.Name = "WaterEffectsCheckBox";
            this.WaterEffectsCheckBox.UseVisualStyleBackColor = false;
            this.WaterEffectsCheckBox.CheckedChanged += new System.EventHandler(this.WaterEffectsCheckBox_CheckedChanged);
            // 
            // DisableDynamicLODCheckBox
            // 
            resources.ApplyResources(this.DisableDynamicLODCheckBox, "DisableDynamicLODCheckBox");
            this.DisableDynamicLODCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.DisableDynamicLODCheckBox.ForeColor = System.Drawing.Color.White;
            this.DisableDynamicLODCheckBox.Name = "DisableDynamicLODCheckBox";
            this.DisableDynamicLODCheckBox.UseVisualStyleBackColor = false;
            this.DisableDynamicLODCheckBox.CheckedChanged += new System.EventHandler(this.DisableDynamicLODCheckBox_CheckedChanged);
            // 
            // ExtraAnimationsCheckBox
            // 
            resources.ApplyResources(this.ExtraAnimationsCheckBox, "ExtraAnimationsCheckBox");
            this.ExtraAnimationsCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.ExtraAnimationsCheckBox.ForeColor = System.Drawing.Color.White;
            this.ExtraAnimationsCheckBox.Name = "ExtraAnimationsCheckBox";
            this.ExtraAnimationsCheckBox.UseVisualStyleBackColor = false;
            this.ExtraAnimationsCheckBox.CheckedChanged += new System.EventHandler(this.ExtraAnimationsCheckBox_CheckedChanged);
            // 
            // ShowPropsCheckBox
            // 
            resources.ApplyResources(this.ShowPropsCheckBox, "ShowPropsCheckBox");
            this.ShowPropsCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.ShowPropsCheckBox.ForeColor = System.Drawing.Color.White;
            this.ShowPropsCheckBox.Name = "ShowPropsCheckBox";
            this.ShowPropsCheckBox.UseVisualStyleBackColor = false;
            this.ShowPropsCheckBox.CheckedChanged += new System.EventHandler(this.ShowPropsCheckBox_CheckedChanged);
            // 
            // BehindBuildingsCheckBox
            // 
            resources.ApplyResources(this.BehindBuildingsCheckBox, "BehindBuildingsCheckBox");
            this.BehindBuildingsCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.BehindBuildingsCheckBox.ForeColor = System.Drawing.Color.White;
            this.BehindBuildingsCheckBox.Name = "BehindBuildingsCheckBox";
            this.BehindBuildingsCheckBox.UseVisualStyleBackColor = false;
            this.BehindBuildingsCheckBox.CheckedChanged += new System.EventHandler(this.BehindBuildingsCheckBox_CheckedChanged);
            // 
            // Shadows3DCheckBox
            // 
            resources.ApplyResources(this.Shadows3DCheckBox, "Shadows3DCheckBox");
            this.Shadows3DCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.Shadows3DCheckBox.ForeColor = System.Drawing.Color.White;
            this.Shadows3DCheckBox.Name = "Shadows3DCheckBox";
            this.Shadows3DCheckBox.UseVisualStyleBackColor = false;
            this.Shadows3DCheckBox.CheckedChanged += new System.EventHandler(this.Shadows3DCheckBox_CheckedChanged);
            // 
            // Shadows2DCheckBox
            // 
            resources.ApplyResources(this.Shadows2DCheckBox, "Shadows2DCheckBox");
            this.Shadows2DCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.Shadows2DCheckBox.ForeColor = System.Drawing.Color.White;
            this.Shadows2DCheckBox.Name = "Shadows2DCheckBox";
            this.Shadows2DCheckBox.UseVisualStyleBackColor = false;
            this.Shadows2DCheckBox.CheckedChanged += new System.EventHandler(this.Shadows2DCheckBox_CheckedChanged);
            // 
            // CloudShadowsCheckBox
            // 
            resources.ApplyResources(this.CloudShadowsCheckBox, "CloudShadowsCheckBox");
            this.CloudShadowsCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.CloudShadowsCheckBox.ForeColor = System.Drawing.Color.White;
            this.CloudShadowsCheckBox.Name = "CloudShadowsCheckBox";
            this.CloudShadowsCheckBox.UseVisualStyleBackColor = false;
            this.CloudShadowsCheckBox.CheckedChanged += new System.EventHandler(this.CloudShadowsCheckBox_CheckedChanged);
            // 
            // ExtraGroundLightingCheckBox
            // 
            resources.ApplyResources(this.ExtraGroundLightingCheckBox, "ExtraGroundLightingCheckBox");
            this.ExtraGroundLightingCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.ExtraGroundLightingCheckBox.ForeColor = System.Drawing.Color.White;
            this.ExtraGroundLightingCheckBox.Name = "ExtraGroundLightingCheckBox";
            this.ExtraGroundLightingCheckBox.UseVisualStyleBackColor = false;
            this.ExtraGroundLightingCheckBox.CheckedChanged += new System.EventHandler(this.ExtraGroundLightingCheckBox_CheckedChanged);
            // 
            // SmoothWaterBordersCheckBox
            // 
            resources.ApplyResources(this.SmoothWaterBordersCheckBox, "SmoothWaterBordersCheckBox");
            this.SmoothWaterBordersCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.SmoothWaterBordersCheckBox.ForeColor = System.Drawing.Color.White;
            this.SmoothWaterBordersCheckBox.Name = "SmoothWaterBordersCheckBox";
            this.SmoothWaterBordersCheckBox.UseVisualStyleBackColor = false;
            this.SmoothWaterBordersCheckBox.CheckedChanged += new System.EventHandler(this.SmoothWaterBordersCheckBox_CheckedChanged);
            // 
            // HotkeyStyleGroupBox
            // 
            this.HotkeyStyleGroupBox.BackColor = System.Drawing.Color.Transparent;
            this.HotkeyStyleGroupBox.Controls.Add(this.LegacyHotkeysRadioButton);
            this.HotkeyStyleGroupBox.Controls.Add(this.LeikezeHotkeysRadioButton);
            resources.ApplyResources(this.HotkeyStyleGroupBox, "HotkeyStyleGroupBox");
            this.HotkeyStyleGroupBox.ForeColor = System.Drawing.Color.White;
            this.HotkeyStyleGroupBox.Name = "HotkeyStyleGroupBox";
            this.HotkeyStyleGroupBox.TabStop = false;
            // 
            // LegacyHotkeysRadioButton
            // 
            resources.ApplyResources(this.LegacyHotkeysRadioButton, "LegacyHotkeysRadioButton");
            this.LegacyHotkeysRadioButton.Name = "LegacyHotkeysRadioButton";
            this.LegacyHotkeysRadioButton.UseVisualStyleBackColor = true;
            this.LegacyHotkeysRadioButton.CheckedChanged += new System.EventHandler(this.LegacyHotkeysRadioButton_CheckedChanged);
            // 
            // LeikezeHotkeysRadioButton
            // 
            resources.ApplyResources(this.LeikezeHotkeysRadioButton, "LeikezeHotkeysRadioButton");
            this.LeikezeHotkeysRadioButton.Checked = true;
            this.LeikezeHotkeysRadioButton.Name = "LeikezeHotkeysRadioButton";
            this.LeikezeHotkeysRadioButton.TabStop = true;
            this.LeikezeHotkeysRadioButton.UseVisualStyleBackColor = true;
            this.LeikezeHotkeysRadioButton.CheckedChanged += new System.EventHandler(this.LeikezeHotkeysRadioButton_CheckedChanged);
            // 
            // camTrackBar
            // 
            this.camTrackBar.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.camTrackBar, "camTrackBar");
            this.camTrackBar.Maximum = 750;
            this.camTrackBar.Minimum = 392;
            this.camTrackBar.Name = "camTrackBar";
            this.camTrackBar.TabStop = true;
            this.camTrackBar.Value = 392;
            this.camTrackBar.Scroll += new System.EventHandler(this.camTrackBar_Scroll);
            // 
            // OptionsForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::Contra.Properties.Resources._bg_options;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.HotkeyStyleGroupBox);
            this.Controls.Add(this.Shadows3DCheckBox);
            this.Controls.Add(this.Shadows2DCheckBox);
            this.Controls.Add(this.CloudShadowsCheckBox);
            this.Controls.Add(this.ExtraGroundLightingCheckBox);
            this.Controls.Add(this.SmoothWaterBordersCheckBox);
            this.Controls.Add(this.BehindBuildingsCheckBox);
            this.Controls.Add(this.ShowPropsCheckBox);
            this.Controls.Add(this.ExtraAnimationsCheckBox);
            this.Controls.Add(this.DisableDynamicLODCheckBox);
            this.Controls.Add(this.WaterEffectsCheckBox);
            this.Controls.Add(this.camHeightLabel);
            this.Controls.Add(this.camTrackBar);
            this.Controls.Add(this.camOkButton);
            this.Controls.Add(this.HeatEffectsCheckBox);
            this.Controls.Add(this.resOkButton);
            this.Controls.Add(this.labelResolution);
            this.Controls.Add(this.resolutionComboBox);
            this.Controls.Add(this.LangFilterCheckBox);
            this.Controls.Add(this.FogCheckBox);
            this.Controls.Add(this.MinBtnSm);
            this.Controls.Add(this.ExitBtnSm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "OptionsForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.HotkeyStyleGroupBox.ResumeLayout(false);
            this.HotkeyStyleGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button MinBtnSm;
        private System.Windows.Forms.Button ExitBtnSm;
        private System.Windows.Forms.CheckBox LangFilterCheckBox;
        private System.Windows.Forms.CheckBox FogCheckBox;
        private System.Windows.Forms.ComboBox resolutionComboBox;
        private System.Windows.Forms.Label labelResolution;
        private System.Windows.Forms.Button resOkButton;
        private System.Windows.Forms.ToolTip toolTip3;
        private System.Windows.Forms.CheckBox HeatEffectsCheckBox;
        private System.Windows.Forms.Button camOkButton;
        private TrackBar camTrackBar;
        private System.Windows.Forms.Label camHeightLabel;
        private System.Windows.Forms.CheckBox WaterEffectsCheckBox;
        private System.Windows.Forms.CheckBox DisableDynamicLODCheckBox;
        private System.Windows.Forms.CheckBox ExtraAnimationsCheckBox;
        private System.Windows.Forms.CheckBox ShowPropsCheckBox;
        private System.Windows.Forms.CheckBox BehindBuildingsCheckBox;
        private System.Windows.Forms.CheckBox Shadows3DCheckBox;
        private System.Windows.Forms.CheckBox Shadows2DCheckBox;
        private System.Windows.Forms.CheckBox CloudShadowsCheckBox;
        private System.Windows.Forms.CheckBox ExtraGroundLightingCheckBox;
        private System.Windows.Forms.CheckBox SmoothWaterBordersCheckBox;
        private System.Windows.Forms.GroupBox HotkeyStyleGroupBox;
        private System.Windows.Forms.RadioButton LegacyHotkeysRadioButton;
        private System.Windows.Forms.RadioButton LeikezeHotkeysRadioButton;
    }
}