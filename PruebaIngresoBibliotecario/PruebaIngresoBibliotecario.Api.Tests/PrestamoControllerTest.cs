using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using Xunit;
using FluentAssertions;


namespace Api.Test
{

    public class PrestamoControllerTest : IntegrationTestBuilder
    {


        [Fact]
        public void GetPrestamoExito()
        {
            var solicitudPrestamo = new
            {
                TipoUsuario = TipoUsuarioPrestamo.INVITADO,
                IdentificacionUsuario = "123456789",
                Isbn = Guid.NewGuid()
            };
            // cargamos la data a la db para poder obtener id y consultar con este si el proceso de carga fue satisfactorio
            var carga = this.TestClient.PostAsync("/api/prestamo", solicitudPrestamo, new JsonMediaTypeFormatter()).Result;
            carga.EnsureSuccessStatusCode();
            var respuestaCarga = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(carga.Content.ReadAsStringAsync().Result);
            var idPrestamo = respuestaCarga["id"];

            var c = this.TestClient.GetAsync($"api/prestamo/{idPrestamo}").Result;
            c.EnsureSuccessStatusCode();
            var response = c.Content.ReadAsStringAsync().Result;
            var respuestaConsulta = System.Text.Json.JsonSerializer.Deserialize<RespuestaConsultaDto>(response);

            respuestaConsulta.IdUsuarioPrestamoLibro.Should().Be(solicitudPrestamo.IdentificacionUsuario);
            respuestaConsulta.IsbnLibroPrestamo.Should().Be(solicitudPrestamo.Isbn);
        }


