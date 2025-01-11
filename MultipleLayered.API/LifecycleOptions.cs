using Microsoft.Extensions.Options;
using Multiple_Layered_DataAccess.Library.Models;
using Multiple_Layered_Service.Library.Services.JwtServices;

namespace Multiple_Layered.API
{
    public static class LifecycleOptions
    {
        public static void AddLifecycle(this IServiceCollection services) 
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService>(provider =>
            {
                var tokenOptions = provider.GetRequiredService<IOptions<CustomTokenOptions>>().Value;
                var userManager = provider.GetRequiredService<UserManager<User>>();
                return new TokenService(tokenOptions, userManager);
            });
        }
    }
}
