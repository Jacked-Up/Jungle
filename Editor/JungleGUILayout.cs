using UnityEditor;
using UnityEngine;

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
            return input.Substring(0, maxLength - 3) + "...";
        }
        
        public static void DrawDividerLine(float height, float topMargin, float bottomMargin)
        {
            GUILayout.Space(topMargin);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height), EditorGUIUtility.isProSkin 
                ? new Color(0.7f, 0.7f, 0.7f, 0.5f) 
                : new Color(0.3f, 0.3f, 0.3f, 0.5f));
            GUILayout.Space(bottomMargin);
        }
    }
}
