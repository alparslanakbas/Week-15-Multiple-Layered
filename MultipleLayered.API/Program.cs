var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Logging
builder.Services.AddLogging(opts =>
{
    opts.AddConsole();
});

// Db Context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
});

// Identity db context
builder.Services.AddIdentityConfiguration();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        await db.Database.MigrateAsync();
        await AppSeed.SeedDataAsync(app.Services);
        logger.LogInformation("Database Migration Ýþlemi Baþarýlý.");
    }
    catch (Exception)
    {
        await db.Database.EnsureDeletedAsync();
        logger.LogError("Database Migration Ýþlemi Baþarýsýz.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
