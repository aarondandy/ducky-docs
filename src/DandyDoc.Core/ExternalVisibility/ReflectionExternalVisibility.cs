using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DandyDoc.ExternalVisibility
{
    public static class ReflectionExternalVisibility
    {

        public static ExternalVisibilityKind Get(this MemberInfo memberInfo) {
            if(null == memberInfo) throw new ArgumentNullException("memberInfo");
            Contract.EndContractBlock();
            if (memberInfo is Type)
                return Get((Type)memberInfo);
            if (memberInfo is MethodBase)
                return Get((MethodBase)memberInfo);
            if (memberInfo is PropertyInfo)
                return Get((PropertyInfo)memberInfo);
            if (memberInfo is FieldInfo)
                return Get((FieldInfo)memberInfo);
            if (memberInfo is EventInfo)
                return Get((EventInfo)memberInfo);
            throw new NotSupportedException();
        }

    }
}
