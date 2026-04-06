using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Models
{
    public class Transactions
    {
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }
        public string Type { get; set; }

        public string Category { get; set; }

        public DateTime Dates { get; set; }

        public int User_Id { get; set; }
    }
}
