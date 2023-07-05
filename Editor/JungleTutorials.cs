using UnityEditor;
using UnityEngine;

namespace Jungle.Editor
{
    public static class JungleTutorials
    {
        #region Variables

        private const string DIALOG_TITLE = "Jungle";
        private const string DIALOG_ACCEPT = "Yes";
        private const string DIALOG_DECLINE = "No thanks";

        // Editor Tutorial
        private const string SHOWN_EDITOR_KEY = "Jungle_ShownEditorTutorialRequest";
        private const string EDITOR_TUTORIAL_URL = "https://www.jackedup.xyz/jungle/docs";
        
        #endregion

        [MenuItem("Window/Jungle/Tutorials/Jungle Editor")]
        public static void TryShowEditorTutorial()
        {
            // Only show if it has never been shown before
            if (EditorPrefs.GetBool(SHOWN_EDITOR_KEY, false))
            {
                return;
            }
            var showTutorial =
                DisplayDialogRequest("Would you like to view a tutorial on how to use the Jungle tree editor?");
            if (showTutorial)
            {
                Application.OpenURL(EDITOR_TUTORIAL_URL);
            }
            EditorPrefs.SetBool(SHOWN_EDITOR_KEY, true);
        }

        private static bool DisplayDialogRequest(string message)
        {
            return EditorUtility.DisplayDialog(
                DIALOG_TITLE,
                message,
                DIALOG_ACCEPT,
                DIALOG_DECLINE);
        }
        
        [MenuItem("Window/Jungle/Tutorials/Reset Tutorial Dialogs")]
        public static void ResetAllTutorialRequests()
        {
            //JungleDebug.Log("Jungle Tutorials", "The Jungle tutorial dialog states have been reset.");
            EditorPrefs.SetBool(SHOWN_EDITOR_KEY, false);
        }
    }
}
