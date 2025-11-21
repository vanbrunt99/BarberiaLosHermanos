using System.Linq;
using System.Threading.Tasks;
using BarberiaLosHermanos.Dominio;
using BarberiaLosHermanos.Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BarberiaLosHermanos.Web.Controllers
{
    // Cualquier usuario autenticado puede acceder al módulo de Citas.
    [Authorize]
    public class CitasController : Controller
    {
        private readonly BarberiaDbContext _context;

        public CitasController(BarberiaDbContext context)
        {
            _context = context;
        }

        // GET: Citas
        // Lista todas las citas ordenadas por fecha y hora.
        public async Task<IActionResult> Index()
        {
            var citas = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .AsNoTracking()
                .OrderBy(c => c.FechaHora)
                .ToListAsync();

            return View(citas);
        }

        // GET: Citas/Details/5
        // Muestra el detalle de una cita.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cita = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cita == null) return NotFound();

            return View(cita);
        }

        // GET: Citas/Create
        // Solo Admin puede ver el formulario para agendar cita.
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            CargarCombosClientesYServicios();
            return View();
        }

        // POST: Citas/Create
        // Solo Admin puede crear nuevas citas.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ClienteId,ServicioId,FechaHora")] Cita cita)
        {
            if (!ModelState.IsValid)
            {
                CargarCombosClientesYServicios(cita.ClienteId, cita.ServicioId);
                return View(cita);
            }

            try
            {
                // Reglas de negocio: fecha no en pasado / dentro de ventana permitida.
                ValidadorCitas.ValidarFecha(cita.FechaHora, DateTime.Now);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                CargarCombosClientesYServicios(cita.ClienteId, cita.ServicioId);
                return View(cita);
            }

            _context.Add(cita);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Citas/Edit/5
        // Solo Admin puede editar citas.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            CargarCombosClientesYServicios(cita.ClienteId, cita.ServicioId);
            return View(cita);
        }

        // POST: Citas/Edit/5
        // Solo Admin puede guardar cambios de una cita.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,ServicioId,FechaHora")] Cita cita)
        {
            if (id != cita.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                CargarCombosClientesYServicios(cita.ClienteId, cita.ServicioId);
                return View(cita);
            }

            try
            {
                ValidadorCitas.ValidarFecha(cita.FechaHora, DateTime.Now);
                _context.Update(cita);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Citas.Any(e => e.Id == cita.Id))
                    return NotFound();

                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                CargarCombosClientesYServicios(cita.ClienteId, cita.ServicioId);
                return View(cita);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Citas/Delete/5
        // Solo Admin ve la confirmación para cancelar/eliminar cita.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cita = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cita == null) return NotFound();

            return View(cita);
        }

        // POST: Citas/Delete/5
        // Solo Admin puede eliminar la cita.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita != null)
            {
                _context.Citas.Remove(cita);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Carga listas para los combos de Cliente y Servicio.
        private void CargarCombosClientesYServicios(int? clienteIdSeleccionado = null, int? servicioIdSeleccionado = null)
        {
            ViewData["ClienteId"] = new SelectList(
                _context.Clientes.OrderBy(c => c.Nombre),
                "Id",
                "Nombre",
                clienteIdSeleccionado
            );

            ViewData["ServicioId"] = new SelectList(
                _context.Servicios.OrderBy(s => s.Nombre),
                "Id",
                "Nombre",
                servicioIdSeleccionado
            );
        }
    }
}
