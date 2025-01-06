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

            if (await context.Database.CanConnectAsync())
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Rolleri Ekle
                    if (!await context.Roles.AnyAsync())
                    {
                        if (!await roleManager.RoleExistsAsync("Admin"))
                        {
                            await roleManager.CreateAsync(new Role { Name = "Admin" });
                        }

                        if (!await roleManager.RoleExistsAsync("User"))
                        {
                            await roleManager.CreateAsync(new Role { Name = "User" });
                        }

                        await context.SaveChangesAsync();
                    }

                    // 2. Kullanıcıları Ekle
                    if (!await context.Users.AnyAsync())
                    {
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

                        var adminResult = await userManager.CreateAsync(adminUser, "A.lparslan123");
                        if (adminResult.Succeeded)
                        {
                            await userManager.AddToRoleAsync(adminUser, "Admin");
                        }

                        var userResult = await userManager.CreateAsync(normalUser, "K.ayhan123");
                        if (userResult.Succeeded)
                        {
                            await userManager.AddToRoleAsync(normalUser, "User");
                        }

                        await context.SaveChangesAsync();
                    }

                    // 3. Ürünleri ve İlgili Verileri Ekle
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
                        var adminUser = await userManager.FindByEmailAsync("admin@example.com");
                        if (adminUser != null)
                        {
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
}
