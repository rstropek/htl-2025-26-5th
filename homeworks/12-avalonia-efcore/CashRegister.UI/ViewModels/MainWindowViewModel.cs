using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    private readonly ApplicationDataContext dbContext;
    private Receipt _currentReceipt = new();

    public MainWindowViewModel(IDbContextFactory<ApplicationDataContext> contextFactory)
    {
        dbContext = contextFactory.CreateDbContext();
        Initialize();
    }

    private async Task Initialize()
    {
        await PopulateDb();
    }

    public ObservableCollection<Product> CashedProducts { get; } = [];
    public ObservableCollection<ReceiptLineVM> CashedReceiptLines { get; } = [];

    [RelayCommand]
    private async Task PopulateDb()
    {
        foreach (var product in new Product[]
                 {
                     new() { Id = 1, ProductName = "Bananen 1kg", UnitPrice = 1.99m },
                     new() { Id = 2, ProductName = "Äpfel 1kg", UnitPrice = 2.99m },
                     new() { Id = 3, ProductName = "Trauben Weiß 500g", UnitPrice = 3.49m },
                     new() { Id = 4, ProductName = "Himbeeren 125g", UnitPrice = 2.99m },
                     new() { Id = 5, ProductName = "Karotten 500g", UnitPrice = 1.29m },
                     new() { Id = 6, ProductName = "Eissalat 1 Stück", UnitPrice = 1.98m },
                     new() { Id = 7, ProductName = "Zucchini 1 Stück", UnitPrice = 0.99m },
                     new() { Id = 8, ProductName = "Knoblauch 150g", UnitPrice = 1.49m },
                     new() { Id = 9, ProductName = "Joghurt 200g", UnitPrice = 0.89m },
                     new() { Id = 10, ProductName = "Butter", UnitPrice = 2.49m }
                 })
        {
            CashedProducts.Add(product);
        }
    }

    [RelayCommand]
    private async Task OnProductButtonClick(Product product)
    {
        var oldReceiptLine = CashedReceiptLines.LastOrDefault(rl => rl.Product.Id == product.Id);

        if (oldReceiptLine == null)
        {
            CashedReceiptLines.Add(new ReceiptLineVM
            {
                Id = CashedReceiptLines.MaxBy(x => x.Id)?.Id ?? 0 + 1,
                Amount = 1,
                Product = product,
                Receipt = _currentReceipt,
                TotalPrice = product.UnitPrice
            });
        }
        else
        {
            oldReceiptLine.Amount++;
            oldReceiptLine.TotalPrice += product.UnitPrice;
        }
    }
}

public partial class ReceiptLineVM : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private int _amount;
    [ObservableProperty] private decimal _totalPrice;
    [ObservableProperty] private Product _product;
    [ObservableProperty] private Receipt _receipt;
}