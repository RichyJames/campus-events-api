using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Campus_events_api.Data;
using Campus_events_api.Repositories;
using Campus_events_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Campus_events_api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        
        // Services

        // Controllers
        builder.Services.AddControllers();

        // DbContext - SQLite
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        builder.Services.AddScoped<IEventRepository, EventRepository>();

        // JWT settings & token service
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Read JWT settings for auth configuration
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key is missing");
        var issuer = jwtSection.GetValue<string>("Issuer") ?? throw new InvalidOperationException("Jwt:Issuer is missing");
        var audience = jwtSection.GetValue<string>("Audience") ?? throw new InvalidOperationException("Jwt:Audience is missing");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        // Authentication
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Relaxed rules so stuff can't randomly fail
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    // We still validate the signature
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,

                    // validate expiry, but no slack window
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = ClaimTypes.Role
                };
            });


        // Authorization
        builder.Services.AddAuthorization();

        // Swagger + JWT support
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Campus-events-api",
                Version = "v1"
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter 'Bearer {your JWT token}'",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });
        });

        var app = builder.Build();

       
        // Middleware pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
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
