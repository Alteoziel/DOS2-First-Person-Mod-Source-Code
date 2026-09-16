namespace FirstPersonTool;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.btnAttach = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.chkFpEnabled = new System.Windows.Forms.CheckBox();
        this.lblController = new System.Windows.Forms.Label();
        this.lblAngles = new System.Windows.Forms.Label();
        this.cmbWrap = new System.Windows.Forms.ComboBox();
        this.chkSwapYawPitch = new System.Windows.Forms.CheckBox();
        this.chkFreezePitch = new System.Windows.Forms.CheckBox();
        this.btnProbe = new System.Windows.Forms.Button();
        this.lblDiag = new System.Windows.Forms.Label();
        this.btnSweepPivot = new System.Windows.Forms.Button();
        this.lblHeight = new System.Windows.Forms.Label();
        this.SuspendLayout();
        this.btnAttach.Location = new System.Drawing.Point(16, 16);
        this.btnAttach.Name = "btnAttach";
        this.btnAttach.Size = new System.Drawing.Size(140, 32);
        this.btnAttach.TabIndex = 0;
        this.btnAttach.Text = "Attach (EoCApp.exe)";
        this.btnAttach.UseVisualStyleBackColor = true;
        this.btnAttach.Click += new System.EventHandler(this.btnAttach_Click);
        this.lblStatus.AutoSize = true;
        this.lblStatus.Location = new System.Drawing.Point(16, 60);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(53, 20);
        this.lblStatus.TabIndex = 1;
        this.lblStatus.Text = "Status:";
        this.chkFpEnabled.AutoSize = true;
        this.chkFpEnabled.Enabled = false;
        this.chkFpEnabled.Location = new System.Drawing.Point(16, 92);
        this.chkFpEnabled.Name = "chkFpEnabled";
        this.chkFpEnabled.Size = new System.Drawing.Size(162, 24);
        this.chkFpEnabled.TabIndex = 2;
        this.chkFpEnabled.Text = "First person enabled";
        this.lblController.AutoSize = true;
        this.lblController.Location = new System.Drawing.Point(16, 126);
        this.lblController.Name = "lblController";
        this.lblController.Size = new System.Drawing.Size(120, 20);
        this.lblController.TabIndex = 3;
        this.lblController.Text = "Controller: (none)";
        this.lblAngles.AutoSize = true;
        this.lblAngles.Location = new System.Drawing.Point(16, 156);
        this.lblAngles.Name = "lblAngles";
        this.lblAngles.Size = new System.Drawing.Size(158, 20);
        this.lblAngles.TabIndex = 4;
        this.lblAngles.Text = "Angles: (not available)";
        this.cmbWrap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbWrap.Location = new System.Drawing.Point(280, 16);
        this.cmbWrap.Name = "cmbWrap";
        this.cmbWrap.Size = new System.Drawing.Size(170, 28);
        this.cmbWrap.TabIndex = 5;
        this.cmbWrap.SelectedIndexChanged += new System.EventHandler(this.cmbWrap_SelectedIndexChanged);
        this.chkSwapYawPitch.AutoSize = true;
        this.chkSwapYawPitch.Location = new System.Drawing.Point(280, 56);
        this.chkSwapYawPitch.Name = "chkSwapYawPitch";
        this.chkSwapYawPitch.Size = new System.Drawing.Size(129, 24);
        this.chkSwapYawPitch.TabIndex = 6;
        this.chkSwapYawPitch.Text = "Swap yaw/pitch";
        this.chkSwapYawPitch.CheckedChanged += new System.EventHandler(this.chkSwapYawPitch_CheckedChanged);
        this.chkFreezePitch.AutoSize = true;
        this.chkFreezePitch.Location = new System.Drawing.Point(280, 86);
        this.chkFreezePitch.Name = "chkFreezePitch";
        this.chkFreezePitch.Size = new System.Drawing.Size(108, 24);
        this.chkFreezePitch.TabIndex = 7;
        this.chkFreezePitch.Text = "Freeze pitch";
        this.chkFreezePitch.CheckedChanged += new System.EventHandler(this.chkFreezePitch_CheckedChanged);
        this.btnProbe.Location = new System.Drawing.Point(470, 16);
        this.btnProbe.Name = "btnProbe";
        this.btnProbe.Size = new System.Drawing.Size(150, 32);
        this.btnProbe.TabIndex = 8;
        this.btnProbe.Text = "Probe (min/max)";
        this.btnProbe.Click += new System.EventHandler(this.btnProbe_Click);
        this.lblDiag.Location = new System.Drawing.Point(280, 120);
        this.lblDiag.Name = "lblDiag";
        this.lblDiag.Size = new System.Drawing.Size(340, 56);
        this.lblDiag.TabIndex = 9;
        this.lblDiag.Text = "Diag: (not available)";
        this.btnSweepPivot.Location = new System.Drawing.Point(16, 192);
        this.btnSweepPivot.Name = "btnSweepPivot";
        this.btnSweepPivot.Size = new System.Drawing.Size(200, 36);
        this.btnSweepPivot.TabIndex = 10;
        this.btnSweepPivot.Text = "Set FP height (sweep)";
        this.btnSweepPivot.Click += new System.EventHandler(this.btnSweepPivot_Click);
        this.lblHeight.Location = new System.Drawing.Point(16, 236);
        this.lblHeight.Name = "lblHeight";
        this.lblHeight.Size = new System.Drawing.Size(600, 48);
        this.lblHeight.TabIndex = 11;
        this.lblHeight.Text = "Press Set FP height once. Then LB+B for first person (off = normal camera height).";
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(640, 290);
        this.Controls.Add(this.lblHeight);
        this.Controls.Add(this.btnSweepPivot);
        this.Controls.Add(this.lblDiag);
        this.Controls.Add(this.btnProbe);
        this.Controls.Add(this.chkFreezePitch);
        this.Controls.Add(this.chkSwapYawPitch);
        this.Controls.Add(this.cmbWrap);
        this.Controls.Add(this.lblAngles);
        this.Controls.Add(this.lblController);
        this.Controls.Add(this.chkFpEnabled);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnAttach);
        this.Name = "Form1";
        this.Text = "DOS2 First Person Tool";
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private Button btnAttach;
    private Label lblStatus;
    private CheckBox chkFpEnabled;
    private Label lblController;
    private Label lblAngles;
    private ComboBox cmbWrap;
    private CheckBox chkSwapYawPitch;
    private CheckBox chkFreezePitch;
    private Button btnProbe;
    private Label lblDiag;
    private Button btnSweepPivot;
    private Label lblHeight;
}
