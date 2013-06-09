using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace DandyDoc.CodeDoc.Mtps.IntegrationTests
{
    [TestFixture]
    public class MsdnSimpleUsages
    {

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

    }
}
