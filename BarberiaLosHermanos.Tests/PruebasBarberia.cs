using System;
using NUnit.Framework;
using BarberiaLosHermanos.Dominio;
using BarberiaLosHermanos.Consola.Persistencia;

namespace BarberiaLosHermanos.Tests
{
    [TestFixture] // Marca esta clase como contenedora de pruebas NUnit
    public class PruebasBarberia
    {
        // Fecha fija para tener resultados consistentes
        private readonly DateTime _ahora = new DateTime(2025, 10, 22, 10, 0, 0);

        // ============================================================
        // TEST 1: No permitir citas con fechas pasadas
        // ============================================================
        [Test]
        public void NoPermiteCitaConFechaPasada()
        {
            var cliente = new Cliente("Juan Pérez", null, null);
            var servicio = new Servicio("Corte", 3500m);

            NUnit.Framework.Assert.Throws<ArgumentException>(
                 () => new Cita(cliente, servicio, _ahora.AddMinutes(-5), ahora: _ahora),
                 "Las citas no deben crearse con fecha/hora pasada."
            );
        }

        // ============================================================
        // TEST 2: No eliminar cliente que tiene citas registradas
        // ============================================================
        [Test]
        public void NoPermiteEliminarClienteConCitas()
        {
            var store = AlmacenDatos.Instancia;
            var cliente = new Cliente("Mario", null, null);
            var servicio = new Servicio("Barba", 2500m);
            store.GuardarCliente(cliente);
            store.GuardarServicio(servicio);

            var cita = new Cita(cliente, servicio, _ahora.AddHours(2), ahora: _ahora);
            store.GuardarCita(cita);

           

            bool eliminado = store.EliminarCliente("Mario");
            NUnit.Framework.Assert.That(eliminado, Is.False,
                "No debe eliminar un cliente con citas activas.");
        }

        // ============================================================
        // TEST 3: Agregar un servicio y actualizar su precio
        // ============================================================
        [Test]
        public void PuedeAgregarYActualizarServicio()
        {
            var store = AlmacenDatos.Instancia;

            store.GuardarServicio(new Servicio("Afeitado", 2500m));
            store.GuardarServicio(new Servicio("Afeitado", 3000m)); // Actualiza precio

            var srv = store.BuscarServicio("Afeitado");
            NUnit.Framework.Assert.That(srv, Is.Not.Null,
                "El servicio debe encontrarse después de ser agregado.");
            NUnit.Framework.Assert.That(srv!.Precio, Is.EqualTo(3000m),
                "El precio debe actualizarse correctamente.");
        }
    }
}
