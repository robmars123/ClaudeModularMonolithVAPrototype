using ECommerce.EmailWorker;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
builder.Services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();

builder.Services.AddDbContext<EmailWorkerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Inbox") ?? "Data Source=emailworker-inbox.db"));
builder.Services.AddScoped<IInboxStore, EfInboxStore<EmailWorkerDbContext>>();

builder.Services.AddHostedService<EmailNotificationConsumer>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<EmailWorkerDbContext>().Database.MigrateAsync();

await host.RunAsync();
