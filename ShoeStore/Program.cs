
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Configuration;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Services;
using Stripe;
using System.Text;
using System.Text.Json.Serialization;
using ProductService = ShoeStore.Services.ProductService;

namespace ShoeStore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));


            var isDevelopment = builder.Environment.IsDevelopment();

            if (isDevelopment)
            {
                builder.Services.AddDbContext<ShoeStoreContext>(options =>
                {
                    options.UseNpgsql(builder.Configuration.GetConnectionString("ShoeStoreConnection"));
                });
            }

            var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            builder.Services.AddHealthChecks().AddNpgSql(connStr, name: "postgres");



            builder.Services.AddScoped<ProductService, ProductService>();
            builder.Services.AddScoped<AuthService, AuthService>();
            builder.Services.AddScoped<PaymentService, PaymentService>();
            builder.Services.AddScoped<OrderService, OrderService>();
            builder.Services.AddScoped<UserService, UserService>();
            builder.Services.AddScoped<AdminService, AdminService>();
            builder.Services.AddScoped<CmsService, CmsService>();

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ShoeStoreContext>()
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



            var test = Environment.GetEnvironmentVariable("Stripe__SecretKey");

            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("Stripe__SecretKey");


            builder.Services.AddControllers()
                .AddJsonOptions(option =>
                {
                    option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

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

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                        var logger = context.RequestServices
                            .GetRequiredService<ILogger<Program>>();

                        logger.LogError(exception, "Unhandled exception");

                        var problem = new ProblemDetails
                        {
                            Title = "Internal Server Error",
                            Status = StatusCodes.Status500InternalServerError,
                            Detail = "Unexpected error occurred."
                        };

                        context.Response.StatusCode = problem.Status.Value;
                        context.Response.ContentType = "application/problem+json";

                        await context.Response.WriteAsJsonAsync(problem);
                    });
                });
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }



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
