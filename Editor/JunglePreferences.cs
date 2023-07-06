using UnityEditor;
using UnityEngine;

namespace Jungle.Editor
{
    public class JunglePreferences : EditorWindow
    {
        #region Variables

        private int _openTabIndex = 0;

        public enum GraphThemes
        {
            Auto,
            Light,
            Dark,
            Blueprint
        }
        
        public enum SupportedLanguages
        {
            English,
            中文,
            한국어,
            日本語
        }
        
        #endregion

        [MenuItem("Window/Jungle/Preferences")]
        public static void OpenWindow()
        {
            GetWindowWithRect<JunglePreferences>
            (
                new Rect(0f, 0f, 300f, 400f),
                true,
                "Jungle Preferences"
            );
        }
        
        private void OnGUI()
        {
            var activeButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                normal =
                {
                    background = EditorGUIUtility.isProSkin ? Texture2D.grayTexture : Texture2D.whiteTexture
                }
            };
            var inactiveButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button
                    (
                        "General",
                        _openTabIndex == 0 ? activeButtonStyle : inactiveButtonStyle, 
                        GUILayout.Width(100f)
                    )
                )
            {
                _openTabIndex = 0;
            }
            if (GUILayout.Button
                (
                    "Validator",
                    _openTabIndex == 1 ? activeButtonStyle : inactiveButtonStyle, 
                    GUILayout.Width(100f)
                )
               )
            {
                _openTabIndex = 1;
            }
            if (GUILayout.Button
                (
                    "Build",
                    _openTabIndex == 2 ? activeButtonStyle : inactiveButtonStyle, 
                    GUILayout.Width(100f)
                )
               )
            {
                _openTabIndex = 2;
            }
            GUILayout.EndHorizontal();
            
            
            
            // General settings
            if (_openTabIndex == 0)
            {
                /* Not quite ready for this just yet...
                GUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.EnumPopup("Language", SupportedLanguages.English);
                GUILayout.EndVertical();
                */
                
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Runtime", EditorStyles.boldLabel);
                    EditorGUILayout.Toggle("Allow Edits in Play Mode", true);
                    EditorGUILayout.Toggle("Create Jungle Runtime", true);
                GUILayout.EndVertical();
                
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Appearance", EditorStyles.boldLabel);
                    EditorGUILayout.EnumPopup("Graph Theme", GraphThemes.Auto);
                    EditorGUILayout.Toggle("Node Color Accents", true);
                    EditorGUILayout.Toggle("Glow While Executing", true);
                GUILayout.EndVertical();
            }
            // Validator settings
            else if (_openTabIndex == 1)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Overhead", EditorStyles.boldLabel);
                    EditorGUILayout.Toggle("Disable Validation", false);
                    EditorGUILayout.Toggle("Auto-refresh Validation", false);
                    EditorGUILayout.Toggle("Validate in Play Mode", false);
                GUILayout.EndVertical();
            }
            // Build settings
            else if (_openTabIndex == 2)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("Build-time", EditorStyles.boldLabel);
                    EditorGUILayout.Toggle("Warn If Validation Failed", true);
                GUILayout.EndVertical();
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.Label("v1.0.0", EditorStyles.centeredGreyMiniLabel);
        }
    }
}
