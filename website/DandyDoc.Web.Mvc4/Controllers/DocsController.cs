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
            ViewBag.TypeNavigationViewModel = CodeDocEntityRepository;

            if (String.IsNullOrWhiteSpace(cRef))
                return View("Api/Index", CodeDocEntityRepository);

            var cRefIdentifier = new CRefIdentifier(cRef);
            var model = CodeDocEntityRepository.GetContentEntity(cRefIdentifier);
            if (model is ICodeDocNamespace)
                return View("Api/Namespace", model);

            throw new NotImplementedException();
        }
    }
}
