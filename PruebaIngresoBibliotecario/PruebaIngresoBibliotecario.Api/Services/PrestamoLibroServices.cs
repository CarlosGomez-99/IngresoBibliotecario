using System;
using System.Linq;
using System.Threading.Tasks;
using PruebaIngresoBibliotecario.Api.Models;
using PruebaIngresoBibliotecario.Infrastructure;

namespace PruebaIngresoBibliotecario.Api.Services
{
    public class PrestamoLibroServices : IPrestamoLibroService
    {
        public enum TipoUsuario
        {
            AFILIADO = 1,
            EMPLEADO,
            INVITADO
        }
        readonly PersistenceContext context;

        public PrestamoLibroServices(PersistenceContext dbContext)
        {
            context = dbContext;
        }

        /// <summary>
        /// Obtener prestamo por un id.
        /// </summary>
        /// <param name="idPrestamo">Identificación de un prestamo(id).</param>
        /// <returns>Prestamo Libro y su información.</returns>
        public PrestamoLibro ObtenerPrestamoById(Guid idPrestamo)
        {
            return context.PrestamoLibroDS.Find(idPrestamo);
        }
        /// <summary>
        /// Agregar prestamos.
        /// </summary>
        /// <param name="prestamoLibro">Objeto Prestamo Libro.</param>
        /// <returns>Objeto prestamo libro con id de creación y fecha en que se debe entregar el libro.</returns>
        public async Task<PrestamoLibro> AgregarPrestamos(PrestamoLibro prestamoLibro)
        {
            PrestamoLibro pretamoLibroPersona = null;
            var prestamoUsuarioInvitado = context.PrestamoLibroDS.Where(p => p.TipoUsuarioPrestamo == (int)TipoUsuario.INVITADO).Count();
            if (prestamoUsuarioInvitado == 0)
            {
                pretamoLibroPersona = new PrestamoLibro()
                {
                    IdPrestamoLibro = Guid.NewGuid(),
                    IdUsuarioPrestamoLibro = prestamoLibro.IdUsuarioPrestamoLibro,
                    IdLibroPrestado = prestamoLibro.IdLibroPrestado,
                    TipoUsuarioPrestamo = prestamoLibro.TipoUsuarioPrestamo,
                    FechaDevolucionPrestamo = CalcularFechaDevolucion(prestamoLibro.TipoUsuarioPrestamo)
                };
                context.Add(pretamoLibroPersona);
                await context.SaveChangesAsync();
            }
            return pretamoLibroPersona;
        }
        /// <summary>
        /// Calcular fecha según tipo cliente.
        /// </summary>
        /// <param name="idUsuario">Id TipoUsuario.</param>
        /// <returns>DateTime con la fecha en que debe devolverse el libro.</returns>
        public DateTime CalcularFechaDevolucion(int idUsuario)
        {
            var finSemana = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
            var fechaDevolucion = DateTime.Now;
            int diasPrestamoUsuario = -1;
            switch (idUsuario)
            {
                case (int)TipoUsuario.AFILIADO:
                    diasPrestamoUsuario = 10;
                    break;
                case (int)TipoUsuario.EMPLEADO:
                    diasPrestamoUsuario = 8;
                    break;
                case (int)TipoUsuario.INVITADO:
                    diasPrestamoUsuario = 7;
                    break;
            }
            for (int i = 0; i < diasPrestamoUsuario;)
            {
                fechaDevolucion = fechaDevolucion.AddDays(1);
                i = (!finSemana.Contains(fechaDevolucion.DayOfWeek)) ? ++i : i;
            }
            return fechaDevolucion;
        }
    }

}
public interface IPrestamoLibroService
{
    public PrestamoLibro ObtenerPrestamoById(Guid idPrestamo);
    public Task<PrestamoLibro> AgregarPrestamos(PrestamoLibro prestamoLibro);
    public DateTime CalcularFechaDevolucion(int idUsuario);
}