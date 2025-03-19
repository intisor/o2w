using Microsoft.Graph;
using Newtonsoft.Json;
using System.Text;
public class GraphService
{
    private readonly GraphServiceClient _graphClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
     private readonly HttpClient _httpClient;
    private const string DiscordWebhookUrl = "https://discord.com/api/webhooks/1350709931506139209/_mjG34uWk4te264Xi7OuvzzjoChtONgmGnJ43e4KWLi4K9tBGgv6bHnZmXjdczM2S5NZ";


    public GraphService(
        GraphServiceClient graphClient,
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _graphClient = graphClient;
        _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClient;
    }

    public async Task<List<CalendarEvent>> GetUserEventsAsync()
    {
        if (!_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? true)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        try
        {
            var events = await _graphClient.Me.Calendar.Events
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter = $"start/dateTime ge '{DateTime.UtcNow:O}'";
                    requestConfiguration.QueryParameters.Orderby = ["start/dateTime"];
                    requestConfiguration.QueryParameters.Top = 5;
                });

            return events?.Value?.ConvertAll(e => new CalendarEvent
            {
                Title = e.Subject!,
                StartTime = e.Start?.DateTime != null ? DateTime.Parse(e.Start.DateTime).ToLocalTime() : DateTime.MinValue,
                MeetingLink = e.OnlineMeeting?.JoinUrl ?? "No link available",
                Description = e.Body?.ToString()!
            }) ?? [];
        }
        catch (ServiceException ex)
        {
            throw new UnauthorizedAccessException("Please sign in again to access your calendar.", ex);
        }
    }

    public async Task SendEventToDiscord(CalendarEvent eventDetails)
    {
        var discordMessage = new
        {
            content = $"📅 **New Outlook Event**: {eventDetails.Title}\n" +
                      $"🗓 **Date**: {eventDetails.StartTime:dddd, MMM dd, yyyy}\n" +
                      $"⏰ **Time**: {eventDetails.StartTime:hh:mm tt}\n" +
                      $"🔗 **Meeting Link**: {eventDetails.MeetingLink}"
        };

        var json = JsonConvert.SerializeObject(discordMessage);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await _httpClient.PostAsync(DiscordWebhookUrl, content);
    }

    public async Task<bool> TestDiscordWebhookAsync()
    {
        var discordMessage = new
        {
            content = "This is a test message from the OutlookToWA application."
        };

        var json = JsonConvert.SerializeObject(discordMessage);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(DiscordWebhookUrl, content);
        return response.IsSuccessStatusCode;
    }
}



