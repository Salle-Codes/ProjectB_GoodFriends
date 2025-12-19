using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models.Interfaces;
using Services;
using Services.Interfaces;

namespace AppRazor.Pages
{
    public class OverviewModel : PageModel
    {
        readonly IFriendsService _service = null;
        readonly ILogger<OverviewModel> _logger = null;

        [BindProperty]
        public bool UseSeeds { get; set; } = true;
        
        public List<IFriend> Friends { get; set; }

        public int NrOfFriends { get; set; }

        //Pagination
        public int NrOfPages { get; set; }
        public int PageSize { get; } = 10;

        public int ThisPageNr { get; set; } = 0;
        public int PrevPageNr { get; set; } = 0;
        public int NextPageNr { get; set; } = 0;
        public int NrVisiblePages { get; set; } = 0;

        //ModelBinding for the form
        [BindProperty]
        public string SearchFilter { get; set; } = null;

        [BindProperty]
        public string CountryFilter { get; set; } = null;

        //List of countries for dropdown
        public List<string> CountryList { get; set; } = new List<string>();

        //will execute on a Get request
        public async Task<IActionResult> OnGet()
        {   
            //Read a QueryParameters
            if (int.TryParse(Request.Query["pagenr"], out int pagenr))
            {
                ThisPageNr = pagenr;
            }

            SearchFilter = Request.Query["search"];
            CountryFilter = Request.Query["country"];

            //Get all friends to populate country list
            var allFriendsResp = await _service.ReadFriendsAsync(UseSeeds, false, null, 0, 10000);
            if (allFriendsResp?.PageItems != null)
            {
                CountryList = allFriendsResp.PageItems
                    .Where(f => f.Address?.Country != null)
                    .Select(f => f.Address.Country)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }

            //Get all friends and apply filters
            var resp = await _service.ReadFriendsAsync(UseSeeds, false, SearchFilter, 0, 10000);
            Friends = resp.PageItems ?? new List<IFriend>();
            NrOfFriends = Friends.Count;

            //Filter by country if selected
            if (!string.IsNullOrEmpty(CountryFilter))
            {
                Friends = Friends
                    .Where(f => f.Address?.Country == CountryFilter)
                    .ToList();
                NrOfFriends = Friends.Count;
            }

            //Now apply pagination to the filtered results
            Friends = Friends
                .Skip(ThisPageNr * PageSize)
                .Take(PageSize)
                .ToList();

            //Pagination
            UpdatePagination(NrOfFriends);

            return Page();
        }

        private void UpdatePagination(int nrOfItems)
        {
            //Pagination
            NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
            PrevPageNr = Math.Max(0, ThisPageNr - 1);
            NextPageNr = Math.Min(NrOfPages - 1, ThisPageNr + 1);
            NrVisiblePages = Math.Min(10, NrOfPages);
        }

        public async Task<IActionResult> OnPostSearch()
        {
            //Get all friends to populate country list
            var allFriendsResp = await _service.ReadFriendsAsync(UseSeeds, false, null, 0, 10000);
            if (allFriendsResp?.PageItems != null)
            {
                CountryList = allFriendsResp.PageItems
                    .Where(f => f.Address?.Country != null)
                    .Select(f => f.Address.Country)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }

            //Get all friends and apply filters
            var resp = await _service.ReadFriendsAsync(UseSeeds, false, SearchFilter, 0, 10000);
            Friends = resp.PageItems ?? new List<IFriend>();
            NrOfFriends = Friends.Count;

            //Filter by country if selected
            if (!string.IsNullOrEmpty(CountryFilter))
            {
                Friends = Friends
                    .Where(f => f.Address?.Country == CountryFilter)
                    .ToList();
                NrOfFriends = Friends.Count;
            }

            //Now apply pagination to the filtered results
            Friends = Friends
                .Skip(ThisPageNr * PageSize)
                .Take(PageSize)
                .ToList();

            //Pagination
            UpdatePagination(NrOfFriends);

            //Page is rendered as the postback is part of the form tag
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteGroup(Guid groupId)
        {
            await _service.DeleteFriendAsync(groupId);

            //Get all friends to populate country list
            var allFriendsResp = await _service.ReadFriendsAsync(UseSeeds, false, null, 0, 10000);
            if (allFriendsResp?.PageItems != null)
            {
                CountryList = allFriendsResp.PageItems
                    .Where(f => f.Address?.Country != null)
                    .Select(f => f.Address.Country)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }

            //Get all friends and apply filters
            var resp = await _service.ReadFriendsAsync(UseSeeds, false, SearchFilter, 0, 10000);
            Friends = resp.PageItems ?? new List<IFriend>();
            NrOfFriends = Friends.Count;

            //Filter by country if selected
            if (!string.IsNullOrEmpty(CountryFilter))
            {
                Friends = Friends
                    .Where(f => f.Address?.Country == CountryFilter)
                    .ToList();
                NrOfFriends = Friends.Count;
            }

            //Now apply pagination to the filtered results
            Friends = Friends
                .Skip(ThisPageNr * PageSize)
                .Take(PageSize)
                .ToList();

            //Pagination
            UpdatePagination(NrOfFriends);

            return Page();
        }

        //Inject services just like in WebApi
        public OverviewModel(IFriendsService service, ILogger<OverviewModel> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
