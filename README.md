# Barbería Los Hermanos

Sistema de gestión para una barbería, desarrollado como Proyecto Final de Programación III (III C 2025).

Incluye:

- Aplicación de consola para administrar clientes, servicios y citas en memoria.
- Aplicación web ASP.NET Core MVC con Entity Framework Core y SQLite.
- Autenticación y autorización con ASP.NET Core Identity (roles Admin y Usuario).
- Separación en capas: Dominio, Infraestructura, Web y Consola.
---

## 1. Objetivo del sistema

El sistema permite administrar la operación básica de una barbería:

- Registrar y consultar clientes.
- Definir servicios y precios de la barbería.
- Programar y gestionar citas entre clientes y servicios.
- Controlar el acceso por roles:
  - Administrador: gestiona catálogos y datos (CRUD completo).
  - Usuario autenticado: puede consultar información pero con operaciones limitadas.

---

## 2. Arquitectura y capas del proyecto

La solución `BarberiaLosHermanos` está organizada en cuatro proyectos:

### 2.1. BarberiaLosHermanos.Dominio

- Contiene las entidades de dominio y reglas de negocio:
  - `Servicio`
    - Propiedades: `Id`, `Nombre`, `Precio`, `Duracion` (opcional, minutos), `EstaActivo`.
    - Reglas: nombre obligatorio, precio > 0.
  - `Cliente` (hereda de `Persona`)
    - Propiedades: `Id`, `Nombre`, `Telefono` (opcional), `Correo` (opcional), `Cedula`.
    - Método de dominio `Resumen()` para mostrar datos resumidos.
  - `Cita`
    - Propiedades: `Id`, `ClienteId`, `ServicioId`, `FechaHora`.
    - Navegación: `Cliente`, `Servicio`.
  - `ValidadorCitas`
    - Reglas de fecha para las citas: no permite fechas en el pasado y restringe el rango de días válidos.

El Dominio no conoce nada de la base de datos ni de la interfaz de usuario. Solo contiene lógica de negocio.

### 2.2. BarberiaLosHermanos.Infraestructura

- Implementa el acceso a datos con Entity Framework Core y SQLite.
- Clase principal: `BarberiaDbContext`, que mapea las entidades:
  - `DbSet<Cliente> Clientes`
  - `DbSet<Servicio> Servicios`
  - `DbSet<Cita> Citas`
- La migración de este contexto se configura para generarse en el proyecto Web.

### 2.3. BarberiaLosHermanos.Web

Aplicación web ASP.NET Core MVC con Identity.

- DbContexts:
  - `BarberiaDbContext` → base de datos principal (`barberia.db`).
  - `AppIdentityDb` → base de datos de Identity (`identity.db`).
- Autenticación y autorización con:
  - Usuarios administrados por Identity.
  - Roles: `Admin` y `Usuario`.
- Controladores principales:
  - `ClientesController`:
    - Lista clientes.
    - Admin puede crear, editar y eliminar.
  - `ServiciosController`:
    - Lista servicios.
    - Admin puede crear, editar, eliminar, activar/desactivar.
  - `CitasController`:
    - Lista citas (cualquier usuario autenticado).
    - Admin puede crear, editar y cancelar (eliminar) citas.
- Vistas MVC (Razor):
  - Home (pantalla de bienvenida).
  - Listado y formularios de Clientes, Servicios y Citas.
  - Layout con barra de navegación y estilos personalizados.
- Identity (área `Areas/Identity`):
  - Páginas de Login, Registro, Logout, gestión de cuenta, etc.

### 2.4. BarberiaLosHermanos.Consola

Aplicación de consola para ejecutar la lógica de negocio en modo texto.

- Clase `AlmacenDatos` (singleton) que mantiene listas en memoria:
  - Clientes, Servicios, Citas.
- Clase `AplicacionConsola` con menús:
  - Control de Clientes.
  - Servicios y precios.
  - Gestión de Citas.
- Permite registrar clientes, listar servicios, crear citas, consultar citas por día y cancelar citas.

---

## 3. Tecnologías utilizadas

- .NET 8
- ASP.NET Core MVC
- ASP.NET Core Identity
- Entity Framework Core
- SQLite
- Bootstrap 5
- C#

---

## 4. Requisitos para ejecutar el proyecto

- .NET 8 SDK instalado.
- Visual Studio 2022 (o superior) con soporte para ASP.NET y desarrollo web.
- SQLite (no requiere servidor, se usan archivos `.db` dentro del proyecto).

