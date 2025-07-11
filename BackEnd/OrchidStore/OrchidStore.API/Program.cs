using JasperFx;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenIddict.Abstractions;
using OrchidStore.API;
using OrchidStore.API.SystemClient;
using OrchidStore.Application.Features.Accounts.Commands;
using OrchidStore.Application.Repositories;
using OrchidStore.Domain.ReadModels;
using OrchidStore.Domain.WriteModels;
using OrchidStore.Infrastructure.Data.Contexts;
using OrchidStore.Infrastructure.Data.Helpers;
using OrchidStore.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

#region Dependency Injection Setup

// Register OpenAPI/Swagger
builder.Services.AddOpenApi();

// Register EF Core context (relational database)
var connectionString = builder.Configuration.GetConnectionString("OrchidStoreDB");

builder.Services.AddDbContext<OrchidStoreContext, AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseOpenIddict();
});


builder.Services.AddHostedService<Worker>();

// Register Marten for event sourcing / document store
builder.Services.AddMarten(options =>
{
    options.Connection(connectionString!);
    options.AutoCreateSchemaObjects = AutoCreate.All;
    options.DatabaseSchemaName = "OrchidStoreDB_Marten";

    // Define identity keys for Marten collections
    options.Schema.For<AccountCollection>().Identity(x => x.AccountId);
    options.Schema.For<CategoryCollection>().Identity(x => x.CategoryId);
    options.Schema.For<OrchidCollection>().Identity(x => x.OrchidId);
    options.Schema.For<OrderCollection>().Identity(x => x.Id);
    options.Schema.For<OrderDetailCollection>().Identity(x => x.Id);
    options.Schema.For<RoleCollection>().Identity(x => x.RoleId);
});

// Register MediatR for CQRS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AccountRegisterCommand).Assembly);
});

// System services
builder.Services.AddScoped<IIdentityApiClient, IdentityApiClient>();

// Register generic and concrete repositories
builder.Services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
builder.Services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));

builder.Services.AddScoped<ICommandRepository<Account>, CommandRepository<Account>>();
builder.Services.AddScoped<ICommandRepository<Role>, CommandRepository<Role>>();
builder.Services.AddScoped<ICommandRepository<Category>, CommandRepository<Category>>();
builder.Services.AddScoped<ICommandRepository<Orchid>, CommandRepository<Orchid>>();
builder.Services.AddScoped<ICommandRepository<Order>, CommandRepository<Order>>();
builder.Services.AddScoped<ICommandRepository<OrderDetail>, CommandRepository<OrderDetail>>();

builder.Services.AddScoped<IQueryRepository<Account>, QueryRepository<Account>>();
builder.Services.AddScoped<IQueryRepository<Category>, QueryRepository<Category>>();
builder.Services.AddScoped<IQueryRepository<Orchid>, QueryRepository<Orchid>>();
builder.Services.AddScoped<IQueryRepository<Order>, QueryRepository<Order>>();
builder.Services.AddScoped<IQueryRepository<OrderDetail>, QueryRepository<OrderDetail>>();

#endregion

#region ▓▓ Swagger (OpenAPI) Configuration ▓▓

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Orchid Store API",
        Version = "v1"
    });

    c.TagActionsBy(api =>
    {
        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        
        // Handle special cases for AuthController
        if (controllerName?.Equals("Auth", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new[] { "Authentication - OAuth2 & OpenIddict" };
        }
        
        var screenCode = controllerName?.Substring(0, 1);
        
        var groupName = screenCode switch
        {
            "I" => "Insert - Add new records",
            "S" => "Select - Retrieve records",
            "U" => "Update - Modify existing records", 
            "D" => "Delete - Remove records", 
            _ => controllerName ?? "Default"
        };
        
        return new[] { groupName };
    });
   
    
    c.CustomSchemaIds(type => type.FullName);

    // Add JWT bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

#region ▓▓ CORS Configuration ▓▓

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

#endregion

#region ▓▓ OpenIddict Server Configuration ▓▓

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<AppDbContext>();
    })
    .AddServer(options =>
    {
        options.DisableAccessTokenEncryption();
        options.AcceptAnonymousClients();

        // Register endpoints
        options.SetTokenEndpointUris("/connect/token");
        options.SetIntrospectionEndpointUris("/connect/introspect");
        options.SetUserInfoEndpointUris("/connect/userinfo");
        options.SetEndSessionEndpointUris("/connect/logout");
        options.SetAuthorizationEndpointUris("/connect/authorize");

        // Enable supported flows
        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();
        options.AllowClientCredentialsFlow();
        options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
        options.AllowCustomFlow("google");
        options.AllowCustomFlow("logout");
        options.AllowCustomFlow("external");

        // Use reference tokens
        options.UseReferenceAccessTokens();
        options.UseReferenceRefreshTokens();
        options.DisableAccessTokenEncryption();

        // Configure certificates (for development)
        options.AddDevelopmentEncryptionCertificate();
        options.AddDevelopmentSigningCertificate();

        // Register scopes
        options.RegisterScopes(
            OpenIddictConstants.Scopes.OfflineAccess,
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles);

        // Set token lifetimes
        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(60));
        options.SetRefreshTokenLifetime(TimeSpan.FromMinutes(120));

        // ASP.NET Core integration
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .DisableTransportSecurityRequirement(); // For dev only
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

#endregion

#region ▓▓ App Host Configuration ▓▓

builder.Services.AddControllers(); 
builder.Services.AddAuthorization();

#endregion

var app = builder.Build();

#region ▓▓ Middleware Pipeline Configuration ▓▓

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePages();
app.UseHttpsRedirection();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

#endregion

app.Run();