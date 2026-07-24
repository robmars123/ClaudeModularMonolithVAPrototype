namespace ECommerce.Shared.Messaging;

/// <summary>Bound from the "RabbitMq" configuration section.</summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    /// <summary>Topic exchange every pub/sub subscriber binds a queue to.</summary>
    public string PubSubExchange { get; set; } = "ecommerce.events";
}
