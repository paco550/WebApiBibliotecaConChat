using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class UsuariosController : ControllerBase
    {

        private readonly MiBibliotecaContext _context;
        private readonly IConfiguration _configuration;
        private readonly IDataProtector _dataProtector;
        private readonly HashService _hashService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuariosController(MiBibliotecaContext context, IConfiguration configuration, IDataProtectionProvider dataProtectionProvider,HashService hashService,IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _dataProtector = dataProtectionProvider.CreateProtector(configuration["ClaveEncriptacion"]);
            _hashService = hashService;
            _httpContextAccessor = httpContextAccessor;
        }

        [AllowAnonymous]
        [HttpPost("encriptar/nuevousuario")]
        public async Task<ActionResult> RegisterUsuarioEncript([FromBody] DTOUsuario usuario)
        {
            var passEncriptado = _dataProtector.Protect(usuario.Password);
            var newUsuario = new Usuario
            {
                Email = usuario.Email,
                Password = passEncriptado
            };

            await _context.Usuarios.AddAsync(newUsuario);
            await _context.SaveChangesAsync();

            return Ok(newUsuario);
        }

        [HttpPost("encriptar/checkusuario")]
        public async Task<ActionResult> CheckUsuarioEncript([FromBody] DTOUsuario usuario)
        {

            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);

            if (usuarioDB != null)
            {
                var passDesencriptado = _dataProtector.Unprotect(usuarioDB.Password);

                if (usuario.Password == passDesencriptado)
                {
                    return Ok("Logado con éxito");
                }
                else
                {
                    return Unauthorized("Contraseña incorrecta");
                }

            }

            return Unauthorized("Usuario incorrecto y/o contraseña incorrecto/s");
        }

        [AllowAnonymous]
        [HttpPost("hash/nuevousuarioencriptar")]
        public async Task<ActionResult> RegisterUsuarioHash([FromBody] DTOUsuario usuario)
        {
            var resultadoHash = _hashService.Hash(usuario.Password);
            var newUsuario = new Usuario
            {
                Email = usuario.Email,
                Password = resultadoHash.Hash,
                Salt = resultadoHash.Salt
            };

            await _context.Usuarios.AddAsync(newUsuario);
            await _context.SaveChangesAsync();

            return Ok(newUsuario);
        }

        [HttpPost("hash/checkusuarioencriptado")]
        public async Task<ActionResult> CheckUsuarioHash([FromBody] DTOUsuario usuario)
        {
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized();
            }

            var resultadoHash = _hashService.Hash(usuario.Password, usuarioDB.Salt);
            if (usuarioDB.Password == resultadoHash.Hash)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpPut("hash/cambiapass")] //endpoint

        //Asignamos por parámetro una DTOS especial para este método:

        public async Task<ActionResult> ChangePassword([FromBody] DTOUsuariosPut usuario)
        {
            var usuarioDB = await _context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Email == usuario.Email);

            if (usuarioDB == null)
            {
                return Unauthorized("El usuario no existe");
            }

            var hashPassActual = _hashService.Hash(usuario.PasswordActual, usuarioDB.Salt);

            if (!hashPassActual.Hash.Equals(usuarioDB.Password))
            {
                return Unauthorized("La contraseña actual no es correcta");
            }

            var nuevoHashConSalt = _hashService.Hash(usuario.PasswordNuevo);

            usuarioDB.Password = nuevoHashConSalt.Hash;
            usuarioDB.Salt = nuevoHashConSalt.Salt;

            _context.Update(usuarioDB);

            await _context.SaveChangesAsync();

            return Ok(usuarioDB);

        }

        [AllowAnonymous]
        [HttpGet("/changepassword/{textoEnlace}")]
        public async Task<ActionResult> LinkChangePasswordHash(string textoEnlace)
        {
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.EnlaceCambioPass == textoEnlace);
            if (usuarioDB == null)
            {
                return Unauthorized("Operación no autorizada");
            }

            var fechaCaducidad = usuarioDB.FechaEnvioEnlace.Value.AddMinutes(3);

            if (fechaCaducidad < DateTime.Now)
            {
                return Unauthorized("Operación no autorizada");
            }

            return Ok("Enlace correcto");
        }

        [AllowAnonymous]
        [HttpPost("hash/linkchangepassword")]
        public async Task<ActionResult> LinkChangePasswordHash([FromBody] DTOUsuarioLinkChangePassword usuario)
        {
            var usuarioDB = await _context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized("Usuario no registrado");
            }

            // Creamos un string aleatorio 
            Guid miGuid = Guid.NewGuid();
            string textoEnlace = Convert.ToBase64String(miGuid.ToByteArray());
            // Eliminar caracteres que pueden causar problemas
            textoEnlace = textoEnlace.Replace("=", "").Replace("+", "").Replace("/", "").Replace("?", "").Replace("&", "").Replace("!", "").Replace("¡", "");
            usuarioDB.EnlaceCambioPass = textoEnlace;
            usuarioDB.FechaEnvioEnlace = DateTime.Now;
            await _context.SaveChangesAsync();
            var ruta = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/changepassword/{textoEnlace}";
            return Ok(ruta);
        }

        [AllowAnonymous]
        [HttpPost("usuarios/changepassword")]
        public async Task<ActionResult> LinkChangePasswordHash([FromBody] DTOUsuarioChangePassword infoUsuario)
        {
            var usuarioDB = await _context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Email == infoUsuario.Email && x.EnlaceCambioPass == infoUsuario.Enlace);
            if (usuarioDB == null)
            {
                return Unauthorized("Operación no autorizada");
            }

            if (usuarioDB.FechaEnvioEnlace.Value.AddMinutes(3) < DateTime.Now)
            {
                return Unauthorized("Operación no autorizada");
            }

            var resultadoHash = _hashService.Hash(infoUsuario.Password);
            usuarioDB.Password = resultadoHash.Hash;
            usuarioDB.Salt = resultadoHash.Salt;
            usuarioDB.EnlaceCambioPass = null;
            usuarioDB.FechaEnvioEnlace = null;

            await _context.SaveChangesAsync();

            return Ok("Password cambiado con exito");
        }
    }
}
