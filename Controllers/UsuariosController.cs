using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckinLog.Data;
using CheckinLog.Models;

namespace CheckinLog.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ListaUsuarios = await _context.Usuarios.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Usuario usuario)
        {
            // 1. CAPTURAR ERROS DE VALIDAÇÃO (Veja o resultado na aba 'Saída' do Visual Studio)
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG CHECKINLOG - ERRO: " + error.ErrorMessage);
                }
            }

            // 2. GARANTIR NÍVEL DE ACESSO
            if (string.IsNullOrEmpty(usuario.NivelAcesso) || usuario.NivelAcesso == "0")
            {
                usuario.NivelAcesso = "1"; // Valor padrão caso venha vazio
            }

            try
            {
                if (usuario.Id == 0)
                {
                    // TENTATIVA DE NOVO CADASTRO
                    _context.Add(usuario);
                }
                else
                {
                    // EDIÇÃO DE USUÁRIO EXISTENTE
                    var userOriginal = await _context.Usuarios.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == usuario.Id);

                    if (userOriginal != null)
                    {
                        // Mantém o nome original na edição
                        usuario.NomeUsuario = userOriginal.NomeUsuario;

                        // Se o nível veio nulo na edição, recupera o original
                        if (string.IsNullOrEmpty(usuario.NivelAcesso))
                        {
                            usuario.NivelAcesso = userOriginal.NivelAcesso;
                        }

                        _context.Update(usuario);
                    }
                }

                // 3. TENTAR SALVAR NO BANCO
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Se der erro no banco (ex: nome duplicado), ele cairá aqui
                System.Diagnostics.Debug.WriteLine("DEBUG CHECKINLOG - ERRO DE BANCO: " + ex.Message);

                // Recarrega a lista para não dar erro de tela vazia
                ViewBag.ListaUsuarios = await _context.Usuarios.ToListAsync();
                return View("Index", usuario);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Action para a tela de detalhes separada
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }
    }
}