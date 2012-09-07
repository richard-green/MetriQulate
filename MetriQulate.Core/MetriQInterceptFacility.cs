using Castle.Core;
using Castle.MicroKernel.Facilities;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace MetriQulate.Core
{
	public class MetriQInterceptFacility : AbstractFacility
	{
		private List<Regex> patterns = new List<Regex>();

		protected override void Init()
		{
			Kernel.ComponentRegistered += new Castle.MicroKernel.ComponentDataDelegate(Kernel_ComponentRegistered);
		}

		private void Kernel_ComponentRegistered(string key, Castle.MicroKernel.IHandler handler)
		{
			// Profile matching components / namespaces, except those from MetriQulate.Core
			if (patterns.Exists(where => where.IsMatch(handler.ComponentModel.Name) && handler.ComponentModel.Name.StartsWith(this.GetType().Namespace) == false))
			{
				handler.ComponentModel.Interceptors.Add(InterceptorReference.ForType<MetriQInterceptor>());
			}
		}

		public MetriQInterceptFacility ForComponent(string component)
		{
			patterns.Add(new Regex(String.Format("^{0}$", Regex.Escape(component))));
			return this;
		}

		public MetriQInterceptFacility ForComponentsInNamespace(string ns)
		{
			patterns.Add(new Regex(String.Format("^{0}\\..*$", Regex.Escape(ns))));
			return this;
		}
	}
}
