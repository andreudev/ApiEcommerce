using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers();
            var usersDto = users.Adapt<List<UserDto>>();
            return Ok(usersDto);
        }

        [HttpGet("{userId}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUser(string userId)
        {
            var user = _userRepository.GetUser(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            var userDto = user.Adapt<UserDto>();
            return Ok(userDto);
        }

        [AllowAnonymous]
        [HttpPost(Name = "RegisterUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto userCreateDto)
        {
            if (userCreateDto == null)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(userCreateDto.Username))
            {
                ModelState.AddModelError("Username", "Username is required");
                return BadRequest(ModelState);
            }

            if (!_userRepository.IsUniqueUser(userCreateDto.Username))
            {
                ModelState.AddModelError("", "Username already exists");
                return StatusCode(400, ModelState);
            }
            var result = await _userRepository.Register(userCreateDto);
            if (result == null)
            {
                return StatusCode(500, "Something went wrong while registering the user");
            }

            return CreatedAtRoute("GetUser", new { userId = result.Id }, result);
        }

        [AllowAnonymous]
        [HttpPost("Login", Name = "LoginUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
        {
            if (userLoginDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userRepository.Login(userLoginDto);
            if (result == null)
            {
                return Unauthorized();
            }

            return Ok(result);
        }
    }
}
