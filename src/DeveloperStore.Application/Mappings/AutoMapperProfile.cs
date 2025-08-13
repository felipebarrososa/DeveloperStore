using System.Linq;
using AutoMapper;
using DeveloperStore.Application.DTOs;
using DeveloperStore.Domain.Entities;

namespace DeveloperStore.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Products
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Rate, o => o.MapFrom(s => s.Rating.Rate))
            .ForMember(d => d.Count, o => o.MapFrom(s => s.Rating.Count));

        CreateMap<CreateProductDto, Product>()
            .ForPath(d => d.Rating.Rate, o => o.MapFrom(s => s.Rate))
            .ForPath(d => d.Rating.Count, o => o.MapFrom(s => s.Count));

        CreateMap<UpdateProductDto, Product>()
            .ForPath(d => d.Rating.Rate, o => o.MapFrom(s => s.Rate))
            .ForPath(d => d.Rating.Count, o => o.MapFrom(s => s.Count));

        // Users
        CreateMap<User, UserDto>()
            .ForMember(d => d.Firstname, o => o.MapFrom(s => s.Name.Firstname))
            .ForMember(d => d.Lastname, o => o.MapFrom(s => s.Name.Lastname))
            .ForMember(d => d.City, o => o.MapFrom(s => s.Address.City))
            .ForMember(d => d.Street, o => o.MapFrom(s => s.Address.Street))
            .ForMember(d => d.Number, o => o.MapFrom(s => s.Address.Number))
            .ForMember(d => d.Zipcode, o => o.MapFrom(s => s.Address.Zipcode))
            .ForMember(d => d.Lat, o => o.MapFrom(s => s.Address.Geo.Lat))
            .ForMember(d => d.Long, o => o.MapFrom(s => s.Address.Geo.Long));

        // Carts
        CreateMap<Cart, CartDto>()
            .ForMember(d => d.Products,
                o => o.MapFrom(s => s.Items.Select(i => new CartItemDto(i.ProductId, i.Quantity))));

        // === Sales (NOVO) ===
        // Item da venda -> DTO de saída
        CreateMap<SaleItem, SaleItemOut>();
        // Venda -> DTO de saída (inclui lista de itens)
        CreateMap<Sale, SaleDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
    }
}
