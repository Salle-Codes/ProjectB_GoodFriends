using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Services;
using Services.Interfaces;

namespace AppRazor.Pages
{
	public class ViewFriendModel : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _service = null;
        readonly ILogger<ViewFriendModel> _logger = null;

        public IFriend Friend  { get; set; }

        public async Task<IActionResult> OnGet()
        {
            if (Guid.TryParse(Request.Query["id"], out Guid _friendId))
            {
                var result = await _service.ReadFriendAsync(_friendId, false);
                if (result?.Item != null)
                {
                    Friend = result.Item;
                }
            }

            return Page();
        }

        //Inject services just like in WebApi
        public ViewFriendModel(IFriendsService service, ILogger<ViewFriendModel> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
