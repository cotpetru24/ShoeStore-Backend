
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Configuration;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Mappings;
using ShoeStore.Services;
using System.Text;

namespace ShoeStore
{
    public class Program
    {
        public static void Main(string[] args)
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
            else
            {
                builder.Services.AddDbContext<ShoeStoreContext>(options =>
                {
                    //Neon PostgreSQL
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
                });
            }




                builder.Services.AddAutoMapper(cfg =>
                {
                    cfg.AddMaps(typeof(MappingProfile).Assembly);
                });

            builder.Services.AddScoped<ProductService, ProductService>();

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



            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
