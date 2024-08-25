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
        ValidateAudience = true, //Olu�turulacak token de�erini kimlerin/hangi originlerin/sitelerin kullanaca��n� belirledi�imiz de�erdir. -> www.bilmemne.com
        ValidateIssuer = true, //Olu�turulacak token de�erini kimin da��tt���n� ifade edece�imiz aland�r. -> www.myapi.com (bu uygulamanin domaini)
        ValidateLifetime = true, //Olu�turulan token de�erinin s�resini kontrol edecek olan do�rulamad�r.
        ValidateIssuerSigningKey = true, //�retilecek token de�erinin uygulamam�za ait bir de�er oldu�unu ifade eden suciry key verisinin do�rulanmas�d�r.

        ValidAudience = builder.Configuration["Token:Audience"],
        ValidIssuer = builder.Configuration["Token:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"])),
        LifetimeValidator = (notBefore, expires, securityToken, validationParameters) => expires != null ? expires > DateTime.UtcNow : false,

        NameClaimType = ClaimTypes.Name //JWT �zerinden Name claimine kar��l�k gelen de�eri User.Identity.Name propertysinden elede edebiliriz.
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

app.UseAuthentication(); //(Kullan�c�/Kimlik) Do�rulama. Uygulama taraf�ndan kullan�c�n�n tan�mlanmas�d�r.

app.UseAuthorization(); //Yetkilendirme. Kimli�i do�rulanm�� kullan�c�lar�n yetkilerini ifade eder.

app.MapControllers();

app.Run();
