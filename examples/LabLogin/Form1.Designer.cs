namespace LabLogin;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private Label lblTitle;
    private Label lblSubtitle;
    private Label lblUsername;
    private Label lblPassword;
    private TextBox tbUsername;
    private TextBox tbPassword;
    private Button btnLogin;
    private Button btnClose;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    private void InitializeComponent()
    {
        lblTitle = new Label();
        lblSubtitle = new Label();
        lblUsername = new Label();
        lblPassword = new Label();
        tbUsername = new TextBox();
        tbPassword = new TextBox();
        btnLogin = new Button();
        btnClose = new Button();
        SuspendLayout();
        // lblTitle
        lblTitle.Name = "lblTitle";
        lblTitle.Location = new Point(32, 25);
        lblTitle.Size = new Size(420, 45);
        lblTitle.Text = "Employee Login";
        lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(22, 105, 76);
        // lblSubtitle
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Location = new Point(35, 77);
        lblSubtitle.Size = new Size(430, 26);
        lblSubtitle.Text = "Modul 7  |  User Interface dan Constructor";
        lblSubtitle.ForeColor = Color.FromArgb(91, 105, 96);
        // lblUsername
        lblUsername.Name = "lblUsername";
        lblUsername.Location = new Point(35, 133);
        lblUsername.Size = new Size(120, 28);
        lblUsername.Text = "Username :";
        // lblPassword
        lblPassword.Name = "lblPassword";
        lblPassword.Location = new Point(35, 184);
        lblPassword.Size = new Size(120, 28);
        lblPassword.Text = "Password :";
        // tbUsername
        tbUsername.Name = "tbUsername";
        tbUsername.Location = new Point(160, 129);
        tbUsername.Size = new Size(287, 29);
        tbUsername.TabIndex = 0;
        // tbPassword
        tbPassword.Name = "tbPassword";
        tbPassword.Location = new Point(160, 180);
        tbPassword.Size = new Size(287, 29);
        tbPassword.TabIndex = 1;
        tbPassword.UseSystemPasswordChar = true;
        // btnLogin
        btnLogin.Name = "btnLogin";
        btnLogin.Location = new Point(160, 243);
        btnLogin.Size = new Size(138, 44);
        btnLogin.Text = "Login";
        btnLogin.TabIndex = 2;
        btnLogin.UseVisualStyleBackColor = false;
        btnLogin.BackColor = Color.FromArgb(22, 105, 76);
        btnLogin.ForeColor = Color.White;
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.Click += btnLogin_Click;
        // btnClose
        btnClose.Name = "btnClose";
        btnClose.Location = new Point(310, 243);
        btnClose.Size = new Size(137, 44);
        btnClose.Text = "Close";
        btnClose.TabIndex = 3;
        btnClose.UseVisualStyleBackColor = true;
        btnClose.Click += btnClose_Click;
        AutoScaleDimensions = new SizeF(9F, 21F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(248, 250, 248);
        ClientSize = new Size(490, 328);
        Controls.Add(lblTitle);
        Controls.Add(lblSubtitle);
        Controls.Add(lblUsername);
        Controls.Add(lblPassword);
        Controls.Add(tbUsername);
        Controls.Add(tbPassword);
        Controls.Add(btnLogin);
        Controls.Add(btnClose);
        Font = new Font("Segoe UI", 12F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Lab 7.1 dan 7.2 - Form Login";
        AcceptButton = btnLogin;
        CancelButton = btnClose;
        ResumeLayout(false);
        PerformLayout();
    }
    #endregion
}
