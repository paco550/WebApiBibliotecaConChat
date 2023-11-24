using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiBiblioteca.DTOs;
using WebApiBiblioteca.Models;
using WebApiBiblioteca.Services;

namespace WebApiBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MiBibliotecaContext _context;
        private readonly HashService _hashService;
        private readonly TokenService _tokenService;
        public AuthController(MiBibliotecaContext context, HashService hashService, TokenService tokenService)
        {
            _context = context;
            _hashService = hashService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] DTOUsuario usuario)
        {
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(login => login.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized("Usuario no autorizado");
            }
            var resultadoHash = _hashService.Hash(usuario.Password, usuarioDB.Salt);
            if (usuarioDB.Password == resultadoHash.Hash)
            {
                var response = _tokenService.GenerarToken(usuario);
                return Ok(response);
            }
            else
            {
                return Unauthorized("Usuario no autorizado");
            }
        }
    }
}
