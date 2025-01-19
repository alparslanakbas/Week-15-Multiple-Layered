namespace Multiple_Layered_DataAccess.Library.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        readonly AppDbContext _context;
        readonly IDistributedCache _cache;

        private IDbContextTransaction _transaction;
        private IRepository<User> _users;
        private IRepository<Product> _products;
        private IRepository<Order> _orders;
        private IRepository<OrderProduct> _orderProducts;

        public UnitOfWork(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IRepository<User> Users => _users ??= new Repository<User>(_context);

        public IRepository<Product> Products => _products ??= new Repository<Product>(_context);

        public IRepository<Order> Orders => _orders ??= new Repository<Order>(_context);

        public IRepository<OrderProduct> OrderProducts => _orderProducts ??= new Repository<OrderProduct>(_context);

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task ClearCacheAsync()
        {
            var cacheKeys = new[]
            {
                "users_cache",
                "products_cache",
                "orders_cache",
                "order_products_cache"
            };

            foreach (var key in cacheKeys)
            {
                await _cache.RemoveAsync(key);
            }

        }

        public async Task CommitAsync()
        {
            try
            {
                await _transaction.CommitAsync();
            }
            catch (Exception)
            {
                await _transaction.RollbackAsync();
                throw;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
            _transaction?.Dispose();
        }

        public async Task<T?> GetFromCacheAsync<T>(string key)
        {
            var cachedData = await _cache.GetAsync(key);
            if (cachedData is null)
                return default;

            return JsonSerializer.Deserialize<T>(cachedData);
        }

        public async Task RefreshCacheAsync()
        {
            await ClearCacheAsync();

            var users = await _context.Users.ToListAsync();
            var products = await _context.Products.ToListAsync();
            var orders = await _context.Orders.ToListAsync();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(5),
                SlidingExpiration = TimeSpan.FromMinutes(10),
            };

            await CreatePagedCache(products, p => new ListAllProductDto(p.Id, p.Name, p.Price, p.Stock), "products_cache", options);
            await CreatePagedCache(users, u => new ListAllUserDto
            (u.Id, u.FirstName, u.LastName, u.UserName!, u.Email!, u.PhoneNumber, u.EmailConfirmed, u.PhoneNumberConfirmed, u.TwoFactorEnabled), 
            "users_cache", options);
            await CreatePagedCache(orders, o => new ListAllOrderDto(o.Id, o.OrderDate, o.TotalAmount, o.UserId, o.User.FirstName + o.User.LastName), "orders_cache", options);
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task SetToCacheAsync<T>(string key, T value)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(10),
            };

            await _cache.SetAsync(key, Serialize(value), options);
        }

        private byte[] Serialize<T>(T obj) => JsonSerializer.SerializeToUtf8Bytes(obj);
        private async Task CreatePagedCache<T, TDto>(IEnumerable<T> items, Func<T, TDto> dtoMapper, string cacheKeyPrefix, DistributedCacheEntryOptions options)
        {
            var pagination = new Pagination(1, 10);

            var paginatedItems = items
                .Skip((pagination.Page - 1) * pagination.Size)
                .Take(pagination.Size);

            var dtos = paginatedItems.Select(dtoMapper);

            var pagedResult = new PagedResult<TDto>
            (
                dtos,
                items.Count(),
                pagination.Page,
                pagination.Size
            );

            var cacheKey = CacheHelpers.GenerateCacheKey(cacheKeyPrefix, pagination);
            await _cache.SetAsync(cacheKey, Serialize(pagedResult), options);
        }
    }
}
