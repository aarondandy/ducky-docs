using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DandyDoc.CRef;
using DandyDoc.CodeDoc;
using DandyDoc.XmlDoc;

namespace SimpleStaticSiteGenerator
{
    class Program
    {
        static void Main(string[] args) {

            var targetRepository = new ReflectionCodeDocMemberRepository(
                new ReflectionCRefLookup(
                    typeof(ReflectionCRefLookup).Assembly,
                    typeof(ICodeDocMemberRepository).Assembly
                ),
                new XmlAssemblyDocument("DandyDoc.Core.XML"),
                new XmlAssemblyDocument("DandyDoc.CodeDoc.XML")
            );

            var msdnRepository = new MsdnCodeDocMemberRepository();
            var supportingRepositories = new CodeDocMergedMemberRepository(msdnRepository);

            var allRepositories = new CodeDocMergedMemberRepository(
                targetRepository,
                supportingRepositories);

        }
    }
}
