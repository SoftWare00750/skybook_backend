using System.Text.Json;

namespace SkyBook.Api.Services;

/// Thrown for any social-auth failure that should be surfaced to the
/// caller as a 400/401 rather than a generic 500 — invalid/expired
/// tokens, missing email permission, wrong audience, etc.
public class SocialAuthException : Exception
{
    public SocialAuthException(string message) : base(message) { }
}

/// Verifies third-party sign-in tokens *server-side* before the app trusts
/// anything derived from them. The Flutter app never sends us a name/email
/// directly — only the raw Google ID token / Facebook access token — so a
/// tampered client can't claim someone else's email address.
public class SocialAuthService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<SocialAuthService> _logger;

    public SocialAuthService(HttpClient http, IConfiguration config, ILogger<SocialAuthService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    /// Verifies a Google ID token via Google's tokeninfo endpoint (simple
    /// HTTP call — no extra SDK/NuGet dependency needed). If
    /// `Google:ClientId` is configured in appsettings, the token's
    /// audience must match it, so a token minted for a *different* app
    /// can't be replayed against this backend.
    public async Task<(string Email, string Name)> VerifyGoogleIdTokenAsync(string idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new SocialAuthException("Missing Google ID token.");
        }

        JsonDocument doc;
        try
        {
            var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}";
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new SocialAuthException("Invalid or expired Google sign-in. Please try again.");
            }
            doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        }
        catch (SocialAuthException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach Google's tokeninfo endpoint.");
            throw new SocialAuthException("Couldn't verify the Google sign-in right now. Please try again.");
        }

        using (doc)
        {
            var root = doc.RootElement;

            var expectedClientId = _config["Google:ClientId"];
            if (!string.IsNullOrWhiteSpace(expectedClientId))
            {
                var aud = root.TryGetProperty("aud", out var audEl) ? audEl.GetString() : null;
                if (aud != expectedClientId)
                {
                    throw new SocialAuthException("This Google sign-in wasn't issued for this app.");
                }
            }

            if (root.TryGetProperty("email_verified", out var verifiedEl))
            {
                var verified = verifiedEl.GetString();
                if (verified != null && verified != "true")
                {
                    throw new SocialAuthException("Please verify your Google email address before signing in.");
                }
            }

            if (!root.TryGetProperty("email", out var emailEl) || string.IsNullOrWhiteSpace(emailEl.GetString()))
            {
                throw new SocialAuthException("Google didn't return an email address for this account.");
            }

            var email = emailEl.GetString()!;
            var name = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
            return (email, string.IsNullOrWhiteSpace(name) ? email.Split('@')[0] : name!);
        }
    }

    /// Verifies a Facebook access token by using it to call the Graph
    /// API's `/me` endpoint — a token that doesn't actually belong to a
    /// real, current Facebook session will simply fail this call.
    public async Task<(string Email, string Name)> VerifyFacebookAccessTokenAsync(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new SocialAuthException("Missing Facebook access token.");
        }

        JsonDocument doc;
        try
        {
            var url = $"https://graph.facebook.com/me?fields=id,name,email&access_token={Uri.EscapeDataString(accessToken)}";
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new SocialAuthException("Invalid or expired Facebook sign-in. Please try again.");
            }
            doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        }
        catch (SocialAuthException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach the Facebook Graph API.");
            throw new SocialAuthException("Couldn't verify the Facebook sign-in right now. Please try again.");
        }

        using (doc)
        {
            var root = doc.RootElement;

            if (!root.TryGetProperty("email", out var emailEl) || string.IsNullOrWhiteSpace(emailEl.GetString()))
            {
                throw new SocialAuthException(
                    "Facebook didn't share an email address for this account. Make sure the 'email' permission was granted."
                );
            }

            var email = emailEl.GetString()!;
            var name = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
            return (email, string.IsNullOrWhiteSpace(name) ? email.Split('@')[0] : name!);
        }
    }
}
