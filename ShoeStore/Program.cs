
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Configuration;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Mappings;
using ShoeStore.Services;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Stripe;
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
                    //Localhost
                    options.UseNpgsql(builder.Configuration.GetConnectionString("ShoeStoreConnection"));
                });
            }
            //else
            //{
            //builder.Services.AddDbContext<ShoeStoreContext>(options =>
            //{
            //    //Neon PostgreSQL
            //    //options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionStrings__DefaultConnection"));
            //    options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"));
            //});
            //}

            //var testConnString = "Host=localhost;Port=5432;Database=shoe_store;Username=postgres;Password=123456789;SslMode=Disable";

            //builder.Services.AddDbContext<ShoeStoreContext>(options =>
            //{
            //    //Local PostgreSQL
            //    options.UseNpgsql(testConnString);
            //});

            var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            builder.Services.AddHealthChecks().AddNpgSql(connStr, name: "postgres");




            builder.Services.AddAutoMapper(cfg =>
                {
                    cfg.AddMaps(typeof(MappingProfile).Assembly);
                });

            builder.Services.AddScoped<ProductService, ProductService>();
            builder.Services.AddScoped<AuthService, AuthService>();
            builder.Services.AddScoped<PaymentService, PaymentService>();
            builder.Services.AddScoped<OrderService, OrderService>();
            builder.Services.AddScoped<UserService, UserService>();
            //builder.Services.AddScoped<PaymentIntentService, PaymentIntentService>();
            //builder.Services.AddScoped<PaymentIntentCreateOptions, PaymentIntentCreateOptions>();

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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
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
