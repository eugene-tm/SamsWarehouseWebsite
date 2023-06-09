using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamsWarehouse.Models;
using System.Diagnostics;
using System.Text;

namespace SamsWarehouse.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly MyContext _context;

		public HomeController(ILogger<HomeController> logger, MyContext context)
		{
			_logger = logger;
			_context = context;
		}


        /// <summary>
        /// This method retrieves list data for the logged-in user and opens the home page
        /// </summary>
        /// <returns></returns>
        // turns off cache. Need this for the partial view auto-refresh to work
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			// Users must be logged in to access the home page
			if (HttpContext.Session.TryGetValue("Authenticated", out var authenticatedBytes))
			{
				// Gets the user ID from the session
				string userId = HttpContext.Session.GetString("UserId");

				// Retrieves all the shopping lists associated with the user ID
				var shoppingLists = await _context.ShoppingLists
					.Where(c => c.UserId.ToString() == userId)
					.ToListAsync();

				// Declares and instantiates a list of view models (for each shopping list)
				var listViewModels = new List<ShoppingListViewModel>();


				// Loops through each shopping list, retrieves the list items associated with
				// that list and creates a new ShoppingListViewModel object for each list
				foreach (var shoppingList in shoppingLists)
				{
					// Gets our list items from the database.
					var listItems = await _context.ListItems

						// Important to use the INCLUDE here to make the Items table accessible
						.Include(c => c.Item)
						.Where(c => c.ShoppingListId == shoppingList.Id)
						.ToListAsync();

					// Create list of shoppingListViewModels to inject into the shoppingListViewModel
					var itemsList = new List<ShoppingListViewModel>();

					// for each listItems (shopping list) found in our database --> retrieve the item name, quantity and price
					foreach (var item in listItems)
					{
						var itemViewModel = new ShoppingListViewModel
						{
							ListItemName = item.Item.ItemName,
							ListItemQuantity = item.Quantity,
							ListItemPrice = item.Item.ItemPrice,
							ListItemUnit = item.Item.ItemUnit
						};
						itemsList.Add(itemViewModel);
					}

					// Putting values into our view model
					var listViewModel = new ShoppingListViewModel
					{
						Id = shoppingList.Id,
						ListName = shoppingList.ListName,
						ListDate = shoppingList.Date,
						ListItems = listItems
					};

					// Adds each ShoppingListViewModel to a list of view models
					listViewModels.Add(listViewModel);

				}


				// Passes the list to the view
				return View(listViewModels);
			}

			// Otherwise --> redirect to login page
			return RedirectToAction("Login");

		}

        /// <summary>
        /// Returns the login page
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
		{
			return View();
		}

		/// <summary>
		/// Receives form data from the login page and logs the user in, and redirects to the home page
		/// </summary>
		/// <param name="userLogin"></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult Login(User userLogin)
		{
			// If user email matches an email in the database --> login
			var user = _context.Users.Where(c => c.Email.Equals(userLogin.Email)).FirstOrDefault();

			if (user == null)
			{
				return View();
			}

			if (BCrypt.Net.BCrypt.Verify(userLogin.PasswordHash, user.PasswordHash))
			{
				// store relevant data in the session
				HttpContext.Session.SetString("UserId", user.Id.ToString());
				HttpContext.Session.SetString("Authenticated", "True");
				HttpContext.Session.SetString("theme", user.DisplaySetting);
				HttpContext.Session.SetString("email", user.Email);

				// goes to index page
				return RedirectToAction("Index");
			}

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Logs the user out by clearing the session
		/// </summary>
		/// <returns></returns>
		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Takes data from the create user form on the login page and adds a new user to the database
		/// </summary>
		/// <param name="form"></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult CreateUser(IFormCollection form)
		{
			// assigns the user input to a variables
			var userEmail = form["Email"];
			var userPassword = form["PasswordHash"];

			// checks for existing emails first 
			if (_context.Users.Any(c => c.Email == userEmail.ToString()))
			{
				// Ideally we want clientside validation here, but for now we're just refreshing the page. 
				return RedirectToAction("Login");
			}

			// new user object
			User newUser = new User()
			{
				Email = userEmail,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(userPassword),
				DisplaySetting = "light"
			};

			// Save the new user to the database
			_context.Users.Add(newUser);
			_context.SaveChanges();

			// Go to the database again and retrieve the new user
			var user = _context.Users.Where(c => c.Email == newUser.Email).FirstOrDefault();

			// Save the users information to the session
			HttpContext.Session.SetString("UserId", user.Id.ToString());
			HttpContext.Session.SetString("Authenticated", "True");
			HttpContext.Session.SetString("theme", "light");
			HttpContext.Session.SetString("email", user.Email);

			return RedirectToAction("Index");
		}



		/// <summary>
		/// Retrieves relevant shopping list data by shoppingListId and passes it to the edit list modal (partial)
		/// </summary>
		/// <param name="shoppingListId"></param>
		/// <returns></returns>
		[HttpGet]
		public async Task<ActionResult> EditList(int shoppingListId)
		{
			// Retrieves all the list items associated with the shoppingListId
			var listItems = await _context.ListItems
				.Include(c => c.Item)
				.Include(c => c.ShoppingList)
				.Where(c => c.ShoppingListId == shoppingListId)
				.ToListAsync();

			// creating a new list to store products and their information
			var itemsList = new List<ShoppingListViewModel>();

			// for each product our collection of listItems with matching shoppingListIDs
			foreach (var item in listItems)
			{
				// get the details of each product
				var product = new ShoppingListViewModel
				{
					Id = item.Id,
					ListName = item.ShoppingList.ListName,
					ListItemName = item.Item.ItemName,
					ListItemQuantity = item.Quantity,
					ListItemUnit = item.Item.ItemUnit,
					ListItemPriceEach = item.Item.ItemPrice,
					ListItemPrice = Math.Round(item.Item.ItemPrice * item.Quantity, 2)
				};
				// and add them to our list
				itemsList.Add(product);
			}
			// we use allProducts to populate the dropdown boxes in the view
			var allProducts = await _context.Items.ToListAsync();
			ViewBag.AllProducts = allProducts;
			ViewBag.ListId = shoppingListId;

			return PartialView("_EditList", itemsList);
		}

        /// <summary>
        /// Adds a new blank list to the database, linked via the userId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="newListName"></param>
        /// <returns></returns>
        [HttpPost]
		public async Task<IActionResult> CreateNewList(int userId, [FromBody] ShoppingList newListName)
		{
			var newList = new ShoppingList()
			{
				ListName = newListName.ListName,
				Date = DateTime.Now,
				UserId = userId
			};
			await _context.ShoppingLists.AddAsync(newList);
			await _context.SaveChangesAsync();
			return Ok();
		}

        /// <summary>
        /// Adds a new item to a shopping list
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="newItem"></param>
        /// <returns></returns>
        [HttpPost]
		public async Task<IActionResult> AddItemToList(int listId, [FromBody] ShoppingListViewModel newItem)
		{
			var itemId = _context.Items
				.Where(c => c.ItemName == newItem.ListItemName)
				.Select(c => c.Id)
				.FirstOrDefault();

			var listItem = new ListItem()
			{
				ItemId = itemId,
				Quantity = newItem.ListItemQuantity,
				ShoppingListId = listId
			};

			await _context.ListItems.AddAsync(listItem);
			await _context.SaveChangesAsync();

			return Ok();
		}

        /// <summary>
        /// Recieves a new list name and ammends the existing list name connected to the shoppingListId
        /// </summary>
        /// <param name="shoppingListId"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        [HttpPut]
		public async Task<IActionResult> EditListName(int shoppingListId, [FromBody] ShoppingList newName)
		{
			var list = _context.ShoppingLists.Where(c => c.Id == shoppingListId).FirstOrDefault();

			list.ListName = newName.ListName;
			_context.Update(list);
			await _context.SaveChangesAsync();
			return Ok();
		}

        /// <summary>
        /// Erases the contents of the old list and rewrites the shopping list based on the recieved data
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="newItems"></param>
        /// <returns></returns>
        [HttpPost]
		public async Task<IActionResult> UpdateList(int listId, [FromBody] IEnumerable<ShoppingListViewModel> newItems)
		{

			// ERASE THE OLD LIST
			var shoppingListErase = await _context.ListItems.Where(c => c.ShoppingListId == listId).ToListAsync();
			_context.ListItems.RemoveRange(shoppingListErase);
			_context.SaveChanges();


			// Loop through each item in the model 
			foreach (var item in newItems)
			{

				// Get the ItemId for the current ListItem by looking up the ItemName in the database
				var itemId = _context.Items
					.Where(c => c.ItemName == item.ListItemName)
					.Select(c => c.Id)
					.FirstOrDefault();

				// Find the shoppingList ID. This is where our new list info will go. 
				var listItem = new ListItem()
				{
					// REWRITE THE NEW LIST
					// Update the ListItem's properties with the values from the form
					Quantity = item.ListItemQuantity,
					ItemId = itemId,
					ShoppingListId = listId
				};

				_context.ListItems.Add(listItem);
			}

			// Save the changes to the database   
			await _context.SaveChangesAsync();
			return Ok();
		}

		/// <summary>
		/// This method is necessary for determining whether a delete request is for an item or an entire list.
		/// This is used to avoid writing duplicate/similar methods in javascript
		/// </summary>
		/// <param name="type"></param>
		/// <param name="itemCount"></param>
		/// <param name="shoppingListId"></param>
		/// <returns></returns>
		[HttpDelete]
		public async Task<IActionResult> HandleDeletes(string type, int itemCount, int shoppingListId)
		{
			if (type == "item")
			{
				await DeleteItem(itemCount, shoppingListId);
				return Ok();
			}
			if (type == "list")
			{
				await DeleteList(shoppingListId);
				return Ok();
			}
			else
			{
				return BadRequest();
			}
		}


        /// <summary>
        /// Deletes a single item from the shopping list
        /// </summary>
        /// <param name="itemCount"></param>
        /// <param name="shoppingListId"></param>
        /// <returns></returns>
        [HttpDelete]
		public async Task<IActionResult> DeleteItem(int itemCount, int shoppingListId)
		{
			// Finds the correct item
			var item = _context.ListItems.Where(c => c.ShoppingListId.Equals(shoppingListId))
				.Skip(itemCount - 1)
				.Take(1)
				.FirstOrDefault();

			// removes the item
			_context.ListItems.Remove(item);
			await _context.SaveChangesAsync();

			return Ok();
		}

        /// <summary>
        /// Deletes an entire shopping list
        /// </summary>
        /// <param name="shoppingListId"></param>
        /// <returns></returns>
        [HttpDelete]
		public async Task<IActionResult> DeleteList(int shoppingListId)
		{
			var itemsToDelete = _context.ListItems.Where(c => c.ShoppingListId == shoppingListId);

			_context.ListItems.RemoveRange(itemsToDelete);

			var listToDelete = _context.ShoppingLists.Where(c => c.Id == shoppingListId).FirstOrDefault();

			_context.ShoppingLists.Remove(listToDelete);
			await _context.SaveChangesAsync();

			return Ok();
		}


        /// <summary>
        /// Changes and saves the display setting (light or dark mode) associated with a user
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPut]
		public async Task<IActionResult> SetTheme(string theme, int userId)
		{
			var user = _context.Users.Find(userId);
			if (user != null)
			{
				user.DisplaySetting = theme;
				_context.Users.Update(user);
				await _context.SaveChangesAsync();
				HttpContext.Session.SetString("theme", theme);
			}
			return Ok();
		}


        /// <summary>
        /// Loads the 
        /// </summary>
        /// <returns></returns>
        public IActionResult AboutUs()
		{
			// Users must be logged in to access this page
			if (HttpContext.Session.TryGetValue("Authenticated", out var authenticatedBytes))
			{
				string auth = Encoding.UTF8.GetString(authenticatedBytes);
				if (auth == "True")
				{
					return View();
				}
			}

			// Otherwise --> redirect to login page
			return RedirectToAction("Login");
		}

		/// <summary>
		/// Returns an error page
		/// </summary>
		/// <returns></returns>
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}