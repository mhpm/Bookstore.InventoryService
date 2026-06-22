using InventoryService.Middleware;
using MassTransit;
using InventoryService.Messaging;
using InventoryService.Data;
using InventoryService.Data.Repositories;
using InventoryService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(connectionString));

// DEPENDENCY INVERSION PRINCIPLE (DIP) & DEPENDENCY INJECTION (DI):
// Registramos las abstracciones IBookRepository e IBookStockService con sus respectivas
// clases concretas en el contenedor IoC. Así, cualquier clase (Handlers, Servicios, Consumidores)
// que requiera estas dependencias recibirá la instancia adecuada en tiempo de ejecución.
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookStockService, BookStockService>();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitUri = builder.Configuration["RabbitMQ:Uri"];
        if (!string.IsNullOrEmpty(rabbitUri))
        {
            cfg.Host(new Uri(rabbitUri));
        }
        else
        {
            var host = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
            cfg.Host(host, "/", h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });
        }

        cfg.ConfigureEndpoints(context);
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<InventoryDbContext>();

    int retries = 6;
    while (retries > 0)
    {
        try
        {
            logger.LogInformation("Intentando aplicar migraciones de base de datos...");
            db.Database.Migrate();
            logger.LogInformation("Migraciones aplicadas con éxito.");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogWarning(ex, "Error al aplicar migraciones. Reintentando en 5 segundos... ({Retries} intentos restantes)", retries);
            if (retries == 0) throw;
            Thread.Sleep(5000);
        }
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
