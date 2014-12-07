using System;
using System.Diagnostics;
using System.Linq;
using DuckyDocs.ExternalVisibility;
using NUnit.Framework;

namespace DuckyDocs.CodeDoc.Mtps.IntegrationTests
{
    [TestFixture]
    public class MsdnSimpleUsages
    {

        public MsdnSimpleUsages() {
            SharedRepository = new MsdnCodeDocMemberRepository();
        }

        protected MsdnCodeDocMemberRepository SharedRepository { get; private set; }

        [Test]
        public void lookup_system_guid(){
            var repository = new MsdnCodeDocMemberRepository();
            var model = repository.GetMemberModel("System.Guid");
            Assert.IsNotNull(model);
            Assert.IsTrue(model.HasSummaryContents);
            Assert.That(model.SummaryContents.First().Node.OuterXml.Contains("GUID"));
        }

        [Test]
        public void cache_performance_test(){
            var repository = new MsdnCodeDocMemberRepository();

            var firstSingleRequestStopwatch = new Stopwatch();
            firstSingleRequestStopwatch.Start();
            var modelGuid = repository.GetMemberModel("System.Guid");
            Assert.IsNotNull(modelGuid);
            firstSingleRequestStopwatch.Stop();

            var secondDoubleRequestStopwatch = new Stopwatch();
            secondDoubleRequestStopwatch.Start();
            // the request is a different code reference but for the same member
            var modelGuidSecond = repository.GetMemberModel("T:System.Guid");
            Assert.IsNotNull(modelGuidSecond);
            var modelObject = repository.GetMemberModel("T:System.Object");
            Assert.IsNotNull(modelObject);
            secondDoubleRequestStopwatch.Stop();

            // the second set of requests should be less than 3/4 of the first request
            var secondRequestTargetTime = new TimeSpan(firstSingleRequestStopwatch.Elapsed.Ticks * 3 / 4);
            Assert.Less(secondDoubleRequestStopwatch.Elapsed, secondRequestTargetTime);
        }

        [Test]
        public void get_sealed_type() {
            var member = SharedRepository.GetMemberModel("T:System.Dynamic.ExpandoObject");
            Assert.IsNotNull(member);
            var type = member as CodeDocType;
            Assert.IsNotNull(type);
            Assert.IsTrue(type.IsSealed.GetValueOrDefault());
        }

        [Test]
        public void get_protected_method() {
            var member = SharedRepository.GetMemberModel("M:System.Object.MemberwiseClone");
            Assert.IsNotNull(member);
            Assert.AreEqual(ExternalVisibilityKind.Protected, member.ExternalVisibility);
        }

    }
}
