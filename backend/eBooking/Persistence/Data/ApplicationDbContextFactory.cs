using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Persistence.Data
{
    /// <summary>
    /// Design-time factory za EF Core CLI alate (npr. `dotnet ef migrations add`).
    ///
    /// Bez ovoga, `dotnet ef` ne zna direktno kako napraviti ApplicationDbContext pa pokušava
    /// podići CIJELU API aplikaciju (API/Program.cs) da bi došao do DbContext-a kroz DI. To
    /// uključuje i JWT konfiguraciju, koja NAMJERNO baca grešku ako JWT_SECRET nije postavljen u
    /// okruženju (vidi Program.cs — JWT ključ ne smije imati hardkodiran fallback po uputama za
    /// seminarski rad), pa `dotnet ef migrations add` bez ovog fajla puca sa "JWT SecretKey nije
    /// konfigurisan" iako migracija nema nikakve veze sa JWT-om.
    ///
    /// Migracijama treba samo connection string, ne cijela app konfiguracija — ovaj factory to
    /// radi direktno i EF alati ga automatski pronalaze i koriste UMJESTO pokretanja Program.cs.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Isti default kao u API/appsettings.json (LocalDB za lokalni razvoj); može se
            // override-ovati env varijablom ako je potrebno migrirati protiv drugog servera.
            var connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=170028;Trusted_Connection=true;MultipleActiveResultSets=true";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
