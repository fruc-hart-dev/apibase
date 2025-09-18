using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiBase.Models;
using Microsoft.IdentityModel.Tokens;

namespace ApiBase.Services;

public interface IAuthService
{
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthService(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request)
    {
        // Get all users and find the one with matching credentials
        var users = await _userService.GetAllUsersAsync();
        var user = users.FirstOrDefault(u => 
            u.Name == request.Username && 
            u.Password == request.Password); // In a real app, use password hashing!

        if (user == null)
            return null;

        // Generate JWT token
        var token = GenerateJwtToken(user);

        return new LoginResponse
        {
            Token = token,
            Username = user.Name
        };
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60")),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}