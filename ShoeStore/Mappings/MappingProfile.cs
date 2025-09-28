using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Product;
using ShoeStore.Dto.Order;
using AutoMapper;

namespace ShoeStore.Mappings
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            // Product -> ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : string.Empty))
                .ForMember(dest => dest.Audience, opt => opt.MapFrom(src => src.Audience != null ? src.Audience.Code : string.Empty));

            // Map individual product images; collections will be mapped automatically
            CreateMap<ProductImage, AdditionalProductImageDto>();

            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.OrderStatusName, opt => opt.MapFrom(src => src.OrderStatus != null ? src.OrderStatus.DisplayName : string.Empty));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImagePath : string.Empty))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Product != null && src.Product.Brand != null ? src.Product.Brand.Name : string.Empty));

            //CreateMap<OrderItem, OrderItemDto>()
            //    .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImagePath : null))
            //    .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Product != null && src.Product.Brand != null ? src.Product.Brand.Name : null));


            // Shipping Address mappings
            CreateMap<ShippingAddress, ShippingAddressDto>();

            // Billing Address mappings
            CreateMap<BillingAddress, BillingAddressDto>();
        }
    }
}
