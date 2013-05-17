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
                return View("Api/Namespace", model);

            if (model is CodeDocType) {
                var codeDocType = (CodeDocType)model;
                if (codeDocType is CodeDocDelegate)
                    return View("Api/Delegate", codeDocType);
                if (codeDocType.IsEnum)
                    return View("Api/Enum", codeDocType);
                return View("Api/Type", codeDocType);
            }

            if (model is CodeDocEvent)
                return View("Api/Event", model);
            if (model is CodeDocField)
                return View("Api/Field", model);
            if (model is CodeDocMethod)
                return View("Api/Method", model);
            if (model is CodeDocProperty)
                return View("Api/Property", model);

            return HttpNotFound();
        }
    }
}
