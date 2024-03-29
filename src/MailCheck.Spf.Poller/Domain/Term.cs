using System;

namespace MailCheck.Spf.Poller.Domain
{
    public abstract class Term : SpfEntity
    {
        protected Term(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            string errorString = string.Join(Environment.NewLine, AllErrors);

            return $"{nameof(Value)}: {Value}{Environment.NewLine}" +
                   $"{(AllValid ? "Valid" : $"Invalid{Environment.NewLine}{errorString}")}";
        }
    }
}