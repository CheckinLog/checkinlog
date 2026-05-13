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

            // 1. Configuração do Banco de Dados (AJUSTADO PARA POSTGRESQL)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // --- CONFIGURAÇÃO DE SESSÃO (PARTE 1) ---
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
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

            // --- NOVO: APLICAR MIGRAÇÕES AUTOMATICAMENTE ---
            // Isso cria as tabelas no banco do Render assim que o site sobe
            // --- NOVO: APLICAR MIGRAÇÕES COM TRATAMENTO DE ERRO E SSL ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var db = services.GetRequiredService<ApplicationDbContext>();

                    // Tenta forçar a validação do certificado via código caso a string falhe
                    var conn = db.Database.GetDbConnection() as Npgsql.NpgsqlConnection;
                    if (conn != null)
                    {
                        conn.UserCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    }

                    Console.WriteLine("==> Tentando aplicar migrações no banco de dados...");
                    db.Database.Migrate();
                    Console.WriteLine("==> Migrações aplicadas com sucesso!");
                }
                catch (Exception ex)
                {
                    // Isso impede o erro 139 (crash) e mostra o motivo real no log
                    Console.WriteLine("###################################################");
                    Console.WriteLine("ERRO AO MIGRAR BANCO:");
                    Console.WriteLine(ex.Message);
                    if (ex.InnerException != null) Console.WriteLine($"DETALHE: {ex.InnerException.Message}");
                    Console.WriteLine("###################################################");

                    // O app continuará tentando subir mesmo se a migração falhar aqui
                }
            }
            // ---------------------------------------------------------------
            // -----------------------------------------------

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}