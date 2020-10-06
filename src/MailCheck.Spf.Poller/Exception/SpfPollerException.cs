using System;
using System.Collections.Generic;
using System.Text;

namespace MailCheck.Spf.Poller.Exception
{
    public class SpfPollerException : System.Exception
    {
        public SpfPollerException()
        {
        }

        public SpfPollerException(string formatString, params object[] values)
            : base(string.Format(formatString, values))
        {
        }

        public SpfPollerException(string message)
            : base(message)
        {
        }
    }
}
