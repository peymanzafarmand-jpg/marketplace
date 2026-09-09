using Marketplace.Application.Common.Interfaces;

namespace Marketplace.Infrastructure.Common;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
