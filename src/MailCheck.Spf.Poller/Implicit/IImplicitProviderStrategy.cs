using System.Collections.Generic;

namespace MailCheck.Spf.Poller.Implicit
{
    public interface IImplicitProviderStrategy<T>
    {
        bool TryGetImplicitTag(List<T> ts, out T t);
    }
}