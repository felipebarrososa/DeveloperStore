using System.ComponentModel.DataAnnotations;

namespace DeveloperStore.Application.DTOs;


public class CartDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public List<CartItemDto> Products { get; set; } = new(); 

    
    public CartDto() { }
}


public record CartItemDto(int ProductId, int Quantity);

public class CreateCartDto
{
    [Required] public int UserId { get; set; }
    [Required] public DateOnly Date { get; set; }
    [Required] public List<CartItemDto> Products { get; set; } = new();
}

public class UpdateCartDto : CreateCartDto { }
