using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DandyDoc.CodeDoc
{
    public class CodeDocMemberDataProvider
    {

        public CodeDocMemberDataProvider(ICodeDocMember core) {
            Core = core;
        }

        public ICodeDocMember Core { get; private set; }



    }
}
