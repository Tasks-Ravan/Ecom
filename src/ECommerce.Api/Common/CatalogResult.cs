namespace ECommerce.Api.Common;

public sealed record CatalogResult<T>(int StatusCode, T? Value = default, string? Error = null)
{
    public bool IsSuccess => Error is null;
    public static CatalogResult<T> Created(T value) => new(StatusCodes.Status201Created, value);
    public static CatalogResult<T> Ok(T value) => new(StatusCodes.Status200OK, value);
    public static CatalogResult<T> NotFound(string message) => new(StatusCodes.Status404NotFound, default, message);
    public static CatalogResult<T> Conflict(string message) => new(StatusCodes.Status409Conflict, default, message);
}
