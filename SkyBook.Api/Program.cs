using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SkyBook.Api.Data;
using SkyBook.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Supabase Postgres via EF Core ---------------------------------------
// Set ConnectionStrings:Supabase in appsettings.json / user-secrets / env
// vars. Get this from Supabase: Project Settings -> Database -> Connection
// string (URI) -> "Connection pooling" (recommended) or direct connection.
var connectionString = builder.Configuration.GetConnectionString("Supabase")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:Supabase in configuration.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- JWT settings + auth ---------------------------------------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<JwtTokenService>();

// SocialAuthService calls Google/Facebook over HTTP to verify tokens
// server-side — AddHttpClient gives it a pooled, DNS-refreshing HttpClient
// rather than a single long-lived instance.
builder.Services.AddHttpClient<SocialAuthService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Missing Jwt:Key in configuration.");

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
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    };
});

builder.Services.AddAuthorization();

// --- CORS: allow the Flutter app (web/dev) to call this API ---------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterApp", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFlutterApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
