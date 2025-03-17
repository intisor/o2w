using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly GraphService _graphService;
    public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
    public string LatestWhatsAppLink { get; set; }

    public IndexModel(GraphService graphService)
    {
        _graphService = graphService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Challenge();
        }

        try
        {
            Events = await _graphService.GetUserEventsAsync();
            LatestWhatsAppLink = TempData["LatestWhatsAppLink"] as string;
        }
        catch (UnauthorizedAccessException)
        {
            return Challenge(); // ✅ Force user to log in again if token is missing or expired
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Error retrieving events. Please log in again.");
        }


        return Page();
    }
}
