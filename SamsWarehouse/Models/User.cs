using System.ComponentModel.DataAnnotations;

namespace SamsWarehouse.Models
{
    public class User
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string ? Email { get; set; }

		[Display(Name = "Password")]
        [Required]
		public string ? PasswordHash { get; set; }

		public string ? DisplaySetting { get; set; }
    }
}
