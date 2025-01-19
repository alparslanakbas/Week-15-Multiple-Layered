namespace Multiple_Layered.API.Endpoints.OrderProduct
{
    public class OrderProductEndpoints : IEndpointDefinition
    {
        readonly ILogger<OrderProductEndpoints> _logger;

        public OrderProductEndpoints(ILogger<OrderProductEndpoints> logger)
        {
            _logger = logger;
        }

        public void RegisterEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/order-product");

            group.MapGet("/all", GetAllOrderProductsAsync)
                .WithName("GetAllOrderProducts")
                .WithDescription("Tüm Sipariş Detaylarını Getirir")
                .Produces<IEnumerable<ListAllOrderProductDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError)
                .AddEndpointFilter<TimeRestrictFilter>();

            group.MapGet("/{orderId}/{productId}", GetOrderProductByIdAsync)
                .WithName("GetOrderProductById")
                .WithDescription("ID'ye Göre Sipariş Detayı Getirir")
                .Produces<ListAllOrderProductDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/", CreateOrderProductAsync)
                .WithName("CreateOrderProduct")
                .WithDescription("Yeni Sipariş Detayı Oluşturur")
                .Produces<CreateOrderProductDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .RequireAuthorization();

            group.MapPut("/", UpdateOrderProductAsync)
                .WithName("UpdateOrderProduct")
                .WithDescription("Sipariş Detayı Günceller")
                .Produces<UpdateOrderProductDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .RequireAuthorization();

            group.MapDelete("/{orderId}/{productId}", DeleteOrderProductAsync)
                .WithName("DeleteOrderProduct")
                .WithDescription("Sipariş Detayı Siler")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .RequireAuthorization();

        }

        private async Task<IResult> GetAllOrderProductsAsync(IOrderProductService orderProductService, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                var pagination = new Pagination(page, size);
                var result = await orderProductService.GetAllAsync(pagination);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayları listelenirken bir hata meydana geldi");
                throw;
            }
        }

        private async Task<IResult> GetOrderProductByIdAsync(Guid orderId, Guid productId, IOrderProductService orderProductService)
        {
            try
            {
                var result = await orderProductService.GetByIdAsync(orderId, productId);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayı getirilirken bir hata meydana geldi");
                throw;
            }
        }

        private async Task<IResult> CreateOrderProductAsync([FromBody] CreateOrderProductDto createOrderProductDto, IOrderProductService orderProductService)
        {
            try
            {
                var result = await orderProductService.AddAsync(createOrderProductDto);
                return Results.Created($"/api/order-product/{result.OrderId}/{result.ProductId}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayı eklenirken bir hata meydana geldi");
                throw;
            }
        }

        private async Task<IResult> UpdateOrderProductAsync([FromBody] UpdateOrderProductDto updateOrderProductDto, IOrderProductService orderProductService)
        {
            try
            {
                var result = await orderProductService.UpdateAsync(updateOrderProductDto);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayı güncellenirken bir hata meydana geldi");
                throw;
            }
        }

        private async Task<IResult> DeleteOrderProductAsync(Guid orderId, Guid productId, IOrderProductService orderProductService)
        {
            try
            {
                await orderProductService.DeleteAsync(orderId, productId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayı silinirken bir hata meydana geldi");
                throw;
            }
        }
    }
}
