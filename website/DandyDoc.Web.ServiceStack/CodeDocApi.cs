using System;
using System.Diagnostics.Contracts;
using System.Net;
using DandyDoc.CodeDoc;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace DandyDoc.Web.ServiceStack
{


    [Api("Provides documentation models.")]
    [Route("/Docs/Api")]
    public class CodeDocCRefRequest
    {
        public string CRef { get; set; }
    }

    public class CodeDocApi : Service
    {

        public CodeDocRepositories Repositories { get; set; }

        public CodeDocSimpleMember Any(CodeDocCRefRequest request) {
            if(request == null) throw new ArgumentNullException("request");
            if(String.IsNullOrEmpty(request.CRef)) throw new ArgumentException("Code reference (CRef) not provided.","request");
            Contract.EndContractBlock();
            var model = Repositories.GetModel(request.CRef) as CodeDocSimpleMember;
            if (model == null)
                throw new HttpError(HttpStatusCode.NotFound, "Documentation model could not be found.");
            return model;
        }

    }
}