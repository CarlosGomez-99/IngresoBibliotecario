using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PruebaIngresoBibliotecario.Api.Models
{
    /// <summary>
    /// Modelo PrestamoLibro para guardar los registros de los prestamos que hacen los diferentes usuarios de la biblioteca.
    /// </summary>
    public class PrestamoLibro
    {
        [JsonPropertyName("id")]
        public Guid IdPrestamoLibro { get; set; }
        [JsonPropertyName("identificacionUsuario")]
        public string IdUsuarioPrestamoLibro { get; set; }
        [JsonPropertyName("isbn")]
        public Guid IdLibroPrestado { get; set; }
        [JsonPropertyName("tipoUsuario")]
        public int TipoUsuarioPrestamo { get; set; }
        [JsonPropertyName("fechaMaximaDevolucion")]
        public DateTime FechaDevolucionPrestamo { get; set; }
    }
}