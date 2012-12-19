using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace SimpleApp1.Test
{
	[TestFixture]
	public class AssemblyLoadTests
	{

		[Test]
		public void LoadReflectionAssembly() {
			var result = Assembly.ReflectionOnlyLoadFrom("./samples/SimpleApp1.exe");
			Assert.IsNotNull(result);
		}

		[Test]
		[Explicit]
		public void LoadFullAssembly() {
			var result = Assembly.LoadFile("./samples/SimpleApp1.exe");
			Assert.IsNotNull(result);
		}

	}
}
