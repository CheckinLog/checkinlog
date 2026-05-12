using Microsoft.EntityFrameworkCore;
using CheckinLog.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CheckinLog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configuração do Banco de Dados
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // --- NOVO: CONFIGURAÇÃO DE SESSÃO (PARTE 1) ---
            // A sessão precisa de um cache para armazenar os dados
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo que a sessão fica ativa
                options.Cookie.HttpOnly = true; // Segurança: impede acesso via JS
                options.Cookie.IsEssential = true; // Essencial para o app funcionar
            });
            // ----------------------------------------------

            // 2. Configuração de Autenticação
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                });

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // --- NOVO: ATIVAR SESSÃO (PARTE 2) ---
            // IMPORTANTE: Deve vir depois de UseRouting e antes de UseAuthorization
            app.UseSession();
            // ------------------------------------

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}