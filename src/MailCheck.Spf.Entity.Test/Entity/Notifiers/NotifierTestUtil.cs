using System;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Entity.Entity.RecordChanged;
using System.Collections.Generic;
using FakeItEasy;
using MailCheck.Spf.Contracts.SharedDomain;
using System.Linq;
using Message = MailCheck.Spf.Contracts.SharedDomain.Message;
using Version = MailCheck.Spf.Contracts.SharedDomain.Version;

namespace MailCheck.Spf.Entity.Test.Entity.Notifiers
{
    public class NotifierTestUtil
    {
        public static SpfRecords CreateSpfRecords(string records = "v=spf1......", List<Message> messages = null,
            List<Term> terms = null)
        {
            return new SpfRecords(new List<SpfRecord>
                {
                    new SpfRecord(records.Split(',').ToList(),
                        new Version("1", true),
                        terms ?? new List<Term>
                        {
                            new All(Qualifier.Fail, "", false, false)
                        },
                        messages ?? new List<Message> {new Message(Guid.Empty, "mailcheck.spf.test", "SPF", MessageType.info, "hello", "testMarkdown")},
                        false)
                }, 100,
                new List<Message>());
        }

        public static void VerifyResults(IMessageDispatcher dispatcher, bool referencedChange = false,
            bool record = false, bool added = false, bool removed = false)
        {
            if (added == false && removed == false)
            {
                VerifyNoRecordMessagesDispatched(dispatcher);
                VerifyNoRecordsDispatched(dispatcher);
            }
            else
            {
                if (referencedChange)
                {
                    FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfAdvisoryAdded>._, A<string>._))
                        .MustNotHaveHappened();
                    FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfAdvisoryRemoved>._, A<string>._))
                        .MustNotHaveHappened();

                    if (added)
                    {
                        if (record)
                        {
                            FakeItEasy.A.CallTo(() =>
                                    dispatcher.Dispatch(A<SpfRecordAdded>._, A<string>._))
                                .MustHaveHappened();
                        }
                        else
                        {
                            FakeItEasy.A.CallTo(() =>
                                    dispatcher.Dispatch(A<SpfReferencedAdvisoryAdded>._, A<string>._))
                                .MustHaveHappened();
                        }
                    }

                    if (removed)
                    {
                        if (record)
                        {
                            FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfReferencedRecordRemoved>._, A<string>._))
                                .MustHaveHappened();
                        }
                        else
                        {

                            FakeItEasy.A.CallTo(() =>
                                    dispatcher.Dispatch(A<SpfReferencedAdvisoryRemoved>._, A<string>._))
                                .MustHaveHappened();
                        }
                    }
                }
                else
                {
                    FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfReferencedAdvisoryAdded>._, A<string>._))
                        .MustNotHaveHappened();
                    FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfReferencedAdvisoryRemoved>._, A<string>._))
                        .MustNotHaveHappened();

                    if (added)
                    {
                        if (record)
                        {
                            FakeItEasy.A.CallTo(() =>
                                    dispatcher.Dispatch(A<SpfRecordAdded>._, A<string>._))
                                .MustHaveHappened();
                        }
                        else
                        {
                            FakeItEasy.A.CallTo(() =>
                                    dispatcher.Dispatch(A<SpfAdvisoryAdded>._, A<string>._))
                                .MustHaveHappened();
                        }
                    }


                    if (removed)
                    {
                        if (record)
                        {
                            FakeItEasy.A.CallTo(() =>
                                    dispatcher.Dispatch(A<SpfRecordRemoved>._, A<string>._))
                                .MustHaveHappened();
                        }
                        else
                        {
                            FakeItEasy.A.CallTo(() =>
                                    dispatcher.Dispatch(A<SpfAdvisoryRemoved>._, A<string>._))
                                .MustHaveHappened();
                        }
                    }
                }
            }
        }

        private static void VerifyNoRecordsDispatched(IMessageDispatcher dispatcher)
        {
            FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfRecordAdded>._, A<string>._))
                          .MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfRecordRemoved>._, A<string>._))
            .MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfReferencedRecordAdded>._, A<string>._))
                .MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfReferencedRecordRemoved>._, A<string>._))
                .MustNotHaveHappened();
        }

        private static void VerifyNoRecordMessagesDispatched(IMessageDispatcher dispatcher)
        {
            FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfAdvisoryAdded>._, A<string>._))
                         .MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfAdvisoryRemoved>._, A<string>._))
            .MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfReferencedAdvisoryAdded>._, A<string>._))
                .MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => dispatcher.Dispatch(A<SpfReferencedAdvisoryRemoved>._, A<string>._))
                .MustNotHaveHappened();
        }
    }
}
