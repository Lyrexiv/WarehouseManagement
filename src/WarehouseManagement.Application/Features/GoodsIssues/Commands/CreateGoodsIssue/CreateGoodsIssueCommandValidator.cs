using FluentValidation;

namespace WarehouseManagement.Application.Features.GoodsIssues.Commands.CreateGoodsIssue;

public class CreateGoodsIssueCommandValidator
    : AbstractValidator<CreateGoodsIssueCommand>
{
    public CreateGoodsIssueCommandValidator()
    {
        RuleFor(x => x.Dto.WarehouseId)
            .NotEmpty()
            .WithMessage("Kho không được để trống.");

        RuleFor(x => x.Dto.ExportDate)
            .NotEmpty()
            .WithMessage("Ngày xuất không được để trống.");

        RuleFor(x => x.Dto.ReceiverName)
            .NotEmpty()
            .WithMessage("Tên người nhận không được để trống.")
            .MaximumLength(100)
            .WithMessage("Tên người nhận không được vượt quá 100 ký tự.");

        RuleFor(x => x.Dto.Details)
            .NotEmpty()
            .WithMessage("Cần ít nhất 1 sản phẩm.");

        RuleForEach(x => x.Dto.Details)
            .ChildRules(d =>
            {
                d.RuleFor(x => x.ProductId)
                    .NotEmpty()
                    .WithMessage("Sản phẩm không được để trống.");

                d.RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Số lượng phải lớn hơn 0.");

                d.RuleFor(x => x.UnitPrice)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Đơn giá không được âm.");
            });
    }
}