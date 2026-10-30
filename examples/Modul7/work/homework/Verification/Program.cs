using SolarScope;
using System.Windows.Forms;
using Screen = SolarScope.Screen;

internal static class Program
{
    private static int checks;
    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        Console.WriteLine("PASS " + message); checks++;
    }
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var session = new AppSession();
        Check(session.History.Count == 0, "New session has empty history");
        session.Draft = new("Sekolah", "Yogyakarta", 3);
        var first = session.Preview();
        Check(first.Months.Count == 12 && first.Months.Select(m => m.Month).SequenceEqual(Enumerable.Range(1,12)), "Result contains January through December");
        Check(first.AnnualTotal == 4265 && first.Highest.Month == 8 && first.Highest.Energy == 410 && first.Lowest.Month == 2 && first.Lowest.Energy == 295, "Total and extrema match fixture");
        Check(session.Preview().Id == first.Id, "Unchanged input preserves result identity");
        Check(session.Save(first) && !session.Save(first) && session.History.Count == 1, "Duplicate save is prevented");
        session.Draft = new("Rumah", "Sleman", 5);
        var second = session.Preview();
        Check(second.Id != first.Id && session.Save(second) && session.History.Count == 2, "Changed input creates separate saved result");
        Check(second.Months.SequenceEqual(first.Months), "Demo energy is independent of input capacity and location");
        Check(session.History[1].Result.Input.Name == "Sekolah", "Saved input remains an immutable snapshot");
        Check(session.Delete(first.Id) && !session.Delete(first.Id) && session.History.Count == 1, "Delete affects selected entry only");
        Check(new AppSession().History.Count == 0, "A new application session does not persist demo history");
        Check(DemoData.Scenarios.Select(s => s.Capacity).SequenceEqual(new decimal[]{2,3,5}), "Comparison has three approved capacity fixtures");
        foreach(var scenario in DemoData.Scenarios)
        {
            Check(scenario.Months.Count == 12 && scenario.Months.All(m => m.Energy > 0), $"Scenario {scenario.Capacity} has twelve positive monthly values");
            Check(scenario.AnnualTotal == scenario.Months.Sum(m => m.Energy), $"Scenario {scenario.Capacity} total equals displayed months");
        }
        Check(!DemoData.ValidComparison(0) && !DemoData.ValidComparison(1) && DemoData.ValidComparison(2) && DemoData.ValidComparison(3) && !DemoData.ValidComparison(4), "Comparison accepts exactly two or three choices");

        using var controller = new AppController();
        controller.Session.Draft = new("Draft tetap", "Yogyakarta", 3);
        for(int cycle=0; cycle<3; cycle++)
        {
            foreach(var screen in new[]{Screen.Input,Screen.History,Screen.Comparison,Screen.Guide,Screen.Home})
            {
                var previous = controller.MainForm!;
                controller.Navigate(screen);
                Application.DoEvents();
                Check(previous.IsDisposed && Application.OpenForms.Count == 1, $"Cycle {cycle+1}, {screen}: previous disposed and one form open");
            }
        }
        Check(controller.Session.Draft.Name == "Draft tetap", "Draft survives repeated navigation");
        controller.Navigate(Screen.Input);
        var inputForm = controller.MainForm!;
        var name = (TextBox)inputForm.Controls.Find("tbNamaSimulasi",true).Single();
        var location = (TextBox)inputForm.Controls.Find("tbLokasi",true).Single();
        var capacity = (NumericUpDown)inputForm.Controls.Find("nudKapasitas",true).Single();
        var preview = (Button)inputForm.Controls.Find("btnLihatHasil",true).Single();
        name.Text="  "; preview.PerformClick();
        Check(controller.MainForm == inputForm && inputForm.Controls.Find("errorNama",true).Single().Text.Length>0, "Whitespace name is rejected inline");
        name.Text="Draft tetap"; location.Text=""; preview.PerformClick();
        Check(controller.MainForm == inputForm && inputForm.Controls.Find("errorLokasi",true).Single().Text.Length>0, "Empty location is rejected inline");
        location.Text="Yogyakarta"; capacity.Value=0; preview.PerformClick();
        Check(controller.MainForm == inputForm && inputForm.Controls.Find("errorKapasitas",true).Single().Text.Length>0, "Zero capacity is rejected inline");
        capacity.Value=3; preview.PerformClick();
        Check(controller.MainForm is HasilForm, "Corrected input opens result");
        controller.ShowResult(controller.Session.Preview(), Screen.Input);
        var results = controller.MainForm!;
        var grid = (DataGridView)results.Controls.Find("dgvBulanan",true).Single();
        Check(grid.Rows.Count == 12 && grid.ReadOnly, "Result UI displays twelve read-only rows");
        ((Button)results.Controls.Find("btnSave",true).Single()).PerformClick();
        Check(!results.Controls.Find("btnSave",true).Single().Enabled && controller.Session.History.Count == 1, "Save button reflects saved state");
        ((Button)results.Controls.Find("btnKembali",true).Single()).PerformClick();
        Check(controller.MainForm is Form1, "Result back returns to Input");
        controller.ShowResult(controller.Session.Preview(), Screen.History);
        Check(!controller.MainForm!.Controls.Find("btnEdit",true).Single().Visible, "Saved detail hides edit action");
        ((Button)controller.MainForm.Controls.Find("btnKembali",true).Single()).PerformClick();
        Check(controller.MainForm is RiwayatForm, "Saved detail back returns to History");
        controller.Navigate(Screen.Comparison);
        var comparison = controller.MainForm!;
        var comparisonGrid = (DataGridView)comparison.Controls.Find("dgvComparison",true).Single();
        Check(comparisonGrid.Rows.Count == 12 && comparisonGrid.Columns.Count == 4, "Three-way comparison UI has twelve months and three capacity columns");
        ((CheckBox)comparison.Controls.Find("chk5",true).Single()).Checked=false;
        ((Button)comparison.Controls.Find("btnCompare",true).Single()).PerformClick();
        Check(comparisonGrid.Rows.Count == 12 && comparisonGrid.Columns.Count == 3, "Two-way comparison updates table");
        ((CheckBox)comparison.Controls.Find("chk3",true).Single()).Checked=false;
        ((Button)comparison.Controls.Find("btnCompare",true).Single()).PerformClick();
        Check(comparisonGrid.Rows.Count == 0 && comparison.Controls.Find("lblValidation",true).Single().Text.Contains("minimal dua"), "One choice produces clear validation without stale data");
        controller.MainForm.Close();
        Application.DoEvents();
        Check(Application.OpenForms.Count == 0, "Closing active form leaves no hidden windows");
        Console.WriteLine($"TOTAL {checks} checks passed.");
    }
}
