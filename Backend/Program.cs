using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
   options.AddPolicy("frontend", builder => {
      builder.WithOrigins("http://localhost:5000").AllowAnyHeader().AllowAnyMethod();
   });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
   {
       c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Calls", Description = "Keep track of your tasks", Version = "v1" });
   });

var app = builder.Build();

app.UseCors(
    options => options.WithOrigins("http://localhost:5000").AllowAnyMethod().AllowAnyHeader()
);

app.UseSwagger();
app.UseSwaggerUI(c =>
   {
     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
   });

Database _db = new Database();

app.MapGet("/", () => JsonSerializer.Serialize("PFK"));
app.MapGet("/init", () => JsonSerializer.Serialize(_db.InitDatabase()));
app.MapGet("/getStatuses", () => JsonSerializer.Serialize(_db.GetStatuses()));
app.MapGet("/deleteTask/{id}", (int id) => JsonSerializer.Serialize(_db.DeleteTask(id)));
app.MapGet("/setTaskAsDone/{id}", (int id) => JsonSerializer.Serialize(_db.SetTaskAsDone(id)));
app.MapGet("/getTasks/{package}", (string package) => JsonSerializer.Serialize(_db.GetTasks(package)));
app.MapGet("/getNumberOfTasks/{package}", (string package) => JsonSerializer.Serialize(_db.GetNumberOfTasks(package)));
app.MapGet("/updateTask/{package}", (string package) => JsonSerializer.Serialize(_db.UpdateTask(package)));
app.MapGet("/addTask/{package}", (string package) => JsonSerializer.Serialize(_db.AddTask(package)));

app.Run();