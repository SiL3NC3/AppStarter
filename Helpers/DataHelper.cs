using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppStarter.Helpers
{
    public static class DataHelper
    {
        public static List<T> EnumToStringArray<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList<T>();
        }

        public static T?[] ConvertArrayToNullableArray<T>(T[] array) where T : struct
        {
            T?[] nullableArray = new T?[array.Length];
            for (int i = 0; i < array.Length; i++)
                nullableArray[i] = array[i];
            return nullableArray;
        }
    }
}
