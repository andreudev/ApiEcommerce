using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEcommerce.Repository.IRepository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    private string? secretKey;

    private readonly UserManager<ApplicationUser> _userManager;

    private readonly RoleManager<IdentityRole> _roleManager;

    public UserRepository(
        ApplicationDbContext db,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        _db = db;
        secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public ApplicationUser? GetUser(string id)
    {
        return _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
    }

    public ICollection<ApplicationUser> GetUsers()
    {
        return _db.ApplicationUsers.OrderBy(u => u.UserName).ToList();
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
                Message = "Secret key is not configured",
            };
        }

        if (
            userLoginDto == null
            || string.IsNullOrEmpty(userLoginDto.Username)
            || string.IsNullOrEmpty(userLoginDto.Password)
        )
        {
            return new UserLoginResponseDto()
            {
                Token = string.Empty,
                User = null,
                Message = "Invalid login request",
            };
        }

        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u =>
            u.UserName != null
            && u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim()
        );

        if (user == null)
        {
            return new UserLoginResponseDto()
            {
                Token = string.Empty,
                User = null,
                Message = "User not found",
            };
        }

        bool isValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);

        if (!isValid)
        {
            return new UserLoginResponseDto()
            {
                Token = string.Empty,
                User = null,
                Message = "Incorrect password",
            };
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("Secret key is not configured.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var key = Encoding.UTF8.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("username", user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty),
                }
            ),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new UserLoginResponseDto()
        {
            Token = tokenHandler.WriteToken(token),
            User = user.Adapt<UserDataDto>(),
            Message = "Login successful",
        };
    }

    public async Task<UserDataDto> Register(CreateUserDto createUserDto)
    {
        if (
            createUserDto == null
            || string.IsNullOrEmpty(createUserDto.Username)
            || string.IsNullOrEmpty(createUserDto.Password)
        )
        {
            throw new ArgumentException("Invalid user data");
        }

        var user = new ApplicationUser()
        {
            UserName = createUserDto.Username,
            Email = createUserDto.Username,
            NormalizedEmail = createUserDto.Username.ToUpper(),
            Name = createUserDto.Name,
        };

        var result = await _userManager.CreateAsync(user, createUserDto.Password);

        if (result.Succeeded)
        {
            var userRole = createUserDto.Role ?? "User";
            var RoleExists = await _roleManager.RoleExistsAsync(userRole);
            if (!RoleExists)
            {
                var identityRole = new IdentityRole(userRole);
                await _roleManager.CreateAsync(identityRole);
            }
            await _userManager.AddToRoleAsync(user, userRole);

            var createdUser = await _db.ApplicationUsers.FirstOrDefaultAsync(u =>
                u.UserName == createUserDto.Username
            );

            return createdUser.Adapt<UserDataDto>();
        }

        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
        throw new ApplicationException(
            $"User creation failed! Please check user details and try again. Errors: {errors}"
        );
    }
}
// ...existing code...
