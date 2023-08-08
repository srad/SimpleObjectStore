namespace SimpleObjectStore.Helpers.Interfaces;

public interface IValidator<in T>
{
    bool IsValid(T t);
}