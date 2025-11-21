using System.Linq;
using System.Threading.Tasks;
using BarberiaLosHermanos.Dominio;
using BarberiaLosHermanos.Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarberiaLosHermanos.Web.Controllers
{
    // Cualquier usuario autenticado puede ver la lista de servicios
    [Authorize]
    public class ServiciosController : Controller
    {
        private readonly BarberiaDbContext _context;

        public ServiciosController(BarberiaDbContext context)
        {
            _context = context;
        }

        // GET: Servicios
        // Lista de servicios ordenados por nombre
        public async Task<IActionResult> Index()
        {
            var servicios = await _context.Servicios
                .AsNoTracking()
                .OrderBy(s => s.Nombre)
                .ToListAsync();

            return View(servicios);
        }

        // GET: Servicios/Details/5
        // Solo Admin puede ver detalles completos de un servicio
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.Servicios
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (servicio == null)
            {
                return NotFound();
            }

            return View(servicio);
        }

        // GET: Servicios/Create
        // Solo Admin puede crear nuevos servicios
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Servicios/Create
        // Solo Admin puede crear nuevos servicios
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Nombre,Precio,Duracion,EstaActivo")] Servicio servicio)
        {
            if (!ModelState.IsValid)
            {
                return View(servicio);
            }

            // Aseguramos que por defecto el servicio quede activo
            if (!servicio.EstaActivo)
            {
                servicio.EstaActivo = true;
            }

            _context.Add(servicio);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Servicio creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Servicios/Edit/5
        // Solo Admin puede editar servicios
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null)
            {
                return NotFound();
            }

            return View(servicio);
        }

        // POST: Servicios/Edit/5
        // Solo Admin puede editar servicios
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Precio,Duracion,EstaActivo")] Servicio servicio)
        {
            if (id != servicio.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(servicio);
            }

            try
            {
                _context.Update(servicio);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Servicio actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                var existe = await _context.Servicios.AnyAsync(e => e.Id == servicio.Id);
                if (!existe)
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Servicios/Delete/5
        // Solo Admin puede ver la pantalla de confirmación de borrado
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.Servicios
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (servicio == null)
            {
                return NotFound();
            }

            return View(servicio);
        }

        // POST: Servicios/Delete/5
        // Solo Admin puede eliminar servicios
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio != null)
            {
                _context.Servicios.Remove(servicio);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Servicio eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Servicios/ToggleActivo
        // Solo Admin puede activar/desactivar un servicio
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null)
            {
                return NotFound();
            }

            servicio.EstaActivo = !servicio.EstaActivo;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Estado del servicio actualizado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
