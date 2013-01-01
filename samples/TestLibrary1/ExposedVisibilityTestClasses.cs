using System;

#pragma warning disable 1591,0067,0169,0649

namespace TestLibrary1
{
	public class PublicExposedTestClass
	{

		private class PrivateClass
		{
			private delegate void PrivateDelegate();
			protected delegate void ProtectedDelegate();
			public delegate void PublicDelegate();
			internal delegate void InternalDelegate();
			protected internal delegate void ProtectedInternalDelegate();

			private void PrivateMethod() { throw new NotImplementedException(); }
			protected void ProtectedMethod() { throw new NotImplementedException(); }
			public void PublicMethod() { throw new NotImplementedException(); }
			internal void InternalMethod() { throw new NotImplementedException(); }
			protected internal void ProtectedInternalMethod() { throw new NotImplementedException(); }

			private int PrivateField;
			protected int ProtectedField;
			public int PublicField;
			internal int InternalField;
			protected internal int ProtectedInternalField;

			public int PropPubPub { get; set; }
			public int PropPubPro { get; protected set; }
			public int PropPubPri { get; private set; }
			public int PropPubInt { get; internal set; }
			public int PropPubPin { get; protected internal set; }

			public int PropProPub { protected get; set; }
			protected int PropProPro { get; set; }
			protected int PropProPri { get; private set; }
			protected internal int PropProPin { protected get; set; }

			public int PropPriPub { private get; set; }
			protected int PropPriPro { private get; set; }
			private int PropPriPri { get; set; }
			protected internal int PropPriPin { private get; set; }

			public int PropIntPub { internal get; set; }
			internal int PropIntPri { get; private set; }
			internal int PropIntInt { get; set; }
			protected internal int PropIntPin { internal get; set; }

			public int PropPinPub { protected internal get; set; }
			protected internal int PropPinPro { get; protected set; }
			protected internal int PropPinPri { get; private set; }
			protected internal int PropPinPin { get; set; }

			private event PrivateDelegate PrivateEvent;
			protected event ProtectedDelegate ProtectedEvent;
			public event PublicDelegate PublicEvent;
			internal event InternalDelegate InternalEvent;
			protected internal event ProtectedInternalDelegate ProtectedInternalEvent;
		}

		protected class ProtectedClass
		{
			private delegate void PrivateDelegate();
			protected delegate void ProtectedDelegate();
			public delegate void PublicDelegate();
			internal delegate void InternalDelegate();
			protected internal delegate void ProtectedInternalDelegate();

			private void PrivateMethod() { throw new NotImplementedException(); }
			protected void ProtectedMethod() { throw new NotImplementedException(); }
			public void PublicMethod() { throw new NotImplementedException(); }
			internal void InternalMethod() { throw new NotImplementedException(); }
			protected internal void ProtectedInternalMethod() { throw new NotImplementedException(); }

			private int PrivateField;
			protected int ProtectedField;
			public int PublicField;
			internal int InternalField;
			protected internal int ProtectedInternalField;

			public int PropPubPub { get; set; }
			public int PropPubPro { get; protected set; }
			public int PropPubPri { get; private set; }
			public int PropPubInt { get; internal set; }
			public int PropPubPin { get; protected internal set; }

			public int PropProPub { protected get; set; }
			protected int PropProPro { get; set; }
			protected int PropProPri { get; private set; }
			protected internal int PropProPin { protected get; set; }

			public int PropPriPub { private get; set; }
			protected int PropPriPro { private get; set; }
			private int PropPriPri { get; set; }
			protected internal int PropPriPin { private get; set; }

			public int PropIntPub { internal get; set; }
			internal int PropIntPri { get; private set; }
			internal int PropIntInt { get; set; }
			protected internal int PropIntPin { internal get; set; }

			public int PropPinPub { protected internal get; set; }
			protected internal int PropPinPro { get; protected set; }
			protected internal int PropPinPri { get; private set; }
			protected internal int PropPinPin { get; set; }

