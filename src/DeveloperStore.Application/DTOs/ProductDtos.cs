namespace DeveloperStore.Application.DTOs;

// ===== Saída =====
public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public decimal Price { get; set; }
    public string Description { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string Image { get; set; } = default!;
    public decimal Rate { get; set; }
    public int Count { get; set; }

    public ProductDto() { } 
}

// ===== Entrada =====
public record CreateProductDto(string Title, decimal Price, string Description, string Category, string Image, decimal Rate, int Count);
public record UpdateProductDto(string Title, decimal Price, string Description, string Category, string Image, decimal Rate, int Count);
