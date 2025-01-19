var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

try
{
    logger.Info("Uygulama Ba�lat�l�yor.");
    var builder = WebApplication.CreateBuilder(args);

    // Logging
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();


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
    app.ConfigureCustomExceptionMiddleware();

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

    using (var scope = app.Services.CreateAsyncScope())
    {
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<AppDbContext>();
        try
        {
            logger.Info("Database Migration Ba�lat�l�yor.");
            await db.Database.MigrateAsync();

            logger.Info("Seed ��lemi Ba�lat�l�yor.");
            await AppSeed.SeedDataAsync(services);
            logger.Info("Database Migration ve Seed ��lemi Ba�ar�yla Tamamland�.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Database Migration veya Seed ��lemi S�ras�nda Bir Hata Olu�tu.");
            throw;
        }
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseStaticFiles();

    app.RegisterEndpoints();

    app.Run();

}
catch (Exception ex)
{
    logger.Error($"Uygulama Ba�lat�l�rken Bir Hata Olu�tu: {ex}");
    throw;
}

finally
{
    logger.Info("Uygulama Kapat�l�yor.");
    LogManager.Shutdown();
}