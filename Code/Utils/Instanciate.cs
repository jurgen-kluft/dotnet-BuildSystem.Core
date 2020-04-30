using System;
using System.Reflection;

namespace Game.Data
{
	public static class Instanciate
	{
		private static Assembly mAssembly;

		public static Assembly assembly
		{
			get
			{
				return mAssembly;
			}
			set
			{
				mAssembly = value;
			}
		}

		public static T create<T>(string className) where T : class
		{
            try
            {
                T instance = mAssembly.CreateInstance("Game.Data." + className) as T;
                if (instance == null)
                    Console.WriteLine("Warning: dynamic instanciation of class " + className + " failed since it doesn't exist.");
                return instance;
            }
            catch (Exception)
            {
                Console.WriteLine("Warning: dynamic instanciation of class " + className + " failed with an exception.");
            }
            return null;
		}
	}
}
