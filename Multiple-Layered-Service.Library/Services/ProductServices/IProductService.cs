using Multiple_Layered_Service.Library.Dtos.ProductDtos;

namespace Multiple_Layered_Service.Library.Services.ProductServices
{
    public interface IProductService
    {
        Task<IEnumerable<ListAllProductDto>> GetAllAsync();
        Task<ListAllProductDto> GetByIdAsync(Guid id);
        Task<UpdateProductDto> UpdateAsync(UpdateProductDto updateProductDto);
        Task<CreateProductDto> AddAsync(CreateProductDto createProductDto);
        Task DeleteAsync(Guid id);
    }
}
