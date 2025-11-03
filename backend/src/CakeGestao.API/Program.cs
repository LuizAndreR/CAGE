using CakeGestao.API.Middlewares;
using CakeGestao.Application.Mappings;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.Services.Service;
using CakeGestao.Application.UseCases.Auth.Cadastro;
using CakeGestao.Application.UseCases.Auth.Login;
using CakeGestao.Application.UseCases.Auth.Refresh;
using CakeGestao.Application.UseCases.Receitas.Interface;
using CakeGestao.Application.UseCases.Receitas.UseCase;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Domain.Interfaces.Security;
using CakeGestao.Infrastructure.Data;
using CakeGestao.Infrastructure.Data.Repositories;
using CakeGestao.Infrastructure.Security;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
using Serilog;
using Serilog.Formatting.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        new JsonFormatter(),
        "logs/log.json",
        rollingInterval: RollingInterval.Day
    )
    .CreateLogger();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"]!,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"]!,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Host.UseSerilog();

builder.Services.AddDbContext<CageContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("CakeGestao.Infrastructure")));

builder.Services.AddAutoMapper(_ => {}, typeof(ReceitaProfile).Assembly);

builder.Services.AddControllers();

builder.Services.AddScoped<ICadastroUseCase, CadastroUseCase>();   
builder.Services.AddScoped<ILoginUseCase, LoginUseCase>();
builder.Services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();

builder.Services.AddScoped<ICreateReceitaUseCase, CreateReceitaUseCase>();
builder.Services.AddScoped<IGetReceitaUseCase, GetReceitaUseCase>();
builder.Services.AddScoped<IGetAllReceitaUseCase, GetAllReceitaUseCase>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReceitaService, ReceitaService>();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();        
builder.Services.AddScoped<IReceitaRepository, ReceitaRepository>();

builder.Services.AddValidatorsFromAssembly(typeof(CreateReceitaUseCase).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Iniciando a Api");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Api encerrado inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
