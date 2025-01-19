namespace Multiple_Layered.API.Endpoints.Order
{
    public class OrderEndpoints : IEndpointDefinition
    {
        readonly ILogger<OrderEndpoints> _logger;

        public OrderEndpoints(ILogger<OrderEndpoints> logger)
        {
            _logger = logger;
        }

        public void RegisterEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/order");

            group.MapGet("/all", GetAllOrdersAsync)
                .WithName("GetAllOrders")
                .WithDescription("Tüm Siparişleri Getirir")
                .Produces<IEnumerable<ListAllOrderDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError)
                .AddEndpointFilter<TimeRestrictFilter>();

            group.MapGet("/{id}", GetOrderByIdAsync)
                .WithName("GetOrderById")
                .WithDescription("ID'ye Göre Sipariş Getirir")
                .Produces<ListAllOrderDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapPost("/", CreateOrderAsync)
                .WithName("CreateOrder")
                .WithDescription("Yeni Sipariş Oluşturur")
                .Produces<CreateOrderDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .RequireAuthorization();

            group.MapPut("/", UpdateOrderAsync)
                .WithName("UpdateOrder")
                .WithDescription("Sipariş Günceller")
                .Produces<UpdateOrderDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .RequireAuthorization();

            group.MapDelete("/{id}", DeleteOrderAsync)
                .WithName("DeleteOrder")
                .WithDescription("Sipariş Siler")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .RequireAuthorization();
        }

        private async Task<IResult> GetAllOrdersAsync([FromServices] IOrderService orderService, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                var pagination = new Pagination(page, size);
                var result = await orderService.GetAllAsync(pagination);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siparişler listelenirken hata meydana geldi");
                throw;
            }
        }

        private async Task<IResult> GetOrderByIdAsync(Guid id, [FromServices] IOrderService orderService)
        {
            try
            {
                var result = await orderService.GetByIdAsync(id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID: {Id} olan sipariş getirilirken hata meydana geldi", id);
                throw;
            }
        }

        private async Task<IResult> CreateOrderAsync([FromBody] CreateOrderDto createOrderDto, [FromServices] IOrderService orderService)
        {
            try
            {
                var result = await orderService.AddAsync(createOrderDto);
                return Results.Created($"/api/order/{result.UserId}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturulurken hata meydana geldi");
                throw;
            }
        }

        private async Task<IResult> UpdateOrderAsync([FromBody] UpdateOrderDto updateOrderDto, [FromServices] IOrderService orderService)
        {
            try
            {
                var result = await orderService.UpdateAsync(updateOrderDto);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID: {Id} olan sipariş güncellenirken hata meydana geldi", updateOrderDto.Id);
                throw;
            }
        }

        private async Task<IResult> DeleteOrderAsync(Guid id, [FromServices] IOrderService orderService)
        {
            try
            {
                await orderService.DeleteAsync(id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID: {Id} olan sipariş silinirken hata meydana geldi", id);
                throw;
            }
        }
    }
}