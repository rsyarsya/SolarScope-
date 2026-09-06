namespace SolarScope.Core.Models;

public class Simulation{
    public int SimulationId { get; set; }
    public string ScenarioName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
    public Location Location { get; set; } = new();
    public SystemParameter SystemParameter { get; set; } = new();
    public SimulationSummary? Summary { get; set; }


    public (bool IsValid, string? err) Validate(){
        if(string.IsNullOrWhiteSpace(ScenarioName)){
            return (false, "Nama scenario tidak boleh kosong");
        }

        var locCheck = Location.Validate();
        if(!locCheck.IsValid){
            return (false, locCheck.err);
        }
        var sysCheck = SystemParameter.Validate();
        if(!sysCheck.IsValid){
            return (false, sysCheck.err);
        }
        return (true, null);
    }
}