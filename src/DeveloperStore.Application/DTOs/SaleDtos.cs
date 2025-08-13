namespace DeveloperStore.Application.DTOs;

// ===== Entrada =====
public record SaleItemIn(int ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record SaleCreateDto(
    string Number, DateOnly Date, int CustomerId, string CustomerName,
    int BranchId, string BranchName, IEnumerable<SaleItemIn> Items);

public record SaleUpdateDto(
    string Number, DateOnly Date, int CustomerId, string CustomerName,
    int BranchId, string BranchName, IEnumerable<SaleItemIn> Items, bool Cancelled);

// ===== Saída =====
public class SaleDto
{
    public int Id { get; set; }
    public string Number { get; set; } = default!;
    public DateOnly Date { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = default!;
    public decimal Total { get; set; }
    public bool Cancelled { get; set; }
    public List<SaleItemOut> Items { get; set; } = new();

    public SaleDto() { } 
}

public record SaleItemOut(
    int ProductId, string ProductName, int Quantity,
    decimal UnitPrice, decimal DiscountPercent, decimal Total, bool Cancelled);
