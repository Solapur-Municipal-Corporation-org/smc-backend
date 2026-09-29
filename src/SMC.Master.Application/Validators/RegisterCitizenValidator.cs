using FluentValidation;
using SMC.Master.Application.DTOs.Citizen;

namespace SMC.Master.Application.Validators;

public class RegisterCitizenValidator : AbstractValidator<RegisterCitizenDto>
{
    public RegisterCitizenValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Gender).Must(g => g is "Male" or "Female" or "Other");
        RuleFor(x => x.AadhaarNumber).Length(12).Matches("^[0-9]{12}$");
        RuleFor(x => x.MobileNumber).Matches("^[0-9]{10}$");
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}
