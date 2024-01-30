using AuthWebApp.Infastructure;
using AuthWebApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var allowedUsers = new AllowedUsersList();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(c =>
{
	c.AddPolicy("AllowLocalhostOrigin", builder =>
	{
		// Have to submit url of trusted urls
		builder.WithOrigins("http://localhost:4200")
			   .AllowAnyHeader()
			   .AllowAnyMethod();
	});
});
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			// указывает, будет ли валидироваться издатель при валидации токена
			ValidateIssuer = true,
			// строка, представляющая издателя
			ValidIssuer = AuthOptions.ISSUER,
			// будет ли валидироваться потребитель токена
			ValidateAudience = true,
			// установка потребителя токена
			ValidAudience = AuthOptions.AUDIENCE,
			// будет ли валидироваться время существования
			ValidateLifetime = true,
			// установка ключа безопасности
			IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
			// валидация ключа безопасности
			ValidateIssuerSigningKey = true,
		};
	});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//// UseHttpsRedirection do not required for local deployment
//app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowLocalhostOrigin");
//app.UseCors(options => options.AllowAnyOrigin());

app.MapGet("/api/getToken", (string username, string password) =>
{
	var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
	var token = new JwtSecurityToken(
		issuer: AuthOptions.ISSUER,
		audience: AuthOptions.AUDIENCE,
		claims: claims,
		expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
		signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
	return new JwtSecurityTokenHandler().WriteToken(token);
})
.WithName("GetToken")
.WithOpenApi();

app.MapPost("/api/login",
	(Person loginData) =>
	{
		Person person = allowedUsers.allowedPersons.FirstOrDefault(p => p.Email == loginData.Email && p.Password == loginData.Password);
		// если пользователь не найден, отправляем статусный код 401
		if (person is null) 
			return Results.Unauthorized();

		var claims = new List<Claim> { new Claim(ClaimTypes.Name, person.Email) };
		// создаем JWT-токен
		var jwt = new JwtSecurityToken(
			issuer: AuthOptions.ISSUER,
			audience: AuthOptions.AUDIENCE,
			claims: claims,
			expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
			signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
		var EncodedToken = new JwtSecurityTokenHandler().WriteToken(jwt);
		var response = new { access_token = EncodedToken, userName = person.Password };
		//return new JwtSecurityTokenHandler().WriteToken(jwt);
		return Results.Json(response);
	})
	.WithName("Login")
	.WithOpenApi(); ;


app.Map("/api/data",
		[Authorize]
		() => new { message = "Hello World!" })
	.WithName("Data");

app.MapGet("/api/getData",
		[Authorize]
		() => new { message = "dfsaf" })
.WithName("GetData");

app.Run();
