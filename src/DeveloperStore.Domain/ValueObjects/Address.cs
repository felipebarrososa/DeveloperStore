namespace DeveloperStore.Domain.ValueObjects;

public class Address
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Zipcode { get; set; } = string.Empty;
    public Geo Geo { get; set; } = new();
}

public class Geo
{
    public string Lat { get; set; } = string.Empty;
    public string Long { get; set; } = string.Empty;
}
