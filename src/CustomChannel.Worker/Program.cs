using CustomChannel.Infrastructure.PubSub;
using CustomChannel.Worker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddGooglePubSub(builder.Configuration);
builder.Services.AddHostedService<PubSubListenerService>();

IHost host = builder.Build();
await host.RunAsync();
