namespace LabLogin;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private void btnLogin_Click(object sender, EventArgs e)
    {
        Employee employee = new Employee(tbUsername.Text, tbPassword.Text);
        if (employee.Login())
        {
            MessageBox.Show(this, "Login Berhasil, ID anda adalah " + employee.EmployeeID,
                "Hasil Login",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show(this, "Login Gagal. Periksa username dan password.",
                "Hasil Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
        this.Close();
    }
}
