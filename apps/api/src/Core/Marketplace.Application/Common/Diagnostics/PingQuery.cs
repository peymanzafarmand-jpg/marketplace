using FluentValidation;
using MediatR;

namespace Marketplace.Application.Common.Diagnostics;

/// <summary>
/// Deliberately trivial Query used only to prove the MediatR + FluentValidation + logging
/// pipeline is wired correctly end-to-end (see UnitTests/IntegrationTests). Not a real
/// business feature — remove once the first real module query exists and covers the same
/// wiring, or keep as a lightweight smoke-test endpoint; either is fine.
/// </summary>
public sealed record PingQuery(string Message) : IRequest<string>;

public sealed class PingQueryValidator : AbstractValidator<PingQuery>
{
    public PingQueryValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(200).WithMessage("Message must not exceed 200 characters.");
    }
}

public sealed class PingQueryHandler : IRequestHandler<PingQuery, string>
{
    public Task<string> Handle(PingQuery request, CancellationToken cancellationToken) =>
        Task.FromResult($"pong: {request.Message}");
}
