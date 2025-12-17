using AppServices.Importer;

namespace ImporterTests;

public class TypedCsvParserTests
{
    private readonly TypedCsvParser _parser = new();

    [Fact]
    public void Parse_ValidFile_ReturnsDictionaries()
    {
        var fileContent = """
            ProductCode: STRING(10), MANDATORY
            ProductName: STRING(100), MANDATORY
            ProductDescription: STRING(255), OPTIONAL
            Category: STRING(50), OPTIONAL
            PricePerUnit: DECIMAL, MANDATORY
            ---
            "BKE0001","Mountain Bike Alpha","Entry-level hardtail mountain bike ideal for light trails and weekend rides.","Mountain Bikes",699.99
            "BKE0002","Road Bike Swift 200","Lightweight aluminum road bike designed for endurance training and long-distance touring.","Road Bikes",1199.50
            "BKE0003","City Bike UrbanEase 7","Comfortable city bike with 7-speed gear hub and integrated rear carrier.","City Bikes",549.00
            "BKE0004","E-Bike TrailBoost 500","Electric mountain bike with mid-drive motor and long-range battery for steep terrain.","E-Bikes",2899.00
            "BKE0005","Kids Bike FunRider 16","Durable 16-inch kids bike with training wheels and colorful frame design.","Kids Bikes",199.95
            "BKE0006","Gravel Bike AllRoad X","Versatile gravel bike built for mixed surfaces with wide tires and endurance geometry.","Gravel Bikes",1599.00
            "BKE0007","Folding Bike CompactOne","Fully foldable commuter bike that fits easily into trains, cars, and small apartments.","Folding Bikes",799.00
            "BKE0008","BMX Bike Freestyle Pro","Sturdy BMX bike optimized for park, street, and dirt jumps.","BMX Bikes",349.99
            "BKE0009","Cargo Bike LoadMaster","Heavy-duty cargo bike designed for transporting groceries, kids, or delivery goods.","Cargo Bikes",1890.00
            "BKE0010","E-Bike CityFlow 300","Urban e-bike with pedal assist, integrated lights, and low-step frame for easy mounting.","E-Bikes",2190.00
            "BKE0011","Replacement Screws",,,1299.00
            """;

        var result = _parser.Parse(fileContent);

        Assert.NotNull(result);
        var records = result.ToList();
        Assert.Equal(11, records.Count);

        var firstRecord = records[0];
        Assert.Equal("BKE0001", firstRecord["ProductCode"]);
        Assert.Equal("Mountain Bike Alpha", firstRecord["ProductName"]);
        Assert.Equal("Entry-level hardtail mountain bike ideal for light trails and weekend rides.", firstRecord["ProductDescription"]);
        Assert.Equal("Mountain Bikes", firstRecord["Category"]);
        Assert.Equal(699.99m, firstRecord["PricePerUnit"]);

        var lastRecord = records[10];
        Assert.Equal("BKE0011", lastRecord["ProductCode"]);
        Assert.Equal("Replacement Screws", lastRecord["ProductName"]);
        Assert.Null(lastRecord["ProductDescription"]);
        Assert.Null(lastRecord["Category"]);
        Assert.Equal(1299.00m, lastRecord["PricePerUnit"]);
    }

    [Fact]
    public void Parse_MissingHeader_ThrowsMissingHeaderException()
    {
        var fileContent = """
            "BKE0001","Mountain Bike Alpha","Entry-level hardtail mountain bike ideal for light trails and weekend rides.","Mountain Bikes",699.99
            "BKE0002","Road Bike Swift 200","Lightweight aluminum road bike designed for endurance training and long-distance touring.","Road Bikes",1199.50
            """;

        var exception = Assert.Throws<FileParseException>(() => _parser.Parse(fileContent));
        Assert.Equal(ImportFileError.MissingHeader, exception.ErrorCode);
    }

    [Fact]
    public void Parse_HeaderFormatError_ThrowsHeaderFormatErrorException()
    {
        var fileContent = """
            ProductCode - STRING(10), MANDATORY
            ProductName: STRING(100); MANDATORY
            ProductDescription -> STRING(255), OPTIONAL
            Category => STRING(50) & OPTIONAL
            PricePerUnit - DECIMAL, MANDATORY
            ---
            "BKE0001","Mountain Bike Alpha","Entry-level hardtail mountain bike ideal for light trails and weekend rides.","Mountain Bikes",699.99
            "BKE0002","Road Bike Swift 200","Lightweight aluminum road bike designed for endurance training and long-distance touring.","Road Bikes",1199.50
            """;

        var exception = Assert.Throws<FileParseException>(() => _parser.Parse(fileContent));
        Assert.Equal(ImportFileError.HeaderFormatError, exception.ErrorCode);
    }

