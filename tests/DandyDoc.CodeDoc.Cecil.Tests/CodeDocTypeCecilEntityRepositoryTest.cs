using System.IO;
using DandyDoc.CRef;
using DandyDoc.Cecil;
using DandyDoc.CodeDoc.Tests;
using DandyDoc.XmlDoc;
using Mono.Cecil;
using NUnit.Framework;

namespace DandyDoc.CodeDoc.Cecil.Tests
{
    [TestFixture]
    public class CodeDocTypeCecilEntityRepositoryTest : CodeDocTypeReflectionEntityRepositoryTest
    {

        public override ICodeDocEntityRepository TestLibrary1Repository {
            get {
                var testLib1Asm = AssemblyDefinition.ReadAssembly(@".\TestLibrary1.dll", CecilImmediateAssemblyResolver.CreateReaderParameters());
                var testLib1AsmPath = CecilUtilities.GetFilePath(testLib1Asm);
                var testLib1XmlPath = Path.ChangeExtension(testLib1AsmPath, "XML");
                return new CecilCodeDocEntityRepository(
                    new CecilCRefLookup(testLib1Asm),
                    new XmlAssemblyDocumentation(testLib1XmlPath)
                );
            }
        }

    }
}
