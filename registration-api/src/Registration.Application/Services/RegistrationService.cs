namespace Registration.Application.Services
{
    using Microsoft.Extensions.Logging;
    using Registration.Domain.Dtos;
    using Registration.Domain.Entities;
    using Registration.Domain.Interfaces;
    using CsvHelper;
    using CsvHelper.Configuration;
    using System.Globalization;
    using System.Security.Cryptography;
    using Registration.Application.Validation;
    using Registration.Application.Exceptions;

    public class RegistrationService(
        IUploadedFileRepository _uploadedFileRepository,
        IVehicleRegistrationRepository _vehicleRegistrationRepository,
    ILogger<RegistrationService> _logger) : IRegistrationService
    {
        /// <summary>
        /// Service for processing vehicle registration uploads
        /// </summary>
        /// <param name="_repository">Repository for vehicle registration data</param>
        /// <param name="_logger">Logger for recording processing events</param>
        public async Task<UploadSummaryDto> ProcessUploadAsync(Stream csvStream)
        {
            var summary = new UploadSummaryDto();
            var validator = new RegistrationRowValidator();

            // Check for duplicate file (hash based)
            string hash;
            using (var sha256 = SHA256.Create())
            {
                csvStream.Position = 0;
                hash = Convert.ToBase64String(sha256.ComputeHash(csvStream));
                csvStream.Position = 0;
            }
            if (await _uploadedFileRepository.ExistsByHashAsync(hash))
            {
                _logger.LogWarning("Duplicate file submission detected");
                throw new ConflictException("File has already been submitted previously");
            }
            await _uploadedFileRepository.AddAsync(hash);

            // Read the CSV file
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim(),
                TrimOptions = TrimOptions.Trim,
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var records = csv.GetRecords<dynamic>().ToList();

            // Check if the CSV file is empty
            if (records.Count == 0)
            {
                _logger.LogWarning("CSV file is empty");
                throw new InvalidDataException("CSV must contain at least one data row.");
            }

            summary.SubmittedRecords = records.Count;

            foreach (var row in records)
            {
                try
                {
                    var dict = ((IDictionary<string, object>)row)
                    .ToDictionary(k => k.Key, v => v.Value?.ToString());

                    var result = validator.Validate(dict);
                    var fullName = $"{result.FirstName} {result.MiddleNames} {result.LastName}".Trim();
                    _logger.LogInformation("Processing record: {FullName} - {VIN}", fullName, result.Vin);

                    var existing = await _vehicleRegistrationRepository.FindAsync(fullName, result.Vin, result.SpgAcn);

                    if (existing is not null)
                    {
                        // Update
                        _logger.LogInformation("Updating existing registration for VIN={VIN}", result.Vin);
                        existing.Update(
                            result.FirstName,
                            result.MiddleNames,
                            result.LastName,
                            result.StartDate,
                            result.Duration,
                            result.SpgAcn,
                            result.SpgOrgName);

                        summary.UpdatedRecords++;
                    }
                    else
                    {
                        // Check: VIN must not exist under a different owner
                        var sameVin = await _vehicleRegistrationRepository.FindByVinAsync(result.Vin);
                        if (sameVin is not null &&
                            $"{sameVin.GrantorFirstName} {sameVin.GrantorMiddleNames} {sameVin.GrantorLastName}".Trim() != fullName)
                        {
                            throw new InvalidOperationException($"VIN {result.Vin} already registered to another grantor");
                        }

                        _logger.LogInformation("Adding new registration for VIN={VIN}", result.Vin);
                        var entity = VehicleRegistration.Create(
                            result.FirstName,
                            result.MiddleNames,
                            result.LastName,
                            result.Vin,
                            result.StartDate,
                            result.Duration,
                            result.SpgAcn,
                            result.SpgOrgName);
                        await _vehicleRegistrationRepository.AddAsync(entity);
                        summary.AddedRecords++;
                    }

                    summary.ProcessedRecords++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping invalid record #{Index}", summary.SubmittedRecords);
                    summary.InvalidRecords++;
                }
            }

            await _vehicleRegistrationRepository.SaveChangesAsync();
            _logger.LogInformation("Upload Summary: {@Summary}", summary);
            return summary;
        }

        /// <summary>
        /// Service for processing vehicle registration uploads
        /// </summary>
        /// <param name="_repository">Repository for vehicle registration data</param>
        /// <param name="_logger">Logger for recording processing events</param>
        public async Task<UploadSummaryDto> ProcessUploadBigFileAsync(Stream csvStream)
        {
            var summary = new UploadSummaryDto();
            var validator = new RegistrationRowValidator();

            // Read the CSV file
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim(),
                TrimOptions = TrimOptions.Trim,
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var records = csv.GetRecords<dynamic>().ToList();

            // Check if the CSV file is empty
            if (records.Count == 0)
            {
                _logger.LogWarning("CSV file is empty");
                throw new InvalidDataException("CSV must contain at least one data row.");
            }

            summary.SubmittedRecords = records.Count;

            foreach (var row in records)
            {
                try
                {
                    var dict = ((IDictionary<string, object>)row)
                    .ToDictionary(k => k.Key, v => v.Value?.ToString());

                    var result = validator.Validate(dict);
                    var fullName = $"{result.FirstName} {result.MiddleNames} {result.LastName}".Trim();
                    _logger.LogInformation("Processing record: {FullName} - {VIN}", fullName, result.Vin);

                    var existing = await _vehicleRegistrationRepository.FindAsync(fullName, result.Vin, result.SpgAcn);

                    if (existing is not null)
                    {
                        // Update
                        _logger.LogInformation("Updating existing registration for VIN={VIN}", result.Vin);
                        existing.Update(
                            result.FirstName,
                            result.MiddleNames,
                            result.LastName,
                            result.StartDate,
                            result.Duration,
                            result.SpgAcn,
                            result.SpgOrgName);

                        summary.UpdatedRecords++;
                    }
                    else
                    {
                        // Check: VIN must not exist under a different owner
                        var sameVin = await _vehicleRegistrationRepository.FindByVinAsync(result.Vin);
                        if (sameVin is not null &&
                            $"{sameVin.GrantorFirstName} {sameVin.GrantorMiddleNames} {sameVin.GrantorLastName}".Trim() != fullName)
                        {
                            throw new InvalidOperationException($"VIN {result.Vin} already registered to another grantor");
                        }

                        _logger.LogInformation("Adding new registration for VIN={VIN}", result.Vin);
                        var entity = VehicleRegistration.Create(
                            result.FirstName,
                            result.MiddleNames,
                            result.LastName,
                            result.Vin,
                            result.StartDate,
                            result.Duration,
                            result.SpgAcn,
                            result.SpgOrgName);
                        await _vehicleRegistrationRepository.AddAsync(entity);
                        summary.AddedRecords++;
                    }

                    summary.ProcessedRecords++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping invalid record #{Index}", summary.SubmittedRecords);
                    summary.InvalidRecords++;
                }
            }

            await _vehicleRegistrationRepository.SaveChangesAsync();
            _logger.LogInformation("Upload Summary: {@Summary}", summary);
            return summary;
        }
    }
}
