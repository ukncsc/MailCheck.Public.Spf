using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Entity.Entity
{
    public interface IChangeNotifier
    {
        void Handle(SpfEntityState state, Common.Messaging.Abstractions.Message message);
    }
}