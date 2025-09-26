namespace CashRegister.Data;

// Add your model classes here
// IMPORTANT: Read https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations
//            to learn about SQLite limitations

// This class ist just for demo purposes. Remove it if you want
public class Greeting
{
    public int Id { get; set; }

    public string GreetingText { get; set; } = string.Empty;
}