using Microsoft.Extensions.Options;

namespace ECommerce.Shared.Messaging;

public sealed class RabbitMqOptionsValidator : IValidateOptions<RabbitMqOptions>
{
    public ValidateOptionsResult Validate(string? name, RabbitMqOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.HostName))
            errors.Add($"{RabbitMqOptions.SectionName}:HostName is required.");
        if (options.Port is <= 0 or > 65535)
            errors.Add($"{RabbitMqOptions.SectionName}:Port must be between 1 and 65535.");
        if (string.IsNullOrWhiteSpace(options.PubSubExchange))
            errors.Add($"{RabbitMqOptions.SectionName}:PubSubExchange is required.");

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
