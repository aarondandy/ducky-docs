using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DandyDoc.CRef;
using DandyDoc.Reflection;
using DandyDoc.XmlDoc;
using NUnit.Framework;
using TestLibrary1;

namespace DandyDoc.CodeDoc.Tests
{

    [TestFixture]
    public class ReflectionCodeDocEntityRepositoryTest
    {

        public ReflectionCodeDocEntityRepositoryTest() {
            var testLib1Asm = typeof(Class1).Assembly;
            var testLib1AsmPath = ReflectionUtilities.GetFilePath(testLib1Asm);
            var testLib1XmlPath = Path.ChangeExtension(testLib1AsmPath, "XML");
            TestLibrary1Repository = new ReflectionCodeDocEntityRepository(
                new ReflectionCRefLookup(new[] { testLib1Asm }),
                new[]{new XmlAssemblyDocumentation(testLib1XmlPath)}
            );
        }

        public ReflectionCodeDocEntityRepository TestLibrary1Repository { get; set; }

        [Test]
        public void invalid_requests() {
            Assert.Throws<ArgumentException>(
                () => TestLibrary1Repository.GetEntity(String.Empty));
            Assert.Throws<ArgumentNullException>(
                () => TestLibrary1Repository.GetEntity((CRefIdentifier)null));
        }


    }
}
