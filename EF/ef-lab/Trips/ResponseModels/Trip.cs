namespace EF.Trips.ResponseModels;

public class Trip
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<Country> Countries { get; set; } = new();
    public List<Client> Clients { get; set; } = new();
}