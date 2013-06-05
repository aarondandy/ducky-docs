using System;
using System.Diagnostics.Contracts;

namespace DandyDoc.CRef
{
    public class CRefTransformer
    {

        private static readonly CRefTransformer _fullSimplification = new CRefTransformer {
            GenericInstanceToDefinition = true,
            RemoveArraySuffix = true,
            RemoveRefOutSuffix = true
        };

        public static CRefTransformer  FullSimplification { get { return _fullSimplification;}}

        bool GenericInstanceToDefinition { get; set; }

        bool RemoveArraySuffix { get; set; }

        bool RemoveRefOutSuffix { get; set; }

        public CRefIdentifier Transform(CRefIdentifier cRef) {
            if(cRef == null) throw new ArgumentNullException("cRef");
            Contract.Ensures(Contract.Result<CRefIdentifier>() != null);

            var coreNameParts = CRefIdentifier.ExtractParams(cRef.CoreName, '.');
            var hasParamText = !String.IsNullOrWhiteSpace(cRef.ParamPart);
            if (coreNameParts.Count > 0) {
                var mayBeMethod = String.Equals("M", cRef.TargetType, StringComparison.OrdinalIgnoreCase)
                    || (
                        !String.Equals("T", cRef.TargetType, StringComparison.OrdinalIgnoreCase)
                        && hasParamText
                    );

                var lastIndex = coreNameParts.Count - 1;
                Contract.Assume(lastIndex >= 0);

                if (RemoveArraySuffix && coreNameParts[lastIndex].EndsWith("[]"))
                    coreNameParts[lastIndex] = coreNameParts[lastIndex].Substring(0, coreNameParts[lastIndex].Length - 2);

                if (RemoveRefOutSuffix && coreNameParts[lastIndex].EndsWith("&") || coreNameParts[lastIndex].EndsWith("@"))
                    coreNameParts[lastIndex] = coreNameParts[lastIndex].Substring(0, coreNameParts[lastIndex].Length - 1);

                if (GenericInstanceToDefinition) {
                    coreNameParts[lastIndex] = NamePartToGenericCardinality(coreNameParts[lastIndex], tickCount: mayBeMethod ? 2 : 1);
                    for (int i = coreNameParts.Count - 1; i >= 0; i--) {
                        coreNameParts[i] = NamePartToGenericCardinality(coreNameParts[i]);
                    }
                }

            }

            var result = String.Join(".", coreNameParts);
            if (!String.IsNullOrWhiteSpace(cRef.TargetType))
                result = String.Concat(cRef.TargetType, ':', result);

            if (hasParamText) {
                if (GenericInstanceToDefinition) {
                    var paramParts = cRef.ParamPartTypes.ConvertAll(t => NamePartToGenericCardinality(t));
                    result = String.Concat(result, '(', String.Join(",", paramParts), ')');
                }
            }

            return new CRefIdentifier(String.IsNullOrEmpty(result) ? "!:" : result);
        }

        private static string NamePartToGenericCardinality(string part, int tickCount = 1) {
            var genericParamListOpenAt = part.IndexOf('{');
            var firstParamPartChar = genericParamListOpenAt + 1;
            if (genericParamListOpenAt < 0 || firstParamPartChar >= part.Length)
                return part; // if an open is not found, don't mess with it
            var genericParamListCloseAt = part.LastIndexOf('}');
            if (genericParamListCloseAt != part.Length - 1)
                return part; // must be the last character

            var correctedParts = CRefIdentifier.ExtractParams(part.Substring(firstParamPartChar));
            var tickText = "`";
            for (int tickIndex = 1; tickIndex < tickCount; tickIndex++)
                tickText += '`';
            return String.Concat(part.Substring(0, genericParamListOpenAt), tickText, correctedParts.Count);
        }

    }
}
