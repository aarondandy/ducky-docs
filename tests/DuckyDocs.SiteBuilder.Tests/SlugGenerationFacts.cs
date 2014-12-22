using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckyDocs.CRef;
using FluentAssertions;
using Xunit;

namespace DuckyDocs.SiteBuilder.Tests
{
    public static class SlugGenerationFacts
    {

        [Fact]
        public static void overload_slugs_differ() {
            var cRefA = new CRefIdentifier("M:TestLibrary1.Class1.Inherits.VirtualInstanceMethod(System.Int32)");
            var cRefB = new CRefIdentifier("M:TestLibrary1.Class1.Inherits.VirtualInstanceMethod(System.String)");

            var slugA = StaticApiPageGenerator.CreateSlugName(cRefA);
            var slugB = StaticApiPageGenerator.CreateSlugName(cRefB);

            slugA.Should().NotBe(slugB);
        }

        [Fact]
        public static void append_target_type_as_suffix() {
            var cRef = new CRefIdentifier("F:Class1.FieldA");

            var slug = StaticApiPageGenerator.CreateSlugName(cRef);

            slug.Should().Be("Class1.FieldA-F");
        }

        [Fact]
        public static void remove_generic_name_parts() {
            var cRef = new CRefIdentifier("M:TestLibrary1.Generic1`2.AMix`1(``1,`0)");

            var slug = StaticApiPageGenerator.CreateSlugName(cRef);

            slug.Should().StartWith("TestLibrary1.Generic1.AMix-");
        }

    }
}
