using Multiple_Layered_Service.Library.Services.AuthRepo;
using Multiple_Layered_Service.Library.Services.AuthServices;

var builder = WebApplication.CreateBuilder(args);

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

// Distributed Cache
builder.Services.AddDistributedMemoryCache();

// Authorization
builder.Services.AddAuthorization();

// Scopes
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAuthService, AuthService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.RegisterEndpoints();

using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Database Migration Baþlatýlýyor.");
        await db.Database.MigrateAsync();

        logger.LogInformation("Seed Ýþlemi Baþlatýlýyor.");
        await AppSeed.SeedDataAsync(services);
        logger.LogInformation("Database Migration ve Seed Ýþlemi Baþarýyla Tamamlandý.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database Migration veya Seed Ýþlemi Sýrasýnda Bir Hata Oluþtu.");
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


app.Run();
