namespace Base.Utils
{
    public static class Util
    {
        public const int BigM = 10_000;

        public static int? ToNullableInt(this string s)
        {
            if (int.TryParse(s, out int i)) return i;
            return null;
        }

        public static string ToString(List<int> integerList)
        {
            string convertedString = "[";
            if (integerList != null && integerList.Any())
            {
                integerList.ForEach(x => convertedString += $"{x}, ");
                convertedString = convertedString.Remove(convertedString.Length - 2, 2);
            }
            convertedString += "]";
            return convertedString;
        }

        public static Dictionary<int, string> ToIntStringDict(Dictionary<int, List<int>> dict)
        {
            return dict.ToDictionary(x => x.Key, x => ToString(x.Value));
        }
    }
}
