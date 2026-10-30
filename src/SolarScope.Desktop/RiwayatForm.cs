namespace SolarScope;
public partial class RiwayatForm : Form
{
    private AppController? controller;
    public RiwayatForm() { InitializeComponent(); Text = "SolarScope | Riwayat Simulasi"; }
    public RiwayatForm(AppController controller) : this()
    {
        this.controller = controller;
                navHome.Click += (_, _) => controller?.Navigate(Screen.Home);
        navInput.Click += (_, _) => controller?.Navigate(Screen.Input);
        navHistory.Click += (_, _) => controller?.Navigate(Screen.History);
        navComparison.Click += (_, _) => controller?.Navigate(Screen.Comparison);
        navGuide.Click += (_, _) => controller?.Navigate(Screen.Guide);
        dgvHistory.Columns.Add("name", "Nama simulasi");
        dgvHistory.Columns.Add("location", "Lokasi");
        dgvHistory.Columns.Add("capacity", "Kapasitas (kWp)");
        dgvHistory.Columns.Add("saved", "Waktu simpan");
        dgvHistory.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        foreach (DataGridViewColumn col in dgvHistory.Columns) col.SortMode = DataGridViewColumnSortMode.NotSortable;
        RefreshHistory();
        btnOpen.Click += (_, _) => OpenSelected();
        dgvHistory.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) OpenSelected(); };
        btnDelete.Click += (_, _) => DeleteSelected();
        btnCreate.Click += (_, _) => controller.Navigate(Screen.Input);
        btnBack.Click += (_, _) => controller.Navigate(Screen.Home);
    }
    private HistoryEntry? Selected() => dgvHistory.CurrentRow?.Tag as HistoryEntry;
    private void OpenSelected()
    {
        if (Selected() is { } entry) controller?.ShowResult(entry.Result, Screen.History);
    }
    private void RefreshHistory()
    {
        dgvHistory.Rows.Clear();
        foreach (var entry in controller!.Session.History)
        {
            var input = entry.Result.Input;
            int row = dgvHistory.Rows.Add(input.Name, input.Location, input.Capacity.ToString("N2", DemoData.Culture), entry.SavedAt.ToString("dd MMM yyyy HH:mm", DemoData.Culture));
            dgvHistory.Rows[row].Tag = entry;
        }
        bool any = dgvHistory.Rows.Count > 0;
        dgvHistory.Visible = any;
        lblEmpty.Visible = !any;
        btnOpen.Enabled = btnDelete.Enabled = any;
        lblStatus.Text = $"{dgvHistory.Rows.Count} simulasi tersimpan dalam sesi ini.";
    }
    private void DeleteSelected()
    {
        if (Selected() is not { } entry) return;
        if (MessageBox.Show(this, $"Hapus simulasi \"{entry.Result.Input.Name}\" dari riwayat sesi?", "Hapus Simulasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        controller!.Session.Delete(entry.Result.Id);
        RefreshHistory();
        lblStatus.Text = "Simulasi dihapus dari riwayat sesi.";
    }
}
