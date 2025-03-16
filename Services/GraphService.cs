using Microsoft.Graph;

public class GraphService
{
    private readonly GraphServiceClient _graphClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GraphService(
        GraphServiceClient graphClient, 
        IHttpContextAccessor httpContextAccessor)
    {
        _graphClient = graphClient;
        _httpContextAccessor = httpContextAccessor;
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
                Description = e.Body?.ToString() !
            }) ?? [];
        }
        catch (ServiceException ex)
        {
            throw new UnauthorizedAccessException("Please sign in again to access your calendar.", ex);
        }
    }
}
