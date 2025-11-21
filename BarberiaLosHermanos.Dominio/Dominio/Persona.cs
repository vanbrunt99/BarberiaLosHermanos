namespace BarberiaLosHermanos.Dominio
{
    // Clase base para personas de la barbería (clientes, barberos, etc.)
    public abstract class Persona
    {
        // Clave primaria para EF Core
        public int Id { get; set; }

        // Datos básicos
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }

        // Constructor sin parámetros requerido por EF Core / MVC
        protected Persona()
        {
        }

        // Constructor útil para crear personas desde código
        protected Persona(string nombre, string? telefono, string? correo)
        {
            Nombre = nombre;
            Telefono = telefono;
            Correo = correo;
        }

        public abstract string Resumen();
    }
}



