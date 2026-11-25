using LocadoraVeiculos.Models;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Data
{
    /// <summary>
    /// Contexto do Entity Framework Core responsável por mapear as classes
    /// da camada Model para as tabelas do SQL Server Express.
    /// </summary>
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        // DbSets -> tabelas do banco
        public DbSet<Fabricante> Fabricantes { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Aluguel> Alugueis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------------- FABRICANTE ----------------
            modelBuilder.Entity<Fabricante>(entity =>
            {
                entity.ToTable("Fabricante");
                entity.HasKey(f => f.FabricanteId);                       // PK

                entity.Property(f => f.Nome)
                      .IsRequired()
                      .HasMaxLength(80);

                entity.Property(f => f.PaisOrigem).HasMaxLength(60);

                entity.HasIndex(f => f.Nome).IsUnique();                  // restrição UNIQUE
            });

            // ---------------- VEÍCULO ----------------
            modelBuilder.Entity<Veiculo>(entity =>
            {
                entity.ToTable("Veiculo");
                entity.HasKey(v => v.VeiculoId);                          // PK

                entity.Property(v => v.Placa)
                      .IsRequired()
                      .HasMaxLength(8);

                entity.Property(v => v.Modelo)
                      .IsRequired()
                      .HasMaxLength(80);

                entity.Property(v => v.AnoFabricacao).IsRequired();
                entity.Property(v => v.Quilometragem).IsRequired();
                entity.Property(v => v.Cor).HasMaxLength(30);
                entity.Property(v => v.Chassi).HasMaxLength(17);

                entity.Property(v => v.Status)
                      .HasConversion<int>();

                entity.HasIndex(v => v.Placa).IsUnique();                 // restrição UNIQUE

                // FK: Veiculo(FabricanteId) -> Fabricante(FabricanteId)
                entity.HasOne(v => v.Fabricante)
                      .WithMany(f => f.Veiculos)
                      .HasForeignKey(v => v.FabricanteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- CLIENTE ----------------
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Cliente");
                entity.HasKey(c => c.ClienteId);                          // PK

                entity.Property(c => c.Nome)
                      .IsRequired()
                      .HasMaxLength(120);

                entity.Property(c => c.Cpf)
                      .IsRequired()
                      .HasMaxLength(11)
                      .IsFixedLength();

                entity.Property(c => c.Email)
                      .IsRequired()
                      .HasMaxLength(120);

                entity.Property(c => c.Telefone).HasMaxLength(20);
                entity.Property(c => c.NumeroCnh).HasMaxLength(11);

                entity.Property(c => c.DataCadastro)
                      .HasColumnType("datetime2")
                      .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(c => c.Cpf).IsUnique();                   // restrição UNIQUE
                entity.HasIndex(c => c.Email).IsUnique();                 // restrição UNIQUE
            });

            // ---------------- ALUGUEL ----------------
            modelBuilder.Entity<Aluguel>(entity =>
            {
                entity.ToTable("Aluguel");
                entity.HasKey(a => a.AluguelId);                          // PK

                entity.Property(a => a.DataRetirada)
                      .HasColumnType("datetime2")
                      .IsRequired();

                entity.Property(a => a.DataPrevistaDevolucao)
                      .HasColumnType("datetime2")
                      .IsRequired();

                entity.Property(a => a.DataDevolucao)
                      .HasColumnType("datetime2");

                entity.Property(a => a.QuilometragemInicial).IsRequired();

                entity.Property(a => a.ValorDiaria)
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                entity.Property(a => a.ValorTotal)
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                entity.Property(a => a.Status)
                      .HasConversion<int>();

                entity.Property(a => a.Observacoes).HasMaxLength(300);

                // FK: Aluguel(ClienteId) -> Cliente(ClienteId)
                entity.HasOne(a => a.Cliente)
                      .WithMany(c => c.Alugueis)
                      .HasForeignKey(a => a.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK: Aluguel(VeiculoId) -> Veiculo(VeiculoId)
                entity.HasOne(a => a.Veiculo)
                      .WithMany(v => v.Alugueis)
                      .HasForeignKey(a => a.VeiculoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => new { a.VeiculoId, a.DataRetirada });
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
