namespace Multiple_Layered.API.Endpoints.Product
{
    public class ProductEndpoints : IEndpointDefinition
    {
        readonly ILogger<ProductEndpoints> _logger;

        public ProductEndpoints(ILogger<ProductEndpoints> logger)
        {
            _logger = logger;
        }

        public void RegisterEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/product");

            group.MapGet("/all", GetAllProductsAsync)
                .WithName("GetAllProducts")
                .WithDescription("Tüm Ürünleri Getirir")
                .Produces<IEnumerable<ListAllProductDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError)
                /*.AddEndpointFilter<TimeRestrictFilter>()*/;

            group.MapGet("/{id}", GetProductByIdAsync)
                .WithName("GetProductById")
                .WithDescription("ID'ye Göre Ürün Getirir")
                .Produces<ListAllProductDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

            group.MapPost("/", CreateProductAsync)
                .WithName("CreateProduct")
                .WithDescription("Yeni Ürün Oluşturur")
                .Produces<CreateProductDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .RequireAuthorization();

            group.MapPut("/", UpdateProductAsync)
                .WithName("UpdateProduct")
                .WithDescription("Ürün Günceller")
                .Produces<UpdateProductDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .RequireAuthorization();

            group.MapDelete("/{id}", DeleteProductAsync)
                .WithName("DeleteProduct")
                .WithDescription("Ürün Siler")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .RequireAuthorization();
        }

        private async Task<IResult> GetAllProductsAsync(IProductService productService, [FromQuery] int page =1, [FromQuery] int size = 10)
        {
            try
            {
                var pagination = new Pagination(page, size);
                var result = await productService.GetAllAsync(pagination);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürünler listelenirken hata meydana geldi");
                throw;
            }
        }

        private async Task<IResult> GetProductByIdAsync(Guid id, IProductService productService)
        {
            try
            {
                var result = await productService.GetByIdAsync(id);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID: {Id} olan ürün getirilirken hata meydana geldi", id);
                throw;
            }
        }

        private async Task<IResult> CreateProductAsync([FromBody] CreateProductDto createProductDto, IProductService productService)
        {
            try
            {
                var result = await productService.AddAsync(createProductDto);
                return Results.Created($"/api/product/{result.Id}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün eklenirken hata meydana geldi");
                throw;
            }
        }

        private async Task<IResult> UpdateProductAsync([FromBody] UpdateProductDto updateProductDto, IProductService productService)
        {
            try
            {
                var result = await productService.UpdateAsync(updateProductDto);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID: {Id} olan ürün güncellenirken hata meydana geldi", updateProductDto.Id);
                throw;
            }
        }

        private async Task<IResult> DeleteProductAsync(Guid id, IProductService productService)
        {
            try
            {
                await productService.DeleteAsync(id);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID: {Id} olan ürün silinirken hata meydana geldi", id);
                throw;
            }
        }
    }
}
