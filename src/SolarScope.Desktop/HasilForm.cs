namespace SolarScope;
public partial class HasilForm : Form
{
    private AppController? controller;
    public HasilForm() { InitializeComponent(); Text = "SolarScope | Hasil Estimasi"; }
    public HasilForm(string namaSimulasi, string lokasi, decimal kapasitas) : this()
        => Bind(DemoData.Create(new(namaSimulasi, lokasi, kapasitas)));
    public HasilForm(AppController controller, SimulationResult result, Screen origin) : this()
    {
        this.controller = controller;
                navHome.Click += (_, _) => controller?.Navigate(Screen.Home);
        navInput.Click += (_, _) => controller?.Navigate(Screen.Input);
        navHistory.Click += (_, _) => controller?.Navigate(Screen.History);
        navComparison.Click += (_, _) => controller?.Navigate(Screen.Comparison);
        navGuide.Click += (_, _) => controller?.Navigate(Screen.Guide);
        Bind(result);
        if (origin == Screen.History) { navInput.BackColor = Color.White; navInput.ForeColor = Color.FromArgb(21,82,61); navHistory.BackColor = Color.FromArgb(22,105,76); navHistory.ForeColor = Color.White; }
        btnEdit.Visible = origin == Screen.Input;
        btnEdit.Click += (_, _) => controller.Navigate(Screen.Input);
        btnKembali.Click += (_, _) => controller.Navigate(origin);
        btnSave.Click += (_, _) => { controller.Session.Save(result); SavedState(result); };
        SavedState(result);
    }
    private void Bind(SimulationResult result)
    {
        var input = result.Input;
        lblIdentitas.Text = $"{input.Name}  |  {input.Location}\nKapasitas input: {input.Capacity.ToString("N2", DemoData.Culture)} kWp";
        dgvBulanan.Columns.Add("month", "Bulan");
        dgvBulanan.Columns.Add("energy", "Energi contoh (kWh)");
        dgvBulanan.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        foreach (DataGridViewColumn col in dgvBulanan.Columns) col.SortMode = DataGridViewColumnSortMode.NotSortable;
        foreach (var month in result.Months) dgvBulanan.Rows.Add(DemoData.Month(month.Month), DemoData.Number(month.Energy));
        lblTotalValue.Text = $"{DemoData.Number(result.AnnualTotal)} kWh";
        lblTertinggiValue.Text = $"{DemoData.Month(result.Highest.Month)} / {DemoData.Number(result.Highest.Energy)} kWh";
        lblTerendahValue.Text = $"{DemoData.Month(result.Lowest.Month)} / {DemoData.Number(result.Lowest.Energy)} kWh";
    }
    private void SavedState(SimulationResult result)
    {
        bool saved = controller?.Session.Contains(result.Id) == true;
        btnSave.Enabled = !saved;
        btnSave.Text = saved ? "Sudah Disimpan" : "Simpan ke Riwayat";
        lblStatus.Text = saved ? "Tersimpan di riwayat sesi. Buka melalui menu Riwayat." : "Belum disimpan. Riwayat tersedia selama aplikasi dibuka.";
        lblStatus.AccessibleRole = AccessibleRole.StaticText;
    }
}
