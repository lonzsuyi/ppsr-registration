namespace Registration.Application.Validation.Interfaces
{
    public interface IFieldValidator<T>
    {
        T Validate(IDictionary<string, string?> row);
    }
}