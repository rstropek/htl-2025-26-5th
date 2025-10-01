using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CashRegister.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CashRegister.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDbContextFactory<ApplicationDataContext> contextFactory;

    public ObservableCollection<Product> Products { get; } = [];

    public ObservableCollection<ReceiptLineViewModel> ReceiptLines { get; } = [];

    [ObservableProperty]
    private decimal _totalPrice;

    public MainWindowViewModel(IDbContextFactory<ApplicationDataContext> contextFactory)
    {
        this.contextFactory = contextFactory;
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await SeedDatabaseAsync();
        await LoadProductsAsync();
    }

    private async Task SeedDatabaseAsync()
    {
        using var context = contextFactory.CreateDbContext();
        
        // Check if products already exist
        if (await context.Products.AnyAsync())
            return;

        // Add the products from the mockup
        var products = new List<Product>
        {
            new() { ProductName = "Bananen 1kg", UnitPrice = 1.99m },
            new() { ProductName = "Äpfel 1kg", UnitPrice = 2.99m },
            new() { ProductName = "Trauben Weiß 500g", UnitPrice = 3.49m },
            new() { ProductName = "Himbeeren 125g", UnitPrice = 2.99m },
            new() { ProductName = "Karotten 500g", UnitPrice = 1.29m },
            new() { ProductName = "Eissalat 1 Stück", UnitPrice = 1.98m },
            new() { ProductName = "Zucchini 1 Stück", UnitPrice = 0.99m },
            new() { ProductName = "Knoblauch 150g", UnitPrice = 1.49m },
            new() { ProductName = "Joghurt 200g", UnitPrice = 0.89m },
            new() { ProductName = "Butter", UnitPrice = 2.49m }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }

    private async Task LoadProductsAsync()
    {
        using var context = contextFactory.CreateDbContext();
        var products = await context.Products.ToListAsync();
        
        Products.Clear();
        foreach (var product in products)
        {
            Products.Add(product);
        }
    }

    [RelayCommand]
    private void AddProduct(Product product)
    {
        var existingLine = ReceiptLines.FirstOrDefault(rl => rl.ProductId == product.Id);
        
        if (existingLine != null)
        {
            // Increase amount if product already in receipt
            existingLine.Amount++;
            existingLine.TotalPrice = existingLine.Amount * product.UnitPrice;
        }
        else
        {
            // Add new receipt line
            var newLine = new ReceiptLineViewModel
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                Amount = 1,
                UnitPrice = product.UnitPrice,
                TotalPrice = product.UnitPrice
            };
            ReceiptLines.Add(newLine);
        }

        CalculateTotal();
    }

    [RelayCommand]
    private async Task Checkout()
    {
        if (!ReceiptLines.Any())
        {
            var emptyBox = MessageBoxManager.GetMessageBoxStandard("Information",
                "No items in receipt to checkout.", ButtonEnum.Ok);
            await emptyBox.ShowAsync();
            return;
        }

        using var context = contextFactory.CreateDbContext();
        
        var receipt = new Receipt
        {
            TotalPrice = TotalPrice
        };

        context.Receipts.Add(receipt);
        await context.SaveChangesAsync(); // Save to get the receipt ID

        foreach (var line in ReceiptLines)
        {
            var receiptLine = new ReceiptLine
            {
                ProductId = line.ProductId,
                Amount = line.Amount,
                TotalPrice = line.TotalPrice,
                ReceiptId = receipt.Id
            };
            context.ReceiptLines.Add(receiptLine);
        }

        await context.SaveChangesAsync();

        // Clear the receipt
        ReceiptLines.Clear();
        TotalPrice = 0;

        var successBox = MessageBoxManager.GetMessageBoxStandard("Success",
            "Checkout completed successfully!", ButtonEnum.Ok);
        await successBox.ShowAsync();
    }

    private void CalculateTotal()
    {
        TotalPrice = ReceiptLines.Sum(rl => rl.TotalPrice);
    }
}

public partial class ReceiptLineViewModel : ObservableObject
{
    [ObservableProperty]
    private int _productId;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private int _amount;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private decimal _totalPrice;
}
