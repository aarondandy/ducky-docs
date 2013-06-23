using System;
using System.Web;
using DandyDoc.CRef;
using DandyDoc.CodeDoc;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace DandyDoc.Web.ServiceStack
{



    [Route("/Docs/Api")]
    public class CodeDocCRefRequest
    {
        public string CRef { get; set; }
    }

    public class CodeDocApi : Service
    {

        public CodeDocRepositories Repositories { get; set; }

        public CodeDocSimpleMember Any(CodeDocCRefRequest request) {
            var model = Repositories.GetModel(request.CRef) as CodeDocSimpleMember;
            if(model == null)
                throw new HttpException(404,"Documentation model not found.");
            return model;
        }

    }
}