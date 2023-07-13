#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Application
{
    [NodeProperties(
        Title = "Quit Application",
        Tooltip = "Quits the application.",
        Category = "Application", 
        Color = Red
    )]
    [IdentityNode(
        InputPortName = "Quit",
        OutputPortName = ""
    )]
    public class QuitApplicationNode : IdentityNode
    {
        public override void OnStart()
        {
            if (!UnityEngine.Application.isEditor)
            {
                UnityEngine.Application.Quit();
            }
            else
            {
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#endif
            }
            CallAndStop();
        }

        public override void OnUpdate()
        {
            
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(QuitApplicationNode))]
    public class QuitApplicationNodeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            
        }
    }
#endif
}