using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Models
{
    public class Users
    {
        [Key]
        public int User_Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
