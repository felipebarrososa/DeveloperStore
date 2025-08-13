using MediatR;
using DeveloperStore.Application.DTOs;

namespace DeveloperStore.Application.Sales;

public record CreateSaleCommand(SaleCreateDto Dto) : IRequest<SaleDto>;
public record UpdateSaleCommand(int Id, SaleUpdateDto Dto) : IRequest;
public record CancelSaleCommand(int Id) : IRequest;
public record CancelSaleItemCommand(int Id, int ItemId) : IRequest;
public record GetSaleByIdQuery(int Id) : IRequest<SaleDto?>;
public record GetSalesQuery(int Page, int Size, string? Order, DateOnly? From, DateOnly? To, string? Customer, string? Branch) : IRequest<(IEnumerable<SaleDto> data, int total)>;
