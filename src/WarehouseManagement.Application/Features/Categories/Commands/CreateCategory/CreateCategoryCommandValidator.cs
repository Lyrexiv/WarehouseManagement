using FluentValidation;

namespace WarehouseManagement.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator
    : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Dto.Code)
            .MaximumLength(20);
    }
}