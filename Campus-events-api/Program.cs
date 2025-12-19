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

        // Authentication (JWT Bearer)
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Keep strict if you want:
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = signingKey,

                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = ClaimTypes.Name,     
                    RoleClaimType = ClaimTypes.Role    
                };
            });

        // Authorization
        builder.Services.AddAuthorization();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ClientPolicy", policy =>
            {
                policy
                    .WithOrigins("http://localhost:63342")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            
            options.EnableAnnotations();

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Campus Events API",
                Version = "v1",
                Description = "API for categories, events, bookings, and JWT-based authentication"
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter: Bearer {your JWT token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.UseHttpsRedirection();

        app.UseCors("ClientPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
