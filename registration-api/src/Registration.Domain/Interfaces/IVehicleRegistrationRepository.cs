using Registration.Domain.Entities;

namespace Registration.Domain.Interfaces
{
    public interface IVehicleRegistrationRepository
    {
        Task<VehicleRegistration?> FindAsync(string grantorFullName, string vin, string spgAcn);
        Task<VehicleRegistration?> FindByVinAsync(string vin);
        Task AddAsync(VehicleRegistration registration);
        Task UpdateAsync(VehicleRegistration registration);
        Task SaveChangesAsync();
    }
}