        [Fact]
        public void GetPrestamoError()
        {
            var prestamoId = Guid.NewGuid().ToString();
            HttpResponseMessage respuesta = null;
            try
            {
                respuesta = this.TestClient.GetAsync($"api/prestamo/{prestamoId}").Result;
                respuesta.EnsureSuccessStatusCode();
                Assert.True(false, "Deberia fallar");
            }
            catch (Exception)
            {
                respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }


        [Fact]
        public void PostPrestamoError()
        {
            var usuarioId = "1234567890";

            var errorMessage = $"El usuario con identificacion {usuarioId} ya tiene un libro prestado por lo cual no se le puede realizar otro prestamo";
            HttpResponseMessage respuesta = null;

            try
            {
                var solicitudPrestamo = new 
                {
                    TipoUsuario = TipoUsuarioPrestamo.INVITADO,
                    IdentificacionUsuario = usuarioId,
                    Isbn = Guid.NewGuid()
                };

                respuesta = this.TestClient.PostAsync("api/prestamo", solicitudPrestamo, new JsonMediaTypeFormatter()).Result;
                respuesta.EnsureSuccessStatusCode();

                respuesta = this.TestClient.PostAsync("api/prestamo", solicitudPrestamo, new JsonMediaTypeFormatter()).Result;
                respuesta.EnsureSuccessStatusCode();

                 Assert.True(false,"No deberia permitir prestar otro libro a este invitado");
            }
            catch (Exception)
            {
                var contenidoRespuesta = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(respuesta.Content.ReadAsStringAsync().Result);
                contenidoRespuesta["mensaje"].Should().Be(errorMessage);
            }
        }


        [Fact]
        public void PostPrestamoInvitadoFechaEntregaExito()
        {

            var solicitudPrestamo = new 
            {
                TipoUsuario = TipoUsuarioPrestamo.INVITADO,
                IdentificacionUsuario = "951263584",
                Isbn = Guid.NewGuid()
            };

            var fechaEsperada = CalcularFechaEntrega(TipoUsuarioPrestamo.INVITADO).ToShortDateString();

            var respuesta = this.TestClient.PostAsync("api/prestamo", solicitudPrestamo, new JsonMediaTypeFormatter()).Result;
            respuesta.EnsureSuccessStatusCode();
            var prestamoRadicado = System.Text.Json.JsonSerializer
               .Deserialize<Dictionary<string, object>>(respuesta.Content.ReadAsStringAsync().Result);
            var fechaEntrega = DateTime.Parse(prestamoRadicado["fechaMaximaDevolucion"].ToString()).ToShortDateString();
            fechaEntrega.Should().Be(fechaEsperada);
        }

        [Fact]
        public void PostPrestamoEmpleadoFechaEntregaExito()
        {

            var solicitudPrestamo = new 
            {
                TipoUsuario = TipoUsuarioPrestamo.EMPLEADO,
                IdentificacionUsuario = "9876543210",
                Isbn = Guid.NewGuid()
            };

            var fechaEsperadaEmpleado = CalcularFechaEntrega(TipoUsuarioPrestamo.EMPLEADO).ToShortDateString();

            var respuesta = this.TestClient.PostAsync("api/prestamo", solicitudPrestamo, new JsonMediaTypeFormatter()).Result;
            respuesta.EnsureSuccessStatusCode();
            var prestamoRadicado = System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, object>>(respuesta.Content.ReadAsStringAsync().Result);
            var fechaEntrega = DateTime.Parse(prestamoRadicado["fechaMaximaDevolucion"].ToString()).ToShortDateString();
            fechaEntrega.Should().Be(fechaEsperadaEmpleado);

        }

        [Fact]
        public void PostPrestamoAfiliadoFechaEntregaExito()
        {           
            var solicitudPrestamo = new 
            {
                TipoUsuario = TipoUsuarioPrestamo.AFILIADO,
                IdentificacionUsuario = "1234568",
                Isbn = Guid.NewGuid()
            };

            var fechaEsperadaAfiliado = CalcularFechaEntrega(TipoUsuarioPrestamo.AFILIADO).ToShortDateString();

            var respuesta = this.TestClient.PostAsync("api/prestamo", solicitudPrestamo, new JsonMediaTypeFormatter()).Result;
            respuesta.EnsureSuccessStatusCode();
            var prestamoRadicado = System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, object>>(respuesta.Content.ReadAsStringAsync().Result);
            var fechaEntrega = DateTime.Parse(prestamoRadicado["fechaMaximaDevolucion"].ToString()).ToShortDateString();
            fechaEntrega.Should().Be(fechaEsperadaAfiliado);
        }

        [Fact]
        public void PostPrestamoAfiliadoIsbnError()
        {
            HttpResponseMessage respuesta = null;
            try
            {
                var solicitudPrestamo = new
                {
                    TipoUsuario = TipoUsuarioPrestamo.AFILIADO,
                    IdentificacionUsuario = Guid.NewGuid().ToString(),
                    Isbn = "ASDFG123456789"
                };

                var fechaEsperadaAfiliado = CalcularFechaEntrega(TipoUsuarioPrestamo.AFILIADO).ToShortDateString();

                respuesta = this.TestClient.PostAsync("api/prestamo", solicitudPrestamo, new JsonMediaTypeFormatter()).Result;
                respuesta.EnsureSuccessStatusCode();
                Assert.True(false,"No deberia responder con exito, mal payload");
            }
            catch (Exception)
            {                
                respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public void PostPrestamoAfiliadoTipoUsuarioError()
        {
            HttpResponseMessage respuesta = null;
            try
            {
                var solicitudPrestamo = new
                {
                    TipoUsuario = 5,
                    IdentificacionUsuario = Guid.NewGuid().ToString(),
                    Isbn = Guid.NewGuid().ToString()
                };

                var fechaEsperadaAfiliado = CalcularFechaEntrega(TipoUsuarioPrestamo.AFILIADO).ToShortDateString();

                respuesta = this.TestClient.PostAsync("api/prestamo", solicitudPrestamo, new JsonMediaTypeFormatter()).Result;
                respuesta.EnsureSuccessStatusCode();
               Assert.True(false,"No deberia responder con exito, mal payload");
            }
            catch (Exception)
            {               
                respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }       

    }

}
