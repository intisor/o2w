using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly GraphService _graphService;
    public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
    [TempData]
    public string? ErrorMessage { get; set; }

    public IndexModel(GraphService graphService)
    {
        _graphService = graphService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Events = await _graphService.GetUserEventsAsync();
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to retrieve calendar events. Please try again.";
            return Page();
        }
    }
}
