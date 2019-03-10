[MassTransit]: http://masstransit-project.com/
[Microsoft.Extensions.Hosting]: https://github.com/aspnet/Hosting
[Microsoft.Extensions.DependencyInjection]: https://github.com/aspnet/DependencyInjection
[Microsoft.Extensions.Options]: https://github.com/aspnet/Options
[MassTransit.Host]: https://github.com/MassTransit/MassTransit/tree/develop/src/MassTransit.Host
[Generic Host]: https://github.com/aspnet/Hosting/issues/1163

# Overview
[![Build Status](https://ci.appveyor.com/api/projects/status/v57og3d9wbj2jgy9/branch/master?svg=true)](https://ci.appveyor.com/project/polewskm/masstransit-extensions-hosting/branch/master)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MassTransit.Extensions.Hosting.svg?style=flat)](https://www.nuget.org/packages/MassTransit.Extensions.Hosting/)
[![NuGet Version](https://img.shields.io/nuget/v/MassTransit.Extensions.Hosting.svg?style=flat)](https://www.nuget.org/packages/MassTransit.Extensions.Hosting/)

This library provides extensions for [MassTransit] to support:
* Hosting with [Microsoft.Extensions.Hosting] (i.e. `IHostedService`)
* Dependency Injection with [Microsoft.Extensions.DependencyInjection] (i.e. `IServiceProvider`)

This library was designed to make no assumptions how to configure MassTransit bus instances and provides the developer with flexible hosting implementation options.

* `MassTransit.Extensions.Hosting`
  * Provides extensions common to all MassTransit transports
  * Includes an in-memory bus

Transport specific libraries:
* `MassTransit.Extensions.Hosting.RabbitMq`
* `MassTransit.Extensions.Hosting.ActiveMq`

## Problem Statement
The core MassTransit library only provides abstractions and no implementation unless the [MassTransit.Host] package is used. Unfortunatly the `MassTransit.Host` package makes many assumptions and forces the developer with many potentially unwanted conventions such as:

* Autofac as the DI container
  * No ability to modify the registrations in ContainerBuilder
* A prebuilt Windows Service executable using Topshelf
  * No ability to modify the Topshelf service configuration
* log4net as the logging provider
* Configuration settings from assembly config files
  * web.config is not supported
 
None of the items mentioned above are bad or wrong, just potentially not intended to be used in every host implementation. The individual libraries such as Autofac, log4net and Topshelf are in fact extremly helpful, just not always needed in every implementation.

Also the `MassTransit.Host` is not usable in other hosting environments such as Web Applications, Console Applications, etc since it provides a prebuilt Topshelf executable. Instead it would be convenient to use the same style of design for MassTransit bus instances but agnostic to their hosting environment.

## Proposed Solution
This library uses the new [Generic Host] pattern from ASP.NET Core as the _glue_ for building MassTransit applications. Other than using the hosting and dependency injection abstractions, this library makes no assumptions on DI containers, logging providers, configuration providers, and the hosting environment.

## Features
* Fluent configuration interfaces
* No assumptions on configuration (i.e. the developer is in full control)
* Access to the existing MassTransit configuration interfaces (i.e. `IBusFactoryConfigurator`)
* Ability to retrieve the bus host by connection name after startup
* Supports all the available MassTransit transports

## Usage

### Step 1) Add NuGet Package(s)
> PM> Install-Package MassTransit.Extensions.Hosting
> 
> Then choose a transport...
> 
> PM> Install-Package MassTransit.Extensions.Hosting.RabbitMq
> 
> PM> Install-Package MassTransit.Extensions.Hosting.ActiveMq

### Step 2) Add MassTransit Services
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMassTransit(busBuilder =>
    {
        // ...
    });
}
```

### Step 3) Configure Bus Transports
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMassTransit(busBuilder =>
    {
        busBuilder.UseInMemory("connection-name-1", hostBuilder => 
        {
            hostBuilder.UseServiceScope();

            hostBuilder.AddConfigurator(configureBus =>
            {
                configureBus.UseRetry(r => r.Immediate(3));
            });

            // ...
        });

        busBuilder.UseRabbitMq("connection-name-2", "127.0.0.1", "/vhost", hostBuilder => 
        {
            hostBuilder.UseCredentials("guest", "guest");

            // ...
        });

        busBuilder.UseActiveMq("connection-name-3", "127.0.0.1", hostBuilder => 
        {
            hostBuilder.UseSsl();
            hostBuilder.UseCredentials("guest", "guest");

            // ...
        });

    });
}
```

### Step 4) Configure Receive Endpoints
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMassTransit(busBuilder =>
    {
        busBuilder.UseInMemory("connection-name-1", hostBuilder => 
        {
            // ...

            hostBuilder.AddReceiveEndpoint("example-queue-1", endpointBuilder =>
            {
                endpointBuilder.AddConfigurator(configureEndpoint =>
                {
                    configureEndpoint.UseRetry(r => r.Immediate(3));
                });

                // ...
            });
        });
    });
}
```

### Step 5) Configure Consumers
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMassTransit(busBuilder =>
    {
        busBuilder.UseInMemory("connection-name-1", hostBuilder => 
        {
            // ...

            hostBuilder.AddReceiveEndpoint("example-queue-1", endpointBuilder =>
            {
                endpointBuilder.AddConsumer<ExampleConsumer>(configureConsumer =>
                {
                    configureConsumer.UseRateLimit(10);

                    // ...
                });
            });
        });
    });
}
```

### Step 6) Bus Manager
```csharp
public async Task Run(IServiceProvider serviceProvider)
{
    var busManager = serviceProvider.GetRequiredService<IBusManager>();

    // start all bus instances
    await busManager.StartAsync().ConfigureAwait(false);

    // get a reference to a named bus instance
    // and publish a message
    IBus bus = busManager.GetBus("connection-name-1");
    await bus.Publish(/* ... */).ConfigureAwait(false);

    // ...

    // stop all bus instances
    await busManager.StopAsync().ConfigureAwait(false);
}
```

## Example Console Host
```csharp
public static class Program
{
    private static void Main(string[] args)
    {
        // use the new generic host from ASP.NET Core
        // see for more info: https://github.com/aspnet/Hosting/issues/1163
        new HostBuilder()
            .ConfigureHostConfiguration(config => config.AddEnvironmentVariables())
            .ConfigureAppConfiguration((context, builder) => ConfigureAppConfiguration(context, builder, args))
            .ConfigureServices(ConfigureServices)
            .Build()
            .Run();
    }

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder configurationBuilder, string[] args)
    {
        var environmentName = context.HostingEnvironment.EnvironmentName;

        configurationBuilder.AddJsonFile("appsettings.json", optional: true);
        configurationBuilder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);

        configurationBuilder.AddCommandLine(args);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddTransient<IWidgetService, WidgetService>();
        // add other DI services...

        // optionally use configuration for any settings
        var configuration = context.Configuration;

        // the following adds IBusManager which is also an IHostedService that is started/stopped by HostBuilder
        services.AddMassTransit(busBuilder =>
        {
            // configure RabbitMQ
            busBuilder.UseRabbitMq(configuration.GetSection("MassTransit:RabbitMq"), hostBuilder =>
            {
                // use scopes for all downstream filters and consumers
                // i.e. per-request lifetime
                hostBuilder.UseServiceScope();

                // example adding an optional configurator to the bus
                // using IRabbitMqBusFactoryConfigurator
                hostBuilder.AddConfigurator(configureBus =>
                {
                    configureBus.UseRetry(r => r.Immediate(1));
                });

                // example adding a receive endpoint to the bus
                hostBuilder.AddReceiveEndpoint("example-queue-1", endpointBuilder =>
                {
                    // example adding an optional configurator to the receive endpoint
                    // using IRabbitMqReceiveEndpointConfigurator
                    endpointBuilder.AddConfigurator(configureEndpoint =>
                    {
                        configureEndpoint.UseRetry(r => r.Immediate(3));
                    });

                    // example adding a consumer to the receive endpoint
                    endpointBuilder.AddConsumer<ExampleConsumer>(configureConsumer =>
                    {
                        // example adding an optional configurator to the consumer
                        // using IConsumerConfigurator<TConsumer>
                        configureConsumer.UseRetry(r => r.Immediate(3));
                    });
                });
            });

            // adding more bus instances...
            busBuilder.UseInMemory("connection-name-2", hostBuilder =>
            {
                hostBuilder.UseServiceScope();
                hostBuilder.AddReceiveEndpoint("example-queue-2", endpointBuilder =>
                {
                    endpointBuilder.AddConsumer<ExampleConsumer>();
                });
            });
        });
    }
}
```

## Example Topshelf Host
```csharp
public static class Program
{
    private static readonly string EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

    private static void Main()
    {
        var configuration = LoadConfiguration();
        using (var services = CreateServiceProvider(configuration))
        {
            Run(services);
        }
    }

    private static IConfiguration LoadConfiguration()
    {
        var configBuilder = new ConfigurationBuilder();

        configBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configBuilder.AddJsonFile($"appsettings.{EnvironmentName}.json", optional: true, reloadOnChange: true);

        configBuilder.AddEnvironmentVariables();

        // WARNING: Do not add the command line arguments to the configuration
        // builder. The arguments used by Topshelf are not compatible with the
        // .NET Core command line provider.
        // configBuilder.AddCommandLine(args);

        var configuration = configBuilder.Build();
        return configuration;
    }

    private static ServiceProvider CreateServiceProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        ConfigureServices(configuration, services);

        return services.BuildServiceProvider();
    }

    private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddSingleton(configuration);

        // configure logging
        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));

            builder.AddNLog();
            builder.AddDebug();
        });

        // the following adds IBusManager which is also an IHostedService that is started/stopped down below
        services.AddMassTransit(busBuilder =>
        {
            // configure RabbitMQ
            busBuilder.UseRabbitMq(configuration.GetSection("MassTransit:RabbitMq"), hostBuilder =>
            {
                hostBuilder.UseServiceScope();

                hostBuilder.AddReceiveEndpoint("example-queue-1", endpointBuilder =>
                {
                    endpointBuilder.AddConsumer<ExampleConsumer>();
                });
            });
        });
    }

    private static void Run(IServiceProvider serviceProvider)
    {
        ExtensionsLogger.Use(); // MassTransit
        NLogLogWriterFactory.Use(); // Topshelf

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(Program));

        logger.LogInformation("Program Starting");
        logger.LogInformation("Environment={0}", EnvironmentName);
        logger.LogInformation(Assembly.GetExecutingAssembly().FullName);

        // configure and run the topshelf service
        HostFactory.Run(hostConfigurator =>
        {
            // log exception details
            hostConfigurator.OnException(exception => logger.LogError(exception.ToString()));

            // configure the hosted service
            hostConfigurator.Service<IHostedService>(serviceConfigurator =>
            {
                serviceConfigurator.ConstructUsing(serviceProvider.GetRequiredService<IHostedService>);
                serviceConfigurator.WhenStarted(async host => await host.StartAsync(CancellationToken.None).ConfigureAwait(false));
                serviceConfigurator.WhenStopped(async host => await host.StopAsync(CancellationToken.None).ConfigureAwait(false));
            });

            // by default use a least privileged account
            hostConfigurator.RunAsNetworkService();

            // default the ServiceName, DisplayName, and Description from the entry assembly
            hostConfigurator.UseAssemblyInfoForServiceInfo();
        });

        logger.LogInformation("Program Exiting");
    }
}
```

## Release Notes
* v1.0.5 - Added the ability to bind RabbitMq configuration options
* v1.0.6 - Updated documentation
* v1.0.7 - Added ActiveMQ transport

## Feedback
Please provide any feedback, comments, or issues to this GitHub project [here][issues].

[issues]: https://github.com/NCodeGroup/MassTransit.Extensions.Hosting/issues
