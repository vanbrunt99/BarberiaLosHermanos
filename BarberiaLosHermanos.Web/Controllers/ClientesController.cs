using BarberiaLosHermanos.Dominio;
using BarberiaLosHermanos.Infraestructura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BarberiaLosHermanos.Web.Controllers
{
    // Cualquier usuario logueado puede entrar al módulo de Clientes.
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly BarberiaDbContext _db;

        // Inyección de dependencias: se recibe el DbContext.
        public ClientesController(BarberiaDbContext db)
        {
            _db = db;
        }

        // GET: /Clientes
        // Muestra la lista de todos los clientes registrados.
        public async Task<IActionResult> Index()
        {
            var clientes = await _db.Clientes.AsNoTracking().ToListAsync();
            return View(clientes);
        }

        // GET: /Clientes/Create
        // Solo Administrador puede registrar nuevos clientes.
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Clientes/Create
        // Solo Administrador puede guardar nuevos clientes.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Cliente clienteModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var nuevoCliente = new Cliente(
                        clienteModel.Nombre ?? string.Empty,
                        clienteModel.Telefono ?? string.Empty,
                        clienteModel.Correo ?? string.Empty,
                        clienteModel.Cedula ?? string.Empty
                    );

                    _db.Clientes.Add(nuevoCliente);
                    await _db.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    // Error de reglas de dominio (por ejemplo, nombre vacío).
                    ModelState.AddModelError(string.Empty, $"Error de dominio: {ex.Message}");
                }
            }

            return View(clienteModel);
        }

        // GET: /Clientes/Edit/5
        // Solo Administrador puede editar clientes.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _db.Clientes
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: /Clientes/Edit/5
        // Solo Administrador puede guardar cambios.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Telefono,Correo,Cedula")] Cliente clienteModel)
        {
            if (id != clienteModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var clienteOriginal = await _db.Clientes.FindAsync(id);
                    if (clienteOriginal == null)
                    {
                        return NotFound();
                    }

                    clienteOriginal.Nombre = clienteModel.Nombre;
                    clienteOriginal.Telefono = clienteModel.Telefono;
                    clienteOriginal.Correo = clienteModel.Correo;
                    clienteOriginal.Cedula = clienteModel.Cedula;

                    _db.Update(clienteOriginal);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(clienteModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(clienteModel);
        }

        // GET: /Clientes/Delete/5
        // Solo Administrador puede ver la confirmación de eliminación.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _db.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: /Clientes/Delete/5
        // Solo Administrador puede ejecutar la eliminación.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _db.Clientes.FindAsync(id);

            if (cliente != null)
            {
                _db.Clientes.Remove(cliente);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para verificar existencia.
        private bool ClienteExists(int id)
        {
            return _db.Clientes.Any(e => e.Id == id);
        }
    }
}
