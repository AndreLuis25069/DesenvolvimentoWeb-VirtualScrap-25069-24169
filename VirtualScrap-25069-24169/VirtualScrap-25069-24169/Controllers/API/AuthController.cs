using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model.ViewModels;

namespace VirtualScrap_25069_24169.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context,
           UserManager<IdentityUser> userManager,
           SignInManager<IdentityUser> signInManager,
           IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        // POST: api/Auth/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            // Procura o utilizador pelo Email enviado no campo Username
            var user = await _userManager.FindByEmailAsync(login.Username);
            if (user == null) return Unauthorized();

            // Valida se a password coincide com o Hash da Base de Dados
            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);
            if (!result.Succeeded) return Unauthorized();

            // Ir buscar as Roles reais deste user à DB antes de gerar o token
            var roles = await _userManager.GetRolesAsync(user);

            // Se as credenciais estiverem certas, gera o Token de Acesso passando o username E as roles
            var token = GenerateJwtToken(login.Username,user.Id, roles);

            // Devolve o token num objeto JSON
            return Ok(new { token });
        }

        //O método agora aceita a lista de roles que veio do Login
        private string GenerateJwtToken(string username,string userId, IList<string> roles)
        {
            // Mudou de Array Fixo para List<Claim> para o .Add() funcionar!
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username)
            };

            // Agora sim, a variável 'roles' existe e as claims vão ser injetadas
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
               issuer: _config["Jwt:Issuer"],
               audience: _config["Jwt:Audience"],
               claims: claims,
               expires: DateTime.Now.AddHours(2),
               signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}