			private event PrivateDelegate PrivateEvent;
			protected event ProtectedDelegate ProtectedEvent;
			public event PublicDelegate PublicEvent;
			internal event InternalDelegate InternalEvent;
			protected internal event ProtectedInternalDelegate ProtectedInternalEvent;
		}

		public class PublicClass
		{
			private delegate void PrivateDelegate();
			protected delegate void ProtectedDelegate();
			public delegate void PublicDelegate();
			internal delegate void InternalDelegate();
			protected internal delegate void ProtectedInternalDelegate();

			private void PrivateMethod() { throw new NotImplementedException(); }
			protected void ProtectedMethod() { throw new NotImplementedException(); }
			public void PublicMethod() { throw new NotImplementedException(); }
			internal void InternalMethod() { throw new NotImplementedException(); }
			protected internal void ProtectedInternalMethod() { throw new NotImplementedException(); }

			private int PrivateField;
			protected int ProtectedField;
			public int PublicField;
			internal int InternalField;
			protected internal int ProtectedInternalField;

			public int PropPubPub { get; set; }
			public int PropPubPro { get; protected set; }
			public int PropPubPri { get; private set; }
			public int PropPubInt { get; internal set; }
			public int PropPubPin { get; protected internal set; }

			public int PropProPub { protected get; set; }
			protected int PropProPro { get; set; }
			protected int PropProPri { get; private set; }
			protected internal int PropProPin { protected get; set; }

			public int PropPriPub { private get; set; }
			protected int PropPriPro { private get; set; }
			private int PropPriPri { get; set; }
			protected internal int PropPriPin { private get; set; }

			public int PropIntPub { internal get; set; }
			internal int PropIntPri { get; private set; }
			internal int PropIntInt { get; set; }
			protected internal int PropIntPin { internal get; set; }

			public int PropPinPub { protected internal get; set; }
			protected internal int PropPinPro { get; protected set; }
			protected internal int PropPinPri { get; private set; }
			protected internal int PropPinPin { get; set; }

			private event PrivateDelegate PrivateEvent;
			protected event ProtectedDelegate ProtectedEvent;
			public event PublicDelegate PublicEvent;
			internal event InternalDelegate InternalEvent;
			protected internal event ProtectedInternalDelegate ProtectedInternalEvent;
		}

		internal class InternalClass
		{
			private delegate void PrivateDelegate();
			protected delegate void ProtectedDelegate();
			public delegate void PublicDelegate();
			internal delegate void InternalDelegate();
			protected internal delegate void ProtectedInternalDelegate();

			private void PrivateMethod() { throw new NotImplementedException(); }
			protected void ProtectedMethod() { throw new NotImplementedException(); }
			public void PublicMethod() { throw new NotImplementedException(); }
			internal void InternalMethod() { throw new NotImplementedException(); }
			protected internal void ProtectedInternalMethod() { throw new NotImplementedException(); }

			private int PrivateField;
			protected int ProtectedField;
			public int PublicField;
			internal int InternalField;
			protected internal int ProtectedInternalField;

			public int PropPubPub { get; set; }
			public int PropPubPro { get; protected set; }
			public int PropPubPri { get; private set; }
			public int PropPubInt { get; internal set; }
			public int PropPubPin { get; protected internal set; }

			public int PropProPub { protected get; set; }
			protected int PropProPro { get; set; }
			protected int PropProPri { get; private set; }
			protected internal int PropProPin { protected get; set; }

			public int PropPriPub { private get; set; }
			protected int PropPriPro { private get; set; }
			private int PropPriPri { get; set; }
			protected internal int PropPriPin { private get; set; }

			public int PropIntPub { internal get; set; }
			internal int PropIntPri { get; private set; }
			internal int PropIntInt { get; set; }
			protected internal int PropIntPin { internal get; set; }

			public int PropPinPub { protected internal get; set; }
			protected internal int PropPinPro { get; protected set; }
			protected internal int PropPinPri { get; private set; }
			protected internal int PropPinPin { get; set; }

