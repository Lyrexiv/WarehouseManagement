using FluentValidation;

namespace WarehouseManagement.Application.Features.GoodsReceipts;

public class CreateGoodsReceiptCommandValidator
    : AbstractValidator<CreateGoodsReceiptCommand>
{
    public CreateGoodsReceiptCommandValidator()
    {
        RuleFor(x => x.Dto.WarehouseId)
            .NotEmpty();

        RuleFor(x => x.Dto.SupplierId)
            .NotEmpty();

        RuleFor(x => x.Dto.ImportDate)
            .NotEmpty();

        RuleFor(x => x.Dto.Details)
            .NotEmpty()
            .WithMessage("Cần ít nhất 1 sản phẩm");

        RuleForEach(x => x.Dto.Details)
            .ChildRules(d =>
            {
                d.RuleFor(x => x.ProductId)
                    .NotEmpty();

                d.RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Số lượng phải lớn hơn 0");

                d.RuleFor(x => x.UnitPrice)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Đơn giá không được âm");
            });
    }
}