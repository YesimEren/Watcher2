using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("WatcherApiCors", opts =>
    {
        opts.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
   
}

app.UseCors("WatcherApiCors");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();

