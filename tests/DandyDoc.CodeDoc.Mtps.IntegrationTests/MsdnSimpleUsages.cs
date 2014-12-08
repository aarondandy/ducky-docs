using System;
using System.Diagnostics;
using System.Linq;
using DuckyDocs.ExternalVisibility;
using Xunit;
using FluentAssertions;

namespace DuckyDocs.CodeDoc.Mtps.IntegrationTests
{

    public class MsdnSimpleUsages
    {

        public MsdnSimpleUsages() {
            SharedRepository = new MsdnCodeDocMemberRepository();
        }

        protected MsdnCodeDocMemberRepository SharedRepository { get; private set; }

        [Fact]
        public void lookup_system_guid(){
            var repository = new MsdnCodeDocMemberRepository();
            var model = repository.GetMemberModel("System.Guid");
            Assert.NotNull(model);
            Assert.True(model.HasSummaryContents);
            Assert.True(model.SummaryContents.First().Node.OuterXml.Contains("GUID"));
        }

        [Fact]
        public void cache_performance_test(){
            var repository = new MsdnCodeDocMemberRepository();

            var firstSingleRequestStopwatch = new Stopwatch();
            firstSingleRequestStopwatch.Start();
            var modelGuid = repository.GetMemberModel("System.Guid");
            Assert.NotNull(modelGuid);
            firstSingleRequestStopwatch.Stop();

            var secondDoubleRequestStopwatch = new Stopwatch();
            secondDoubleRequestStopwatch.Start();
            // the request is a different code reference but for the same member
            var modelGuidSecond = repository.GetMemberModel("T:System.Guid");
            Assert.NotNull(modelGuidSecond);
            var modelObject = repository.GetMemberModel("T:System.Object");
            Assert.NotNull(modelObject);
            secondDoubleRequestStopwatch.Stop();

            // the second set of requests should be less than 3/4 of the first request
            var secondRequestTargetTime = new TimeSpan(firstSingleRequestStopwatch.Elapsed.Ticks * 4 / 5);
            secondDoubleRequestStopwatch.Elapsed.Should().BeLessThan(secondRequestTargetTime);
        }

        [Fact]
        public void get_sealed_type() {
            var member = SharedRepository.GetMemberModel("T:System.Dynamic.ExpandoObject");
            Assert.NotNull(member);
            var type = member as CodeDocType;
            Assert.NotNull(type);
            Assert.True(type.IsSealed.GetValueOrDefault());
        }

        [Fact]
        public void get_protected_method() {
            var member = SharedRepository.GetMemberModel("M:System.Object.MemberwiseClone");
            Assert.NotNull(member);
            Assert.Equal(ExternalVisibilityKind.Protected, member.ExternalVisibility);
        }

    }
}
