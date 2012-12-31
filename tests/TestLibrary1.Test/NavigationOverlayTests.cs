using System.Linq;
using DandyDoc;
using DandyDoc.Overlays.Navigation;
using NUnit.Framework;

namespace TestLibrary1.Test
{
	[TestFixture]
	public class NavigationOverlayTests
	{

		[Test]
		public void simple_namespace_overlay_test(){
			var assemblyCollection = new AssemblyDefinitionCollection("./TestLibrary1.dll");
			var navigationOverlay = new NavigationOverlay(assemblyCollection);
			Assert.Greater(navigationOverlay.Namespaces.Count, 0);
			Assert.That(navigationOverlay.Namespaces.Select(x => x.Types.Count), Has.All.GreaterThan(0));
		}

	}
}
