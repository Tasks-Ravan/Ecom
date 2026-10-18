using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Common;

public static class ValidationFilter
{
    public static async Task<IResult?> ValidateAsync<T>(T request, IValidator<T> validator, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (result.IsValid) return null;
        var errors = result.Errors.GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
        return Results.ValidationProblem(errors, statusCode: StatusCodes.Status400BadRequest);
    }
}
