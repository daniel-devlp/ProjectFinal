using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Project.Application.Services;
using Project.Application.Settings;
using Project.Domain.Interfaces;
using Project.Infrastructure.Frameworks.EntityFramework;
using Project.Infrastructure.Frameworks.Identity;
using Project.Infrastructure.Repositories;
using Project.Infrastructure.Services;
using System.Text;
using CloudinaryDotNet;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // SQL Server Database Context (actual)
            builder.Services.AddDbContext<ApplicationDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            // PostgreSQL Database Context (preparado para futura migración - comentado)
            /*
            builder.Services.AddDbContext<ApplicationDBContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection"),
                    b => b.MigrationsAssembly("Project.Infrastructure"))
            );
            */

            // Identity configuration
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 4;
                options.Password.RequiredUniqueChars = 1;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.AllowedForNewUsers = true;
            })

                .AddEntityFrameworkStores<ApplicationDBContext>()
                .AddDefaultTokenProviders()
                .AddPasswordValidator<CustomPasswordValidator>();

            // JWT Configuration
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ??
                "F2C7#9z8l8$4b6@e5!r2v7w1q3x6n4u3p0s9d7mZ8kL4nQ1tY6wE9rT2yU5iO0pA3sD6fG9hJ2kL5nM8bV1cX4zQ7w");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });


            // Cloudinary Configuration
            Console.WriteLine("☁️ Configurando Cloudinary...");
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

            builder.Services.AddSingleton(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var cloudinarySection = configuration.GetSection("CloudinarySettings");

                var cloudName = cloudinarySection["CloudName"]?.Trim();
                var apiKey = cloudinarySection["ApiKey"]?.Trim();
                var apiSecret = cloudinarySection["ApiSecret"]?.Trim();

                if (string.IsNullOrWhiteSpace(cloudName) ||
                        string.IsNullOrWhiteSpace(apiKey) ||
                    string.IsNullOrWhiteSpace(apiSecret))
                {
                    throw new InvalidOperationException("Configuración de Cloudinary incompleta en appsettings.json");
                }

                var account = new Account(cloudName, apiKey, apiSecret);
                var cloudinary = new Cloudinary(account);

                try
                {
                    var testResult = cloudinary.Api.Account;
                    Console.WriteLine($"✅ Cloudinary configurado exitosamente - CloudName: {testResult.Cloud}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error al configurar Cloudinary: {ex.Message}");
                    throw new InvalidOperationException($"Fallo en configuración de Cloudinary: {ex.Message}");
                }

                return cloudinary;
            });



            // Dependency Injection - Clean Architecture
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();

            // Application Services
            builder.Services.AddScoped<IClientServices, ClientServices>();
            builder.Services.AddScoped<IProductServices, ProductService>();
            builder.Services.AddScoped<IInvoiceServices, InvoiceService>();
            builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
            builder.Services.AddScoped<IInvoiceNumberService, InvoiceNumberService>(); // ✅ Nuevo servicio

            // Image Service - Cloudinary
            builder.Services.AddScoped<IImageService, ImageService>();

            // Servicios para módulo de pagos - ACTIVADO
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();

            // Infrastructure Services
            builder.Services.AddScoped<PasswordHistoryService>();
            builder.Services.AddScoped<CustomPasswordValidator>();
            builder.Services.AddScoped<IRoleValidationService, Project.Infrastructure.Services.RoleValidationService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Configure Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ProjectFinal API - E-commerce Complete",
                    Version = "v1",
                    Description = "Clean Architecture API with Shopping Cart, Payment System & Image Upload - Ready for Mobile"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. 
                        Enter: 'Bearer {token}' (include the word Bearer and a space before the token)",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectFinal API V1");
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}