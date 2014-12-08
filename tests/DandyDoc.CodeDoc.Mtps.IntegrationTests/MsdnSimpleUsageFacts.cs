using System;
using System.Diagnostics;
using System.Linq;
using DuckyDocs.ExternalVisibility;
using Xunit;
using FluentAssertions;

namespace DuckyDocs.CodeDoc.Mtps.IntegrationTests
{
    public class MsdnSimpleUsageFacts
    {
        public MsdnSimpleUsageFacts() {
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
            var requestNames = new[] { "System.Guid", "System.Object", "System.Array"};

            var firstRequestStopwatch = new Stopwatch();
            firstRequestStopwatch.Start();
            var firstRequests = Array.ConvertAll(requestNames, x => repository.GetMemberModel(x));
            firstRequests.Should().NotContainNulls();
            firstRequestStopwatch.Stop();

            var secondRequestStopwatch = new Stopwatch();
            secondRequestStopwatch.Start();
            // the request is a different code reference but for the same member
            var secondRequests = Array.ConvertAll(requestNames, x => repository.GetMemberModel("T:" + x));
            secondRequests.Should().NotContainNulls();
            secondRequestStopwatch.Stop();

            // the second set of requests should be (a lot) less than 3/4 of the first request
            var secondRequestTargetTime = new TimeSpan(firstRequestStopwatch.Elapsed.Ticks * 3 / 4);
            secondRequestStopwatch.Elapsed.Should().BeLessThan(secondRequestTargetTime);
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
