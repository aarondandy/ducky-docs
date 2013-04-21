using System;
using System.Web.Mvc;
using DandyDoc.CRef;
using DandyDoc.CodeDoc;

namespace DandyDoc.Web.Mvc4.Controllers
{
    public class DocsController : Controller
    {

        public DocsController(ICodeDocEntityRepository codeDocEntityRepository){
            CodeDocEntityRepository = codeDocEntityRepository;
        }

        ICodeDocEntityRepository CodeDocEntityRepository { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Api(string cRef) {
            ViewBag.CodeDocEntityRepository = CodeDocEntityRepository;

            if (String.IsNullOrWhiteSpace(cRef))
                return View("Api/Index", CodeDocEntityRepository);

            var cRefIdentifier = new CRefIdentifier(cRef);
            var model = CodeDocEntityRepository.GetContentEntity(cRefIdentifier);
            if (model == null)
                return HttpNotFound();

            if (model is ICodeDocNamespace)
                return View("Api/Namespace", model);

            if (model is ICodeDocType) {
                var codeDocType = (ICodeDocType)model;
                if (codeDocType is ICodeDocDelegate)
                    return View("Api/Delegate", codeDocType);
                if (codeDocType.IsEnum)
                    return View("Api/Enum", codeDocType);
                return View("Api/Type", codeDocType);
            }

            if (model is ICodeDocEvent)
                return View("Api/Event", model);
            if (model is ICodeDocField)
                return View("Api/Field", model);
            if (model is ICodeDocMethod)
                return View("Api/Method", model);

            return HttpNotFound();
        }
    }
}
