using AutoMapper;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Order;
using ShoeStore.Dto.Payment;
using ShoeStore.Dto.Product;

namespace ShoeStore.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : string.Empty))
                .ForMember(dest => dest.Audience, opt => opt.MapFrom(src => src.Audience != null ? src.Audience.Code : string.Empty));

            CreateMap<ProductImage, AdditionalProductImageDto>();

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.OrderStatusName,
                    opt => opt.MapFrom(src => src.OrderStatus != null ? src.OrderStatus.DisplayName : string.Empty))
                .ForMember(dest => dest.Payment,
                    opt => opt.MapFrom(src => src.Payments.FirstOrDefault()));



            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.ProductSize.Product != null && src.ProductSize.Product.Brand != null ? src.ProductSize.Product.Brand.Name : string.Empty));

            // Payments mapping
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src =>
                        src.PaymentStatus != null ? src.PaymentStatus.DisplayName : string.Empty))
                .ForMember(dest => dest.PaymentMethod,
                    opt => opt.MapFrom(src =>
                        src.PaymentMethod != null ? src.PaymentMethod.DisplayName : string.Empty));

            // Shipping Address mappings
            CreateMap<ShippingAddress, ShippingAddressDto>();

            // Billing Address mappings
            CreateMap<BillingAddress, BillingAddressDto>();
        }
    }
}
