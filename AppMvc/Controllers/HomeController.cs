using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AppMvc.Models;
using Services.Interfaces;

namespace AppMvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAdminService _adminService;
    private readonly IAddressesService _addressesService;

    public HomeController(ILogger<HomeController> logger, IAdminService adminService, IAddressesService addressesService)
    {
        _logger = logger;
        _adminService = adminService;
        _addressesService = addressesService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> Overview()
        {
            var vm = new OverviewViewModel();
            
            var info = await _adminService.GuestInfoAsync();
            vm.CountryInfo = info.Item.Friends.Where(f => f.Country == "Denmark");
            return View(vm);
        }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
