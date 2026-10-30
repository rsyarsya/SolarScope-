namespace SolarScope;
public partial class PerbandinganForm : Form
{
    private AppController? controller;
    private CheckBox[] Choices => new[] { chk2, chk3, chk5 };
    public PerbandinganForm() { InitializeComponent(); Text = "SolarScope | Perbandingan Skenario"; }
    public PerbandinganForm(AppController controller) : this()
    {
        this.controller = controller;
                navHome.Click += (_, _) => controller?.Navigate(Screen.Home);
        navInput.Click += (_, _) => controller?.Navigate(Screen.Input);
        navHistory.Click += (_, _) => controller?.Navigate(Screen.History);
        navComparison.Click += (_, _) => controller?.Navigate(Screen.Comparison);
        navGuide.Click += (_, _) => controller?.Navigate(Screen.Guide);
        for (int i=0; i<3; i++) Choices[i].Checked = controller.Session.ComparisonSelection[i];
        foreach (var choice in Choices) choice.CheckedChanged += (_, _) => {
            CaptureSelection();
            dgvComparison.Rows.Clear(); dgvComparison.Columns.Clear();
            lblSummary.Text = "Pilihan berubah. Klik Bandingkan untuk memperbarui tabel.";
            lblValidation.Text = "";
        };
        btnCompare.Click += (_, _) => Compare();
        btnBack.Click += (_, _) => controller.Navigate(Screen.Home);
        Compare();
    }
    private void CaptureSelection()
    {
        if (controller == null) return;
        for (int i=0; i<3; i++) controller.Session.ComparisonSelection[i] = Choices[i].Checked;
    }
    private void Compare()
    {
        var selected = DemoData.Scenarios.Where((_, i) => Choices[i].Checked).ToArray();
        dgvComparison.Rows.Clear(); dgvComparison.Columns.Clear();
        if (!DemoData.ValidComparison(selected.Length))
        {
            lblValidation.Text = "Pilih minimal dua skenario untuk dibandingkan (maksimal tiga).";
            lblSummary.Text = "Belum ada perbandingan. Periksa pilihan skenario.";
            return;
        }
        lblValidation.Text = "";
        dgvComparison.Columns.Add("month", "Bulan");
        for (int i=0; i<selected.Length; i++)
        {
            dgvComparison.Columns.Add("scenario"+i, $"{selected[i].Capacity} kWp (kWh)");
            dgvComparison.Columns[i+1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        foreach (DataGridViewColumn col in dgvComparison.Columns) col.SortMode = DataGridViewColumnSortMode.NotSortable;
        for (int month=0; month<12; month++)
        {
            var cells = new List<object> { DemoData.Month(month+1) };
            cells.AddRange(selected.Select(s => (object)DemoData.Number(s.Months[month].Energy)));
            dgvComparison.Rows.Add(cells.ToArray());
        }
        lblSummary.Text = "Total tahunan: " + string.Join("     |     ", selected.Select(s => $"{s.Capacity} kWp: {DemoData.Number(s.AnnualTotal)} kWh"));
    }
}
