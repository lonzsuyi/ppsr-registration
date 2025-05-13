namespace Registration.Infrastructure.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Registration.Domain.Entities;
    using Registration.Domain.Interfaces;
    using Registration.Infrastructure.Persistence;

    /// <summary>
    /// Repository for managing vehicle registration records
    /// </summary>  
    public class VehicleRegistrationRepository(ApplicationDbContext _context, ILogger<VehicleRegistrationRepository> _logger) : IVehicleRegistrationRepository
        {
        /// <summary>
        /// Finds a vehicle registration record by grantor full name, VIN, and SPG ACN
        /// </summary>
        /// <param name="grantorFullName">The full name of the grantor</param>
        /// <param name="vin">The VIN of the vehicle</param>
        /// <param name="spgAcn">The SPG ACN of the Secured Party Group</param>
        public async Task<VehicleRegistration?> FindAsync(string grantorFullName, string vin, string spgAcn)
        {
            var entity = await _context.VehicleRegistrations.FirstOrDefaultAsync(v =>
                v.VIN == vin &&
                v.SpgAcn == spgAcn &&
                ($"{v.GrantorFirstName} {v.GrantorMiddleNames} {v.GrantorLastName}".Trim() == grantorFullName));

            _logger.LogDebug("Find: VIN={VIN}, SPG={SPGACN}, Grantor={Grantor} → Found={Found}", vin, spgAcn, grantorFullName, entity != null);
            return entity;
        }


        /// <summary>   
        /// Finds a vehicle registration record by VIN
        /// </summary>
        /// <param name="vin">The VIN of the vehicle</param>
        public async Task<VehicleRegistration?> FindByVinAsync(string vin)
        {
            var entity = await _context.VehicleRegistrations.FirstOrDefaultAsync(v => v.VIN == vin);
            _logger.LogDebug("Find: VIN={VIN} → Found={Found}", vin, entity != null);
            return entity;
        }

        /// <summary>
        /// Adds a new vehicle registration record to the database
        /// </summary>
        /// <param name="registration">The vehicle registration record to add</param>
        public async Task AddAsync(VehicleRegistration registration)
        {
            _logger.LogDebug("Add registration: VIN={VIN}", registration.VIN);
            await _context.VehicleRegistrations.AddAsync(registration);
        }

        /// <summary>
        /// Updates an existing vehicle registration record in the database
        /// </summary>
        /// <param name="registration">The vehicle registration record to update</param>
        public async Task UpdateAsync(VehicleRegistration registration)
        {
            _logger.LogDebug("Update registration: VIN={VIN}", registration.VIN);
            _context.VehicleRegistrations.Update(registration);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Saves all changes to the database
        /// </summary>
        public async Task SaveChangesAsync()
        {
            _logger.LogDebug("SaveChanges called");
            await _context.SaveChangesAsync();
            _logger.LogDebug("SaveChanges completed");
        }
    }
}