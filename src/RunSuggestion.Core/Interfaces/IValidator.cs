namespace RunSuggestion.Core.Interfaces;

public interface IValidator<in T>
{
    IEnumerable<string> Validate(IEnumerable<T> collection);
}