using System.Net.Mime;
using System.Reflection;
using System.Text;
using DeveloperStore.Infrastructure.Data;
using DeveloperStore.Infrastructure.Extensions;
using DeveloperStore.Infrastructure.ReadModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MediatR;
using FluentValidation;
using DeveloperStore.Application.Sales;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:7000");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DeveloperStore API", Version = "v1" });
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT Bearer token **_only_**",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
    cfg.RegisterServicesFromAssembly(typeof(DeveloperStore.Application.Mappings.AutoMapperProfile).Assembly);
});


builder.Services.AddValidatorsFromAssemblyContaining<SaleCreateValidator>();


builder.Services.AddScoped<SalesReadModel>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "devstore_dev_secret_key_please_change";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DeveloperStore";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DeveloperStoreAudience";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();


app.UseExceptionHandler(errApp =>
{
    errApp.Run(async ctx =>
    {
        var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
        var status = ex switch
        {
            KeyNotFoundException => 404,
            InvalidOperationException => 422,
            _ => 500
        };
        ctx.Response.ContentType = MediaTypeNames.Application.Json;
        ctx.Response.StatusCode = status;
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            type = status == 422 ? "ValidationError" : status == 404 ? "NotFound" : "ServerError",
            error = ex?.Message ?? "Unexpected error"
        });
        await ctx.Response.WriteAsync(payload);
    });
});

 
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DeveloperStore API v1");
    c.RoutePrefix = string.Empty; 
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeveloperStoreDbContext>();
    await DeveloperStore.Infrastructure.Seed.SeedData.EnsureSeedAsync(db);
}

app.Run();
