namespace ECommerce.Shared.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Errors = [];
    }

    private Result(IReadOnlyList<string> errors)
    {
        IsSuccess = false;
        Errors = errors;
        Error = string.Join("; ", errors);
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new([error]);
    public static Result<T> Failure(IReadOnlyList<string> errors) => new(errors);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    protected Result(bool isSuccess, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);
}