			private event PrivateDelegate PrivateEvent;
			protected event ProtectedDelegate ProtectedEvent;
			public event PublicDelegate PublicEvent;
			internal event InternalDelegate InternalEvent;
			protected internal event ProtectedInternalDelegate ProtectedInternalEvent;
		}

		protected internal class ProtectedInternalClass
		{
			private delegate void PrivateDelegate();
			protected delegate void ProtectedDelegate();
			public delegate void PublicDelegate();
			internal delegate void InternalDelegate();
			protected internal delegate void ProtectedInternalDelegate();

			private void PrivateMethod() { throw new NotImplementedException(); }
			protected void ProtectedMethod() { throw new NotImplementedException(); }
			public void PublicMethod() { throw new NotImplementedException(); }
			internal void InternalMethod() { throw new NotImplementedException(); }
			protected internal void ProtectedInternalMethod() { throw new NotImplementedException(); }

			private int PrivateField;
			protected int ProtectedField;
			public int PublicField;
			internal int InternalField;
			protected internal int ProtectedInternalField;

			public int PropPubPub { get; set; }
			public int PropPubPro { get; protected set; }
			public int PropPubPri { get; private set; }
			public int PropPubInt { get; internal set; }
			public int PropPubPin { get; protected internal set; }

			public int PropProPub { protected get; set; }
			protected int PropProPro { get; set; }
			protected int PropProPri { get; private set; }
			protected internal int PropProPin { protected get; set; }

			public int PropPriPub { private get; set; }
			protected int PropPriPro { private get; set; }
			private int PropPriPri { get; set; }
			protected internal int PropPriPin { private get; set; }

			public int PropIntPub { internal get; set; }
			internal int PropIntPri { get; private set; }
			internal int PropIntInt { get; set; }
			protected internal int PropIntPin { internal get; set; }

			public int PropPinPub { protected internal get; set; }
			protected internal int PropPinPro { get; protected set; }
			protected internal int PropPinPri { get; private set; }
			protected internal int PropPinPin { get; set; }

			private event PrivateDelegate PrivateEvent;
			protected event ProtectedDelegate ProtectedEvent;
			public event PublicDelegate PublicEvent;
			internal event InternalDelegate InternalEvent;
			protected internal event ProtectedInternalDelegate ProtectedInternalEvent;
		}

		private delegate void PrivateDelegate();
		protected delegate void ProtectedDelegate();
		public delegate void PublicDelegate();
		internal delegate void InternalDelegate();
		protected internal delegate void ProtectedInternalDelegate();

		private void PrivateMethod(){ throw new NotImplementedException();}
		protected void ProtectedMethod() { throw new NotImplementedException();}
		public void PublicMethod() { throw new NotImplementedException();}
		internal void InternalMethod() { throw new NotImplementedException(); }
		protected internal void ProtectedInternalMethod() { throw new NotImplementedException();}

		private int PrivateField;
		protected int ProtectedField;
		public int PublicField;
		internal int InternalField;
		protected internal int ProtectedInternalField;

		public int PropPubPub { get; set; }
		public int PropPubPro { get; protected set; }
		public int PropPubPri { get; private set; }
		public int PropPubInt { get; internal set; }
		public int PropPubPin { get; protected internal set; }

		public int PropProPub { protected get; set; }
		protected int PropProPro { get; set; }
		protected int PropProPri { get; private set; }
		protected internal int PropProPin { protected get; set; }
		
		public int PropPriPub { private get; set; }
		protected int PropPriPro { private get; set; }
		private int PropPriPri { get; set; }
		protected internal int PropPriPin { private get; set; }

		public int PropIntPub { internal get; set; }
		internal int PropIntPri { get; private set; }
		internal int PropIntInt { get; set; }
		protected internal int PropIntPin { internal get; set; }

		public int PropPinPub { protected internal get; set; }
		protected internal int PropPinPro { get; protected set; }
		protected internal int PropPinPri { get; private set; }
		protected internal int PropPinPin { get; set; }

		private event PrivateDelegate PrivateEvent;
		protected event ProtectedDelegate ProtectedEvent;
		public event PublicDelegate PublicEvent;
		internal event InternalDelegate InternalEvent;
		protected internal event ProtectedInternalDelegate ProtectedInternalEvent;

	}

