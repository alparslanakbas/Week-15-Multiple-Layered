using Multiple_Layered_Service.Library.Dtos.ProductDtos;
using Multiple_Layered_Service.Library.Services.ProductServices;

public class ProductService : IProductService
{
    readonly IUnitOfWork _unitOfWork;
    readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<ListAllProductDto>> GetAllAsync()
    {
        try
        {
            var productDtoList = await _unitOfWork.GetFromCacheAsync<IEnumerable<ListAllProductDto>>("products_cache");
            if (productDtoList is not null)
            {
                _logger.LogInformation("Ürünler cache'den getirildi");
                return productDtoList;
            }

            var products = await _unitOfWork.Products.GetAllAsync();
            productDtoList = products.Select(p => new ListAllProductDto(
                p.Id,
                p.Name,
                p.Price,
                p.Stock
            ));

            await _unitOfWork.SetToCacheAsync("products_cache", productDtoList);
            _logger.LogInformation("Tüm ürünler başarıyla getirildi");
            return productDtoList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürünleri getirirken hata oluştu");
            throw;
        }
    }

    public async Task<ListAllProductDto> GetByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null)
            throw new KeyNotFoundException();

        return new ListAllProductDto(
            product.Id,
            product.Name,
            product.Price,
            product.Stock
        );
    }

    public async Task<CreateProductDto> AddAsync(CreateProductDto createProductDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var product = new Product
            {
                Name = createProductDto.Name,
                Price = createProductDto.Price,
                Stock = createProductDto.Stock
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.ClearCacheAsync();
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Yeni ürün eklendi: {ProductName}", product.Name);

            return createProductDto;
        }
        catch (UnauthorizedAccessException)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogWarning("Yetkisiz işlem: {ProductName}", createProductDto.Name);
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Ürün eklenirken hata: {ProductName}", createProductDto.Name);
            throw;
        }
    }

    public async Task<UpdateProductDto> UpdateAsync(UpdateProductDto updateProductDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var product = await _unitOfWork.Products.GetByIdAsync(updateProductDto.Id);
            if (product is null)
                throw new KeyNotFoundException();

            product.Name = updateProductDto.Name;
            product.Price = updateProductDto.Price;
            product.Stock = updateProductDto.Stock;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.ClearCacheAsync();
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Ürün güncellendi: {ProductName}", product.Name);

            return updateProductDto;
        }

        catch (UnauthorizedAccessException)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogWarning("Yetkisiz işlem: {ProductId}", updateProductDto.Id);
            throw;
        }

        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Ürün güncellenirken hata: {ProductId}", updateProductDto.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product is null)
                throw new KeyNotFoundException();

            await _unitOfWork.Products.DeleteAsync(product.Id);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.ClearCacheAsync();
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Ürün silindi: {ProductId}", id);
        }
        catch (UnauthorizedAccessException)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogWarning("Yetkisiz işlem: {ProductId}", id);
            throw;
        }

        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Ürün silinirken hata: {ProductId}", id);
            throw;
        }
    }
}