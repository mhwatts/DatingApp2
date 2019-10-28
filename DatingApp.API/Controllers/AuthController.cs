using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController (IAuthRepository repo, IConfiguration config) {
            _config= config;
            _repo = repo;
        }

        [HttpPost ("register")]
        public async Task<IActionResult> Register (UserForRegisterDto userForRegisterDto) {
            // validate the request

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower ();

            if (await _repo.DoesUserExist (userForRegisterDto.Username))
                return BadRequest ("username already exists");

            var userToCreate = new User {
                Username = userForRegisterDto.Username
            };

            var createdUser = await _repo.Register (userToCreate, userForRegisterDto.Password);

            return Ok(StatusCode(201));
        }

        [HttpPost ("login")]
        public async Task<IActionResult> Login (UserForLoginDto userForLoginDto) {
            var userFromRepo = await _repo.Login (userForLoginDto.UserName.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized ();

            //Start building the JWT Web Token

            //Build the Claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name,userFromRepo.Username)
            };

            //Server needs to sign the token - So we create the key and use it to get the server signature
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);
            
            //Begin creating the Token - Start by creating the token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            //Create the handler which is used to create the token using the token descriptor
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            //Use the created token to write the token into the response
            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}