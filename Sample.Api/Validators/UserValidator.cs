using Sample.Shared.Validators;
using FluentValidation;

namespace Sample.API.Validators
{
    public class UserValidator : BaseValidator<User>
    {
        public UserValidator() {

            RuleFor(p => p.Name).NotEmpty()
                   .MaximumLength(50).WithMessage("{PropertyName} should  have length {MaxLength}.");
            
            RuleFor(p => p.Email).EmailAddress();
        }

    }
}
