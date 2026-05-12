using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace CheckinLog.Controllers
{
    public class ConfiguracoesController : Controller
    {
        // GET: Configuracoes/Index
        public IActionResult Index()
        {
            return View();
        }

        // Action para salvar a preferência via Cookie ou LocalStorage
        [HttpPost]
        public IActionResult SalvarTema(string tema)
        {
            CookieOptions option = new CookieOptions { Expires = DateTime.Now.AddYears(1) };
            Response.Cookies.Append("TemaSistema", tema, option);
            return Ok();
        }
    }
}