	internal class InternalExposedTestClass
	{


		private class PrivateClass
		{
			private delegate void PrivateDelegate();
			protected delegate void ProtectedDelegate();
			public delegate void PublicDelegate();
			internal delegate void InternalDelegate();
			protected internal delegate void ProtectedInternalDelegate();

			private void PrivateMethod() { throw new NotImplementedException(); }
			protected void ProtectedMethod() { throw new NotImplementedException(); }
			public void PublicMethod() { throw new NotImplementedException(); }
			internal void InternalMethod() { throw new NotImplementedException(); }
			protected internal void ProtectedInternalMethod() { throw new NotImplementedException(); }

			private int PrivateField;
			protected int ProtectedField;
			public int PublicField;
			internal int InternalField;
			protected internal int ProtectedInternalField;

			public int PropPubPub { get; set; }
			public int PropPubPro { get; protected set; }
			public int PropPubPri { get; private set; }
			public int PropPubInt { get; internal set; }
			public int PropPubPin { get; protected internal set; }

			public int PropProPub { protected get; set; }
			protected int PropProPro { get; set; }
			protected int PropProPri { get; private set; }
			protected internal int PropProPin { protected get; set; }

			public int PropPriPub { private get; set; }
			protected int PropPriPro { private get; set; }
			private int PropPriPri { get; set; }
			protected internal int PropPriPin { private get; set; }

			public int PropIntPub { internal get; set; }
			internal int PropIntPri { get; private set; }
			internal int PropIntInt { get; set; }
			protected internal int PropIntPin { internal get; set; }

			public int PropPinPub { protected internal get; set; }
			protected internal int PropPinPro { get; protected set; }
			protected internal int PropPinPri { get; private set; }
			protected internal int PropPinPin { get; set; }

			private event PrivateDelegate PrivateEvent;
			protected event ProtectedDelegate ProtectedEvent;
			public event PublicDelegate PublicEvent;
			internal event InternalDelegate InternalEvent;
			protected internal event ProtectedInternalDelegate ProtectedInternalEvent;
		}

		protected class ProtectedClass
		{
			private delegate void PrivateDelegate();
			protected delegate void ProtectedDelegate();
			public delegate void PublicDelegate();
			internal delegate void InternalDelegate();
			protected internal delegate void ProtectedInternalDelegate();

			private void PrivateMethod() { throw new NotImplementedException(); }
			protected void ProtectedMethod() { throw new NotImplementedException(); }
			public void PublicMethod() { throw new NotImplementedException(); }
			internal void InternalMethod() { throw new NotImplementedException(); }
			protected internal void ProtectedInternalMethod() { throw new NotImplementedException(); }

			private int PrivateField;
			protected int ProtectedField;
			public int PublicField;
			internal int InternalField;
			protected internal int ProtectedInternalField;

			public int PropPubPub { get; set; }
			public int PropPubPro { get; protected set; }
			public int PropPubPri { get; private set; }
			public int PropPubInt { get; internal set; }
			public int PropPubPin { get; protected internal set; }

			public int PropProPub { protected get; set; }
			protected int PropProPro { get; set; }
			protected int PropProPri { get; private set; }
			protected internal int PropProPin { protected get; set; }

			public int PropPriPub { private get; set; }
			protected int PropPriPro { private get; set; }
			private int PropPriPri { get; set; }
			protected internal int PropPriPin { private get; set; }

			public int PropIntPub { internal get; set; }
			internal int PropIntPri { get; private set; }
			internal int PropIntInt { get; set; }
			protected internal int PropIntPin { internal get; set; }

			public int PropPinPub { protected internal get; set; }
			protected internal int PropPinPro { get; protected set; }
			protected internal int PropPinPri { get; private set; }
			protected internal int PropPinPin { get; set; }

			private event PrivateDelegate PrivateEvent;
			protected event ProtectedDelegate ProtectedEvent;
			public event PublicDelegate PublicEvent;
			internal event InternalDelegate InternalEvent;
			protected internal event ProtectedInternalDelegate ProtectedInternalEvent;
		}

