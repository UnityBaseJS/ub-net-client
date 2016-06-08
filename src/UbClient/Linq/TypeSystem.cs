using System;
using System.Collections.Generic;

namespace Softengi.UbClient.Linq
{
	/// <summary>
	/// Helper class to perform specific reflection queries.
	/// </summary>
	static internal class TypeSystem
	{
		/// <summary>
		/// If <paramref name="seqType"/> implements <see cref="IEnumerable{T}"/>, returns its type,
		/// otherwise return argument.
		/// </summary>
		static internal Type GetElementType(Type seqType)
		{
			Type ienum = FindIEnumerable(seqType);
			if (ienum == null) return seqType;
			return ienum.GetGenericArguments()[0];
		}

		static private Type FindIEnumerable(Type seqType)
		{
			if (seqType == null || seqType == typeof(string))
				return null;

			if (seqType.IsArray)
				return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

			if (seqType.IsGenericType)
			{
				foreach (Type arg in seqType.GetGenericArguments())
				{
					Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
					if (ienum.IsAssignableFrom(seqType))
						return ienum;
				}
			}

			Type[] ifaces = seqType.GetInterfaces();
			if (ifaces.Length > 0)
			{
				foreach (var iface in ifaces)
				{
					var ienum = FindIEnumerable(iface);
					if (ienum != null) return ienum;
				}
			}

			if (seqType.BaseType == null || seqType.BaseType == typeof(object))
				return null;

			return FindIEnumerable(seqType.BaseType);
		}
	}
}