namespace SolarScope;
public partial class Form1 : Form
{
    private AppController? controller;
    public Form1() { InitializeComponent(); Text = "SolarScope | Input Simulasi"; }
    public Form1(AppController controller) : this()
    {
        this.controller = controller;
        tbNamaSimulasi.AccessibleName = "Nama simulasi";
        tbLokasi.AccessibleName = "Lokasi";
        nudKapasitas.AccessibleName = "Kapasitas sistem dalam kWp";
        var draft = controller.Session.Draft;
        tbNamaSimulasi.Text = draft.Name;
        tbLokasi.Text = draft.Location;
        nudKapasitas.Value = draft.Capacity;
                navHome.Click += (_, _) => controller?.Navigate(Screen.Home);
        navInput.Click += (_, _) => controller?.Navigate(Screen.Input);
        navHistory.Click += (_, _) => controller?.Navigate(Screen.History);
        navComparison.Click += (_, _) => controller?.Navigate(Screen.Comparison);
        navGuide.Click += (_, _) => controller?.Navigate(Screen.Guide);
        tbNamaSimulasi.TextChanged += (_, _) => { CaptureDraft(); errorNama.Text = ""; };
        tbLokasi.TextChanged += (_, _) => { CaptureDraft(); errorLokasi.Text = ""; };
        nudKapasitas.ValueChanged += (_, _) => { CaptureDraft(); errorKapasitas.Text = ""; };
        btnLihatHasil.Click += (_, _) => Preview();
        btnKembali.Click += (_, _) => controller.Navigate(Screen.Home);
        AcceptButton = btnLihatHasil;
    }
    private void CaptureDraft()
    {
        if (controller != null) controller.Session.Draft = new(tbNamaSimulasi.Text, tbLokasi.Text, nudKapasitas.Value);
    }
    private void Preview()
    {
        errorNama.Text = string.IsNullOrWhiteSpace(tbNamaSimulasi.Text) ? "Nama simulasi wajib diisi." : "";
        errorLokasi.Text = string.IsNullOrWhiteSpace(tbLokasi.Text) ? "Lokasi wajib diisi." : "";
        errorKapasitas.Text = nudKapasitas.Value <= 0 ? "Kapasitas harus lebih dari 0 kWp." : "";
        if (errorNama.Text.Length > 0) { tbNamaSimulasi.Focus(); return; }
        if (errorLokasi.Text.Length > 0) { tbLokasi.Focus(); return; }
        if (errorKapasitas.Text.Length > 0) { nudKapasitas.Focus(); return; }
        CaptureDraft();
        if (controller != null) controller.ShowResult(controller.Session.Preview(), Screen.Input);
    }
}
