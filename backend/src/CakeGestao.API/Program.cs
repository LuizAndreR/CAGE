using CakeGestao.API.Middlewares;
using CakeGestao.Application.Mappings;
using CakeGestao.Application.Services.Interface;
using CakeGestao.Application.Services.Service;
using CakeGestao.Application.UseCases.Receitas.Interface;
using CakeGestao.Application.UseCases.Receitas.UseCase;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Infrastructure.Data;
using CakeGestao.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Json;
using FluentValidation;

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

builder.Host.UseSerilog();

builder.Services.AddDbContext<CageContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("CakeGestao.Infrastructure")));

builder.Services.AddAutoMapper(_ => {}, typeof(ReceitaProfile).Assembly);

builder.Services.AddControllers();

builder.Services.AddScoped<ICreateReceitaUseCase, CreateReceitaUseCase>();

builder.Services.AddScoped<IReceitaService, ReceitaService>();

builder.Services.AddScoped<IReceitaRepository, ReceitaRepository>();

builder.Services.AddValidatorsFromAssembly(typeof(CreateReceitaUseCase).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

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
