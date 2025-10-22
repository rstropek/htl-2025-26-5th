namespace AppServicesTests;

using AppServices;

public class UnitTest1
{
    [Fact]
    public void Dummy_IncrementDecimal_Increments()
    {
        // Arrange
        var dummy = new Dummy { DecimalProperty = 10.5m };
        var logic = new DummyLogic();
        var increment = 2.5m;

        // Act
        logic.IncrementDecimal(dummy, increment);

        // Assert
        Assert.Equal(13.0m, dummy.DecimalProperty);
    }

    [Fact]
    public void Dummy_IncrementDecimal_NegativeIncrement()
    {
        // Arrange
        var dummy = new Dummy { DecimalProperty = 10.5m };
        var logic = new DummyLogic();
        var increment = -3.0m;

        // Act
        logic.IncrementDecimal(dummy, increment);

        // Assert
        Assert.Equal(7.5m, dummy.DecimalProperty);
    }

    [Fact]
    public void Dummy_IncrementDecimal_ZeroIncrement()
    {
        // Arrange
        var dummy = new Dummy { DecimalProperty = 10.5m };
        var logic = new DummyLogic();
        var increment = 0.0m;

        // Act
        logic.IncrementDecimal(dummy, increment);

        // Assert
        Assert.Equal(10.5m, dummy.DecimalProperty);
    }

    [Fact]
    public void Dummy_IncrementDecimal_Overflow()
    {
        // Arrange
        var dummy = new Dummy { DecimalProperty = decimal.MaxValue - 1 };
        var logic = new DummyLogic();
        var increment = 2.0m;

        // Act & Assert
        Assert.Throws<OverflowException>(() => logic.IncrementDecimal(dummy, increment));
    }

    [Theory]
    [InlineData(0.0, 1.0, 1.0)]
    [InlineData(-5.0, 5.0, 0.0)]
    public void Dummy_IncrementDecimal_VariousCases(decimal initial, decimal increment, decimal expected)
    {
        // Arrange
        var dummy = new Dummy { DecimalProperty = initial };
        var logic = new DummyLogic();

        // Act
        logic.IncrementDecimal(dummy, increment);

        // Assert
        Assert.Equal(expected, dummy.DecimalProperty);
    }
}
