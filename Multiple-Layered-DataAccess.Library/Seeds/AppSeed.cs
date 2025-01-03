namespace Multiple_Layered_DataAccess.Library.Seeds
{
    public static class AppSeed
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // 1. Rolleri Ekle
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new Role { Name = "Admin" });
                    await context.SaveChangesAsync();
                }

                if (!await roleManager.RoleExistsAsync("User"))
                {
                    await roleManager.CreateAsync(new Role { Name = "User" });
                    await context.SaveChangesAsync();
                }

                // 2. Kullanıcıyı Ekle

                var adminUser = new User
                {
                    UserName = "Alparslan Akbaş",
                    Email = "admin@example.com",
                    FirstName = "Alparslan",
                    LastName = "Akbaş",
                    EmailConfirmed = true
                };

                var normalUser = new User
                {
                    UserName = "Kayhan Akbaş",
                    Email = "user@example.com",
                    FirstName = "Kayhan",
                    LastName = "Akbaş",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "A.lparslan123");
                if (!result.Succeeded) throw new Exception("Admin rolü oluşturulamadı.");

                result = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (!result.Succeeded) throw new Exception("Admin rolü eklenemedi.");

                var result2 = await userManager.CreateAsync(normalUser, "K.ayhan123");
                if (!result2.Succeeded) throw new Exception("User rolü oluşturulamadı.");

                result2 = await userManager.AddToRoleAsync(normalUser, "User");
                if (!result2.Succeeded) throw new Exception("User rolü eklenemedi.");

                await context.SaveChangesAsync();


                // 3. Ürünleri Ekle
                if (!await context.Products.AnyAsync())
                {
                    var products = new[]
                    {
                new Product { Id = Guid.NewGuid(), Name = "Laptop", Price = 15000, Stock = 50 },
                new Product { Id = Guid.NewGuid(), Name = "Ekran", Price = 5000, Stock = 100 }
            };

                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();

                    // 4. Sipariş Ekle
                    var order = new Order
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminUser.Id,
                        OrderDate = DateTime.UtcNow,
                        TotalAmount = products.Sum(p => p.Price)
                    };

                    await context.Orders.AddAsync(order);
                    await context.SaveChangesAsync();

                    // 5. Sipariş-Ürün İlişkilerini Ekle
                    var orderProducts = products.Select(p => new OrderProduct
                    {
                        OrderId = order.Id,
                        ProductId = p.Id,
                        Quantity = 1
                    });

                    await context.OrderProducts.AddRangeAsync(orderProducts);
                    await context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
