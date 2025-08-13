using AutoMapper;
using DeveloperStore.Application.Mappings;
using DeveloperStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DeveloperStore.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Postgres") 
                 ?? "Host=localhost;Port=5432;Database=developerstore;Username=devuser;Password=devpass";
        services.AddDbContext<DeveloperStoreDbContext>(opt => { opt.UseNpgsql(cs); });

        
        var mongoCs = config.GetConnectionString("Mongo") ?? "mongodb://localhost:27017";
        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoCs));
        services.AddScoped(provider => provider.GetRequiredService<IMongoClient>().GetDatabase("developerstore"));

        services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

        return services;
    }
}
