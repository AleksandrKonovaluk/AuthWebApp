using AuthWebApp.Infastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(c =>
{
	//c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
	c.AddPolicy("AllowOrigin", builder => 
	{
		// Have to submit url of trusted urls
		builder.WithOrigins("http://localhost:4200")
			   .AllowAnyHeader()
			   .AllowAnyMethod();
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Do not required for local deployment
//app.UseHttpsRedirection();

app.UseCors("AllowOrigin");
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
	//return token;
})
.WithName("GetToken")
.WithOpenApi();

app.Run();
