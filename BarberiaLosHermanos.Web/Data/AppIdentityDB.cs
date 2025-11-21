using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BarberiaLosHermanos.Web.Data
{
    public class AppIdentityDb : IdentityDbContext<IdentityUser>
    {
        public AppIdentityDb(DbContextOptions<AppIdentityDb> options)
            : base(options)
        {
        }
    }
}

