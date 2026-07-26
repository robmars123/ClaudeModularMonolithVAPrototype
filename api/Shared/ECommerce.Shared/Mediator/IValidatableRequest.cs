namespace ECommerce.Shared.Mediator;

/// <summary>
/// Implemented by a command/query that wants declarative validation. <see cref="Validate"/> runs inside
/// <c>ValidationBehavior</c>, before the handler — return one message per violation, empty when valid.
/// </summary>
public interface IValidatableRequest
{
    IEnumerable<string> Validate();
}
