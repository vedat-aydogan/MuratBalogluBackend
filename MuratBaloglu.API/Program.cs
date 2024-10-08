using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using MuratBaloglu.API.Filters;
using MuratBaloglu.Application;
using MuratBaloglu.Infrastructure;
using MuratBaloglu.Infrastructure.Services.Storage.Azure;
using MuratBaloglu.Infrastructure.Services.Storage.Local;
using MuratBaloglu.Persistence;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddPersistenceServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
//builder.Services.AddStorage(MuratBaloglu.Infrastructure.Enums.StorageTypes.Local);
//builder.Services.AddStorage<LocalStorage>();
builder.Services.AddStorage<AzureStorage>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://drmuratbaloglu.com",
                           "https://drmuratbaloglu.com",
                           "http://www.drmuratbaloglu.com",
                           "https://www.drmuratbaloglu.com",
                           "http://localhost:7015",
                           "https://localhost:7015",
                           "http://localhost:4200",
                           "https://localhost:4200")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

//builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers(options =>
{
    options.Filters.Add<RolePermissionFilter>();
});
//builder.Services.AddControllers(options =>
//{
//    options.Filters.Add<ValidationFilter>();
//})
//    .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblyContaining<BlogCreateModelValidator>())
//    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer("Admin", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = true, //Oluşturulacak token değerini kimlerin/hangi originlerin/sitelerin kullanacağını belirlediğimiz değerdir. -> www.bilmemne.com
        ValidateIssuer = true, //Oluşturulacak token değerini kimin dağıttığını ifade edeceğimiz alandır. -> www.myapi.com (bu uygulamanin domaini)
        ValidateLifetime = true, //Oluşturulan token değerinin süresini kontrol edecek olan doğrulamadır.
        ValidateIssuerSigningKey = true, //Üretilecek token değerinin uygulamamıza ait bir değer olduğunu ifade eden suciry key verisinin doğrulanmasıdır.

        ValidAudience = builder.Configuration["Token:Audience"],
        ValidIssuer = builder.Configuration["Token:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"])),
        LifetimeValidator = (notBefore, expires, securityToken, validationParameters) => expires != null ? expires > DateTime.UtcNow : false,

        NameClaimType = ClaimTypes.Name //JWT üzerinden Name claimine karşılık gelen değeri User.Identity.Name propertysinden elede edebiliriz.
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseCors();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

//app.UseRouting();

app.UseCors();

app.UseAuthentication(); //(Kullanıcı/Kimlik) Doğrulama. Uygulama tarafından kullanıcının tanımlanmasıdır.

app.UseAuthorization(); //Yetkilendirme. Kimliği doğrulanmış kullanıcıların yetkilerini ifade eder.

app.MapControllers();

app.Run();
