using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PruebaIngresoBibliotecario.Api.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace PruebaIngresoBibliotecario.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestamoController : ControllerBase
    {
        readonly IPrestamoLibroService service;
        private object resultado = "";

        public PrestamoController(IPrestamoLibroService prestamoLibroService)
        {
            service = prestamoLibroService;
        }
        /// <summary>
        /// Obtener prestamo por un id que envia el usuario.
        /// </summary>
        /// <param name="idPrestamo">Identificación del prestamo.</param>
        /// <returns>ActionResult con mensaje de información o que no se ha encontrado el registro.</returns>
        [HttpGet("{idPrestamo}")]
        public IActionResult GetPrestamoById(Guid idPrestamo)
        {
            var prestamo = service.ObtenerPrestamoById(idPrestamo);
            if (prestamo == null)
            {
                resultado = new
                {
                    mensaje = $"El prestamo con id {idPrestamo} no existe"
                };
                return NotFound(resultado);
            }
            else
            {
                resultado = new
                {
                    id = prestamo.IdPrestamoLibro,
                    isbn = prestamo.IdLibroPrestado,
                    identificaciónUsuario = prestamo.IdUsuarioPrestamoLibro,
                    tipoUsuario = prestamo.TipoUsuarioPrestamo,
                    fechaMaximaDevolucion = prestamo.FechaDevolucionPrestamo.ToShortDateString()
                };
                return Ok(resultado);
            }
        }
        /// <summary>
        /// Crear un prestamo.
        /// </summary>
        /// <param name="solicitudPrestamoLibro">Objeto para obtener información del body.</param>
        /// <returns>Información del prestamo creado o si se encuentra error en alguna validación un BadRequest.</returns>
        [HttpPost]
        public async Task<IActionResult> PostPrestamo([FromBody] object solicitudPrestamoLibro)
        {
            dynamic libroPrestar = JObject.Parse(solicitudPrestamoLibro.ToString());

            string guidLibro = libroPrestar.Isbn;
            string idUsuario = libroPrestar.IdentificacionUsuario;

            if (!Guid.TryParse(guidLibro, out _) || idUsuario.Length > 10)
            {
                return BadRequest();
            }

            PrestamoLibro prestamoLibro = new PrestamoLibro
            {
                IdLibroPrestado = Guid.Parse(guidLibro),
                TipoUsuarioPrestamo = libroPrestar.TipoUsuario,
                IdUsuarioPrestamoLibro = idUsuario
            };

            var prestamoAsigado = await service.AgregarPrestamos(prestamoLibro);

            if (prestamoAsigado == null)
            {
                resultado = new
                {
                    mensaje = $"El usuario con identificacion {prestamoLibro.IdUsuarioPrestamoLibro} ya tiene un libro prestado por lo cual no se le puede realizar otro prestamo"
                };
                return NotFound(resultado);
            }
            else
            {
                resultado = new
                {
                    id = prestamoAsigado.IdPrestamoLibro,
                    fechaMaximaDevolucion = prestamoAsigado.FechaDevolucionPrestamo.ToShortDateString()
                };
                return Ok(resultado);
            }
        }
    }
}
