using System;
using System.Threading.Tasks;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Evaluator.Explainers;
using MailCheck.Spf.Evaluator.Rules;

namespace MailCheck.Spf.Evaluator
{
    public interface ISpfEvaluationProcessor
    {
        Task Process(SpfRecords spfRecords);
    }

    public class SpfEvaluationProcessor : ISpfEvaluationProcessor
    {
        private readonly IEvaluator<SpfRecord> _evaluator;
        private readonly ISpfRecordExplainer _recordExplainer;
        private readonly ISpfRecordsJobsProcessor _spfRecordsJobsProcessor;

        public SpfEvaluationProcessor(IEvaluator<SpfRecord> evaluator, 
           ISpfRecordExplainer recordExplainer, 
           ISpfRecordsJobsProcessor spfRecordsJobsProcessor)
        {
            _evaluator = evaluator;
            _recordExplainer = recordExplainer;
            _spfRecordsJobsProcessor = spfRecordsJobsProcessor;
        }

        public async Task Process(SpfRecords spfRecords)
        {
            async Task EvaluationAdaptor(SpfRecords records)
            {
                foreach (SpfRecord spfRecord in records.Records)
                {
                    EvaluationResult<SpfRecord> evaluationResult = await _evaluator.Evaluate(spfRecord);
                    spfRecord.Messages.AddRange(evaluationResult.Errors);
                }
            }

            Task ExplanationAdaptor(SpfRecords records)
            {
                foreach (SpfRecord spfRecord in records.Records)
                {
                    _recordExplainer.Explain(spfRecord);
                }

                return Task.CompletedTask;
            }

            await _spfRecordsJobsProcessor.Process(spfRecords, 
                EvaluationAdaptor, 
                ExplanationAdaptor);
        }
    }
}