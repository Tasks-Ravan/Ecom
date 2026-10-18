using ECommerce.Api.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Features.Categories;

public sealed class CategoryValidator : AbstractValidator<CategoryRequest>
{
    public CategoryValidator(AppDbContext db)
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        RuleFor(request => request.ParentCategoryId).MustAsync(async (id, cancellationToken) =>
            id is null || await db.Categories.AnyAsync(category => category.Id == id, cancellationToken))
            .WithMessage("Parent category does not exist.");
    }
}
