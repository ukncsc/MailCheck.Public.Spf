using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Rules.Record
{
    public class ModifiersOccurAfterMechanisms : IRule<DomainSpfRecord>
    {
        public Guid Id => Guid.Parse("EFEE950F-D1AB-4A3E-8E63-47DD605CC94F");

        public Task<List<Error>> Evaluate(DomainSpfRecord spfRecord)
        {
            int lastIndexOfMechanism = spfRecord.Record.Terms.FindLastIndex(_ => _ is Mechanism && IsNotImplicit(_));
            int firstIndexOfModifier = spfRecord.Record.Terms.FindIndex(_ => _ is Modifier);

            List<Error> errors = new List<Error>();
            if (lastIndexOfMechanism != -1 && firstIndexOfModifier != -1 && lastIndexOfMechanism > firstIndexOfModifier)
            {
                string errorMessage = string.Format(SpfRulesResource.ModifiersOccurAfterMechanismsErrorMessage, spfRecord.Domain);
                string markdown = string.Format(SpfRulesMarkdownResource.ModifiersOccurAfterMechanismsErrorMessage, spfRecord.Domain);

                errors.Add(new Error(Id, "mailcheck.spf.modifiersOccurAfterMechanisms", ErrorType.Error, errorMessage, markdown));
            }

            return Task.FromResult(errors);
        }

        private static bool IsNotImplicit(Term _) =>
            !(_ is OptionalDefaultMechanism) || !((OptionalDefaultMechanism)_).IsImplicit;

        public int SequenceNo => 5;

        public bool IsStopRule => false;
    }
}