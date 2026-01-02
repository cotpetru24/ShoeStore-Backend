using System.Runtime.Serialization;

namespace ShoeStore.Dto.Product
{
    public enum SortByOption
    {
        [EnumMember(Value = "name-asc")]
        NameAsc,

        [EnumMember(Value = "name-desc")]
        NameDesc,

        [EnumMember(Value = "price-asc")]
        PriceAsc,

        [EnumMember(Value = "price-desc")]
        PriceDesc,

        [EnumMember(Value = "brand-asc")]
        BrandAsc,

        [EnumMember(Value = "brand-desc")]
        BrandDesc
    }
}
