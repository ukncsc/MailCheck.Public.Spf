using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.EntityHistory.Entity
{
    public class SpfHistoryRecord
    {
        public SpfHistoryRecord(DateTime startDate, DateTime? endDate, List<string> spfRecords = null)
        {
            StartDate = startDate;
            EndDate = endDate;
            SpfRecords = spfRecords ?? new List<string>();
        }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> SpfRecords { get; set; }
    }

    public class SpfHistoryEntityState
    {
        public SpfHistoryEntityState(string id, List<SpfHistoryRecord> records = null)
        {
            Id = id;
            SpfHistory = records ?? new List<SpfHistoryRecord>();
        }

        public string Id { get; set; }
        public List<SpfHistoryRecord> SpfHistory { get; set; }

        public bool UpdateHistory(List<string> polledRecords, DateTime timeStamp)
        {
            bool hasChanged;

            SpfHistoryRecord currentRecord = SpfHistory.FirstOrDefault();

            if (currentRecord == null)
            {
                SpfHistory.Add(new SpfHistoryRecord(timeStamp, null, polledRecords));
                hasChanged = true;
            }
            else
            {
                hasChanged = !(currentRecord.SpfRecords.All(polledRecords.Contains) && polledRecords.Count == currentRecord.SpfRecords.Count);

                if (hasChanged)
                {
                    currentRecord.EndDate = timeStamp;

                    SpfHistory.Insert(0, new SpfHistoryRecord(timeStamp, null, polledRecords));
                }
            }


            return hasChanged;
        }
    }
}
