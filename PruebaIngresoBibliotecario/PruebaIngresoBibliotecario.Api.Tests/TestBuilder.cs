using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace Api.Test
{
    public abstract class IntegrationTestBuilder : IDisposable
    {
        protected HttpClient TestClient;
        public enum TipoUsuarioPrestamo { AFILIADO = 1, EMPLEADO, INVITADO }
        private bool Disposed;

        protected IntegrationTestBuilder(){
            BootstrapTestingSuite();
        }

        protected void BootstrapTestingSuite()
        {
            Disposed = false;
            var appFactory = new WebApplicationFactory<PruebaIngresoBibliotecario.Api.Startup>();                                   
            TestClient = appFactory.CreateClient();
        }
       

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                TestClient.Dispose();
            }

            Disposed = true;
        }

        public DateTime CalcularFechaEntrega(TipoUsuarioPrestamo tipoUsuario)
        {
            var weekend = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
            var fechaDevolucion = DateTime.Now;
            int diasPrestamo = tipoUsuario switch
            {
                TipoUsuarioPrestamo.AFILIADO => 10,
                TipoUsuarioPrestamo.EMPLEADO => 8,
                TipoUsuarioPrestamo.INVITADO => 7,
                _ => -1,
            };

            for (int i = 0; i < diasPrestamo;)
            {
                fechaDevolucion = fechaDevolucion.AddDays(1);
                i = (!weekend.Contains(fechaDevolucion.DayOfWeek)) ? ++i : i;                
            }

            return fechaDevolucion;
        }


    }

    public  class RespuestaConsultaDto
    {
        [JsonPropertyName("id")]
        public Guid IdPrestamoLibro { get; set; }
        [JsonPropertyName("identificacionUsuario")]
        public string IdUsuarioPrestamoLibro { get; set; }
        [JsonPropertyName("isbn")]
        public Guid IsbnLibroPrestamo { get; set; }
        [JsonPropertyName("tipoUsuario")]
        public int TipoUsuarioServicioBibliteca { get; set; }
        [JsonPropertyName("fechaMaximaDevolucion")]
        public DateTime FechaDevolucionPrestamoLibro { get; set; }       
    }
}
