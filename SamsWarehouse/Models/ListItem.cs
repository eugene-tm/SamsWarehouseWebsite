using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SamsWarehouse.Models
{
    public class ListItem
    {
		public int Id { get; set; }
		[Required]
		public int ItemId { get; set; }
		[Required]
		public int Quantity { get; set; }

		public int ShoppingListId { get; set; }


		[ForeignKey("ItemId")]
		public Item ? Item { get; set; }

		[ForeignKey("ShoppingListId")]
		public ShoppingList ? ShoppingList { get; set; }

	}
}
