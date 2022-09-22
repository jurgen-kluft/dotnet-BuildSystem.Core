
namespace GameData
{
    public static class StringUtils
    {
        public static string[] emptyArray()
        {
            string[] array = new string[0];
            return array;
        }
        public static string[] toArray(string s)
        {
            string[] array = new string[1];
            array[0] = s;
            return array;
        }
    }
}
