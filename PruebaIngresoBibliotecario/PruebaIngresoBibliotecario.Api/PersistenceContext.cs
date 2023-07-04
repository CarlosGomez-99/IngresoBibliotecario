using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PruebaIngresoBibliotecario.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PruebaIngresoBibliotecario.Infrastructure
{
    public class PersistenceContext : DbContext
    {
        public DbSet<PrestamoLibro> PrestamoLibroDS { get; set; }

        private readonly IConfiguration Config;

        public PersistenceContext(DbContextOptions<PersistenceContext> options, IConfiguration config) : base(options)
        {
            Config = config;
        }

        public async Task CommitAsync()
        {
            await SaveChangesAsync().ConfigureAwait(false);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Config.GetValue<string>("SchemaName"));

            //Se crea modelo para contruir la tabla en la base de datos en memoria
            modelBuilder.Entity<PrestamoLibro>(ptLibro =>
            {
                ptLibro.HasKey(key => key.IdPrestamoLibro);
                ptLibro.Property(prop => prop.IdUsuarioPrestamoLibro);
                ptLibro.Property(prop => prop.IdLibroPrestado);
                ptLibro.Property(prop => prop.TipoUsuarioPrestamo);
                ptLibro.Property(prop => prop.FechaDevolucionPrestamo);
            });
        }
    }
}
