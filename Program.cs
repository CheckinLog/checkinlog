using Microsoft.EntityFrameworkCore;
using CheckinLog.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net; // Necessário para SecurityProtocol

namespace CheckinLog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // AJUSTE 1: Força o uso de protocolos de segurança modernos antes de iniciar o builder
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            var builder = WebApplication.CreateBuilder(args);

            // 1. Configuração do Banco de Dados (COM RESILIÊNCIA)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
                {
                    // AJUSTE 2: Tenta reconectar automaticamente se a conexão "piscar" (muito comum em nuvem)
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

            // --- CONFIGURAÇÃO DE SESSÃO ---
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 2. Configuração de Autenticação
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                });

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // --- BLOCO DE MIGRAÇÃO COM TRATAMENTO DE ERRO E SSL ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var db = services.GetRequiredService<ApplicationDbContext>();

                    // AJUSTE 3: Garante que o driver Npgsql aceite o certificado do Render
                    var conn = db.Database.GetDbConnection() as Npgsql.NpgsqlConnection;
                    if (conn != null)
                    {
                        // Se a conexão estiver fechada, abrimos manualmente para injetar o callback de SSL
                        conn.UserCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    }

                    Console.WriteLine("==> Tentando aplicar migrações no banco de dados...");
                    db.Database.Migrate();
                    Console.WriteLine("==> Migrações aplicadas com sucesso!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("###################################################");
                    Console.WriteLine("ERRO AO MIGRAR BANCO:");
                    Console.WriteLine(ex.Message);
                    if (ex.InnerException != null) Console.WriteLine($"DETALHE: {ex.InnerException.Message}");
                    Console.WriteLine("###################################################");
                }
            }

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