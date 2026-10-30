namespace SolarScope;
public partial class BerandaForm : Form
{
    private AppController? controller;
    public BerandaForm() { InitializeComponent(); Text = "SolarScope | Beranda"; }
    public BerandaForm(AppController controller) : this()
    {
        this.controller = controller;
                navHome.Click += (_, _) => controller?.Navigate(Screen.Home);
        navInput.Click += (_, _) => controller?.Navigate(Screen.Input);
        navHistory.Click += (_, _) => controller?.Navigate(Screen.History);
        navComparison.Click += (_, _) => controller?.Navigate(Screen.Comparison);
        navGuide.Click += (_, _) => controller?.Navigate(Screen.Guide);
        btnCreate.Click += (_, _) => controller.Navigate(Screen.Input);
        btnHistory.Click += (_, _) => controller.Navigate(Screen.History);
        btnCompare.Click += (_, _) => controller.Navigate(Screen.Comparison);
        btnGuide.Click += (_, _) => controller.Navigate(Screen.Guide);
        btnExit.Click += (_, _) => Close();
    }
}
