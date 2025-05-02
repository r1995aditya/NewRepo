
using Microsoft.Extensions.Options;
using ReqresApi.Application.Interfaces;
using ReqresApi.Infrastructure.Configuration;
using ReqresApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ReqresApiOptions>(builder.Configuration.GetSection("ReqresApi"));
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IUserService, UserService>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ReqresApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
