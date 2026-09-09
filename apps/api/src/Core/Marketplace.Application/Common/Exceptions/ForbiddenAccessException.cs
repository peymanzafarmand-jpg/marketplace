namespace Marketplace.Application.Common.Exceptions;

/// <summary>Thrown when the authenticated user lacks a required permission or resource ownership.</summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base("Access to this resource is forbidden.")
    {
    }
}
