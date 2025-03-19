using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

[Authorize]
public class IndexModel : PageModel
{
    private readonly GraphService _graphService;

    public IndexModel(GraphService graphService)
    {
        _graphService = graphService;
    }

    public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
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
        catch (MsalUiRequiredException)
        {
            return Challenge(); 
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Error retrieving events. Please log in again.");
        }

        return Page();
    }

    public async Task<IActionResult> OnGetTestDiscordWebhookAsync()
    {
        var result = await _graphService.TestDiscordWebhookAsync();
        if (result)
        {
            TempData["TestResult"] = "Discord webhook test message sent successfully.";
        }
        else
        {
            TempData["TestResult"] = "Failed to send Discord webhook test message.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Login");
    }
}
