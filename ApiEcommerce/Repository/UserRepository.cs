using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEcommerce.Repository.IRepository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    private string? secretKey;

    public UserRepository(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
    }
    public User? GetUser(int id)
    {
        return _db.Users.FirstOrDefault(u => u.Id == id);
    }

    public ICollection<User> GetUsers()
    {
        return _db.Users.OrderBy(u => u.Username).ToList();
    }

    public bool IsUniqueUser(string username)
    {
        return !_db.Users.Any(u => u.Username.ToLower().Trim() == username.ToLower().Trim());
    }

    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        if (string.IsNullOrEmpty(secretKey))
        {
            return new UserLoginResponseDto()
            {
                Token = string.Empty,
                User = null,
                Message = "Secret key is not configured"
            };
        }

        if (userLoginDto == null || string.IsNullOrEmpty(userLoginDto.Username) || string.IsNullOrEmpty(userLoginDto.Password))
        {
            return new UserLoginResponseDto()
            {
                Token = string.Empty,
                User = null,
                Message = "Invalid login request"
            };
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());

        if (user == null)
        {
            return new UserLoginResponseDto()
            {
                Token = string.Empty,
                User = null,
                Message = "User not found"
            };
        }

        if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password))
        {
            return new UserLoginResponseDto()
            {
                Token = string.Empty,
                User = null,
                Message = "Incorrect password"
            };
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("Secret key is not configured.");
        }
        var key = Encoding.UTF8.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new[] {
                    new Claim("id", user.Id.ToString()),
                    new Claim("username", user.Username),
                    new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
                }
            ),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new UserLoginResponseDto()
        {
            Token = tokenHandler.WriteToken(token),
            User = new UserRegisterDto
            {
                Id = user.Id.ToString(),
                Name = user.Name,
                Username = user.Username,
                Role = user.Role,
                Password = user.Password ?? string.Empty
            },
            Message = "Login successful"
        };
    }

    public async Task<User> Register(CreateUserDto createUserDto)
    {
        var encriptedPass = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
        var user = new User
        {
            Name = createUserDto.Name,
            Username = createUserDto.Username ?? "No username",
            Password = encriptedPass,
            Role = createUserDto.Role
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }
}
