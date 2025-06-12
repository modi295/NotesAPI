
using FluentValidation;
using NotesAPI.DTO;

namespace NotesAPI.Validator
{
    public class CreateLookupDtoValidator : AbstractValidator<CreateLookupDto>
    {
        public CreateLookupDtoValidator()
        {
            RuleFor(x => x.TypeId)
                .NotEmpty().WithMessage("TypeId is required.")
                .Length(6).WithMessage("TypeId must be exactly 6 characters.")
                .Matches("^[A-Z]{3}[0-9]{3}$").WithMessage("TypeId must be 3 uppercase letters followed by 3 digits (e.g., TYP001).");
        }
    }
}