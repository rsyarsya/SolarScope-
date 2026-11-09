namespace SolarScope;
public partial class PanduanForm : Form
{
    private AppController? controller;
    public PanduanForm() { InitializeComponent(); Text = "SolarScope | Panduan dan Tentang"; }
    public PanduanForm(AppController controller) : this()
    {
        this.controller = controller;
                navHome.Click += (_, _) => controller?.Navigate(Screen.Home);
        navInput.Click += (_, _) => controller?.Navigate(Screen.Input);
        navHistory.Click += (_, _) => controller?.Navigate(Screen.History);
        navComparison.Click += (_, _) => controller?.Navigate(Screen.Comparison);
        navGuide.Click += (_, _) => controller?.Navigate(Screen.Guide);
        btnCreate.Click += (_, _) => controller.Navigate(Screen.Input);
        btnBack.Click += (_, _) => controller.Navigate(Screen.Home);
    }
}
