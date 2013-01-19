using System;
using System.Linq;
using System.Web.Mvc;
using DandyDoc.Overlays.MsdnLinks;
using DandyDoc.SimpleModels;
using DandyDoc.SimpleModels.Contracts;
using DandyDocSite.Infrastructure;

namespace DandyDocSite.Controllers
{
	public class DocsController : Controller
	{

		public DocsController(ApiDocNavigation navigation, IMsdnLinkOverlay msdnLinkOverlay){
			Navigation = navigation;
			MsdnLinkOverlay = msdnLinkOverlay;
		}

		public ApiDocNavigation Navigation { get; set; }

		public IMsdnLinkOverlay MsdnLinkOverlay { get; set; }

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Api(string cRef){
			var assemblies = StructureMap.ObjectFactory.GetInstance<AssemblyCollectionGenerator>().GenerateDefinitions();
			var repository = new SimpleModelRepository(assemblies);

			ViewBag.TypeNavigationViewModel = Navigation.NavigationRepository;
			ViewBag.ApiDocLinkResolver = new ApiDocLinkResolver {
				UrlHelper = Url,
				MsdnLinkOverlay = MsdnLinkOverlay,
				LocalRepository = repository
			};

			if (String.IsNullOrEmpty(cRef))
				return View("Api/Index", Navigation.NavigationRepository);

			var model = repository.GetModelFromCref(cRef);
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
