# Multiple Layered API Project

Bu proje, .NET 9 kullanılarak geliştirilmiş katmanlı mimari yapısına sahip bir Web API projesidir.

## 🛠️ Kullanılan Teknolojiler

- .NET 9
- Entity Framework Core
- Identity Framework
- SQL Server
- NLog (Loglama için)
- JWT (Authentication için)
- Distributed Cache

## 📁 Proje Yapısı

Proje üç ana katmandan oluşmaktadır:

1. **API Layer**
   - Controllers/Endpoints
   - Middlewares (Exception ve Request Logging)
   - Filters (Time Restriction)
   - JWT Configurations
   - Swagger Configurations

2. **Service Layer**
   - Business Logic
   - DTOs
   - Service Implementations
   - Helpers
   - Pagination yapısı

3. **Data Access Layer**
   - Entity Configurations
   - Migrations
   - Repositories
   - Unit of Work Pattern
   - Database Context

## 🚀 Kurulum

1. Repository'yi klonlayın
```bash
git clone [repository-url]
```

2. Gerekli NuGet paketlerini yükleyin
```bash
dotnet restore
```

3. appsettings.json dosyasında veritabanı bağlantı dizesini güncelleyin
```json
"ConnectionStrings": {
    "SqlServerConnection": "your-connection-string"
}
```

4. Veritabanını oluşturun
```bash
dotnet ef database update
```

5. Projeyi çalıştırın
```bash
dotnet run
```

## ⚙️ Konfigürasyon

### JWT Ayarları
appsettings.json dosyasında JWT ayarlarını yapılandırın:
```json
"TokenOptions": {
    "Audience": ["your-audience"],
    "Issuer": "your-issuer",
    "AccessTokenExpiration": 60,
    "RefreshTokenExpiration": 600,
    "SecurityKey": "your-security-key"
}
```

### Loglama
NLog kullanılarak farklı log seviyeleri için yapılandırma yapılmıştır:
- Info logları
- Error logları
- Request/Response logları
- Performans logları

## 🔒 Authentication/Authorization

- JWT tabanlı kimlik doğrulama
- Role bazlı yetkilendirme
- Refresh token desteği

## 🌟 Özellikler

- Exception Middleware ile merkezi hata yönetimi
- Request Logging Middleware ile tüm isteklerin loglanması
- Time Restrict Filter ile API erişim zaman kısıtlaması
- Distributed Cache yapısı
- Pagination desteği
- Swagger UI ile API dokümantasyonu

## 📝 Notlar

- API'ye erişim için JWT token gereklidir
- Varsayılan olarak yeni kayıt olan kullanıcılar "User" rolüne atanır
- API erişimi belirli saat aralıklarında kısıtlanabilir
- Tüm istekler ve hatalar loglanır

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'feat: Add amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📄 Lisans

[MIT License](LICENSE)

## 📞 İletişim

- Proje Sahibi: Alparslan Akbaş
- Email: alparslan43341@gmail.com
- LinkedIn: [LinkedIn Profili](https://www.linkedin.com/in/alparslan-akbas/)
- GitHub: [GitHub Profili](https://github.com/Alparslan524)