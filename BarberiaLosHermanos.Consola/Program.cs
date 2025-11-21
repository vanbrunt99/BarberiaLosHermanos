using System.Linq; // Para usar Any()
using BarberiaLosHermanos.Consola.Presentacion;
using BarberiaLosHermanos.Dominio;
using BarberiaLosHermanos.Consola.Persistencia;

namespace BarberiaLosHermanos.Consola
{
    internal class Programa
    {
        static void Main(string[] args)
        {
            // Soporte de acentos en consola.
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 1) Obtenemos el almacén de datos (Singleton en memoria).
            var store = AlmacenDatos.Instancia;

            // 2) Sembramos servicios básicos una sola vez si aún no existen.
            if (!store.ListarServicios().Any())
            {
                store.GuardarServicio(new Servicio("Corte Caballero", 3500m));   // Servicio con precio
                store.GuardarServicio(new Servicio("Afeitado Clásico", 2500m));  // Servicio con precio
                store.GuardarServicio(new Servicio("Corte + Barba", 5000m));     // Paquete
            }

            // 3) Lanzamos la app de consola (menús).
            var app = new AplicacionConsola();
            app.Ejecutar();
        }
    }
}


