using Registration.Domain.ValueObjects;

namespace Registration.Domain.Entities
{
    /// <summary>
    /// Represents a vehicle registration record in the system
    /// </summary>
    public class VehicleRegistration
    {
        /// <summary>
        /// Unique identifier for the registration
        /// </summary>
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// First name of the grantor (vehicle owner)
        /// </summary>
        public string GrantorFirstName { get; private set; } = null!;

        /// <summary>
        /// Optional middle names of the grantor
        /// </summary>
        public string? GrantorMiddleNames { get; private set; }

        /// <summary>
        /// Last name of the grantor
        /// </summary>
        public string GrantorLastName { get; private set; } = null!;

        /// <summary>
        /// Vehicle Identification Number (VIN)
        /// </summary>
        public string VIN { get; private set; } = null!;

        /// <summary>
        /// Start date of the registration period
        /// </summary>
        public DateTime RegistrationStartDate { get; private set; }

        /// <summary>
        /// Duration of the registration period
        /// </summary>
        public RegistrationDuration Duration { get; private set; } = null!;

        /// <summary>
        /// Australian Company Number (ACN) of the Secured Party Group
        /// </summary>
        public string SpgAcn { get; private set; } = null!;

        /// <summary>
        /// Organization name of the Secured Party Group
        /// </summary>
        public string SpgOrgName { get; private set; } = null!;

        /// <summary>
        /// Creates a new vehicle registration record
        /// </summary>
        /// <param name="firstName">First name of the grantor</param>
        /// <param name="middleNames">Optional middle names of the grantor</param>
        /// <param name="lastName">Last name of the grantor</param>
        /// <param name="vin">Vehicle Identification Number</param>
        /// <param name="startDate">Start date of registration</param>
        /// <param name="duration">Duration of registration</param>
        /// <param name="spgAcn">ACN of Secured Party Group</param>
        /// <param name="spgOrgName">Organization name of Secured Party Group</param>
        /// <returns>A new VehicleRegistration instance</returns>
        public static VehicleRegistration Create(
            string firstName,
            string? middleNames,
            string lastName,
            string vin,
            DateTime startDate,
            RegistrationDuration duration,
            string spgAcn,
            string spgOrgName)
        {
            return new VehicleRegistration
            {
                GrantorFirstName = firstName,
                GrantorMiddleNames = middleNames,
                GrantorLastName = lastName,
                VIN = vin,
                RegistrationStartDate = startDate,
                Duration = duration,
                SpgAcn = spgAcn,
                SpgOrgName = spgOrgName
            };
        }

        /// <summary>
        /// Updates an existing vehicle registration record
        /// </summary>
        /// <param name="firstName">First name of the grantor</param>
        /// <param name="middleNames">Optional middle names of the grantor</param>
        /// <param name="lastName">Last name of the grantor</param>
        /// <param name="startDate">Start date of registration</param>
        /// <param name="duration">Duration of registration</param>
        /// <param name="spgAcn">ACN of Secured Party Group</param>
        /// <param name="spgOrgName">Organization name of Secured Party Group</param>
        public void Update(
            string firstName,
            string? middleNames,
            string lastName,
            DateTime startDate,
            RegistrationDuration duration,
            string spgAcn,
            string spgOrgName)
        {
            GrantorFirstName = firstName;
            GrantorMiddleNames = middleNames;
            GrantorLastName = lastName;
            RegistrationStartDate = startDate;
            Duration = duration;
            SpgAcn = spgAcn;
            SpgOrgName = spgOrgName;
        }
    }
}