using Microsoft.AspNetCore.Mvc;
using API.Interfaces;
using API.Data;
using API.DTOs;
using System.Security.Cryptography;
using API.Entities;

using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {

        // private readonly ITokenService _tokenService;
        //     private readonly DataContext _context;

        // public AccountController(ITokenService _tokenService,DataContext _context)
        // {
        //     this._tokenService=_tokenService;
        //     this._context=_context;
        // }


        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");


            var user = _mapper.Map<AppUser>(registerDto);


            user.UserName = registerDto.Username.ToLower();


            var result = await _userManager.CreateAsync(user, registerDto.Password);


            if (!result.Succeeded) return BadRequest(result.Errors);


            var roleResult = await _userManager.AddToRoleAsync(user, "Member");


            if (!roleResult.Succeeded) return BadRequest(result.Errors);


            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());


            if (user == null) return Unauthorized("Invalid username");


            var result = await _signInManager
                .CheckPasswordSignInAsync(user, loginDto.Password, false);


            if (!result.Succeeded) return Unauthorized();


            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }



        #region older code before implementing identity
        // [HttpPost("register")]
        // public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        // {

        //     using var hmac = new HMACSHA512();
        //     byte[] bytes = Encoding.ASCII.GetBytes(registerDto.Password);
        //     var user = new AppUser
        //     {
        //         UserName = registerDto.Username.ToLower(),
        //         Gender = registerDto.Gender,
        //         PasswordHash = hmac.ComputeHash(bytes).ToString(),
        //         PasswordSalt = hmac.Key
        //     };
        //     var isuserexist = await _context.Users
        //        .SingleOrDefaultAsync(x => x.UserName == registerDto.Username.ToLower());
        //     if (isuserexist != null) return Unauthorized("Username is taken");
        //     _context.Users.Add(user);
        //     await _context.SaveChangesAsync();

        //     return new UserDto
        //     {
        //         Username = user.UserName,
        //         Token = _tokenService.CreateToken(user),
        //         KnownAs = user.KnownAs,
        //         Gender = user.Gender
        //     };
        // }

        // [HttpPost("login")]
        // public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        // {
        //     var user = await _context.Users.Include(p => p.Photos)
        //         .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

        //     if (user == null) return Unauthorized();

        //     using var hmac = new HMACSHA512(user.PasswordSalt);

        //     var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        //     for (int i = 0; i < computedHash.Length; i++)
        //     {
        //         if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
        //     }

        //     return new UserDto
        //     {
        //         Username = user.UserName,
        //         Token = _tokenService.CreateToken(user),
        //         PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
        //         KnownAs = user.KnownAs,
        //         Gender = user.Gender


        //     };
        // }
        #endregion

    }
}