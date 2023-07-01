namespace Jungle.Editor
{
    public static class JungleGUILayout
    {
        public static string ShortenString(string input, int maxLength)
        {
            if (input.Length <= maxLength)
            {
                return input;
            }
            else
            {
                return input.Substring(0, maxLength - 3) + "...";
            }
        }
    }
}
