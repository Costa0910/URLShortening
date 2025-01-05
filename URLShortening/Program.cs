using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using URLShortening.Data;
using URLShortening.Data.Repository;
using URLShortening.Helpers;
using URLShortening.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<User, IdentityRole>(
        config =>
        {
            config.Tokens.AuthenticatorTokenProvider
                = TokenOptions.DefaultAuthenticatorProvider;
            config.SignIn.RequireConfirmedEmail = true;
        })
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<DataContext>();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(op =>
    {
        op.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = builder.Configuration["JWT:Audience"],
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!))
        };
    });

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var connectionString
    = builder.Configuration.GetConnectionString("DefaultConnection") ??
      throw new ArgumentException("Connection string not found");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo { Title = "URL Shortening", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
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
            new string[] { }
        }
    });
});


// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions
        = true; // Include version information in responses
});

// Add versioned API explorer (for Swagger compatibility)
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // e.g., v1
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<ICodeGeneratorHelper, CodeGeneratorHelper>();
builder.Services.AddScoped<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<IAccessLogRepository, AccessLogRepository>();
builder.Services.AddSingleton<IDeviceInfoHelper, DeviceInfoHelper>();
builder.Services.AddScoped<IGeoHelper, GeoHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Json(new
{
    Message = "App is running",
    Documentation = "http://localhost:port here/swagger/index.html",
    Timestamp = DateTime.UtcNow
}));

app.Map("/error", (HttpContext httpContext) =>
{
    var exception = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
    var statusCode = exception is FileNotFoundException
        ? StatusCodes.Status404NotFound
        : StatusCodes.Status500InternalServerError;

    return Results.Problem(
        detail: exception?.Message,
        statusCode: statusCode,
        title: "An error occurred"
    );
});

app.MapFallback(() => Results.NotFound(new
{
    Message = "The requested resource was not found.",
    Timestamp = DateTime.UtcNow
}));

await app.RunAsync();
