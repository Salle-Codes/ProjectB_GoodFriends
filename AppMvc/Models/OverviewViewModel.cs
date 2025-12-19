using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Models.Interfaces;
using Services.Interfaces;

namespace AppMvc.Models;

public class OverviewViewModel
{
    public IEnumerable<GstUsrInfoFriendsDto>? CountryInfo;
}
