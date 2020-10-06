using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Rules.Record
{
    [TestFixture]
    public class AllMustBeLastMechanismTests
    {
        private AllMustBeLastMechanism _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new AllMustBeLastMechanism();
        }

        [Test]
        public async Task NoAllMechanismNoError()
        {
            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), new List<Term>()));

            List<Error> errors = await _rule.Evaluate(spfRecord);

            Assert.That(!errors.Any());
        }

        [Test]
        public async Task AllIsLastMechanismNoError()
        {
            All all = new All(string.Empty, Qualifier.Fail);
            Include include = new Include(string.Empty, Qualifier.Pass, new DomainSpec(string.Empty));
            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(""), new List<Term> { include, all }));

            List<Error> errors = await _rule.Evaluate(spfRecord);

            Assert.That(!errors.Any());
        }

        [Test]
        public async Task AllIsNotLastMechanismError()
        {
            Include include = new Include(string.Empty, Qualifier.Pass, new DomainSpec(string.Empty));
            All all = new All(string.Empty, Qualifier.Fail);
            DomainSpfRecord spfRecord = new DomainSpfRecord("domain", new SpfRecord(new List<string>(), new Version(""), new List<Term> { all, include }));

            List<Error> errors = await _rule.Evaluate(spfRecord);

            Assert.That(errors.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task RedirectWithNoAllShouldHaveNoError()
        {
            Include include = new Include(string.Empty, Qualifier.Pass, new DomainSpec(string.Empty));
            All all = new All(string.Empty, Qualifier.Fail);
            Redirect redirect = new Redirect("ncsc.gov.uk", new DomainSpec("ncsc.gov.uk"));
            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(""), new List<Term> { all, include, redirect }));

            List<Error> errors = await _rule.Evaluate(spfRecord);

            Assert.That(!errors.Any());
        }
    }
}