		public class PublicClass
		{
			private delegate void PrivateDelegate();
			protected delegate void ProtectedDelegate();
			public delegate void PublicDelegate();
			internal delegate void InternalDelegate();
			protected internal delegate void ProtectedInternalDelegate();

			private void PrivateMethod() { throw new NotImplementedException(); }
			protected void ProtectedMethod() { throw new NotImplementedException(); }
			public void PublicMethod() { throw new NotImplementedException(); }
			internal void InternalMethod() { throw new NotImplementedException(); }
			protected internal void ProtectedInternalMethod() { throw new NotImplementedException(); }

			private int PrivateField;
			protected int ProtectedField;
			public int PublicField;
			internal int InternalField;
			protected internal int ProtectedInternalField;

			public int PropPubPub { get; set; }
			public int PropPubPro { get; protected set; }
			public int PropPubPri { get; private set; }
			public int PropPubInt { get; internal set; }
			public int PropPubPin { get; protected internal set; }

			public int PropProPub { protected get; set; }
			protected int PropProPro { get; set; }
			protected int PropProPri { get; private set; }
			protected internal int PropProPin { protected get; set; }

			public int PropPriPub { private get; set; }
			protected int PropPriPro { private get; set; }
			private int PropPriPri { get; set; }
			protected internal int PropPriPin { private get; set; }

			public int PropIntPub { internal get; set; }
			internal int PropIntPri { get; private set; }
			internal int PropIntInt { get; set; }
			protected internal int PropIntPin { internal get; set; }

			public int PropPinPub { protected internal get; set; }
			protected internal int PropPinPro { get; protected set; }
			protected internal int PropPinPri { get; private set; }
			protected internal int PropPinPin { get; set; }

			private event PrivateDelegate PrivateEvent;
			protected event ProtectedDelegate ProtectedEvent;
			public event PublicDelegate PublicEvent;
			internal event InternalDelegate InternalEvent;
			protected internal event ProtectedInternalDelegate ProtectedInternalEvent;
		}

		internal class InternalClass
		{
			private delegate void PrivateDelegate();
			protected delegate void ProtectedDelegate();
			public delegate void PublicDelegate();
			internal delegate void InternalDelegate();
			protected internal delegate void ProtectedInternalDelegate();

			private void PrivateMethod() { throw new NotImplementedException(); }
			protected void ProtectedMethod() { throw new NotImplementedException(); }
			public void PublicMethod() { throw new NotImplementedException(); }
			internal void InternalMethod() { throw new NotImplementedException(); }
			protected internal void ProtectedInternalMethod() { throw new NotImplementedException(); }

			private int PrivateField;
			protected int ProtectedField;
			public int PublicField;
			internal int InternalField;
			protected internal int ProtectedInternalField;

			public int PropPubPub { get; set; }
			public int PropPubPro { get; protected set; }
			public int PropPubPri { get; private set; }
			public int PropPubInt { get; internal set; }
			public int PropPubPin { get; protected internal set; }

			public int PropProPub { protected get; set; }
			protected int PropProPro { get; set; }
			protected int PropProPri { get; private set; }
			protected internal int PropProPin { protected get; set; }

			public int PropPriPub { private get; set; }
			protected int PropPriPro { private get; set; }
			private int PropPriPri { get; set; }
			protected internal int PropPriPin { private get; set; }

			public int PropIntPub { internal get; set; }
			internal int PropIntPri { get; private set; }
			internal int PropIntInt { get; set; }
			protected internal int PropIntPin { internal get; set; }

			public int PropPinPub { protected internal get; set; }
			protected internal int PropPinPro { get; protected set; }
			protected internal int PropPinPri { get; private set; }
			protected internal int PropPinPin { get; set; }

			private event PrivateDelegate PrivateEvent;
			protected event ProtectedDelegate ProtectedEvent;
			public event PublicDelegate PublicEvent;
			internal event InternalDelegate InternalEvent;
			protected internal event ProtectedInternalDelegate ProtectedInternalEvent;
		}

