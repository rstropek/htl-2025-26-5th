namespace AppServices;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Secrets
{
    public int Id { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
}