using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Jungle.Editor
{
    /// <summary>
    /// Checks that all Jungle Trees validate successfully at build time.
    /// </summary>
    public class JungleBuilder : IPreprocessBuildWithReport
    {
        #region Variables

        public int callbackOrder
        {
            get;
        }

        #endregion

        public void OnPreprocessBuild(BuildReport report)
        {
            // No point in a warning if all the trees validate
            if (JungleValidator.AllJungleTreesValid())
            {
                return;
            }
            
            var continueBuild = EditorUtility.DisplayDialog("Jungle - Build Warning",
                "Failed to validate all of the Jungle Trees. Do you still want to build the project?" +
                "\n\n*Continuing the build may add broken Jungle Trees to your game.",
                "Continue Build", "Cancel Build");

            if (continueBuild)
            {
                return;
            }
            throw new BuildFailedException("Jungle has cancelled the build due to Jungle Tree validation errors");
        }
    }
}