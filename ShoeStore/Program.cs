
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Configuration;
using ShoeStore.DataContext.PostgreSQL;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Services;
using Stripe;
using System.Text;
using System.Text.Json.Serialization;
using ProductService = ShoeStore.Services.ProductService;

namespace ShoeStore
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            var isDevelopment = builder.Environment.IsDevelopment();
            //var isDevelopment = false;

            string connKey = isDevelopment
                ? "ShoeStoreConnection"
                : "DefaultConnection";

            var connectionString = Environment.GetEnvironmentVariable($"ConnectionStrings__{connKey}");


            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{connKey}' is not configured.");
            }

            builder.Services.AddDbContext<ShoeStoreContext>(options =>
            {
                options.UseNpgsql(
                    connectionString,
                    o => o.EnableRetryOnFailure(0)
                );
            });

            builder.Services.AddDbContext<IdentityContext>(options =>
            {
                options.UseNpgsql(
                    connectionString,
                    o => o.EnableRetryOnFailure(0)
                );
            });


            var healthCheckConnStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            if (!string.IsNullOrEmpty(healthCheckConnStr))
            {
                builder.Services.AddHealthChecks().AddNpgSql(healthCheckConnStr, name: "postgres");
            }

            builder.Services.AddScoped<ProductService, ProductService>();
            builder.Services.AddScoped<AuthService, AuthService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<OrderService, OrderService>();
            builder.Services.AddScoped<UserService, UserService>();
            builder.Services.AddScoped<AddressService, AddressService>();
            builder.Services.AddScoped<AdminDashboardService, AdminDashboardService>();
            builder.Services.AddScoped<AdminUserService, AdminUserService>();
            builder.Services.AddScoped<AdminOrderService, AdminOrderService>();
            builder.Services.AddScoped<AdminProductService, AdminProductService>();
            builder.Services.AddScoped<CmsService, CmsService>();

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
                .AddJwtBearer(options =>
                {
                    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                        ClockSkew = TimeSpan.Zero,
                    };
                });


            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontEnd", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .WithHeaders("Content-Type", "Authorization")
                          .WithMethods("GET", "POST", "PUT", "DELETE")
                          .AllowCredentials();
                });
            });


            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("Stripe__SecretKey");


            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (!app.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
            {
                using (var scope = app.Services.CreateScope())
                {
                    var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
                    await authService.SeedAdminAccount();
                }

                using (var scope = app.Services.CreateScope())
                {
                    var cmsSevice = scope.ServiceProvider.GetRequiredService<CmsService>();
                    await cmsSevice.SeedDefaultProfile();
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                    var logger = context.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    if (exception != null)
                    {
                        logger.LogError(exception, "Unhandled exception");
                    }

                    var (statusCode, title) = exception switch
                    {
                        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                        KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                        ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
                        InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
                        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
                    };

                    var problem = new ProblemDetails
                    {
                        Title = title,
                        Status = statusCode,
                        Detail = isDevelopment ? exception?.Message : "Unexpected error occurred."
                    };

                    problem.Extensions["traceId"] = context.TraceIdentifier;

                    context.Response.StatusCode = problem.Status.Value;
                    context.Response.ContentType = "application/problem+json";

                    await context.Response.WriteAsJsonAsync(problem);
                });
            });


            app.UseHttpsRedirection();
            app.UseCors("FrontEnd");


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
