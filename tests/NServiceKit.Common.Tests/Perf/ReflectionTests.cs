using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NServiceKit.Common.Tests.Models;

namespace NServiceKit.Common.Tests.Perf
{
    /// <summary>A reflection tests.</summary>
	[Ignore("Benchmark for comparing expressions / delegates around generic methods.")]
	[TestFixture]
	public class ReflectionTests
		: PerfTestBase
	{
        /// <summary>Initializes a new instance of the NServiceKit.Common.Tests.Perf.ReflectionTests class.</summary>
		public ReflectionTests()
			: base()
		{
			this.MultipleIterations = new List<int> { 100000000 };
		}

        /// <summary>Gets property value method via expressions.</summary>
        ///
        /// <param name="type">        The type.</param>
        /// <param name="propertyInfo">Information describing the property.</param>
        ///
        /// <returns>The property value method via expressions.</returns>
		public static Func<object, object> GetPropertyValueMethodViaExpressions(
			Type type, PropertyInfo propertyInfo)
		{
			var getMethodInfo = propertyInfo.GetGetMethod();
			var oInstanceParam = Expression.Parameter(typeof(object), "oInstanceParam");
			var instanceParam = Expression.Convert(oInstanceParam, type);

			var exprCallPropertyGetFn = Expression.Call(instanceParam, getMethodInfo);
			var oExprCallPropertyGetFn = Expression.Convert(exprCallPropertyGetFn, typeof(object));

			var propertyGetFn = Expression.Lambda<Func<object, object>>
			(
				oExprCallPropertyGetFn,
				oInstanceParam
			).Compile();

			return propertyGetFn;
		}

        /// <summary>Gets property value method via delegate.</summary>
        ///
        /// <param name="type">        The type.</param>
        /// <param name="propertyInfo">Information describing the property.</param>
        ///
        /// <returns>The property value method via delegate.</returns>
		public static Func<object, object> GetPropertyValueMethodViaDelegate(
			Type type, PropertyInfo propertyInfo)
		{
			var mi = typeof(ReflectionTests).GetMethod("CreateFunc");

			var genericMi = mi.MakeGenericMethod(type, propertyInfo.PropertyType);
			var del = genericMi.Invoke(null, new[] { propertyInfo.GetGetMethod() });

			return (Func<object, object>) del;
		}

        /// <summary>Creates a function.</summary>
        ///
        /// <typeparam name="T1">Generic type parameter.</typeparam>
        /// <typeparam name="T2">Generic type parameter.</typeparam>
        /// <param name="mi">The mi.</param>
        ///
        /// <returns>The new function.</returns>
		public static Func<object, object> CreateFunc<T1,T2>(MethodInfo mi)
		{
			var del = (Func<T1, T2>)Delegate.CreateDelegate(typeof(Func<T1, T2>), mi);
			return x => del((T1) x);
		}

        /// <summary>Compares objects.</summary>
		[Test]
		public void Compare()
		{
			var model = ModelWithIdAndName.Create(1);
			var pi = model.GetType().GetProperty("Name");
			var simpleExpr = GetPropertyValueMethodViaExpressions(typeof(ModelWithIdAndName), pi);
			var simpleDelegate = GetPropertyValueMethodViaDelegate(typeof(ModelWithIdAndName), pi);

			CompareMultipleRuns(
				"Expressions", () => simpleExpr(model),
				"Delegate", () => simpleDelegate(model)
			);

		}

	}
}