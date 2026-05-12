using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckinLog.Data;
using CheckinLog.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using CheckinLog.Extensions;

namespace CheckinLog.Controllers
{
    [Authorize]
    public class CheckinsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CheckinsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Checkins
        // AJUSTE: Adicionado parâmetros de filtro
        public async Task<IActionResult> Index(string searchString, DateTime? startDate, DateTime? endDate)
        {
            if (!User.HasPermission("menu_checkin"))
            {
                return Forbid();
            }

            // Preserva os valores nos inputs da View
            ViewData["CurrentFilter"] = searchString;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

            var query = _context.Checkins.AsQueryable();

            // Filtro de Texto (Nome, Matrícula ou Empresa)
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(s => s.Nome.Contains(searchString)
                                      || s.Matricula.Contains(searchString)
                                      || s.Empresa.Contains(searchString));
            }

            // Filtro de Data Início
            if (startDate.HasValue)
            {
                query = query.Where(s => s.DataCheckin >= startDate.Value);
            }

            // Filtro de Data Fim (ajustado para o final do dia)
            if (endDate.HasValue)
            {
                var finalDoDia = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(s => s.DataCheckin <= finalDoDia);
            }

            var lista = await query.OrderByDescending(c => c.DataCheckin).ToListAsync();
            return View(lista);
        }

        // GET: Checkins/Create
        public IActionResult Create()
        {
            if (!User.HasPermission("btn_incluir") && !User.HasPermission("menu_autocheckin"))
            {
                return Forbid();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Matricula,Nome,Email,Funcao,Empresa")] Checkin checkin, IFormFile? arquivoCNH, IFormFile? arquivoCurso)
        {
            if (ModelState.IsValid)
            {
                await ProcessarUploads(checkin, arquivoCNH, arquivoCurso);
                checkin.DataCheckin = DateTime.Now;
                _context.Add(checkin);
                await _context.SaveChangesAsync();

                // Redireciona para a página de sucesso em vez da listagem (Index)
                return RedirectToAction(nameof(Sucesso));
            }
            return View(checkin);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!User.HasPermission("btn_editar")) return Forbid();
            if (id == null) return NotFound();
            var checkin = await _context.Checkins.FindAsync(id);
            if (checkin == null) return NotFound();
            return View(checkin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Matricula,Nome,Email,Funcao,Empresa,CaminhoCNH,CaminhoCursoDefensiva,DataCheckin")] Checkin checkin, IFormFile? arquivoCNH, IFormFile? arquivoCurso)
        {
            if (id != checkin.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await ProcessarUploads(checkin, arquivoCNH, arquivoCurso);
                    _context.Update(checkin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CheckinExists(checkin.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(checkin);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!User.HasPermission("btn_excluir")) return Forbid();
            if (id == null) return NotFound();
            var checkin = await _context.Checkins.FirstOrDefaultAsync(m => m.Id == id);
            if (checkin == null) return NotFound();
            return View(checkin);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.HasPermission("btn_excluir")) return Forbid();
            var checkin = await _context.Checkins.FindAsync(id);
            if (checkin != null) _context.Checkins.Remove(checkin);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!User.HasPermission("btn_visualizar")) return Forbid();
            if (id == null) return NotFound();
            var checkin = await _context.Checkins.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (checkin == null) return NotFound();
            return View(checkin);
        }

        private async Task ProcessarUploads(Checkin checkin, IFormFile? arquivoCNH, IFormFile? arquivoCurso)
        {
            string pastaUploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(pastaUploads)) Directory.CreateDirectory(pastaUploads);

            if (arquivoCNH != null && arquivoCNH.Length > 0)
            {
                string nomeArquivoUnico = Guid.NewGuid().ToString() + "_" + arquivoCNH.FileName;
                string caminhoFisico = Path.Combine(pastaUploads, nomeArquivoUnico);
                using (var fileStream = new FileStream(caminhoFisico, FileMode.Create))
                {
                    await arquivoCNH.CopyToAsync(fileStream);
                }
                checkin.CaminhoCNH = "/uploads/" + nomeArquivoUnico;
            }

            if (arquivoCurso != null && arquivoCurso.Length > 0)
            {
                string nomeArquivoUnico = Guid.NewGuid().ToString() + "_" + arquivoCurso.FileName;
                string caminhoFisico = Path.Combine(pastaUploads, nomeArquivoUnico);
                using (var fileStream = new FileStream(caminhoFisico, FileMode.Create))
                {
                    await arquivoCurso.CopyToAsync(fileStream);
                }
                checkin.CaminhoCursoDefensiva = "/uploads/" + nomeArquivoUnico;
            }
        }

        // AJUSTE: Exportação agora aceita filtros para exportar apenas o que está na tela
        public async Task<IActionResult> ExportarExcel(string searchString, DateTime? startDate, DateTime? endDate)
        {
            if (!User.HasPermission("btn_exportar")) return Forbid();

            var query = _context.Checkins.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(s => s.Nome.Contains(searchString) || s.Matricula.Contains(searchString) || s.Empresa.Contains(searchString));

            if (startDate.HasValue) query = query.Where(s => s.DataCheckin >= startDate.Value);
            if (endDate.HasValue) query = query.Where(s => s.DataCheckin <= endDate.Value.Date.AddDays(1).AddTicks(-1));

            var dados = await query.OrderByDescending(x => x.DataCheckin).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Checkins");
                worksheet.Cell(1, 1).Value = "Data/Hora";
                worksheet.Cell(1, 2).Value = "Matrícula";
                worksheet.Cell(1, 3).Value = "Colaborador";
                worksheet.Cell(1, 4).Value = "Empresa";

                for (int i = 0; i < dados.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = dados[i].DataCheckin.ToString();
                    worksheet.Cell(i + 2, 2).Value = dados[i].Matricula;
                    worksheet.Cell(i + 2, 3).Value = dados[i].Nome;
                    worksheet.Cell(i + 2, 4).Value = dados[i].Empresa;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Relatorio_{DateTime.Now:ddMMyyyy}.xlsx");
                }
            }
        }

        private bool CheckinExists(int id) => _context.Checkins.Any(e => e.Id == id);


        // GET: Checkins/Sucesso
        [AllowAnonymous] // Permite que o motorista veja a confirmação mesmo sem claims extras
        public IActionResult Sucesso()
        {
            return View();
        }
    }

}