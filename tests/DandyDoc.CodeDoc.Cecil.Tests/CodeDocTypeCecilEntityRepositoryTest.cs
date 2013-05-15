using System.IO;
using DandyDoc.CRef;
using DandyDoc.Cecil;
using DandyDoc.CodeDoc.Tests;
using DandyDoc.XmlDoc;
using Mono.Cecil;
using NUnit.Framework;

#pragma warning disable 1591

namespace DandyDoc.CodeDoc.Cecil.Tests
{
    [TestFixture]
    public class CodeDocTypeCecilEntityRepositoryTest : CodeDocTypeReflectionEntityRepositoryTest
    {

        public override ICodeDocEntityRepository TestLibrary1Repository {
            get {
                var testLib1Asm = AssemblyDefinition.ReadAssembly(@".\TestLibrary1.dll");
                var testLib1AsmPath = testLib1Asm.GetFilePath();
                var testLib1XmlPath = Path.ChangeExtension(testLib1AsmPath, "XML");
                var coreRepo = new CecilCodeDocEntityRepository(
                    new CecilCRefLookup(testLib1Asm),
                    new XmlAssemblyDocumentation(testLib1XmlPath)
                );
                return new ThreadSafeCodeDocRepositoryWrapper(coreRepo);
            }
        }

    }
}
