using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class IndexModel(GraphService graphService) : PageModel
{
    private readonly GraphService _graphService = graphService;
    public List<CalendarEvent> Events { get; set; } = [];
    public string LatestWhatsAppLink { get; set; }

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
            return Challenge(); 
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Error retrieving events. Please log in again.");
        }


        return Page();
    }
}
