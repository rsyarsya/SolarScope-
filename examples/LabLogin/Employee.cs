namespace LabLogin;

public class Employee
{
    public int EmployeeID { get; private set; }
    public string LoginName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public Employee()
    {
    }

    public Employee(string loginName, string password)
    {
        LoginName = loginName;
        Password = password;
    }

    public bool Login()
    {
        // Akun lokal untuk latihan, bukan autentikasi produksi.
        EmployeeID = 0;
        if (LoginName == "John" && Password == "WkRW")
        {
            EmployeeID = 1;
            return true;
        }
        if (LoginName == "Jones" && Password == "haha")
        {
            EmployeeID = 2;
            return true;
        }
        return false;
    }
}
