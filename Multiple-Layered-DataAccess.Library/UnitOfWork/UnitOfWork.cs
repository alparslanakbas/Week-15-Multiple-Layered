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

        public async Task RefreshCacheAsync()
        {
            await ClearCacheAsync();

            var users = await _context.Users.ToListAsync();
            var products = await _context.Products.ToListAsync();
            var orders = await _context.Orders.ToListAsync();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(30),
            };

            await _cache.SetAsync("users_cache", Serialize(users), options);
            await _cache.SetAsync("products_cache", Serialize(products), options);
            await _cache.SetAsync("orders_cache", Serialize(orders), options);
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

        private byte[] Serialize<T>(T obj) => JsonSerializer.SerializeToUtf8Bytes(obj);
    }
}
