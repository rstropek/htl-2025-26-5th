namespace AppServices;

public interface IDummyLogic
{
    void IncrementDecimal(Dummy dummy, decimal increment);
}

public class DummyLogic : IDummyLogic
{
    public void IncrementDecimal(Dummy dummy, decimal increment)
        => dummy.DecimalProperty += increment;
}