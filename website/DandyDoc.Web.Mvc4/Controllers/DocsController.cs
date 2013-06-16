using System;
using System.Linq;
using System.Web.Mvc;
using DandyDoc.CRef;
using DandyDoc.CodeDoc;

namespace DandyDoc.Web.Mvc4.Controllers
{
    public class DocsController : Controller
    {

        public DocsController(MvcApplication.CodeDocRepositories codeDocRepositories) {
            CodeDocRepositories = codeDocRepositories;
        }

        MvcApplication.CodeDocRepositories CodeDocRepositories { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Api(string cRef) {
            var targetRepository = CodeDocRepositories.TargetRepository;
            ViewBag.CodeDocEntityRepository = targetRepository;
            ViewBag.CRefToModel = new Func<CRefIdentifier,ICodeDocMember>(searchCRef => CodeDocRepositories.CreateSearchContext().Search(searchCRef));

            if (String.IsNullOrWhiteSpace(cRef))
                return View("Api/Index", targetRepository);

            var searchContext = CodeDocRepositories
                .CreateSearchContext()
                .CloneWithOneUnvisited(targetRepository);

            var cRefIdentifier = new CRefIdentifier(cRef);
            var model = searchContext.Search(cRefIdentifier);
            if (model == null)
                return HttpNotFound();

            if (model is CodeDocNamespace)
                return View("Api/Namespace", (CodeDocNamespace)model);

            if (model is CodeDocType) {
                var codeDocType = (CodeDocType)model;
                if (codeDocType is CodeDocDelegate)
                    return View("Api/Delegate", (CodeDocDelegate)codeDocType);
                if (codeDocType.IsEnum.GetValueOrDefault())
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
