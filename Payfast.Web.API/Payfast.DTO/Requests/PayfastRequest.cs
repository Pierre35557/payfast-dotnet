using System.ComponentModel.DataAnnotations;

namespace Payfast.DTO.Requests
{
    public class PayfastRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string MobileNumber { get; set; }
        [Required]
        public string ItemName { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public bool ConfirmEmail { get; set; }  
    }
}
