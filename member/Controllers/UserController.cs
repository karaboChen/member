using member.Dtos;
using member.Extensions;
using member.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace member.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService= userService;
        }

        [ProducesResponseType(typeof(ServiceResult<UserDto.LoginResponse>), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDto.LoginRequest request)
        {
            var result = await _userService.LoginAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        [ProducesResponseType(typeof(ServiceResult<UserDto.CreateResponse>), StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserDto.CreateRequest request)
        {
            var result = await _userService.CreateUserAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return StatusCode(StatusCodes.Status201Created, result);
        }


        [ProducesResponseType(typeof(ServiceResult<bool>), StatusCodes.Status200OK)]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UserDto.UpdateRequest request)
        {
            var result = await _userService.UpdateUserAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
