namespace AppServices;

/// <summary>
/// Represents a customer order stored in the database.
/// </summary>
public class Order
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the name of the customer who placed the order.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer's delivery address.</summary>
    public string CustomerAddress { get; set; } = string.Empty;

    /// <summary>Gets or sets the total cost of all bracelets in the order, in EUR.</summary>
    public decimal TotalCosts { get; set; }

    /// <summary>Gets or sets the date and time when the order was placed.</summary>
    public DateTime OrderDate { get; set; }

    /// <summary>Gets or sets the line items (bracelets) belonging to this order.</summary>
    public List<OrderItem> OrderItems { get; set; } = [];
}

/// <summary>
/// Represents a single bracelet line item within an <see cref="Order"/>.
/// </summary>
public class OrderItem
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the foreign key to the parent <see cref="Order"/>.</summary>
    public int OrderId { get; set; }

    /// <summary>Gets or sets the navigation property to the parent order.</summary>
    public Order Order { get; set; } = null!;

    /// <summary>Gets or sets the pipe-delimited bracelet data string (e.g. <c>H|pink|I</c>).</summary>
    public string BraceletData { get; set; } = string.Empty;

    /// <summary>Gets or sets the cost of this bracelet, in EUR.</summary>
    public decimal Costs { get; set; }
}
