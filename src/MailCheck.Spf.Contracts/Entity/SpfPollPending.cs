using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Spf.Contracts.Entity
{
    public class SpfPollPending : Message
    {
        public SpfPollPending(string id) 
            : base(id){}

        public SpfState State => SpfState.PollPending;
    }
}