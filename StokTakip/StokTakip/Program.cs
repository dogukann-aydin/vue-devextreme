using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Baðlantý Ayarlarý
var mongoConnectionString = "mongodb://localhost:27017";
var databaseName = "OnMuhasebe";
var collectionName = "Kullanicilar";

// MongoDB Baðlantýsýný Yap
var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(databaseName);
var mongoCollection = mongoDatabase.GetCollection<BsonDocument>(collectionName);

// MongoDB Baðlantýsýný Servis Olarak Kaydet
builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton(mongoCollection);

// Cookie Authentication yapýlandýrmasý
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS üzerinden gönderilsin
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.LoginPath = "/api/login";  // Giriþ URL'si
    });

// CORS yapýlandýrmasý ekleyin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:8080")  // Vue.js uygulamanýzýn adresi
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// CORS'u kullanmak için middleware ekleyin
app.UseCors("AllowVueApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();  // Kimlik doðrulamasýný aktifleþtir
app.UseAuthorization();

// LoginController
app.MapPost("/api/login", async ([FromBody] LoginRequest request, IMongoCollection<BsonDocument> collection, HttpContext httpContext) =>
{
    // Kullanýcýdan gelen parolayý hashle
    string hashedPassword = ComputeSha256Hash(request.Password);

    // MongoDB'de kullanýcý adý ve hashli parola ile eþleþen kayýt ara
    var filter = Builders<BsonDocument>.Filter.And(
        Builders<BsonDocument>.Filter.Eq("kullaniciAdi", request.Username),
        Builders<BsonDocument>.Filter.Eq("parola", hashedPassword)
    );

    var user = await collection.Find(filter).FirstOrDefaultAsync();

    if (user != null)
    {
        // Cookie oluþtur
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim("kullaniciId", user["_id"].ToString()) // Kullanýcýya ait id veya diðer bilgileri cookie'ye ekleyebilirsiniz
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await httpContext.SignInAsync("Cookies", claimsPrincipal);

        // Baþarýlý giriþ sonrasý kullanýcýyý /dashboard adresine yönlendir
        httpContext.Response.Redirect("http://localhost:8080/dashboard");
        return Results.Ok(new { success = true, message = "Giriþ baþarýlý!" });
    }
    else
    {
        // Hatalý giriþ durumunda 401 hata kodu ile JSON dönülmesi
        return Results.Problem(detail: "Hatalý kullanýcý adý veya parola.", statusCode: 401);
    }
});

app.MapControllers();
app.Run();

// Parolayý SHA256 ile hashlemek için yardýmcý metot
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

// Giriþ için gerekli veri modeli
record LoginRequest(string Username, string Password);
