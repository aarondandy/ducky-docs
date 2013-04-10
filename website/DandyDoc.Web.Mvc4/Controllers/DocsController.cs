using System;
using System.Web.Mvc;
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

            throw new NotImplementedException();
        }
    }
}
