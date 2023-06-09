using System.ComponentModel.DataAnnotations;

namespace SamsWarehouse.Models
{
    public class Item
    {
        [Required]
        public int Id { get; set; }
        public string ? ItemName { get; set; }
        public string ? ItemUnit { get; set; }
        [Required]
        public double ItemPrice { get; set; }

    }
}
