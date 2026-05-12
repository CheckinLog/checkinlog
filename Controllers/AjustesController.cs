using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckinLog.Data;
using CheckinLog.Models;
using Microsoft.AspNetCore.Authorization;
using CheckinLog.Extensions;

namespace CheckinLog.Controllers
{
    [Authorize]
    public class AjustesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AjustesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [ActionName("Index")]
        public async Task<IActionResult> Acessos()
        {
            // Valida se o usuário logado tem a chave 'menu_ajustes' no banco
            if (!User.HasPermission("menu_ajustes_perfis"))
            {
                return Forbid();
            }

            var permissoes = await _context.Permissoes
                .OrderBy(p => p.Id)
                .ToListAsync();

            return View("Acessos", permissoes);
        }

        [HttpGet]
        public async Task<JsonResult> GetAcessosPorPerfil(int perfilId)
        {
            // Busca apenas permissões que estão marcadas como Ativo = True
            var acessos = await _context.PerfilPermissao
                .Where(p => p.PerfilId == perfilId && p.Ativo == true)
                .Select(p => p.PermissaoId)
                .ToListAsync();

            return Json(acessos);
        }

        [HttpPost]
        public async Task<IActionResult> SalvarAcessos([FromBody] AcessosRequest data)
        {
            if (data == null || data.PerfilId <= 0) return BadRequest(new { mensagem = "Dados inválidos." });

            if (!User.HasPermission("btn_salvar_ajustes"))
            {
                return Json(new { erro = "Sem permissão para salvar." });
            }

            try
            {
                // 1. Busca todos os registros atuais (ativos ou não) para este perfil
                var acessosExistentes = await _context.PerfilPermissao
                    .Where(p => p.PerfilId == data.PerfilId)
                    .ToListAsync();

                // Garantimos que PermissoesIds não seja nulo para evitar erros de lógica
                var idsSelecionados = data.PermissoesIds ?? new List<int>();

                // 2. DESATIVAR: O que está no banco mas NÃO veio na lista da tela
                foreach (var acesso in acessosExistentes.Where(a => !idsSelecionados.Contains(a.PermissaoId)))
                {
                    acesso.Ativo = false;
                }

                // 3. ATIVAR OU CRIAR: Para cada ID que veio da tela
                foreach (var id in idsSelecionados)
                {
                    var existente = acessosExistentes.FirstOrDefault(a => a.PermissaoId == id);

                    if (existente != null)
                    {
                        // Se já existe, apenas garante que está True (resolve o "indefinido" que voltou a ser marcado)
                        existente.Ativo = true;
                    }
                    else
                    {
                        // Se é uma permissão totalmente nova para este perfil, aí sim criamos a linha
                        _context.PerfilPermissao.Add(new PerfilPermissao
                        {
                            PerfilId = data.PerfilId,
                            PermissaoId = id,
                            Ativo = true
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { mensagem = "Acessos sincronizados com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { erro = "Erro ao sincronizar: " + ex.Message });
            }
        }
    }

    public class AcessosRequest
    {
        public int PerfilId { get; set; }
        public List<int>? PermissoesIds { get; set; }
    }
}