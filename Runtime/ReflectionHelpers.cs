using System;
using System.Collections.Generic;
using System.Reflection;

namespace TW.DataPersistence
{

	public static class ReflectionHelpers
	{
		// Find all non-abstract types in loaded assemblies that inherit from 'abstractBase'
		public static IEnumerable<Type> FindDerivedTypes(Type abstractBase)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var list = new List<Type>();
			foreach (var baseType in assemblies)
			{
				try
				{
					baseType.GetTypes();
				}
				catch (ReflectionTypeLoadException)
				{
					continue;
				}
				var subTypes = baseType.GetTypes();
				if (subTypes == null)
				{
					continue;
				}
				foreach (var subType in subTypes)
				{
					if (subType != null
							&& subType.IsClass
							&& !subType.IsAbstract
							&& subType != abstractBase
							&& IsDerivedFrom(subType, abstractBase))
					{
						list.Add(subType);
					}

				}
			}
			return list;
		}

		// Checks inheritance including generic base definitions
		private static bool IsDerivedFrom(Type type, Type baseType)
		{
			for (var cur = type; cur != null; cur = cur.BaseType)
			{
				if (cur == baseType) return true;
				if (cur.IsGenericType)
				{
					var def = cur.GetGenericTypeDefinition();
					if (baseType.IsGenericTypeDefinition ? def == baseType : def == baseType) return true;
				}
			}
			return false;
		}

		// Returns the first generic argument of the first generic base encountered (or null)
		public static Type GetFirstGenericArgument(Type derivedType, Type abstractBase)
		{
			for (var cur = derivedType; cur != null; cur = cur.BaseType)
			{
				if (cur.IsGenericType)
				{
					var args = cur.GetGenericArguments();
					return args.Length > 0 ? args[0] : null;

				}
				else if (cur == abstractBase)
				{
					return null;
				}
			}
			return null;
		}
	}

}
