namespace SolarScope.Core.Models;

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public (bool IsValid, string? err) Validate()
    {
        if (Latitude < -90 || Latitude > 90)
        {
            return (false, "Latitude harus bernilai antara -90 dan 90");
        }
        if (Longitude < -180 || Longitude > 180)
        {
            return (false, "Longitude harus bernilai antara -180 dan 180");
        }
        return (true, null);
    }
}