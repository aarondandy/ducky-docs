using System;
using System.Web.Mvc;
using DandyDoc.Overlays.MsdnLinks;
using DandyDoc.SimpleModels.Contracts;
using DandyDocSite.Infrastructure;

namespace DandyDocSite.Controllers
{
	public class DocsController : Controller
	{

		public DocsController(ISimpleModelRepository docModelRepository, IMsdnLinkOverlay msdnLinkOverlay){
			DocModelRepository = docModelRepository;
			MsdnLinkOverlay = msdnLinkOverlay;
		}

		public ISimpleModelRepository DocModelRepository { get; set; }

		public IMsdnLinkOverlay MsdnLinkOverlay { get; set; }

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Api(string cRef){
			ViewBag.TypeNavigationViewModel = DocModelRepository;
			ViewBag.ApiDocLinkResolver = new ApiDocLinkResolver {
				UrlHelper = Url,
				MsdnLinkOverlay = MsdnLinkOverlay,
				LocalRepository = DocModelRepository
			};

			if (String.IsNullOrEmpty(cRef))
				return View("Api/Index", DocModelRepository);

			var model = DocModelRepository.GetModelFromCref(cRef);
			if (model == null)
				return new HttpNotFoundResult();

			if (model is ITypeSimpleModel){
				if (model is IDelegateSimpleModel)
					return View("Api/Delegate", (IDelegateSimpleModel)model);
				
				var typeModel = (ITypeSimpleModel) model;
				return View(typeModel.IsEnum ? "Api/Enum" : "Api/Type", typeModel);
			}

			if (model is INamespaceSimpleModel)
				return View("Api/Namespace", (INamespaceSimpleModel)model);
			if (model is IMethodSimpleModel)
				return View("Api/Method", (IMethodSimpleModel)model);
			if (model is IFieldSimpleModel)
				return View("Api/Field", (IFieldSimpleModel)model);
			if (model is IPropertySimpleModel)
				return View("Api/Property", (IPropertySimpleModel)model);
			if (model is IEventSimpleModel)
				return View("Api/Event", (IEventSimpleModel)model);

			return new HttpNotFoundResult();
		}

	}
}
