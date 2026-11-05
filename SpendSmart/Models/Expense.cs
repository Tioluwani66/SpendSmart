using System.ComponentModel.DataAnnotations;

namespace SpendSmart.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public decimal Value { get; set; }

        [Required]
        public string? Description { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}
