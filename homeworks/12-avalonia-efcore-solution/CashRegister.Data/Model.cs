using System.ComponentModel.DataAnnotations;

namespace CashRegister.Data;

public class Product
{
    public int Id { get; set; }

    [Required]
    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public List<ReceiptLine> ReceiptLines { get; set; } = [];
}

public class Receipt
{
    public int Id { get; set; }

    public decimal TotalPrice { get; set; }

    public List<ReceiptLine> ReceiptLines { get; set; } = [];
}

public class ReceiptLine
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Amount { get; set; }

    public decimal TotalPrice { get; set; }

    public int ReceiptId { get; set; }

    public Product? Product { get; set; }
    public Receipt? Receipt { get; set; }
}