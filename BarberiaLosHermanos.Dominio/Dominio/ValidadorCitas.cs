using System;

namespace BarberiaLosHermanos.Dominio
{
    // Validador de reglas de negocio de Cita (SRP: separa validaciones).
    public static class ValidadorCitas
    {
        /// <summary>
        /// Valida que la fecha/hora de la cita no sea pasada
        /// y no exceda los 7 días de anticipación.
        /// </summary>
        /// <param name="fechaHora">Fecha y hora de la cita.</param>
        /// <param name="ahora">
        /// Momento actual para comparar. Si es null, se usa DateTime.Now.
        /// (Permite inyectar un "reloj" en las pruebas unitarias).
        /// </param>
        public static void ValidarFecha(DateTime fechaHora, DateTime? ahora = null)
        {
            // Permite inyectar "ahora" para pruebas. Si no viene, usa DateTime.Now.
            var clock = ahora ?? DateTime.Now;

            // No se permiten fechas pasadas (comparación a minuto exacto).
            if (fechaHora <= clock)
                throw new ArgumentException("No se permiten citas en fechas/horas pasadas.", nameof(fechaHora));

            // Límite máximo de anticipación: 7 días calendario.
            if (fechaHora > clock.AddDays(7))
                throw new ArgumentException("Solo se puede agendar con un máximo de 7 días de anticipación.", nameof(fechaHora));
        }
    }
}


