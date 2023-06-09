using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SamsWarehouse.Models
{
    public class ShoppingList
    {
        [Required]
        public int Id { get; set; }
        public string ListName { get; set; }
        public DateTime Date { get; set; }

        //FOREIGN KEYS
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

    }
}
