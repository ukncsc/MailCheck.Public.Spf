using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Entity.Entity.RecordChanged
{
    public class AdvisoryMessage
    {
        public AdvisoryMessage(MessageType messageType, string text)
        {
            MessageType = messageType;
            Text = text;
        }

        public MessageType MessageType { get; }
        public string Text { get; }
    }
}
