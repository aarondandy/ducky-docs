using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DandyDoc.CRef;
using DandyDoc.Cecil;
using Mono.Cecil;
using NUnit.Framework;

namespace DandyDoc.Core.Cecil.Tests
{
    [TestFixture]
    public class CecilUtilityTests
    {

        private static AssemblyDefinition GetAssembly() {
            var assemblyDefinition = AssemblyDefinition.ReadAssembly("./TestLibrary1.dll");
            Assert.IsNotNull(assemblyDefinition);
            return assemblyDefinition;
        }

        public CecilUtilityTests() {
            Lookup = new CecilCRefLookup(new[] { GetAssembly() });
        }

        public CecilCRefLookup Lookup { get; private set; }

        public IMemberDefinition GetMember(string cRef) {
            return Lookup.GetMember(cRef) as IMemberDefinition;
        }

        [Test]
        public void is_nullable_test() {
            var aNullableField = GetMember("TestLibrary1.Class1.SomeNullableInt") as FieldDefinition;
            Assert.IsNotNull(aNullableField);
            Assert.IsTrue(aNullableField.FieldType.IsNullable());
            var aNormalField = GetMember("TestLibrary1.Class1.SomeClasses") as FieldDefinition;
            Assert.IsNotNull(aNormalField);
            Assert.IsFalse(aNormalField.FieldType.IsNullable());
        }

    }
}
