using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DandyDoc.CodeDoc.Mtps.IntegrationTests
{
    [TestFixture]
    public class SimpleUsages
    {

        [Test]
        public void lookup_system_guid(){
            var repository = new MsdnCodeDocMemberRepository();
            var model = repository.GetMemberModel("System.Guid");
            Assert.IsNotNull(model);
        }

    }
}