---

## 5. Configuración de la base de datos

El proyecto usa dos bases SQLite:

- `barberia.db` → datos de la barbería (Clientes, Servicios, Citas).
- `identity.db` → datos de Identity (usuarios, roles, claims).

En `appsettings.json` (proyecto Web) se pueden ver las cadenas de conexión:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=barberia.db",
  "IdentityConnection": "Data Source=identity.db"
}
Las migraciones de BarberiaDbContext se generan en el proyecto Web y se aplican en el arranque de la aplicación con Database.Migrate().

6. Ejecución de la aplicación Web
Abrir la solución BarberiaLosHermanos.sln en Visual Studio.

Establecer BarberiaLosHermanos.Web como proyecto de inicio (Set as Startup Project).

Restaurar paquetes NuGet si es necesario.

Ejecutar el proyecto (Ctrl + F5 o botón de Run).

Al iniciar:

Se aplican las migraciones de:

BarberiaDbContext (barbería).

AppIdentityDb (Identity).

Se cargan servicios iniciales de la barbería si la tabla está vacía.

Se crean los roles Admin y Usuario si no existen.

Se crea un usuario administrador por defecto.

Usuario administrador de prueba
Usuario: admin@barberialoshermanos.com

Contraseña: Admin123!

Este usuario se agrega automáticamente al rol Admin.

Crear un usuario “normal”
Desde la aplicación web, ir a Registrarse (Register).

Completar el formulario de registro.

El usuario creado tendrá acceso a las partes protegidas solo con [Authorize], pero no podrá ejecutar acciones restringidas a [Authorize(Roles = "Admin")].

7. Funcionalidades principales (aplicación Web)
7.1. Módulo de Servicios
Listar servicios con nombre, precio, duración y estado (activo/inactivo).

Crear nuevos servicios (solo Admin).

Editar servicios existentes (solo Admin).

Eliminar servicios (solo Admin).

Activar/desactivar servicios sin eliminarlos físicamente (solo Admin).

7.2. Módulo de Clientes
Listar todos los clientes registrados.

Crear clientes nuevos (solo Admin).

Editar clientes (solo Admin).

Eliminar clientes (solo Admin).

7.3. Módulo de Citas
Listar todas las citas con información de cliente, servicio y fecha/hora.

Validar reglas de negocio al crear/editar citas:

No se permite agendar citas en el pasado.

Se respeta un rango de días permitido.

Crear citas (solo Admin).

Editar citas (solo Admin).

Cancelar (eliminar) citas (solo Admin).

8. Aplicación de consola
Para ejecutar la aplicación de consola:

Establecer BarberiaLosHermanos.Consola como proyecto de inicio.

Ejecutar desde Visual Studio (Ctrl + F5).

La consola permite:

Registrar y listar clientes.

Registrar y listar servicios.

Crear y gestionar citas en memoria.

Consultar citas por día.

Cancelar citas por Id.

Esta app no usa base de datos, sino un repositorio en memoria (AlmacenDatos).

9. Seguridad y roles
Autenticación:

Implementada con ASP.NET Core Identity.

Login y registro incorporados en el área Identity.

Autorización:

[Authorize] en los controladores para requerir usuario autenticado.

[Authorize(Roles = "Admin")] en acciones que modifican datos:

Crear/Editar/Eliminar Clientes.

Crear/Editar/Eliminar/Activar/Desactivar Servicios.

Crear/Editar/Eliminar Citas.

Con esto se diferencia claramente el rol de administrador del rol de usuario normal.

10. Estilo y diseño
Layout principal con barra de navegación:

Acceso a Home, Clientes, Servicios y Citas.

Parcial de login que muestra:

“Iniciar sesión / Registrarse” si no hay usuario autenticado.

Correo del usuario y botón de “Cerrar sesión” si está autenticado.

Estilos personalizados en wwwroot/css/site.css:

Paleta basada en tonos oscuros/rojos para la barbería.

Cards con bordes redondeados.

Botones y enlaces con estilos personalizados sobre Bootstrap.

11. Posibles mejoras futuras
Reportes de citas por día/semana/mes.

Búsqueda avanzada de clientes y citas (por nombre, fecha, servicio).

Gestión de horarios de barberos y bloqueo de espacios ocupados.

Envío de recordatorios de cita por correo electrónico.

Gestión de usuarios y roles desde una interfaz de administración dentro de la app.
