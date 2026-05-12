using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using CheckinLog.Data;
using CheckinLog.Models;
using System.Security.Claims;

namespace CheckinLog.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // 1. Busca o usuário ignorando espaços em branco (Resolve o erro da image_ca7a56.png)
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NomeUsuario.Trim() == model.Usuario.Trim()
                                     && u.Senha.Trim() == model.Senha.Trim()
                                     && u.Ativo);

            if (usuario != null)
            {
                // 2. Cria a lista de identidades (Claims)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NomeUsuario),
                    new Claim("UsuarioId", usuario.Id.ToString()),
                    new Claim("NivelAcesso", usuario.NivelAcesso.ToString())
                };

                // 3. Busca as permissões
                // AJUSTE CS0019: Convertemos o PerfilId para string para comparar com NivelAcesso (string)
                var permissoes = await _context.PerfilPermissao
                    .Include(p => p.Permissao)
                    .Where(p => p.PerfilId.ToString() == usuario.NivelAcesso)
                  .Select(p => p.Permissao != null ? p.Permissao.ChaveSistema : string.Empty)
                    .ToListAsync();

                // 4. Adiciona as chaves de permissão à identidade do usuário
                foreach (var chave in permissoes)
                {
                    if (!string.IsNullOrEmpty(chave))
                    {
                        claims.Add(new Claim("Permission", chave));
                    }
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 5. Efetua o login e cria o Cookie (AJUSTE CS1503)
                // Usamos a chamada explícita para evitar que o compilador confunda com BinaryReader
                await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync(
                    HttpContext,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            // Caso o login falhe (image_ca7a56.png)
            ViewBag.Erro = "Usuário ou senha inválidos";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}