    [Fact]
    public void Parse_InvalidHeader_ThrowsHeaderFormatErrorException()
    {
        var fileContent = """
            ProductCode: STRING(10), MANDATORY
            This is a test
            ---
            "BKE0001","Mountain Bike Alpha","Entry-level hardtail mountain bike ideal for light trails and weekend rides.","Mountain Bikes",699.99
            "BKE0002","Road Bike Swift 200","Lightweight aluminum road bike designed for endurance training and long-distance touring.","Road Bikes",1199.50
            """;

        var exception = Assert.Throws<FileParseException>(() => _parser.Parse(fileContent));
        Assert.Equal(ImportFileError.HeaderFormatError, exception.ErrorCode);
    }

    [Fact]
    public void Parse_UnknownDataType_ThrowsUnknownDataTypeException()
    {
        var fileContent = """
            ProductCode: VARCHAR(10), MANDATORY
            ProductName: VARCHAR(100), MANDATORY
            ProductDescription: VARCHAR(255), OPTIONAL
            Category: VARCHAR(50), OPTIONAL
            PricePerUnit: DECIMAL, MANDATORY
            ---
            "BKE0001","Mountain Bike Alpha","Entry-level hardtail mountain bike ideal for light trails and weekend rides.","Mountain Bikes",699.99
            "BKE0002","Road Bike Swift 200","Lightweight aluminum road bike designed for endurance training and long-distance touring.","Road Bikes",1199.50
            """;

        var exception = Assert.Throws<FileParseException>(() => _parser.Parse(fileContent));
        Assert.Equal(ImportFileError.UnknownDataType, exception.ErrorCode);
    }

    [Fact]
    public void Parse_InvalidOptionalMarker_ThrowsInvalidOptionalMarkerException()
    {
        var fileContent = """
            ProductCode: STRING(10), NOT NULL
            ProductName: STRING(100), NOT NULL
            ProductDescription: STRING(255), NULLABLE
            Category: STRING(50), NULLABLE
            PricePerUnit: DECIMAL, NOT NULL
            ---
            "BKE0001","Mountain Bike Alpha","Entry-level hardtail mountain bike ideal for light trails and weekend rides.","Mountain Bikes",699.99
            "BKE0002","Road Bike Swift 200","Lightweight aluminum road bike designed for endurance training and long-distance touring.","Road Bikes",1199.50
            """;

        var exception = Assert.Throws<FileParseException>(() => _parser.Parse(fileContent));
        Assert.Equal(ImportFileError.InvalidOptionalMarker, exception.ErrorCode);
    }

    [Fact]
    public void Parse_MissingColumn_ThrowsMissingColumnException()
    {
        var fileContent = """
            ProductCode: STRING(10), MANDATORY
            ProductName: STRING(100), MANDATORY
            ProductDescription: STRING(255), OPTIONAL
            Category: STRING(50), OPTIONAL
            PricePerUnit: DECIMAL, MANDATORY
            ---
            "BKE0001","Mountain Bike Alpha","Entry-level hardtail mountain bike ideal for light trails and weekend rides.","Mountain Bikes"
            "BKE0002","Road Bike Swift 200","Lightweight aluminum road bike designed for endurance training and long-distance touring.","Road Bikes"
            """;

        var exception = Assert.Throws<FileParseException>(() => _parser.Parse(fileContent));
        Assert.Equal(ImportFileError.MissingColumn, exception.ErrorCode);
    }

    [Fact]
    public void Parse_MissingQuotes_ThrowsMissingQuotesException()
    {
        var fileContent = """
            ProductCode: STRING(10), MANDATORY
            ProductName: STRING(100), MANDATORY
            ProductDescription: STRING(255), OPTIONAL
            Category: STRING(50), OPTIONAL
            PricePerUnit: DECIMAL, MANDATORY
            ---
            BKE0001,Mountain Bike Alpha,Entry-level hardtail mountain bike ideal for light trails and weekend rides,Mountain Bikes,699.99
            BKE0002,Road Bike Swift 200,Lightweight aluminum road bike designed for endurance training and long-distance touring,Road Bikes,1199.50
            """;

        var exception = Assert.Throws<FileParseException>(() => _parser.Parse(fileContent));
        Assert.Equal(ImportFileError.MissingQuotes, exception.ErrorCode);
    }

    [Fact]
    public void Parse_WrongDataType_ThrowsWrongDataTypeException()
    {
        var fileContent = """
            ProductCode: STRING(10), MANDATORY
            ProductName: STRING(100), MANDATORY
            ProductDescription: STRING(255), OPTIONAL
            Category: STRING(50), OPTIONAL
            PricePerUnit: DECIMAL, MANDATORY
            ---
            "BKE0002","Road Bike Swift 200","Lightweight aluminum road bike designed for endurance training and long-distance touring.","Road Bikes","1199.50"
            """;

        var exception = Assert.Throws<FileParseException>(() => _parser.Parse(fileContent));
        Assert.Equal(ImportFileError.WrongDataType, exception.ErrorCode);
    }
}

