using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Ba�lant� Ayarlar�
var mongoConnectionString = "mongodb://localhost:27017";
var databaseName = "OnMuhasebe";
var collectionName = "Kullanicilar";

// MongoDB Ba�lant�s�n� Yap
var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(databaseName);
var mongoCollection = mongoDatabase.GetCollection<BsonDocument>(collectionName);

// MongoDB Ba�lant�s�n� Servis Olarak Kaydet
builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton(mongoCollection);

// Cookie Authentication yap�land�rmas�
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS �zerinden g�nderilsin
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.LoginPath = "/api/login";  // Giri� URL'si
    });

// CORS yap�land�rmas� ekleyin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:8080")  // Vue.js uygulaman�z�n adresi
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// CORS'u kullanmak i�in middleware ekleyin
app.UseCors("AllowVueApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();  // Kimlik do�rulamas�n� aktifle�tir
app.UseAuthorization();

// LoginController
app.MapPost("/api/login", async ([FromBody] LoginRequest request, IMongoCollection<BsonDocument> collection, HttpContext httpContext) =>
{
    // Kullan�c�dan gelen parolay� hashle
    string hashedPassword = ComputeSha256Hash(request.Password);

    // MongoDB'de kullan�c� ad� ve hashli parola ile e�le�en kay�t ara
    var filter = Builders<BsonDocument>.Filter.And(
        Builders<BsonDocument>.Filter.Eq("kullaniciAdi", request.Username),
        Builders<BsonDocument>.Filter.Eq("parola", hashedPassword)
    );

    var user = await collection.Find(filter).FirstOrDefaultAsync();

    if (user != null)
    {
        // Cookie olu�tur
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim("kullaniciId", user["_id"].ToString()) // Kullan�c�ya ait id veya di�er bilgileri cookie'ye ekleyebilirsiniz
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await httpContext.SignInAsync("Cookies", claimsPrincipal);

        // Ba�ar�l� giri� sonras� kullan�c�y� /dashboard adresine y�nlendir
        httpContext.Response.Redirect("http://localhost:8080/dashboard");
        return Results.Ok(new { success = true, message = "Giri� ba�ar�l�!" });
    }
    else
    {
        // Hatal� giri� durumunda 401 hata kodu ile JSON d�n�lmesi
        return Results.Problem(detail: "Hatal� kullan�c� ad� veya parola.", statusCode: 401);
    }
});

app.MapControllers();
app.Run();

// Parolay� SHA256 ile hashlemek i�in yard�mc� metot
string ComputeSha256Hash(string rawData)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        StringBuilder builder = new StringBuilder();
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}

// Giri� i�in gerekli veri modeli
record LoginRequest(string Username, string Password);
