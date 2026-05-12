using Microsoft.EntityFrameworkCore;
using CheckinLog.Models;

namespace CheckinLog.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Suas tabelas existentes
        public DbSet<Checkin> Checkins { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        // ADICIONE ESTAS LINHAS PARA CORRIGIR O ERRO:
        public DbSet<Permissao> Permissoes { get; set; }
        public DbSet<PerfilPermissao> PerfilPermissao { get; set; }
    }
}