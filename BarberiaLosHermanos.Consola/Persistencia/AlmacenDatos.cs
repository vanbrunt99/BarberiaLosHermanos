using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BarberiaLosHermanos.Dominio;

namespace BarberiaLosHermanos.Consola.Persistencia
{
    // Repositorio en memoria (Singleton) para clientes, servicios y citas.
    public sealed class AlmacenDatos
    {
        // Instancia única de la clase (hilo-seguro por inicialización estática).
        private static readonly AlmacenDatos _instancia = new AlmacenDatos();

        // Exposición pública de la instancia única.
        public static AlmacenDatos Instancia => _instancia;

        // Colecciones en memoria. Concurrent para seguridad básica si hubiera paralelismo.
        private readonly ConcurrentDictionary<string, Cliente> _clientes = new();
        private readonly ConcurrentDictionary<string, Servicio> _servicios = new();
        private readonly ConcurrentDictionary<int, Cita> _citas = new();   // <-- ahora int

        // Constructor privado: evita crear más instancias (pilar del Singleton).
        private AlmacenDatos() { }

        // --- CLIENTES ---

        // Agrega o actualiza un cliente por su nombre (clave simple para este escenario).
        public void GuardarCliente(Cliente cliente) => _clientes[cliente.Nombre.ToUpperInvariant()] = cliente;

        // Busca cliente por nombre (case-insensitive).
        public Cliente? BuscarCliente(string nombre)
        {
            _clientes.TryGetValue(nombre.ToUpperInvariant(), out var cli);
            return cli;
        }

        // --- SERVICIOS ---

        // Agrega o actualiza un servicio por su nombre como clave.
        public void GuardarServicio(Servicio servicio) => _servicios[servicio.Nombre.ToUpperInvariant()] = servicio;

        // Devuelve todos los servicios disponibles.
        public IEnumerable<Servicio> ListarServicios() => _servicios.Values.OrderBy(s => s.Nombre);

        // Busca un servicio por nombre (case-insensitive).
        public Servicio? BuscarServicio(string nombre)
        {
            _servicios.TryGetValue(nombre.ToUpperInvariant(), out var svc);
            return svc;
        }

        // --- CITAS ---

        // Guarda una cita (clave por Id).
        public void GuardarCita(Cita cita) => _citas[cita.Id] = cita;

        // Elimina una cita por Id; retorna true si la borró.
        public bool EliminarCita(int id) => _citas.TryRemove(id, out _);

        // Lista todas las citas ordenadas por fecha/hora.
        public IEnumerable<Cita> ListarCitas() => _citas.Values.OrderBy(c => c.FechaHora);

        // Filtra citas por día calendario.
        public IEnumerable<Cita> ListarCitasPorDia(DateTime dia)
            => _citas.Values
                     .Where(c => c.FechaHora.Date == dia.Date)
                     .OrderBy(c => c.FechaHora);

        // Devuelve todos los clientes ordenados por nombre (útil para listar).
        public IEnumerable<Cliente> ListarClientes()
            => _clientes.Values.OrderBy(c => c.Nombre);

        // Elimina un servicio por nombre (case-insensitive). Retorna true si lo eliminó.
        public bool EliminarServicio(string nombre)
            => _servicios.TryRemove(nombre.ToUpperInvariant(), out _);

        public bool TieneCitasDeCliente(string nombreCliente)
        {
            var clave = nombreCliente.ToUpperInvariant();

            return _citas.Values.Any(c =>
                c.Cliente != null &&
                !string.IsNullOrWhiteSpace(c.Cliente.Nombre) &&
                c.Cliente.Nombre.ToUpperInvariant() == clave);
        }

        public bool EliminarCliente(string nombre)
        {
            // Solo elimina si no tiene citas
            if (TieneCitasDeCliente(nombre))
                return false;

            return _clientes.TryRemove(nombre.ToUpperInvariant(), out _);
        }

        public IEnumerable<Cita> ListarCitasDeCliente(string nombreCliente)
        {
            var clave = nombreCliente.ToUpperInvariant();

            return _citas.Values
                         .Where(c =>
                             c.Cliente != null &&
                             !string.IsNullOrWhiteSpace(c.Cliente.Nombre) &&
                             c.Cliente.Nombre.ToUpperInvariant() == clave)
                         .OrderBy(c => c.FechaHora);
        }
    }
}
