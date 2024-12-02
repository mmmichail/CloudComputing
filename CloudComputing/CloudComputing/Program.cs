using Consul;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add Consul client
var consulURI = Environment.GetEnvironmentVariable("CONSUL_URI");
if(consulURI == null){
    consulURI = "httl://consul:8500";
}

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(config =>
{
    config.Address = new Uri(consulURI);
}));

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

// Register service with Consul
var registrationID = Guid.NewGuid().ToString();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var consulClient = app.Services.GetRequiredService<IConsulClient>();
    var registration = new AgentServiceRegistration()
    {
        ID = registrationID,
        Name = "CloudComputing",
        Address = "localhost",
        Port = 8080
    };
    consulClient.Agent.ServiceRegister(registration).Wait();
});

// Deregister on shutdown
app.Lifetime.ApplicationStopped.Register(() =>
{
    var consulClient = app.Services.GetRequiredService<IConsulClient>();
    consulClient.Agent.ServiceDeregister(registrationID).Wait();
});

app.Run();
