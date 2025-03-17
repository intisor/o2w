using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class WebhookModel : PageModel
{
    private readonly ILogger<WebhookModel> _logger;
    private readonly GraphService _graphService;

    public string LatestWhatsAppLink { get; private set; }

    public WebhookModel(ILogger<WebhookModel> logger, GraphService graphService)
    {
        _logger = logger;
        _graphService = graphService;
    }

    public async Task<IActionResult> OnPostAsync([FromBody] dynamic notification)
    {
        _logger.LogInformation($"Received Outlook event notification: {notification}");

        string eventId = notification.value[0].resourceData.id;
        var events = await _graphService.GetUserEventsAsync();

        var calendarEvent = events.FirstOrDefault();


        // Generate WhatsApp share link
        string message = $"📅 New Event: {calendarEvent.Title}\n🗓 {calendarEvent.StartTime:dddd, MMM dd, yyyy} at {calendarEvent.StartTime:hh:mm tt}\n🔗 {calendarEvent.MeetingLink}";
        string whatsappLink = $"https://wa.me/?text={Uri.EscapeDataString(message)}";

        // Store WhatsApp link in TempData for UI notification
        TempData["LatestWhatsAppLink"] = whatsappLink;

        await _graphService.SendEventToDiscord(calendarEvent);

        return new JsonResult(new { success = true });
    }
}
   

