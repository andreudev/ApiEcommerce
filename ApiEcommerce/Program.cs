using System.Text;
using ApiEcommerce.Constants;
using ApiEcommerce.Models;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
string? dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(dbConnectionString)
);

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1 MB
    options.UseCaseSensitivePaths = true;
});

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder
    .Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");

if (string.IsNullOrEmpty(secretKey))
    throw new InvalidOperationException("SecretKey is not configured.");

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });

builder.Services.AddControllers(options =>
{
    options.CacheProfiles.Add(CacheProfiles.Default10, CacheProfiles.Profile10);
    options.CacheProfiles.Add(CacheProfiles.Default30, CacheProfiles.Profile30);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r"
                + "Ingresa la palabra a continuación el token generado en login.\n\r\n\r"
                + "Ejemplo: \"12345abcdef\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            },
        }
    );

    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "API V1",
            Version = "v1",
            Description = "Descripción de la API V1",
            TermsOfService = new Uri("https://example.com/terms"),
            Contact = new OpenApiContact { Name = "Andreu Dev", Email = "andreu.dev@example.com" },
            License = new OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://example.com/license"),
            },
        }
    );

    options.SwaggerDoc(
        "v2",
        new OpenApiInfo
        {
            Title = "API V2",
            Version = "v2",
            Description = "Descripción de la API V2",
            TermsOfService = new Uri("https://example.com/terms"),
            Contact = new OpenApiContact { Name = "Andreu Dev", Email = "andreu.dev@example.com" },
            License = new OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://example.com/license"),
            },
        }
    );
});

var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    // options.ApiVersionReader = ApiVersionReader.Combine(
    //     new QueryStringApiVersionReader("api-version")
    // );
});

apiVersioningBuilder.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        PolicyNames.AllowSpecificOrigin,
        builder =>
        {
            builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "API V2");
    });
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors(PolicyNames.AllowSpecificOrigin);

app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
