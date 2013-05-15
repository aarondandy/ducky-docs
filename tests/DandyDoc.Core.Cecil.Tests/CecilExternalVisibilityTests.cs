using System;
using System.Collections.Generic;
using System.Linq;
using DandyDoc.CRef;
using DandyDoc.ExternalVisibility;
using Mono.Cecil;
using NUnit.Framework;

#pragma warning disable 1591

namespace DandyDoc.Core.Cecil.Tests
{
    public class CecilExternalVisibilityTests
    {

        public CecilExternalVisibilityTests() {
            Assembly = AssemblyDefinition.ReadAssembly("./TestLibrary1.dll");
        }

        private AssemblyDefinition Assembly { get; set; }

        public CecilCRefLookup Lookup {
            get {
                return new CecilCRefLookup(new[] { Assembly });
            }
        }

        private ExternalVisibilityKind GetVisibility(string cRef) {
            var memberReference = Lookup.GetMember(cRef);
            return GetVisibility(memberReference);
        }

        private ExternalVisibilityKind GetVisibility(MemberReference memberReference) {
            return memberReference.GetExternalVisibility();
        }

        public static IEnumerable<Tuple<ExternalVisibilityKind, string>> AllClasses {
            get {
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Public, "T:TestLibrary1.PublicExposedTestClass");
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Hidden, "T:TestLibrary1.PublicExposedTestClass.PrivateClass");
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Protected, "T:TestLibrary1.PublicExposedTestClass.ProtectedClass");
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Public, "T:TestLibrary1.PublicExposedTestClass.PublicClass");
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Hidden, "T:TestLibrary1.PublicExposedTestClass.InternalClass");
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Protected, "T:TestLibrary1.PublicExposedTestClass.ProtectedInternalClass");
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Hidden, "T:TestLibrary1.InternalExposedTestClass");
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Hidden, "T:TestLibrary1.InternalExposedTestClass.PrivateClass");
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Hidden, "T:TestLibrary1.InternalExposedTestClass.ProtectedClass");
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Hidden, "T:TestLibrary1.InternalExposedTestClass.PublicClass");
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Hidden, "T:TestLibrary1.InternalExposedTestClass.InternalClass");
                yield return new Tuple<ExternalVisibilityKind, string>(ExternalVisibilityKind.Hidden, "T:TestLibrary1.InternalExposedTestClass.ProtectedInternalClass");
            }
        }

        [Test]
        public void type_visibility() {
            foreach (var tuple in AllClasses)
                Assert.AreEqual(tuple.Item1, GetVisibility(tuple.Item2));
        }

        [Test]
        public void field_visibility() {
            var fields = new[] {
                new {Vis = ExternalVisibilityKind.Hidden, Name = "PrivateField"},
                new {Vis = ExternalVisibilityKind.Protected, Name = "ProtectedField"},
                new {Vis = ExternalVisibilityKind.Public, Name = "PublicField"},
                new {Vis = ExternalVisibilityKind.Hidden, Name = "InternalField"},
                new {Vis = ExternalVisibilityKind.Protected, Name = "ProtectedInternalField"}
            };
            foreach (var typeTuple in AllClasses) {
                var type = Lookup.GetMember(typeTuple.Item2) as TypeDefinition;
                var typeVis = typeTuple.Item1;
                foreach (var fieldSet in fields) {
                    var fieldDefinition = type.Fields.Single(x => x.Name == fieldSet.Name);
                    var expectedVis = ExternalVisibilityOperations.LeastVisible(typeVis, fieldSet.Vis);
                    Assert.AreEqual(expectedVis, GetVisibility(fieldDefinition));
                }
            }
        }

        [Test]
        public void method_visibility() {
            var methods = new[] {
                new {Vis = ExternalVisibilityKind.Hidden, Name = "PrivateMethod"},
                new {Vis = ExternalVisibilityKind.Protected, Name = "ProtectedMethod"},
                new {Vis = ExternalVisibilityKind.Public, Name = "PublicMethod"},
                new {Vis = ExternalVisibilityKind.Hidden, Name = "InternalMethod"},
                new {Vis = ExternalVisibilityKind.Protected, Name = "ProtectedInternalMethod"}
            };
            foreach (var typeTuple in AllClasses) {
                var type = Lookup.GetMember(typeTuple.Item2) as TypeDefinition;
                var typeVis = typeTuple.Item1;
                foreach (var methodSet in methods) {
                    var methodDefinition = type.Methods.Single(x => x.Name == methodSet.Name);
                    var expectedVis = ExternalVisibilityOperations.LeastVisible(typeVis, methodSet.Vis);
                    Assert.AreEqual(expectedVis, GetVisibility(methodDefinition));
                }
            }
        }

        [Test]
        public void delegate_visibility() {
            var delegates = new[] {
                new {Vis = ExternalVisibilityKind.Hidden, Name = "PrivateDelegate"},
                new {Vis = ExternalVisibilityKind.Protected, Name = "ProtectedDelegate"},
                new {Vis = ExternalVisibilityKind.Public, Name = "PublicDelegate"},
                new {Vis = ExternalVisibilityKind.Hidden, Name = "InternalDelegate"},
                new {Vis = ExternalVisibilityKind.Protected, Name = "ProtectedInternalDelegate"}
            };
            foreach (var typeTuple in AllClasses) {
                var type = Lookup.GetMember(typeTuple.Item2) as TypeDefinition;
                var typeVis = typeTuple.Item1;
                foreach (var delegateSet in delegates) {
                    var delegateDefinition = type.NestedTypes.Single(x => x.Name == delegateSet.Name);
                    var expectedVis = ExternalVisibilityOperations.LeastVisible(typeVis, delegateSet.Vis);
                    Assert.AreEqual(expectedVis, GetVisibility(delegateDefinition));
                }
            }
        }

        [Test]
        public void event_visibility() {
            var events = new[] {
                new {Vis = ExternalVisibilityKind.Hidden, Name = "PrivateEvent"},
                new {Vis = ExternalVisibilityKind.Protected, Name = "ProtectedEvent"},
                new {Vis = ExternalVisibilityKind.Public, Name = "PublicEvent"},
                new {Vis = ExternalVisibilityKind.Hidden, Name = "InternalEvent"},
                new {Vis = ExternalVisibilityKind.Protected, Name = "ProtectedInternalEvent"}
            };
            foreach (var typeTuple in AllClasses) {
                var type = Lookup.GetMember(typeTuple.Item2) as TypeDefinition;
                var typeVis = typeTuple.Item1;
                foreach (var eventSet in events) {
                    var eventDefinition = type.Events.Single(x => x.Name == eventSet.Name);
                    var expectedVis = ExternalVisibilityOperations.LeastVisible(typeVis, eventSet.Vis);
                    Assert.AreEqual(expectedVis, GetVisibility(eventDefinition));
                }
            }
        }

        [Test]
        public void property_visibility() {
            var props = new[] {
                new{Name = "PropPubPub", Get = ExternalVisibilityKind.Public, Set = ExternalVisibilityKind.Public},
                new{Name = "PropPubPro", Get = ExternalVisibilityKind.Public, Set = ExternalVisibilityKind.Protected},
                new{Name = "PropPubPri", Get = ExternalVisibilityKind.Public, Set = ExternalVisibilityKind.Hidden},
                new{Name = "PropPubInt", Get = ExternalVisibilityKind.Public, Set = ExternalVisibilityKind.Hidden},
                new{Name = "PropPubPin", Get = ExternalVisibilityKind.Public, Set = ExternalVisibilityKind.Protected},
                new{Name = "PropProPub", Get = ExternalVisibilityKind.Protected, Set = ExternalVisibilityKind.Public},
                new{Name = "PropProPro", Get = ExternalVisibilityKind.Protected, Set = ExternalVisibilityKind.Protected},
                new{Name = "PropProPri", Get = ExternalVisibilityKind.Protected, Set = ExternalVisibilityKind.Hidden},
                new{Name = "PropProPin", Get = ExternalVisibilityKind.Protected, Set = ExternalVisibilityKind.Protected},
                new{Name = "PropPriPub", Get = ExternalVisibilityKind.Hidden, Set = ExternalVisibilityKind.Public},
                new{Name = "PropPriPro", Get = ExternalVisibilityKind.Hidden, Set = ExternalVisibilityKind.Protected},
                new{Name = "PropPriPri", Get = ExternalVisibilityKind.Hidden, Set = ExternalVisibilityKind.Hidden},
                new{Name = "PropPriPin", Get = ExternalVisibilityKind.Hidden, Set = ExternalVisibilityKind.Protected},
                new{Name = "PropIntPub", Get = ExternalVisibilityKind.Hidden, Set=ExternalVisibilityKind.Public},
                new{Name = "PropIntPri", Get = ExternalVisibilityKind.Hidden, Set = ExternalVisibilityKind.Hidden},
                new{Name = "PropIntInt", Get = ExternalVisibilityKind.Hidden, Set = ExternalVisibilityKind.Hidden},
                new{Name = "PropIntPin", Get = ExternalVisibilityKind.Hidden, Set = ExternalVisibilityKind.Protected},
                new{Name = "PropPinPub", Get = ExternalVisibilityKind.Protected, Set = ExternalVisibilityKind.Public},
                new{Name = "PropPinPro", Get = ExternalVisibilityKind.Protected, Set = ExternalVisibilityKind.Protected},
                new{Name = "PropPinPri", Get = ExternalVisibilityKind.Protected, Set = ExternalVisibilityKind.Hidden},
                new{Name = "PropPinPin", Get = ExternalVisibilityKind.Protected, Set = ExternalVisibilityKind.Protected}
            };
            foreach (var typeTuple in AllClasses) {
                var type = Lookup.GetMember(typeTuple.Item2) as TypeDefinition;
                var typeVis = typeTuple.Item1;
                foreach (var propSet in props) {
                    var propDef = type.Properties.Single(x => x.Name == propSet.Name);
                    var expectedGetVis = ExternalVisibilityOperations.LeastVisible(typeVis, propSet.Get);
                    var expectedSetVis = ExternalVisibilityOperations.LeastVisible(typeVis, propSet.Set);
                    var expectedPropVis = ExternalVisibilityOperations.LeastVisible(
                        typeVis,
                        ExternalVisibilityOperations.MostVisible(propSet.Get, propSet.Set));
                    Assert.AreEqual(expectedGetVis, GetVisibility(propDef.GetMethod));
                    Assert.AreEqual(expectedSetVis, GetVisibility(propDef.SetMethod));
                    Assert.AreEqual(expectedPropVis, GetVisibility(propDef));
                }
            }
        }

    }
}
