using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.Poller.Domain
{
    public class SpfRecord : SpfEntity
    {
        public SpfRecord(List<string> recordStrings, Version version = null, List<Term> terms = null)
        {
            RecordStrings = recordStrings;
            Record = string.Join(string.Empty, RecordStrings);
            Version = version;
            Terms = terms;
        }

        public List<string> RecordStrings { get; }

        public string Record { get; }

        public Version Version { get; }

        public List<Term> Terms { get; }

        public override int AllErrorCount => Version.AllErrorCount + Terms.Sum(_ => _.AllErrorCount) + ErrorCount;

        public override IReadOnlyList<Error> AllErrors => Version.AllErrors.Concat(Terms.SelectMany(_ => _.AllErrors)).Concat(Errors).ToArray();

        public override string ToString()
        {
            return $"{nameof(RecordStrings)}: {RecordStrings}{Environment.NewLine}" +
                   $"{nameof(Version)}: {Version}{Environment.NewLine}" +
                   $"{nameof(Terms)}:{Environment.NewLine}" +
                   $"{string.Join(Environment.NewLine, Terms)}{Environment.NewLine}" +
                   $"{(AllValid ? "Valid" : $"Invalid{Environment.NewLine}{string.Join(Environment.NewLine, Errors)}")}";
        }
    }
}
