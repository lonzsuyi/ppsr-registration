namespace Registration.Domain.ValueObjects
{
    /// <summary>
    /// Represents the duration of a vehicle registration
    /// </summary>
    public record RegistrationDuration
    {
        /// <summary>
        /// The value of the registration duration
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Private constructor to initialize the duration value
        /// </summary>
        /// <param name="value">The duration value</param>
        private RegistrationDuration(string value) => Value = value;

        /// <summary>
        /// Creates a new RegistrationDuration from a string value
        /// </summary>
        /// <param name="value">The duration value</param>
        /// <returns>A new RegistrationDuration instance</returns>
        public static RegistrationDuration From(string value) =>
            value switch
            {
                "7 years" => new("7 years"),
                "25 years" => new("25 years"),
                "N/A" => new("N/A"),
                _ => throw new ArgumentException($"Invalid duration: {value}")
            };
    }
}