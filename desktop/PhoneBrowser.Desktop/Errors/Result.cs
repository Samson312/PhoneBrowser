namespace PhoneBrowser.Desktop.Errors;

public readonly record struct Result<TValue, TError> where TError : struct, Enum
{
    public bool IsSuccess { get; }
    public TValue? Value { get; }
    public TError? Error { get; }

    private Result(bool isSuccess, TValue? value, TError? error)
        => (IsSuccess, Value, Error) = (isSuccess, value, error);

    public static Result<TValue, TError> Ok(TValue value) => new(true, value, null);
    public static Result<TValue, TError> Fail(TError error) => new(false, default, error);
}

