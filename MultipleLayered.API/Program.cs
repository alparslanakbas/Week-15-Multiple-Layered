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
builder.Services.AddLifecycle();

// Token settings
builder.Services.Configure<CustomTokenOptions>(builder.Configuration.GetSection("TokenOptions"));

// Jwt settings
builder.Services.AddJwtOptions(builder.Configuration);

// Swagger settings
builder.Services.ConfigureSwagger();


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
    app.UseSwaggerUI(opts =>
    {
        opts.SwaggerEndpoint("/swagger/v1/swagger.json", "Multiple-Layer.Api v1.0.0");
        opts.InjectStylesheet("/swagger-css/swagger-custom-ui.css");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();


app.Run();
