namespace DeveloperStore.Application.Sales;

public static class DiscountCalculator
{
    public static (decimal discount, string? error) FromQuantity(int qty)
    {
        if (qty > 20) return (0m, "Quantity above 20 identical items is not allowed.");
        if (qty >= 10) return (0.20m, null);
        if (qty >= 4) return (0.10m, null);
        return (0m, null);
    }
}
