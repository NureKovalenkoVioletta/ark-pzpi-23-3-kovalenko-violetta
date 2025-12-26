using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using FitnessProject.DAL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitnessProject.Security;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserRepository _userRepository;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IUserRepository userRepository)
        : base(options, logger, encoder, clock)
    {
        _userRepository = userRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            Logger.LogWarning("No Authorization header");
            return AuthenticateResult.NoResult();
        }

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (!"Basic".Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning("Auth scheme is not Basic");
                return AuthenticateResult.NoResult();
            }

            var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? string.Empty);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
            if (credentials.Length != 2)
            {
                Logger.LogWarning("Invalid Authorization header format");
                return AuthenticateResult.Fail("Invalid Authorization header");
            }

            var email = credentials[0];
            var password = credentials[1];

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                Logger.LogWarning("User not found for email {Email}", email);
                return AuthenticateResult.Fail("Invalid credentials");
            }

            var (hashedBase64, hashedHex) = HashPassword(password);
            var stored = user.PasswordHash ?? string.Empty;
            if (!string.Equals(hashedBase64, stored, StringComparison.Ordinal) &&
                !string.Equals(hashedHex, stored, StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning("Password mismatch for email {Email}. Stored='{Stored}', calcBase64='{CalcBase64}', calcHex='{CalcHex}'",
                    email, stored, hashedBase64, hashedHex);
                return AuthenticateResult.Fail("Invalid credentials");
            }

            var locale = NormalizeLocale(user.Locale);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("locale", locale)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Logger.LogInformation("Authenticated {Email} with role {Role}", email, user.Role);
            return AuthenticateResult.Success(ticket);
        }
        catch (FormatException)
        {
            Logger.LogError("Authorization header format exception");
            return AuthenticateResult.Fail("Invalid Authorization header");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Authentication failed");
            return AuthenticateResult.Fail(ex);
        }
    }

    private static (string Base64, string Hex) HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        var base64 = Convert.ToBase64String(hash);
        var hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        return (base64, hex);
    }

    private static string NormalizeLocale(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return "uk-UA";
        }

        var lower = locale.ToLowerInvariant();
        if (lower.StartsWith("uk") || lower.StartsWith("ua"))
            return "uk-UA";
        if (lower.StartsWith("en"))
            return "en-US";

        return "uk-UA";
    }
}

