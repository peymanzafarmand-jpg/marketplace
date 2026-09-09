using Serilog.Core;
using Serilog.Events;

namespace Marketplace.Infrastructure.Logging;

/// <summary>
/// Redacts any object property whose name suggests sensitive data (password, token, secret,
/// authorization, refresh, otp, cardnumber, ...) before Serilog serializes it. Defense in
/// depth on top of "never log the raw request" discipline in LoggingBehavior.
/// </summary>
public class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    private static readonly string[] SensitiveNameFragments =
    {
        "password", "token", "secret", "authorization", "refresh", "otp", "cardnumber", "cvv"
    };

    public bool TryDestructure(
        object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue? result)
    {
        result = null;

        var type = value.GetType();
        if (!type.IsClass || type == typeof(string)) return false;

        var propertyName = type.Name;
        var isSensitiveByTypeName = SensitiveNameFragments.Any(f =>
            propertyName.Contains(f, StringComparison.OrdinalIgnoreCase));

        if (!isSensitiveByTypeName) return false;

        result = new ScalarValue("***REDACTED***");
        return true;
    }
}
