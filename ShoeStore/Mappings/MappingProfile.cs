using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Product;
using AutoMapper;

namespace ShoeStore.Mappings
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : string.Empty))
                .ForMember(dest => dest.Audience, opt => opt.MapFrom(src => src.Audience != null ? src.Audience.Code : string.Empty));

        }
    }
}
