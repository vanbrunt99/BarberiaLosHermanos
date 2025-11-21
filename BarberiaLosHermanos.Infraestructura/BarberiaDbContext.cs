using BarberiaLosHermanos.Dominio;
using Microsoft.EntityFrameworkCore;

namespace BarberiaLosHermanos.Infraestructura
{
    // DbContext de EF Core para la base de datos de la barbería.
    public class BarberiaDbContext : DbContext
    {
        public BarberiaDbContext(DbContextOptions<BarberiaDbContext> options)
            : base(options)
        {
        }

        // Entidades mapeadas a la base de datos (tablas)
        public DbSet<Servicio> Servicios => Set<Servicio>();
        public DbSet<Cliente> Clientes => Set<Cliente>(); // NUEVA TABLA
        public DbSet<Cita> Citas => Set<Cita>();         // NUEVA TABLA

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuración de Servicio (la clave primaria se define por convención, 
            // pero la mantenemos explícita por claridad).
            modelBuilder.Entity<Servicio>(entity =>
            {
                entity.HasKey(s => s.Id);
            });

            // 2. Configuración de Cliente.
            // EF Core usará la propiedad Id heredada de Persona como clave primaria.

            // 3. Configuración de Cita.
            modelBuilder.Entity<Cita>(entity =>
            {
                entity.HasKey(c => c.Id);

                // Configuración de las relaciones (Foreign Keys)
                // Cita tiene una relación con un Cliente (uno-a-muchos).
                entity.HasOne(c => c.Cliente)
                      .WithMany() // No navegamos desde Cliente a Citas por ahora, es una relación simple.
                      .HasForeignKey(c => c.ClienteId);

                // Cita tiene una relación con un Servicio.
                entity.HasOne(c => c.Servicio)
                      .WithMany()
                      .HasForeignKey(c => c.ServicioId);
            });

          
        }
    }
}



