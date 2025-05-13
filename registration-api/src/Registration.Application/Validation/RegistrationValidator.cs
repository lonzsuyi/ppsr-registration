using Registration.Application.Validation.Interfaces;
using Registration.Domain.ValueObjects;

namespace Registration.Application.Validation
{
    public record RegistrationValidationResult(
        string FirstName,
        string? MiddleNames,
        string LastName,
        string Vin,
        DateTime StartDate,
        RegistrationDuration Duration,
        string SpgAcn,
        string SpgOrgName);

    /// <summary>
    /// Represents the result of validating a vehicle registration record
    /// </summary>
    /// <param name="FirstName">The first name of the grantor</param>
    /// <param name="MiddleNames">Optional middle names of the grantor</param>
    /// <param name="LastName">The last name of the grantor</param>
    /// <param name="Vin">The Vehicle Identification Number</param>
    /// <param name="StartDate">The start date of the registration</param>
    /// <param name="Duration">The duration of the registration</param>
    /// <param name="SpgAcn">The ACN of the Secured Party Group</param>
    /// <param name="SpgOrgName">The organization name of the Secured Party Group</param>

    public class RegistrationRowValidator
    {
        private readonly Dictionary<string, IFieldValidator<object>> _validators = new()
        {
            ["Grantor First Name"] = new RequiredStringValidator("Grantor First Name", 35),
            ["Grantor Middle Names"] = new OptionalStringValidator(75),
            ["Grantor Last Name"] = new RequiredStringValidator("Grantor Last Name", 35),
            ["VIN"] = new VinValidator(),
            ["Registration start date"] = new DateValidator(),
            ["Registration duration"] = new DurationValidator(),
            ["SPG ACN"] = new AcnValidator(),
            ["SPG Organization Name"] = new RequiredStringValidator("SPG Organization Name", 75),
        };

        public RegistrationValidationResult Validate(IDictionary<string, string?> row)
        {
            string firstName = (string)_validators["Grantor First Name"].Validate(row);
            string? middleNames = (string?)_validators["Grantor Middle Names"].Validate(row);
            string lastName = (string)_validators["Grantor Last Name"].Validate(row);
            string vin = (string)_validators["VIN"].Validate(row);
            DateTime startDate = (DateTime)_validators["Registration start date"].Validate(row);
            RegistrationDuration duration = (RegistrationDuration)_validators["Registration duration"].Validate(row);
            string spgAcn = (string)_validators["SPG ACN"].Validate(row);
            string spgOrg = (string)_validators["SPG Organization Name"].Validate(row);

            return new RegistrationValidationResult(firstName, middleNames, lastName, vin, startDate, duration, spgAcn, spgOrg);
        }
    }

    public class RequiredStringValidator : IFieldValidator<object>
    {
        private readonly string _fieldName;
        private readonly int _maxLength;

        public RequiredStringValidator(string fieldName, int maxLength)
        {
            _fieldName = fieldName;
            _maxLength = maxLength;
        }

        public object Validate(IDictionary<string, string?> row)
        {
            var value = row[_fieldName]?.Trim();
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{_fieldName} is required.");
            if (value.Length > _maxLength) throw new ArgumentException($"{_fieldName} exceeds {_maxLength} characters.");
            return value;
        }
    }

    public class OptionalStringValidator : IFieldValidator<object>
    {
        private readonly int _maxLength;
        public OptionalStringValidator(int maxLength) => _maxLength = maxLength;

        public object Validate(IDictionary<string, string?> row)
        {
            var value = row["Grantor Middle Names"]?.Trim();
            if (value?.Length > _maxLength) throw new ArgumentException("Grantor Middle Names exceeds allowed length.");
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }
    }

    public class VinValidator : IFieldValidator<object>
    {
        public object Validate(IDictionary<string, string?> row)
        {
            var value = row["VIN"]?.Trim();
            if (string.IsNullOrWhiteSpace(value) || value.Length != 17) throw new ArgumentException("VIN must be 17 characters.");
            return value;
        }
    }

    public class DateValidator : IFieldValidator<object>
    {
        public object Validate(IDictionary<string, string?> row)
        {
            var value = row["Registration start date"]?.Trim();
            return DateTime.TryParse(value, out var result)
                ? result
                : throw new ArgumentException("Invalid date.");
        }
    }

    public class DurationValidator : IFieldValidator<object>
    {
        public object Validate(IDictionary<string, string?> row)
        {
            var value = row["Registration duration"]?.Trim();
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Duration required.");
            if (value == "N/A") return RegistrationDuration.From("N/A");
            if (int.TryParse(value, out int years) && (years == 7 || years == 25))
                return RegistrationDuration.From($"{years} years");
            throw new ArgumentException("Invalid duration.");
        }
    }

    public class AcnValidator : IFieldValidator<object>
    {
        public object Validate(IDictionary<string, string?> row)
        {
            var value = row["SPG ACN"]?.Replace(" ", "").Trim();
            if (string.IsNullOrWhiteSpace(value) || value.Length != 9 || !value.All(char.IsDigit))
                throw new ArgumentException("SPG ACN must be 9 digits.");
            return value;
        }
    }
}