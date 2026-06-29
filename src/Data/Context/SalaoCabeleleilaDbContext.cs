using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public class SalaoCabeleleilaDbContext : DbContext
    {
        public SalaoCabeleleilaDbContext(DbContextOptions<SalaoCabeleleilaDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<PerfilUsuario> PerfisUsuario => Set<PerfilUsuario>();
        public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
        public DbSet<AgendamentoServico> AgendamentosServico => Set<AgendamentoServico>();
        public DbSet<Servico> Servicos => Set<Servico>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalaoCabeleleilaDbContext).Assembly);

            modelBuilder.Entity<PerfilUsuario>().HasData(
                new PerfilUsuario
                {
                    Id = 1,
                    Descricao = "Admin",
                    Ativo = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new PerfilUsuario
                {
                    Id = 2,
                    Descricao = "Cliente",
                    Ativo = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                });

            modelBuilder.Entity<Servico>()
                .Property(s => s.Valor)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Agendamento>()
                .HasMany(a => a.Servicos)
                .WithOne(s => s.Agendamento)
                .HasForeignKey(s => s.AgendamentoId)
                .OnDelete(DeleteBehavior.Cascade);

            var seedDate = new DateTime(2026, 1, 1);

            modelBuilder.Entity<Servico>().HasData(
                new Servico { Id = 1, Nome = "Corte de Cabelo", Valor = 50m, DuracaoMinutos = 30, Ativo = true, CreatedAt = seedDate },
                new Servico { Id = 2, Nome = "Escova", Valor = 40m, DuracaoMinutos = 45, Ativo = true, CreatedAt = seedDate },
                new Servico { Id = 3, Nome = "Coloração", Valor = 120m, DuracaoMinutos = 120, Ativo = true, CreatedAt = seedDate },
                new Servico { Id = 4, Nome = "Manicure", Valor = 35m, DuracaoMinutos = 40, Ativo = true, CreatedAt = seedDate });

            base.OnModelCreating(modelBuilder);
        }
    }
}
