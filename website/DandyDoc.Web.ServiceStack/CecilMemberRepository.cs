using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Web;
using DandyDoc.CRef;
using DandyDoc.CodeDoc;
using Mono.Cecil;

namespace DandyDoc.Web.ServiceStack
{
    public class CecilMemberRepository : ReflectionCodeDocMemberRepository
    {

        protected class MemberGenerator : ReflectionCodeDocMemberRepository.MemberGenerator
        {

            private readonly ObjectCache _cache = MemoryCache.Default;

            public MemberGenerator(ReflectionCodeDocMemberRepository repository, CodeDocRepositorySearchContext searchContext)
                : base(repository, searchContext) { }

            private Uri CreateUri(MemberInfo memberInfo) {
                Contract.Requires(memberInfo != null);
                var type = memberInfo as Type ?? memberInfo.DeclaringType;
                var searchKeywords = memberInfo.Name;
                if (type != null && type != memberInfo)
                    searchKeywords += " " + type.Name;

                return new Uri(
                    String.Format(
                        "https://github.com/jbevain/cecil/search?q={0}+repo%3Ajbevain%2Fcecil+extension%3Acs&type=Code",
                        Uri.EscapeDataString(searchKeywords)),
                    UriKind.Absolute);
            }

            protected override Uri GetUri(MemberInfo memberInfo) {
                if (memberInfo == null)
                    return null;

                var cRef = GetCRefIdentifier(memberInfo);
                var cacheKey = String.Format("{0}_GetUri({1})", GetType().FullName, cRef);
                var uri = _cache[cacheKey] as Uri;
                if (uri == null) {
                    uri = CreateUri(memberInfo);
                    if (uri != null)
                        _cache[cacheKey] = uri;
                }
                return uri;
            }
        }

        public CecilMemberRepository() : base(new ReflectionCRefLookup(typeof(AssemblyDefinition).Assembly)) { }

        protected override MemberGeneratorBase CreateGenerator(CodeDocRepositorySearchContext searchContext) {
            return new MemberGenerator(this, searchContext);
        }

    }
}