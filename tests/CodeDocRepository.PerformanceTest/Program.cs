using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DuckyDocs.CRef;
using DuckyDocs.CodeDoc;
using DuckyDocs.Reflection;
using DuckyDocs.XmlDoc;
using TestLibrary1;

namespace CodeDocRepository.PerformanceTest
{
    class Program
    {

        static ReflectionCodeDocMemberRepository CreateReflectionRepository() {
            var testLib1Asm = typeof(Class1).Assembly;
            var testLib1AsmPath = ReflectionUtilities.GetFilePath(testLib1Asm);
            var testLib1XmlPath = Path.ChangeExtension(testLib1AsmPath, "XML");
            return new ReflectionCodeDocMemberRepository(
                new ReflectionCRefLookup(testLib1Asm),
                new XmlAssemblyDocument(testLib1XmlPath)
            );
        }

        static ICodeDocMemberRepository CreateRepository() {
            return CreateReflectionRepository();
        }

        static void Main(string[] args) {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var repository = CreateRepository();
            var rootTypeCRefs = repository.Namespaces.SelectMany(x => x.TypeCRefs);
            var rootTypes = rootTypeCRefs.Select(cRef => repository.GetMemberModel(cRef));
            var typeCount = CountTypes(rootTypes);
            stopwatch.Stop();
            Console.WriteLine("TypeCount:\t" + typeCount);
            Console.WriteLine("Elapsed:\t" + stopwatch.Elapsed);
        }

        static int CountTypes(IEnumerable<ICodeDocMember> entities) {
            return CountTypes(entities.OfType<CodeDocType>());
        }

        static int CountTypes(IEnumerable<CodeDocType> types) {
            int count = 0;
            foreach (var typeEntity in types) {
                count++;
                if (typeEntity.HasNestedTypes)
                    count += CountTypes(typeEntity.NestedTypes);
                if (typeEntity.HasNestedDelegates)
                    count += CountTypes(typeEntity.NestedDelegates);
            }
            return count;
        }
    }
}
