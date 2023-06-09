using System.ComponentModel.DataAnnotations;

namespace SamsWarehouse.Models
{
    public class ShoppingListViewModel
    {
        public int Id { get; set; }

        public string ListName { get; set; }
        public DateTime ListDate { get; set; }
        public string ListItemName { get; set; }
		public int ListItemQuantity { get; set; }
        public string ListItemUnit { get; set; }
        public double ListItemPriceEach { get; set; }
        public double ListItemPrice { get; set; }
        public List<ListItem> ListItems { get; set; }
    }
}
