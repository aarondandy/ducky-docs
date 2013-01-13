using System;
using System.Web.Mvc;
using DandyDoc.SimpleModels;
using DandyDoc.SimpleModels.Contracts;
using DandyDocSite.Infrastructure;

namespace DandyDocSite.Controllers
{
	public class DocsController : Controller
	{

		public DocsController(ApiDocNavigation navigation){
			Navigation = navigation;
		}

		public ApiDocNavigation Navigation { get; set; }

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Api(string cref){
			ViewBag.TypeNavigationViewModel = Navigation.NavigationRepository;

			if (String.IsNullOrEmpty(cref))
				return View("Api/Index", Navigation.NavigationRepository);

			var assemblies = StructureMap.ObjectFactory.GetInstance<AssemblyCollectionGenerator>().GenerateDefinitions();
			var repository = new SimpleModelRepository(assemblies);

			var model = repository.GetModelFromCref(cref);

			if(null == model)
				return new HttpNotFoundResult();
			if (model is INamespaceSimpleModel)
				return View("Api/Namespace", (INamespaceSimpleModel)model);
			if(model is IDelegateSimpleModel)
				throw new NotImplementedException();
			if (model is ITypeSimpleModel)
				return View("Api/Type", (ITypeSimpleModel)model);

			return new HttpNotFoundResult();
		}

	}
}
