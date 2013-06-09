using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using DandyDoc.CRef;
using DandyDoc.Reflection;
using DandyDoc.XmlDoc;
using TestLibrary1;
using NUnit.Framework;

namespace DandyDoc.CodeDoc.Tests
{

    [TestFixture]
    public class CodeDocRepositorySearchTests
    {

        private ReflectionCodeDocMemberRepository TestLibrary1Repository {
            get {
                var testLib1Asm = typeof(Class1).Assembly;
                var testLib1AsmPath = testLib1Asm.GetFilePath();
                var testLib1XmlPath = Path.ChangeExtension(testLib1AsmPath, "XML");
                return new ReflectionCodeDocMemberRepository(
                    new ReflectionCRefLookup(testLib1Asm),
                    new XmlAssemblyDocument(testLib1XmlPath)
                );
            }
        }

        private class TypeRestrictedRepository : ReflectionCodeDocMemberRepository
        {
            public TypeRestrictedRepository(ReflectionCodeDocMemberRepository core, string typeName)
                : base(core.CRefLookup,core.XmlDocs){
                Contract.Requires(!String.IsNullOrEmpty(typeName));
                TypeName = typeName;
            }

            public string TypeName { get; private set;}

            protected override bool MemberFilter(Type type) {
                if (!base.MemberFilter(type))
                    return false;
                return type.Name == TypeName;
            }
        }

        protected virtual ICodeDocMemberRepository GetTypeRestrictedTestLibrary1Repository(string typeName){
            return new TypeRestrictedRepository(TestLibrary1Repository, typeName);
        }

        [Test]
        public void search_three_repositories_for_types(){
            var class1Repo = GetTypeRestrictedTestLibrary1Repository("Class1");
            var generic1Repo = GetTypeRestrictedTestLibrary1Repository("Generic1`2");
            var userOtherStuffRepo = GetTypeRestrictedTestLibrary1Repository("UsesOtherStuff");

            var searchContext = new CodeDocRepositorySearchContext(new[] { class1Repo, generic1Repo, userOtherStuffRepo});

            var model = searchContext.Search(new CRefIdentifier("T:TestLibrary1.UsesOtherStuff")) as CodeDocType;
            Assert.IsNotNull(model);

            var generic1FieldType = ((CodeDocField) model.Fields.Single()).ValueType as CodeDocType;
            Assert.IsNotNull(generic1FieldType);
            Assert.That(generic1FieldType.Assembly.TypeCRefs.Contains(generic1FieldType.CRef));

            var class1PropertyType = ((CodeDocProperty) model.Properties.Single()).ValueType as CodeDocType;
            Assert.IsNotNull(class1PropertyType);
            Assert.That(class1PropertyType.Assembly.TypeCRefs.Contains(class1PropertyType.CRef));
        }


    }
}
