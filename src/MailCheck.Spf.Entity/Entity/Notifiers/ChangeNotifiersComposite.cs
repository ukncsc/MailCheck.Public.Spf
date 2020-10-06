using System.Collections.Generic;

namespace MailCheck.Spf.Entity.Entity.Notifiers
{
    public interface IChangeNotifiersComposite : IChangeNotifier
    {

    }

    public class ChangeNotifiersComposite : IChangeNotifiersComposite
    {
        private readonly IEnumerable<IChangeNotifier> _notifiers;

        public ChangeNotifiersComposite(IEnumerable<IChangeNotifier> notifiers)
        {
            _notifiers = notifiers;
        }

        public void Handle(SpfEntityState state, Common.Messaging.Abstractions.Message message)
        {
            foreach (IChangeNotifier changeNotifier in _notifiers)
            {
                changeNotifier.Handle(state, message);
            }
        }
    }
}