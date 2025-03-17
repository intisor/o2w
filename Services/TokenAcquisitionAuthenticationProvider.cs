using Microsoft.Identity.Web;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

public class TokenAcquisitionAuthenticationProvider : IAuthenticationProvider
{
    private readonly ITokenAcquisition _tokenAcquisition;

    public TokenAcquisitionAuthenticationProvider(ITokenAcquisition tokenAcquisition)
    {
        _tokenAcquisition = tokenAcquisition;
    }



    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { "Calendars.Read", "User.Read" });
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
    }
}
