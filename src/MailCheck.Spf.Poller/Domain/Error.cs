using System;

namespace MailCheck.Spf.Poller.Domain
{
    public class Error
    {
        public Error(Guid id, string name, ErrorType errorType, string message, string markdown)
        {
            Id = id;
            Name = name;
            ErrorType = errorType;
            Message = message;
            Markdown = markdown;
        }

        public Guid Id { get; }
        public ErrorType ErrorType { get; }

        public string Message { get; }
        public string Markdown { get; }
        public string Name { get; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(ErrorType)}: {ErrorType}, {nameof(Message)}: {Message}";
        }
    }
}
