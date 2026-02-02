using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Dto.Payment
{
    public class CreatePaymentIntentRequestDto
    {
        [Range(1, long.MaxValue)]
        public required long Amount { get; set; }
    }
}