		protected internal class ProtectedInternalClass
		{
			private delegate void PrivateDelegate();
			protected delegate void ProtectedDelegate();
			public delegate void PublicDelegate();
			internal delegate void InternalDelegate();
			protected internal delegate void ProtectedInternalDelegate();

			private void PrivateMethod() { throw new NotImplementedException(); }
			protected void ProtectedMethod() { throw new NotImplementedException(); }
			public void PublicMethod() { throw new NotImplementedException(); }
			internal void InternalMethod() { throw new NotImplementedException(); }
			protected internal void ProtectedInternalMethod() { throw new NotImplementedException(); }

			private int PrivateField;
			protected int ProtectedField;
			public int PublicField;
			internal int InternalField;
			protected internal int ProtectedInternalField;

			public int PropPubPub { get; set; }
			public int PropPubPro { get; protected set; }
			public int PropPubPri { get; private set; }
			public int PropPubInt { get; internal set; }
			public int PropPubPin { get; protected internal set; }

			public int PropProPub { protected get; set; }
			protected int PropProPro { get; set; }
			protected int PropProPri { get; private set; }
			protected internal int PropProPin { protected get; set; }

			public int PropPriPub { private get; set; }
			protected int PropPriPro { private get; set; }
			private int PropPriPri { get; set; }
			protected internal int PropPriPin { private get; set; }

			public int PropIntPub { internal get; set; }
			internal int PropIntPri { get; private set; }
			internal int PropIntInt { get; set; }
			protected internal int PropIntPin { internal get; set; }

			public int PropPinPub { protected internal get; set; }
			protected internal int PropPinPro { get; protected set; }
			protected internal int PropPinPri { get; private set; }
			protected internal int PropPinPin { get; set; }

			private event PrivateDelegate PrivateEvent;
			protected event ProtectedDelegate ProtectedEvent;
			public event PublicDelegate PublicEvent;
			internal event InternalDelegate InternalEvent;
			protected internal event ProtectedInternalDelegate ProtectedInternalEvent;
		}

		private delegate void PrivateDelegate();
		protected delegate void ProtectedDelegate();
		public delegate void PublicDelegate();
		internal delegate void InternalDelegate();
		protected internal delegate void ProtectedInternalDelegate();

		private void PrivateMethod() { throw new NotImplementedException(); }
		protected void ProtectedMethod() { throw new NotImplementedException(); }
		public void PublicMethod() { throw new NotImplementedException(); }
		internal void InternalMethod() { throw new NotImplementedException(); }
		protected internal void ProtectedInternalMethod() { throw new NotImplementedException(); }

		private int PrivateField;
		protected int ProtectedField;
		public int PublicField;
		internal int InternalField;
		protected internal int ProtectedInternalField;

		public int PropPubPub { get; set; }
		public int PropPubPro { get; protected set; }
		public int PropPubPri { get; private set; }
		public int PropPubInt { get; internal set; }
		public int PropPubPin { get; protected internal set; }

		public int PropProPub { protected get; set; }
		protected int PropProPro { get; set; }
		protected int PropProPri { get; private set; }
		protected internal int PropProPin { protected get; set; }

		public int PropPriPub { private get; set; }
		protected int PropPriPro { private get; set; }
		private int PropPriPri { get; set; }
		protected internal int PropPriPin { private get; set; }

		public int PropIntPub { internal get; set; }
		internal int PropIntPri { get; private set; }
		internal int PropIntInt { get; set; }
		protected internal int PropIntPin { internal get; set; }

		public int PropPinPub { protected internal get; set; }
		protected internal int PropPinPro { get; protected set; }
		protected internal int PropPinPri { get; private set; }
		protected internal int PropPinPin { get; set; }

		private event PrivateDelegate PrivateEvent;
		protected event ProtectedDelegate ProtectedEvent;
		public event PublicDelegate PublicEvent;
		internal event InternalDelegate InternalEvent;
		protected internal event ProtectedInternalDelegate ProtectedInternalEvent;
	}

}
