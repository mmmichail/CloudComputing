using Consul;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var consulURI = Environment.GetEnvironmentVariable("CONSUL_URI");
if (consulURI == null)
{
    consulURI = "http://localhost:8500";
}

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(config =>
{
    config.Address = new Uri(consulURI);
}));

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecksUI().AddInMemoryStorage();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var registrationID = Guid.NewGuid().ToString();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var consulClient = app.Services.GetRequiredService<IConsulClient>();
    var registration = new AgentServiceRegistration()
    {
        ID = registrationID,
        Name = "CloudComputing",
        Address = "localhost",
        Port = 8080,
        Tags = new[] { "CloudComputingService" }
    };
    consulClient.Agent.ServiceRegister(registration).Wait();
});

app.Lifetime.ApplicationStopped.Register(() =>
{
    var consulClient = app.Services.GetRequiredService<IConsulClient>();
    consulClient.Agent.ServiceDeregister(registrationID).Wait();
});

app.MapHealthChecks("/health");

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health";
});

app.Run();
