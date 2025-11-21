namespace BarberiaLosHermanos.Dominio
{
    public class Cliente : Persona
    {
        public string Cedula { get; set; } = string.Empty;

        // Constructor vacío para EF
        public Cliente()
        {
        }

        // Constructor principal usado por consola y web.
        // El parámetro cedula es opcional para no romper código viejo.
        public Cliente(string nombre, string telefono, string correo, string cedula = "")
            : base(nombre, telefono, correo)
        {
            Cedula = cedula;
        }

        public override string Resumen()
        {
            return $"{Nombre} - Tel: {Telefono} - Correo: {Correo} - Cédula: {Cedula}";
        }
    }
}
