using System.ComponentModel.DataAnnotations;

namespace AppServices;

public class Product
{
    public int Id { get; set; }

    [MaxLength(10)]
    public string ProductCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ProductDescription { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    public decimal PricePerUnit { get; set; }
}
