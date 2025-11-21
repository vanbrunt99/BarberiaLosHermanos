namespace BarberiaLosHermanos.Dominio
{
    // Entidad de dominio: una cita con cliente, servicio y fecha/hora.
    public class Cita
    {
        // Clave primaria para EF Core
        public int Id { get; set; }

        // Claves foráneas
        public int ClienteId { get; set; }
        public int ServicioId { get; set; }

        // Propiedades de navegación
        public Cliente? Cliente { get; set; }
        public Servicio? Servicio { get; set; }

        // Momento exacto de la cita.
        public DateTime FechaHora { get; set; }

        // Constructor sin parámetros para EF Core / MVC
        public Cita()
        {
        }

        // Constructor de dominio opcional (si la creas desde código)
        public Cita(Cliente cliente, Servicio servicio, DateTime fechaHora, DateTime? ahora = null)
        {
            Cliente = cliente ?? throw new ArgumentNullException(nameof(cliente));
            Servicio = servicio ?? throw new ArgumentNullException(nameof(servicio));

            ClienteId = cliente.Id;
            ServicioId = servicio.Id;

            ValidadorCitas.ValidarFecha(fechaHora, ahora);
            FechaHora = fechaHora;
        }

        // Representación amigable para listados.
        public override string ToString()
            => $"{FechaHora:g} — {Cliente?.Nombre} — {Servicio?.Nombre} (₡{Servicio?.Precio:N2})";
    }
}
