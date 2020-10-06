using FluentValidation;
using MailCheck.Common.Util;
using MailCheck.Spf.Api.Domain;

namespace MailCheck.Spf.Api.Validation
{
    public class SpfDomainRequestValidator : AbstractValidator<SpfDomainRequest>
    {
        public SpfDomainRequestValidator(IDomainValidator domainValidator)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(_ => _.Domain)
                .NotNull()
                .WithMessage("A \"domain\" field is required.")
                .NotEmpty()
                .WithMessage("The \"domain\" field should not be empty.")
                .Must(domainValidator.IsValidDomain)
                .WithMessage("The domains must be be a valid domain");
        }
    }
}
