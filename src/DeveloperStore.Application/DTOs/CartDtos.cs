using System.ComponentModel.DataAnnotations;

namespace DeveloperStore.Application.DTOs;

// DTO de leitura
public class CartDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public List<CartItemDto> Products { get; set; } = new(); // evita null

    // construtor sem parâmetros exigido pelo pipeline/reflection
    public CartDto() { }
}

// pode manter como record posicional (usamos no AutoMapper ao projetar os itens)
public record CartItemDto(int ProductId, int Quantity);

// DTOs de escrita (entrada)
public class CreateCartDto
{
    [Required] public int UserId { get; set; }
    [Required] public DateOnly Date { get; set; }
    [Required] public List<CartItemDto> Products { get; set; } = new();
}

public class UpdateCartDto : CreateCartDto { }
