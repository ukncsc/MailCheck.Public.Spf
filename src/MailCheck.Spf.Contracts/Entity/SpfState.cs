namespace MailCheck.Spf.Contracts.Entity
{
    public enum SpfState
    {
        Created,
        PollPending,
        EvaluationPending,
        Unchanged,
        Evaluated
    }
}