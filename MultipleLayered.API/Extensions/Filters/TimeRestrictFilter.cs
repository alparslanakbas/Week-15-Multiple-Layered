namespace Multiple_Layered.API.Extensions.Filters
{
    public class TimeRestrictFilter : IEndpointFilter
    {
        private readonly TimeOnly _startTime;
        private readonly TimeOnly _endTime;
        private readonly ILogger<TimeRestrictFilter> _logger;

        public TimeRestrictFilter(ILogger<TimeRestrictFilter> logger,
            string startTime = "06:00", string endTime = "23:59")
        {
            _logger = logger;
            _startTime = TimeOnly.Parse(startTime);
            _endTime = TimeOnly.Parse(endTime);
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);

            if (currentTime < _startTime || currentTime > _endTime)
            {
                _logger.LogWarning("Mesai saatleri dışında erişim denemesi: {Time}", currentTime);

                return Results.Json(new
                {
                    Error = "Bu API'ye sadece mesai saatleri içinde erişilebilir, eğer bu ayarı kapatmak istiyorsanız endpointten filterı kaldırın.",
                    WorkingHours = $"{_startTime} - {_endTime}",
                    CurrentTime = currentTime.ToString()
                }, statusCode: StatusCodes.Status403Forbidden);
            }

            _logger.LogInformation("API erişimi başarılı: {Time}", currentTime);
            return await next(context);
        }
    }
}
