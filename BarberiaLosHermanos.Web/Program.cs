using BarberiaLosHermanos.Dominio;
using BarberiaLosHermanos.Infraestructura;
using BarberiaLosHermanos.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// TimeProvider para .NET 8.
// Se registra un proveedor de tiempo centralizado para poder inyectarlo donde se necesite.
builder.Services.AddSingleton<TimeProvider>(_ => TimeProvider.System);

// Registramos MVC (controladores con vistas) y Razor Pages.
// MVC se usa para la app principal de la barbería.
// Razor Pages se usa principalmente para las páginas de Identity (Login, Register, etc.).
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// =========================
// DbContext de la Barbería
// =========================
// DbContext principal que maneja Clientes, Servicios y Citas, usando SQLite.
// Se indica que las migraciones de este contexto se generan en el proyecto Web.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BarberiaDbContext>(options =>
    options.UseSqlite(
        connectionString,
        b => b.MigrationsAssembly("BarberiaLosHermanos.Web") // migraciones en el proyecto Web
    ));

// =========================
// DbContext de Identity
// =========================
// DbContext separado para Identity (usuarios, roles, claims) usando otra base SQLite.
builder.Services.AddDbContext<AppIdentityDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("IdentityConnection")));

// =========================
// Configuración de Identity
// =========================
// Se configura Identity para manejar usuarios y roles.
// - No se requiere confirmación de correo para iniciar sesión.
// - Se agregan roles (Admin, Usuario).
// - Se indica que Identity usa AppIdentityDb como almacenamiento.
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppIdentityDb>();

var app = builder.Build();

// =====================================================
// Seed de bases de datos: Barbería + Roles + Usuario Admin
// =====================================================
// Se crea un scope manualmente para resolver los DbContext y servicios necesarios.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // ---------------------------------------------
    // Base de datos principal (barbería: citas, etc.)
    // ---------------------------------------------
    var db = services.GetRequiredService<BarberiaDbContext>();

    // En lugar de EnsureCreated se usan migraciones.
    // Esto aplica cualquier migración pendiente al iniciar la aplicación.
    db.Database.Migrate();

    // Seed de servicios iniciales si la tabla está vacía.
    if (!db.Servicios.Any())
    {
        db.Servicios.AddRange(
            new Servicio("Corte Caballero", 3500m),
            new Servicio("Afeitado Clásico", 2500m),
            new Servicio("Corte + Barba", 6000m),
            new Servicio("Perfilado de Cejas", 2000m),
            new Servicio("Arreglo de Barba y Bigote", 3000m),
            new Servicio("Corte Niño", 3000m),
            new Servicio("Mascarilla Facial Detox", 4000m),
            new Servicio("Delineado de Contornos", 2500m)
        );

        db.SaveChanges();
    }

    // ---------------------------------------------
    // Base de datos de Identity (usuarios y roles)
    // ---------------------------------------------
    var identityDb = services.GetRequiredService<AppIdentityDb>();

    // Aplica migraciones de Identity (aspnetusers, aspnetroles, etc.).
    identityDb.Database.Migrate();

    // ---------------------------------------------
    // Creación de roles y usuario administrador
    // ---------------------------------------------
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // Se crean los roles "Admin" y "Usuario" si aún no existen.
    var roles = new[] { "Admin", "Usuario" };
    foreach (var role in roles)
    {
        if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
        }
    }

    // Usuario administrador por defecto.
    var adminEmail = "admin@barberialoshermanos.com";
    var adminPassword = "Admin123!";

    var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
    if (adminUser == null)
    {
        // Se crea el usuario admin si no existe.
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
        if (result.Succeeded)
        {
            // Se asigna el rol Admin al usuario creado.
            userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
        }
        // En un proyecto real aquí se podría registrar/loguear el error si falla.
    }
    else
    {
        // Si el usuario ya existe, se asegura que tenga el rol Admin.
        if (!userManager.IsInRoleAsync(adminUser, "Admin").GetAwaiter().GetResult())
        {
            userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
        }
    }
}

// =================
// Pipeline HTTP
// =================
// Configuración del manejo de errores y seguridad en producción.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware para redirección a HTTPS y servir archivos estáticos (CSS, JS, imágenes).
app.UseHttpsRedirection();
app.UseStaticFiles();

// Habilita el enrutamiento de la aplicación.
app.UseRouting();

// Habilita autenticación y autorización.
app.UseAuthentication();
app.UseAuthorization();

// Ruta por defecto de la aplicación MVC.
// Si no se especifica controlador/acción, usa Home/Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapea las Razor Pages, en especial las de Identity (Login, Register, etc.).
app.MapRazorPages();

// Inicia la aplicación web.
app.Run();
