using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DandyDoc.ExternalVisibility
{
    public static class ExternalVisibilityKindOperations
    {

        public static ExternalVisibilityKind Min(ExternalVisibilityKind a, ExternalVisibilityKind b) {
            return a <= b ? a : b;
        }

        public static ExternalVisibilityKind Max(ExternalVisibilityKind a, ExternalVisibilityKind b) {
            return a >= b ? a : b;
        }

    }
}
