// Necesarios para Console, DateTime, TimeSpan, etc.
using System;
// Necesario para .ToList() y .Any()
using System.Linq;

// Nuestras entidades de dominio (Cliente, Servicio, Cita)
using BarberiaLosHermanos.Dominio;
// Nuestro repositorio en memoria (Singleton)
using BarberiaLosHermanos.Consola.Persistencia;

namespace BarberiaLosHermanos.Consola.Presentacion
{
    // Orquesta los menús y delega a casos de uso simples (SRP).
    public class AplicacionConsola
    {
        // Instancia única del almacén en memoria (repositorio).
        private readonly AlmacenDatos _store = AlmacenDatos.Instancia;

        // Inicia el bucle principal de la aplicación.
        public void Ejecutar()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.Clear();
                MostrarMenuPrincipal();

                Console.Write("Seleccione una opción: ");
                var entrada = Console.ReadLine();

                switch (entrada)
                {
                    case "1":
                        // Submenú de Gestión de Citas (crear/consultar/cancelar).
                        MenuCitas();
                        break;

                    case "2":
                        // Submenú de Control de Clientes.
                        MenuClientes();
                        break;

                    case "3":
                        // Submenú de Servicios y Precios.
                        MenuServicios();
                        break;

                    case "4":
                        Console.WriteLine("Saliendo... ¡Gracias!");
                        return;

                    default:
                        Console.WriteLine("Opción no válida. Presione una tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // ====== Menú principal ======
        private void MostrarMenuPrincipal()
        {
            Console.WriteLine("===== Barbería Los Hermanos =====");
            Console.WriteLine("1. Gestión de Citas");
            Console.WriteLine("2. Control de Clientes");
            Console.WriteLine("3. Servicios y Precios");
            Console.WriteLine("4. Salir");
            Console.WriteLine();
        }

        // ====== Submenú: Citas ======
        private void MenuCitas()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Gestión de Citas ===");
                Console.WriteLine("1) Crear cita");
                Console.WriteLine("2) Consultar todas las citas");
                Console.WriteLine("3) Consultar citas por día");
                Console.WriteLine("4) Cancelar cita por ID");
                Console.WriteLine("5) Volver");
                Console.WriteLine();

                Console.Write("Seleccione una opción: ");
                var op = Console.ReadLine();

                switch (op)
                {
                    case "1": CrearCita(); break;
                    case "2": ConsultarCitas(); break;
                    case "3": ConsultarCitasPorDia(); break;
                    case "4": CancelarCitaPorId(); break;
                    case "5": return; // vuelve al menú principal
                    default:
                        Console.WriteLine("Opción inválida. Presione una tecla...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // 1) Crear una cita (cumple reglas: no pasado y máx 7 días).
        private void CrearCita()
        {
            Console.Clear();
            Console.WriteLine("--- Crear Cita ---");

            // 1) Datos del cliente (si existe, lo reutilizamos).
            var nombre = LeerNoVacio("Nombre completo del cliente: ");
            var telefono = LeerOpcional("Teléfono (opcional): ");
            var correo = LeerOpcional("Correo (opcional): ");

            // CrearCita()
            var cliente = _store.BuscarCliente(nombre)
                          ?? new Cliente(nombre, telefono ?? string.Empty, correo ?? string.Empty);

            _store.GuardarCliente(cliente); // idempotente

            // 2) Selección de servicio por número (con opción 0 para volver).
            var servicio = SeleccionarServicioPorNumero();
            if (servicio is null)
            {
                Console.WriteLine("Volviendo al menú de Gestión de Citas...");
                Console.WriteLine("Presione una tecla para continuar...");
                Console.ReadKey();
                return;
            }

            // 3) Fecha y Hora (cada lector ya reintenta hasta obtener un valor válido).
            var fecha = LeerFecha("Fecha de la cita (AAAA-MM-DD): ");
            var hora = LeerHora("Hora de la cita (HH:MM 24h): ");
            var fechaHora = fecha.Date.Add(hora);

            // 4) Intento de creación con validaciones de negocio (no pasado / <= 7 días).
            try
            {
                var cita = new Cita(cliente, servicio, fechaHora);
                _store.GuardarCita(cita);

                Console.WriteLine($"\n✅ Cita creada con ID: {cita.Id}");
                Console.WriteLine(cita);
                Console.WriteLine("\nPresione una tecla para continuar...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ No se pudo crear la cita: {ex.Message}");
                Console.Write("¿Desea intentarlo de nuevo? (S/N): ");
                var resp = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

                if (resp == "S" || resp == "SI" || resp == "SÍ")
                {
                    CrearCita(); // reintento
                }
                // si no, volvemos al submenú de Citas
            }
        }

        // 2) Consultar todas las citas (ordenadas por fecha/hora).
        private void ConsultarCitas()
        {
            Console.Clear();
            Console.WriteLine("--- Todas las Citas ---");

            var citas = _store.ListarCitas().ToList();

            if (!citas.Any())
            {
                Console.WriteLine("No hay citas registradas.");
            }
            else
            {
                foreach (var c in citas)
                    Console.WriteLine(c);
            }

            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        // 3) Consultar citas por día calendario con opción de volver.
        private void ConsultarCitasPorDia()
        {
            Console.Clear();
            Console.WriteLine("--- Consultar por Día ---");

            var fecha = LeerFechaOVolver("Ingrese la fecha (AAAA-MM-DD) o '0' para volver: ");
            if (fecha is null)
            {
                Console.WriteLine("Volviendo al menú de Gestión de Citas...");
                Console.WriteLine("Presione una tecla para continuar...");
                Console.ReadKey();
                return;
            }

            var citas = _store.ListarCitasPorDia(fecha.Value).ToList();

            if (!citas.Any())
            {
                Console.WriteLine("No hay citas para ese día.");
            }
            else
            {
                foreach (var c in citas)
                    Console.WriteLine(c);
            }

            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        // 4) Cancelar cita ingresando su ID (numérico).
        private void CancelarCitaPorId()
        {
            Console.Clear();
            Console.WriteLine("--- Cancelar Cita ---");

            var textoId = LeerNoVacio("Ingrese el ID de la cita: ");

            // Validamos que el usuario escriba un número entero
            if (!int.TryParse(textoId, out var id))
            {
                Console.WriteLine("\n❌ ID inválido. Debe ser un número entero.");
                Console.WriteLine("\nPresione una tecla para continuar...");
                Console.ReadKey();
                return;
            }

            var ok = _store.EliminarCita(id);
            Console.WriteLine(ok
                ? "✅ Cita cancelada correctamente."
                : "❌ No se encontró una cita con ese ID.");

            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        // ====== Submenú: Control de Clientes ======
        private void MenuClientes()
        {

            Console.Clear();
            Console.WriteLine("=== Control de Clientes ===");
            Console.WriteLine("1) Registrar cliente");
            Console.WriteLine("2) Buscar cliente por nombre");
            Console.WriteLine("3) Listar clientes");
            Console.WriteLine("4) Ver historial de citas de un cliente"); 
            Console.WriteLine("5) Eliminar cliente");                     
            Console.WriteLine("6) Volver");
            Console.WriteLine();

            Console.Write("Seleccione una opción: ");
            var op = Console.ReadLine();

            switch (op)
            {
                case "1": RegistrarCliente(); break;
                case "2": BuscarClientePorNombre(); break;
                case "3": ListarClientes(); break;
                case "4": HistorialDeCitasDeCliente(); break; 
                case "5": EliminarClienteUI(); break;
                case "6": return;
                default:
                    Console.WriteLine("Opción inválida. Presione una tecla...");
                    Console.ReadKey();
                    break;
            }


        }

        private void RegistrarCliente()
        {
            Console.Clear();
            Console.WriteLine("--- Registrar Cliente ---");

            var nombre = LeerNoVacio("Nombre completo: ");
            var telefono = LeerOpcional("Teléfono (opcional): ");
            var correo = LeerOpcional("Correo (opcional): ");

            // RegistrarCliente()
            var cli = _store.BuscarCliente(nombre)
                      ?? new Cliente(nombre, telefono ?? string.Empty, correo ?? string.Empty);

            _store.GuardarCliente(cli);

            Console.WriteLine("\n✅ Cliente guardado.");
            Console.WriteLine("Presione una tecla para continuar...");
            Console.ReadKey();
        }

        private void BuscarClientePorNombre()
        {
            Console.Clear();
            Console.WriteLine("--- Buscar Cliente ---");

            var nombre = LeerNoVacio("Ingrese el nombre a buscar: ");
            var cli = _store.BuscarCliente(nombre);

            if (cli is null)
            {
                Console.WriteLine("No se encontró cliente con ese nombre.");
            }
            else
            {
                Console.WriteLine($"Nombre: {cli.Nombre}");
                Console.WriteLine($"Teléfono: {(cli.Telefono ?? "-")}");
                Console.WriteLine($"Correo: {(cli.Correo ?? "-")}");
            }

            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        private void ListarClientes()
        {
            Console.Clear();
            Console.WriteLine("--- Lista de Clientes ---");

            var clientes = _store.ListarClientes().ToList();

            if (!clientes.Any())
            {
                Console.WriteLine("No hay clientes registrados.");
            }
            else
            {
                foreach (var c in clientes)
                    Console.WriteLine($"- {c.Nombre} | Tel: {c.Telefono ?? "-"} | Correo: {c.Correo ?? "-"}");
            }

            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        private void HistorialDeCitasDeCliente()
        {
            Console.Clear();
            Console.WriteLine("--- Historial de Citas por Cliente ---");

            var nombre = LeerNoVacio("Nombre del cliente: ");
            var citas = _store.ListarCitasDeCliente(nombre).ToList();

            if (!citas.Any())
            {
                Console.WriteLine("No hay citas registradas para este cliente.");
            }
            else
            {
                foreach (var c in citas)
                    Console.WriteLine(c); // asume ToString() amigable en Cita
            }

            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }


        private void EliminarClienteUI()
        {
            Console.Clear();
            Console.WriteLine("--- Eliminar Cliente ---");

            // Ayuda visual: listar clientes existentes
            var clientes = _store.ListarClientes().ToList();
            if (!clientes.Any())
            {
                Console.WriteLine("No hay clientes registrados.");
                Console.WriteLine("\nPresione una tecla para continuar...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Clientes:");
            foreach (var c in clientes)
                Console.WriteLine($"- {c.Nombre}");

            var nombre = LeerNoVacio("\nEscriba el nombre EXACTO del cliente a eliminar: ");

            // Regla: si tiene citas, no se permite eliminar
            if (_store.TieneCitasDeCliente(nombre))
            {
                Console.WriteLine("❌ No se puede eliminar: el cliente tiene citas registradas.");
                Console.WriteLine("Sugerencia: cancele primero las citas del cliente y vuelva a intentar.");
                Console.WriteLine("\nPresione una tecla para continuar...");
                Console.ReadKey();
                return;
            }

            var ok = _store.EliminarCliente(nombre);
            Console.WriteLine(ok ? "✅ Cliente eliminado." : "❌ No se encontró un cliente con ese nombre.");
            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }


        // ====== Submenú: Servicios y Precios ======
        private void MenuServicios()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Servicios y Precios ===");
                Console.WriteLine("1) Listar servicios");
                Console.WriteLine("2) Agregar servicio");
                Console.WriteLine("3) Editar precio de servicio");
                Console.WriteLine("4) Eliminar servicio");
                Console.WriteLine("5) Volver");
                Console.WriteLine();

                Console.Write("Seleccione una opción: ");
                var op = Console.ReadLine();

                switch (op)
                {
                    case "1": ListarServiciosUI(); break;
                    case "2": AgregarServicio(); break;
                    case "3": EditarPrecioServicio(); break;
                    case "4": EliminarServicioUI(); break;
                    case "5": return; // vuelve al menú principal
                    default:
                        Console.WriteLine("Opción inválida. Presione una tecla...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void ListarServiciosUI()
        {
            Console.Clear();
            Console.WriteLine("--- Servicios Disponibles ---");

            var lista = _store.ListarServicios().ToList();
            if (!lista.Any())
            {
                Console.WriteLine("No hay servicios configurados.");
            }
            else
            {
                for (int i = 0; i < lista.Count; i++)
                    Console.WriteLine($"{i + 1}) {lista[i].Nombre} (₡{lista[i].Precio:N2})");
            }

            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        private void AgregarServicio()
        {
            Console.Clear();
            Console.WriteLine("--- Agregar Servicio ---");

            var nombre = LeerNoVacio("Nombre del servicio: ");
            var precio = LeerDecimalOVolver("Precio (use punto) o '0' para volver: ");
            if (precio is null)
            {
                Console.WriteLine("Volviendo al menú de Servicios...");
                Console.WriteLine("Presione una tecla para continuar...");
                Console.ReadKey();
                return;
            }

            var servicio = new Servicio(nombre, precio.Value);
            _store.GuardarServicio(servicio);

            Console.WriteLine("✅ Servicio agregado/actualizado.");
            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        private void EditarPrecioServicio()
        {
            Console.Clear();
            Console.WriteLine("--- Editar Precio de Servicio ---");

            ListarServiciosUI();

            var nombre = LeerNoVacio("Nombre EXACTO del servicio a modificar: ");
            var existente = _store.BuscarServicio(nombre);
            if (existente is null)
            {
                Console.WriteLine("No existe un servicio con ese nombre.");
                Console.WriteLine("Presione una tecla para continuar...");
                Console.ReadKey();
                return;
            }

            var precio = LeerDecimalOVolver("Nuevo precio o '0' para volver: ");
            if (precio is null)
            {
                Console.WriteLine("Volviendo al menú de Servicios...");
                Console.WriteLine("Presione una tecla para continuar...");
                Console.ReadKey();
                return;
            }

            _store.GuardarServicio(new Servicio(existente.Nombre, precio.Value));
            Console.WriteLine("✅ Precio actualizado.");
            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        private void EliminarServicioUI()
        {
            Console.Clear();
            Console.WriteLine("--- Eliminar Servicio ---");

            ListarServiciosUI();

            var nombre = LeerNoVacio("Nombre EXACTO del servicio a eliminar: ");
            var ok = _store.EliminarServicio(nombre);

            Console.WriteLine(ok ? "✅ Servicio eliminado." : "❌ No se encontró el servicio.");
            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        // ====== Utilidades de entrada por consola ======

        private string LeerNoVacio(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                var t = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(t))
                    return t.Trim();

                Console.WriteLine("El valor es obligatorio. Intente de nuevo.");
            }
        }

        private string? LeerOpcional(string mensaje)
        {
            Console.Write(mensaje);
            var t = Console.ReadLine();
            return string.IsNullOrWhiteSpace(t) ? null : t.Trim();
        }

        private DateTime LeerFecha(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                var txt = Console.ReadLine();

                if (DateTime.TryParse(txt, out var fecha))
                    return fecha.Date;

                Console.WriteLine("Fecha inválida. Use formato AAAA-MM-DD.");
            }
        }

        private TimeSpan LeerHora(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                var txt = Console.ReadLine();

                if (TimeSpan.TryParse(txt, out var hora))
                    return hora;

                Console.WriteLine("Hora inválida. Use formato HH:MM (24 horas).");
            }
        }

        // Lee una fecha o permite volver escribiendo "0".
        // Devuelve null si el usuario elige volver.
        private DateTime? LeerFechaOVolver(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                var txt = Console.ReadLine()?.Trim();

                if (txt == "0") return null; // opción para volver sin error
                if (DateTime.TryParse(txt, out var fecha)) return fecha.Date;

                Console.WriteLine("Fecha inválida. Use formato AAAA-MM-DD o '0' para volver.");
            }
        }

        // Lee un decimal positivo o permite volver con '0' literal.
        // Devuelve null si el usuario elige volver.
        private decimal? LeerDecimalOVolver(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                var txt = Console.ReadLine()?.Trim();

                if (txt == "0") return null;

                if (decimal.TryParse(txt, out var valor) && valor >= 0)
                    return valor;

                Console.WriteLine("Valor inválido. Ingrese un número válido o '0' para volver.");
            }
        }

        // Marcador temporal (ya casi no lo usamos, lo dejo por si quieres otros módulos).
        private void MostrarPendiente(string modulo)
        {
            Console.WriteLine($"[{modulo}] — módulo en construcción.");
            Console.WriteLine("Presione una tecla para volver al menú...");
            Console.ReadKey();
        }

        // ====== Helper: elegir servicio por número ======
        private Servicio? SeleccionarServicioPorNumero()
        {
            while (true)
            {
                Console.WriteLine("\nServicios disponibles:");

                var lista = _store.ListarServicios().ToList();

                if (!lista.Any())
                {
                    Console.WriteLine("No hay servicios configurados. Presione una tecla para volver...");
                    Console.ReadKey();
                    return null;
                }

                for (int i = 0; i < lista.Count; i++)
                {
                    var s = lista[i];
                    Console.WriteLine($"{i + 1}) {s.Nombre} (₡{s.Precio:N2})");
                }
                Console.WriteLine("0) Volver");

                Console.Write("Elija una opción: ");
                var entrada = Console.ReadLine();

                if (entrada == "0")
                    return null;

                if (int.TryParse(entrada, out int opcion) && opcion >= 1 && opcion <= lista.Count)
                    return lista[opcion - 1];

                Console.WriteLine("Entrada inválida. Debe ingresar un número de la lista. Intente de nuevo...");
            }
        }
    }
}
