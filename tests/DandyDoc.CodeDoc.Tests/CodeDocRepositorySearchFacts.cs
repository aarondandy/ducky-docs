using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using DuckyDocs.CRef;
using DuckyDocs.Reflection;
using DuckyDocs.XmlDoc;
using TestLibrary1;
using Xunit;

namespace DuckyDocs.CodeDoc.Tests
{
    public class CodeDocRepositorySearchFacts
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

        [Fact]
        public void search_three_repositories_for_types(){
            var class1Repo = GetTypeRestrictedTestLibrary1Repository("Class1");
            var generic1Repo = GetTypeRestrictedTestLibrary1Repository("Generic1`2");
            var userOtherStuffRepo = GetTypeRestrictedTestLibrary1Repository("UsesOtherStuff");

            var searchContext = new CodeDocRepositorySearchContext(new[] { class1Repo, generic1Repo, userOtherStuffRepo});

            var model = searchContext.Search(new CRefIdentifier("T:TestLibrary1.UsesOtherStuff")) as CodeDocType;
            Assert.NotNull(model);

            var generic1FieldType = ((CodeDocField) model.Fields.Single()).ValueType as CodeDocType;
            Assert.NotNull(generic1FieldType);
            Assert.Equal("T:TestLibrary1.Generic1{System.Int32,System.Int32[]}", generic1FieldType.CRef.FullCRef);

            var class1PropertyType = ((CodeDocProperty) model.Properties.Single()).ValueType as CodeDocType;
            Assert.NotNull(class1PropertyType);
            Assert.Equal("T:TestLibrary1.Class1", class1PropertyType.CRef.FullCRef);
        }


    }
}
