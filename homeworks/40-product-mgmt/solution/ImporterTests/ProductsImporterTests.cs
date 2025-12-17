using AppServices;
using AppServices.Importer;

namespace ImporterTests;

public class ProductsImporterTests
{
    private readonly IFileReader fileReader;
    private readonly ITypedCsvParser parser;
    private readonly IProductImportDatabaseWriter databaseWriter;
    private readonly ProductImporter importer;

    public ProductsImporterTests()
    {
        fileReader = Substitute.For<IFileReader>();
        parser = Substitute.For<ITypedCsvParser>();
        databaseWriter = Substitute.For<IProductImportDatabaseWriter>();
        importer = new ProductImporter(fileReader, parser, databaseWriter);
    }

    [Fact]
    public async Task ImportProductsAsync_SuccessfulImport_ReturnsCount()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0001",
                ["ProductName"] = "Mountain Bike Alpha",
                ["PricePerUnit"] = 699.99m
            }
        };

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        // Act
        var result = await importer.ImportProductsAsync(filePath, isDryRun: false);

        // Assert
        Assert.Equal(1, result);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).ClearProductsAsync();
        await databaseWriter.Received(1).WriteProductsAsync(Arg.Is<IEnumerable<Product>>(p => p.Count() == 1));
        await databaseWriter.Received(1).CommitTransactionAsync();
        await databaseWriter.DidNotReceive().RollbackTransactionAsync();
    }

    [Fact]
    public async Task ImportProductsAsync_DryRun_RollsBackTransaction()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0001",
                ["ProductName"] = "Mountain Bike Alpha",
                ["PricePerUnit"] = 699.99m
            }
        };

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        // Act
        var result = await importer.ImportProductsAsync(filePath, isDryRun: true);

        // Assert
        Assert.Equal(1, result);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportProductsAsync_FileReaderThrows_RethrowsWithoutDatabaseCalls()
    {
        // Arrange
        var filePath = "dummy.txt";
        var expectedException = new FileNotFoundException("File not found");
        fileReader.ReadAllTextAsync(filePath).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await importer.ImportProductsAsync(filePath));

        await databaseWriter.DidNotReceive().BeginTransactionAsync();
        await databaseWriter.DidNotReceive().RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportProductsAsync_ParserThrows_RethrowsWithoutDatabaseCalls()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "Invalid content";
        var expectedException = new FileParseException(ImportFileError.MissingHeader);

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<FileParseException>(
            async () => await importer.ImportProductsAsync(filePath));

        await databaseWriter.DidNotReceive().BeginTransactionAsync();
        await databaseWriter.DidNotReceive().RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportProductsAsync_DatabaseWriterThrows_RollsBackAndRethrows()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0001",
                ["ProductName"] = "Mountain Bike Alpha",
                ["PricePerUnit"] = 699.99m
            }
        };
        var expectedException = new InvalidOperationException("Database error");

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);
        databaseWriter.WriteProductsAsync(Arg.Any<IEnumerable<Product>>()).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await importer.ImportProductsAsync(filePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportProductsAsync_EmptyFile_ReturnsZeroWithoutDatabaseCalls()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>();

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        // Act
        var result = await importer.ImportProductsAsync(filePath, isDryRun: false);

        // Assert
        Assert.Equal(0, result);
        await databaseWriter.DidNotReceive().BeginTransactionAsync();
        await databaseWriter.DidNotReceive().ClearProductsAsync();
        await databaseWriter.DidNotReceive().WriteProductsAsync(Arg.Any<IEnumerable<Product>>());
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
        await databaseWriter.DidNotReceive().RollbackTransactionAsync();
    }

    [Fact]
    public async Task ImportProductsAsync_MultipleProducts_ImportsAll()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0001",
                ["ProductName"] = "Mountain Bike Alpha",
                ["PricePerUnit"] = 699.99m
            },
            new()
            {
                ["ProductCode"] = "BKE0002",
                ["ProductName"] = "Road Bike Swift",
                ["PricePerUnit"] = 1199.50m
            }
        };

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        // Act
        var result = await importer.ImportProductsAsync(filePath, isDryRun: false);

        // Assert
        Assert.Equal(2, result);
        await databaseWriter.Received(1).WriteProductsAsync(Arg.Is<IEnumerable<Product>>(p => p.Count() == 2));
    }

    [Fact]
    public async Task ImportProductsAsync_ClearProductsThrows_RollsBackAndRethrows()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0001",
                ["ProductName"] = "Mountain Bike Alpha",
                ["PricePerUnit"] = 699.99m
            }
        };
        var expectedException = new InvalidOperationException("Clear products error");

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);
        databaseWriter.ClearProductsAsync().Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await importer.ImportProductsAsync(filePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportProductsAsync_CallsServicesInCorrectOrder()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0001",
                ["ProductName"] = "Mountain Bike Alpha",
                ["PricePerUnit"] = 699.99m
            }
        };

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        // Act
        await importer.ImportProductsAsync(filePath, isDryRun: false);

        // Assert - Verify order of calls
        Received.InOrder(async () =>
        {
            await fileReader.ReadAllTextAsync(filePath);
            parser.Parse(fileContent);
            await databaseWriter.BeginTransactionAsync();
            await databaseWriter.ClearProductsAsync();
            await databaseWriter.WriteProductsAsync(Arg.Any<IEnumerable<Product>>());
            await databaseWriter.CommitTransactionAsync();
        });
    }

    [Fact]
    public async Task ImportProductsAsync_WithOptionalFields_ConvertsCorrectly()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0001",
                ["ProductName"] = "Mountain Bike Alpha",
                ["ProductDescription"] = "Entry-level mountain bike",
                ["Category"] = "Mountain Bikes",
                ["PricePerUnit"] = 699.99m
            }
        };

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        Product? capturedProduct = null;
        await databaseWriter.WriteProductsAsync(Arg.Do<IEnumerable<Product>>(p => capturedProduct = p.First()));

        // Act
        await importer.ImportProductsAsync(filePath, isDryRun: false);

        // Assert
        Assert.NotNull(capturedProduct);
        Assert.Equal("BKE0001", capturedProduct.ProductCode);
        Assert.Equal("Mountain Bike Alpha", capturedProduct.ProductName);
        Assert.Equal("Entry-level mountain bike", capturedProduct.ProductDescription);
        Assert.Equal("Mountain Bikes", capturedProduct.Category);
        Assert.Equal(699.99m, capturedProduct.PricePerUnit);
    }

    [Fact]
    public async Task ImportProductsAsync_WithNullOptionalFields_ConvertsCorrectly()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0011",
                ["ProductName"] = "Replacement Screws",
                ["PricePerUnit"] = 1299.00m
            }
        };

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        Product? capturedProduct = null;
        await databaseWriter.WriteProductsAsync(Arg.Do<IEnumerable<Product>>(p => capturedProduct = p.First()));

        // Act
        await importer.ImportProductsAsync(filePath, isDryRun: false);

        // Assert
        Assert.NotNull(capturedProduct);
        Assert.Equal("BKE0011", capturedProduct.ProductCode);
        Assert.Equal("Replacement Screws", capturedProduct.ProductName);
        Assert.Null(capturedProduct.ProductDescription);
        Assert.Null(capturedProduct.Category);
        Assert.Equal(1299.00m, capturedProduct.PricePerUnit);
    }

    [Fact]
    public async Task ImportProductsAsync_MissingRequiredColumn_ThrowsProductConversionException()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0001",
                // Missing ProductName
                ["PricePerUnit"] = 699.99m
            }
        };

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductConversionException>(
            async () => await importer.ImportProductsAsync(filePath));

        Assert.Equal(ProductConversionError.MissingRequiredField, exception.ErrorCode);
        await databaseWriter.Received(1).RollbackTransactionAsync();
    }

    [Fact]
    public async Task ImportProductsAsync_MissingProductCode_ThrowsProductConversionException()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                // Missing ProductCode
                ["ProductName"] = "Mountain Bike Alpha",
                ["PricePerUnit"] = 699.99m
            }
        };

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductConversionException>(
            async () => await importer.ImportProductsAsync(filePath));

        Assert.Equal(ProductConversionError.MissingRequiredField, exception.ErrorCode);
        await databaseWriter.Received(1).RollbackTransactionAsync();
    }

    [Fact]
    public async Task ImportProductsAsync_MissingPricePerUnit_ThrowsProductConversionException()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0001",
                ["ProductName"] = "Mountain Bike Alpha"
                // Missing PricePerUnit
            }
        };

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductConversionException>(
            async () => await importer.ImportProductsAsync(filePath));

        Assert.Equal(ProductConversionError.MissingRequiredField, exception.ErrorCode);
        await databaseWriter.Received(1).RollbackTransactionAsync();
    }

    [Fact]
    public async Task ImportProductsAsync_WrongDataTypeForMandatoryColumn_ThrowsProductConversionException()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0001",
                ["ProductName"] = "Mountain Bike Alpha",
                ["PricePerUnit"] = "not a decimal" // Wrong type - should be decimal
            }
        };

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductConversionException>(
            async () => await importer.ImportProductsAsync(filePath));

        Assert.Equal(ProductConversionError.WrongDataType, exception.ErrorCode);
        await databaseWriter.Received(1).RollbackTransactionAsync();
    }

    [Fact]
    public async Task ImportProductsAsync_WrongDataTypeForOptionalColumn_ThrowsProductConversionException()
    {
        // Arrange
        var filePath = "dummy.txt";
        var fileContent = "dummy content";
        var records = new List<Dictionary<string, object>>
        {
            new()
            {
                ["ProductCode"] = "BKE0001",
                ["ProductName"] = "Mountain Bike Alpha",
                ["ProductDescription"] = 12345, // Wrong type - should be string or null
                ["PricePerUnit"] = 699.99m
            }
        };

        fileReader.ReadAllTextAsync(filePath).Returns(Task.FromResult(fileContent));
        parser.Parse(fileContent).Returns(records);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductConversionException>(
            async () => await importer.ImportProductsAsync(filePath));

        Assert.Equal(ProductConversionError.WrongDataType, exception.ErrorCode);
        await databaseWriter.Received(1).RollbackTransactionAsync();
    }
}
