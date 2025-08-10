using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Dto.Product
{
    public class AdditionalProductImageDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ImagePath { get; set; }
    }
}
