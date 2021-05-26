namespace MailCheck.Spf.Poller.Expansion
{
    public class SpfMacroExpander
    {
        public bool IsMacro(string argument)
        {
            if (argument.StartsWith('%'))
            {
                return true;
            }

            return false;
        }
    }
}
