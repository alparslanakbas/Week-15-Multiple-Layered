using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Multiple_Layered.API
{
    public static class SwaggerOptions
    {
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(setup =>
            {
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "API Kimlik Doğrulama",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Aldığınız Token'ı Girin",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    },

                };

                var openApiInfo = new OpenApiInfo
                {
                    Title = "Multiple Layered API",
                    Version = "V1",
                    Description = @"Multiple-Layered.API.v1.0.0 Için Yapılan Ilk API.<br> 
                                    Debug İşleminden Sonra Bu Ekranı Görüyorsanız Log Kaydını Kontrol Ediniz.<br> 
                                    Kullanıcılar, Roller, Claimler için Birer Örnek Data Yüklenecektir.<br> 
                                    Giriş Bilgileri; <br>
                                    <strong> Email: admin@example.com <br>
                                    Şifre: A.lparslan123 </strong> <br>
                                    Altta Kişisel Web Sitelerim Bulunmaktadır.",

                    Contact = new OpenApiContact
                    {
                        Name = "Alparslan Akbaş",
                        Email = "bybluestht@gmail.com",
                        Url = new System.Uri("https://www.linkedin.com/in/alparslanakbas/"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Alparslan Akbas Github:",
                        Url = new System.Uri("https://github.com/Alparslanakbas")
                    },
                    TermsOfService = new System.Uri("https://alparslanakbas.github.io"),
                };

                setup.SwaggerDoc("v1", openApiInfo);

                setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });
        }
    }
}
