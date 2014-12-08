using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DuckyDocs.Reflection;
using TestLibrary1.Seal;
using Xunit;

namespace DuckyDocs.Core.Tests
{
    public class ReflectionUtilityTests
    {

        public interface IFace
        {
            string Method(string input);
        }

        public abstract class A : IFace
        {
            public virtual string Method(string input) {
                return input + " from A";
            }

            public abstract string Method2();
        }

        public abstract class B : A
        {
            // Method passes through here

            public abstract override string Method2();

        }

        public class C : B
        {
            public override string Method(string input) {
                return input + " from C";
            }

            public override string Method2() {
                throw new NotImplementedException();
            }
        }

        public static readonly MethodInfo IFaceMethod = typeof(IFace).GetMethod("Method", new[] { typeof(string) });
        public static readonly MethodInfo AMethod = typeof(A).GetMethod("Method", new[] { typeof(string) });
        public static readonly MethodInfo BMethod = typeof(B).GetMethod("Method", new[] { typeof(string) });
        public static readonly MethodInfo CMethod = typeof(C).GetMethod("Method", new[] { typeof(string) });

        public static readonly MethodInfo AMethod2 = typeof(A).GetMethod("Method2", new Type[0]);
        public static readonly MethodInfo BMethod2 = typeof(B).GetMethod("Method2", new Type[0]);
        public static readonly MethodInfo CMethod2 = typeof(C).GetMethod("Method2", new Type[0]);

        [Fact]
        public void test_get_property_base_from_override() {
            var actual = CMethod.FindNextAncestor();
            Assert.Equal(AMethod.DeclaringType, actual.DeclaringType);
            Assert.Equal(AMethod.Name, actual.Name);
        }

        [Fact]
        public void test_find_interface_ancestor() {
            var actual = AMethod.FindNextAncestor();
            Assert.Equal(IFaceMethod.DeclaringType, actual.DeclaringType);
            Assert.Equal(IFaceMethod.Name, actual.Name);
        }

        [Fact]
        public void test_find_ancestor_from_the_middle() {
            var actual = BMethod.FindNextAncestor(); // this is really AMethod but with a different reflected type
            Assert.Equal(AMethod.DeclaringType, actual.DeclaringType);
            Assert.Equal(AMethod.Name, actual.Name);
        }

        [Fact]
        public void test_dont_skip_abstract_override() {
            var actual = CMethod2.FindNextAncestor();
            Assert.Equal(BMethod2.DeclaringType, actual.DeclaringType);
            Assert.Equal(BMethod2.Name, actual.Name);
        }

        [Fact]
        public void dont_go_over_the_top() {
            Assert.Null(AMethod2.FindNextAncestor());
        }

    }
}
