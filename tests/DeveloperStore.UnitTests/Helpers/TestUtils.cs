using System;
using AutoMapper;
using DeveloperStore.Application.Mappings;
using DeveloperStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.UnitTests.Helpers;

public static class TestUtils
{
    public static DeveloperStoreDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<DeveloperStoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        return new DeveloperStoreDbContext(options);
    }

    public static IMapper NewMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile(new AutoMapperProfile()));
        return cfg.CreateMapper();
    }
}
