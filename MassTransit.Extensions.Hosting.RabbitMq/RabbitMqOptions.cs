namespace MassTransit.Extensions.Hosting.RabbitMq
{
    /// <summary>
    /// Contains the options for configuring a RabbitMq host.
    /// </summary>
    public class RabbitMqOptions
    {
        /// <summary>
        /// Gets the default port for RabbitMq.
        /// </summary>
        public const ushort DefaultPort = 5672;

        /// <summary>
        /// Gets or sets the client-provided connection name.
        /// </summary>
        public string ConnectionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the RabbitMq host to connect to (should be a valid hostname).
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the RabbitMq port to connect to.
        /// </summary>
        public ushort Port { get; set; } = DefaultPort;

        /// <summary>
        /// Gets or sets the heartbeat interval (in seconds) to keep the host connection alive.
        /// </summary>
        public ushort? Heartbeat { get; set; }

        /// <summary>
        /// Gets or sets the virtual host for the connection.
        /// </summary>
        public string VirtualHost { get; set; }

        /// <summary>
        /// Gets or sets the username for connecting to the host.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for connection to the host.
        /// </summary>
        public string Password { get; set; }
    }
}