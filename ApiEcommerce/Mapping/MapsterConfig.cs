using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using Mapster;

namespace ApiEcommerce.Mapping;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Category, CategoryDto>.NewConfig();
        TypeAdapterConfig<CreateCategoryDto, Category>.NewConfig();
        TypeAdapterConfig<Product, ProductoDto>.NewConfig();
        TypeAdapterConfig<CreateProductDto, Product>.NewConfig();
        TypeAdapterConfig<User, UserDto>.NewConfig();
        TypeAdapterConfig<CreateUserDto, User>.NewConfig();
        TypeAdapterConfig<ApplicationUser, UserDataDto>.NewConfig();
        // Agrega aquí más configuraciones según tus modelos y DTOs
    }
}
