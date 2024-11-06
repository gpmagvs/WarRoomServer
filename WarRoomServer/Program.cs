using WarRoomServer.Data.Contexts;
using WarRoomServer.Hubs;
using WarRoomServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true) // 允许任何来源
              .AllowCredentials(); // 允许凭据
    });
});

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});


builder.Services.AddMemoryCache();
string _connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:Default");
builder.Services.AddSqlServer<WarRoomDbContext>(_connectionString); // 注入資料庫
builder.Services.AddScoped<DataCacheService>();
//builder.Services.AddHostedService<DataBaseMigrateService>(); // 開發時使用，不要在生產環境使用
builder.Services.AddHostedService<RealTimeDataCacheService>(); // Realtime 數據緩存服務
builder.Services.AddScoped<VersionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowAll");

app.MapHub<TestHub>("/Test");
app.MapHub<EquipmentStatusHub>("/EquipmentStatus");

app.Run();


