using Consul;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Add Consul client
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(config =>
{
    config.Address = new Uri(builder.Configuration["Consul:ConsulAddress"]);
}));

// Register service with Consul
app.Lifetime.ApplicationStarted.Register(() =>
{
    var consulClient = app.Services.GetRequiredService<IConsulClient>();
    var registration = new AgentServiceRegistration()
    {
        ID = builder.Configuration["Consul:ServiceId"],
        Name = builder.Configuration["Consul:ServiceName"],
        Address = "localhost",
        Port = 5000
    };
    consulClient.Agent.ServiceRegister(registration).Wait();
});

// Deregister on shutdown
app.Lifetime.ApplicationStopped.Register(() =>
{
    var consulClient = app.Services.GetRequiredService<IConsulClient>();
    consulClient.Agent.ServiceDeregister(builder.Configuration["Consul:ServiceId"]).Wait();
});

app.Run();
