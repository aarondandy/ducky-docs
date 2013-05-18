using System;
using System.Web.Mvc;
using DandyDoc.CRef;
using DandyDoc.CodeDoc;

namespace DandyDoc.Web.Mvc4.Controllers
{
    public class DocsController : Controller
    {

        public DocsController(ICodeDocMemberRepository codeDocMemberRepository){
            CodeDocMemberRepository = codeDocMemberRepository;
        }

        ICodeDocMemberRepository CodeDocMemberRepository { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Api(string cRef) {
            ViewBag.CodeDocEntityRepository = CodeDocMemberRepository;

            if (String.IsNullOrWhiteSpace(cRef))
                return View("Api/Index", CodeDocMemberRepository);

            var cRefIdentifier = new CRefIdentifier(cRef);
            var model = CodeDocMemberRepository.GetContentMember(cRefIdentifier);
            if (model == null)
                return HttpNotFound();

            if (model is CodeDocNamespace)
                return View("Api/Namespace", (CodeDocNamespace)model);

            if (model is CodeDocType) {
                var codeDocType = (CodeDocType)model;
                if (codeDocType is CodeDocDelegate)
                    return View("Api/Delegate", (CodeDocDelegate)codeDocType);
                if (codeDocType.IsEnum)
                    return View("Api/Enum", codeDocType);
                return View("Api/Type", codeDocType);
            }

            if (model is CodeDocEvent)
                return View("Api/Event", (CodeDocEvent)model);
            if (model is CodeDocField)
                return View("Api/Field", (CodeDocField)model);
            if (model is CodeDocMethod)
                return View("Api/Method", (CodeDocMethod)model);
            if (model is CodeDocProperty)
                return View("Api/Property", (CodeDocProperty)model);

            return HttpNotFound();
        }
    }
}
