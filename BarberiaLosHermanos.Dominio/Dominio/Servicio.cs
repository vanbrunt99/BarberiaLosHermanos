using System.ComponentModel.DataAnnotations;

namespace BarberiaLosHermanos.Dominio
{
    // Clase de Entidad de Dominio para los Servicios que ofrece la barbería.
    public class Servicio
    {
        // Clave primaria para Entity Framework Core
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del servicio es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio.")]
        // Range(0.01, 1000.00) asegura que el precio sea positivo y razonable.
        [Range(0.01, 1000.00, ErrorMessage = "El precio debe ser un valor positivo.")]
        public decimal Precio { get; set; }

        // Campo opcional para una breve descripción del servicio
        [StringLength(500, ErrorMessage = "La descripción no debe exceder los 500 caracteres.")]
        public string? Descripcion { get; set; }

        // Duración estimada del servicio en minutos (opcional)
        [Range(1, 600, ErrorMessage = "La duración debe estar entre 1 y 600 minutos.")]
        public int? Duracion { get; set; }

        // Indica si el servicio está activo y se puede ofrecer a clientes
        public bool EstaActivo { get; set; } = true;

        // Constructor sin parámetros requerido por EF Core y MVC
        public Servicio()
        {
        }

        // Constructor de dominio opcional, por si quieres seguir creando servicios desde código.
        public Servicio(string nombre, decimal precio, string? descripcion = null, int? duracion = null, bool estaActivo = true)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del servicio no puede estar vacío.", nameof(nombre));

            if (precio <= 0)
                throw new ArgumentException("El precio debe ser mayor a cero.", nameof(precio));

            Nombre = nombre.Trim();
            Precio = precio;
            Descripcion = descripcion?.Trim();
            Duracion = duracion;
            EstaActivo = estaActivo;
        }

        // Método de dominio para actualizar la información del servicio
        public void Actualizar(string nuevoNombre, decimal nuevoPrecio, string? nuevaDescripcion = null, int? nuevaDuracion = null, bool nuevoEstado = true)
        {
            if (string.IsNullOrWhiteSpace(nuevoNombre))
                throw new ArgumentException("El nombre del servicio no puede estar vacío.", nameof(nuevoNombre));

            if (nuevoPrecio <= 0)
                throw new ArgumentException("El precio debe ser mayor a cero.", nameof(nuevoPrecio));

            Nombre = nuevoNombre.Trim();
            Precio = nuevoPrecio;
            Descripcion = nuevaDescripcion?.Trim();
            Duracion = nuevaDuracion;
            EstaActivo = nuevoEstado;
        }
    }
}
