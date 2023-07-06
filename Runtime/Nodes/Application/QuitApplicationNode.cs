using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Application
{
    [Node(
        Title = "Quit Application",
        Category = "Application", 
        Color = Color.Red,
        InputPortName = "Quit",
        OutputPortNames = new string[0],
        OutputPortTypes = new Type[0]
    )]
    public class QuitApplicationNode : JungleNode
    {
        #region Variables

        

        #endregion
        
        public override void Initialize(in object inputValue)
        {
            
        }

        public override bool Execute(out PortCall[] call)
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
            call = new[]
            {
                new PortCall(0, true)
            };
            return true;
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(QuitApplicationNode))]
    public class QuitApplicationNodeEditor : UnityEditor.Editor
    {
        #region Variables

        

        #endregion

        public override void OnInspectorGUI()
        {
            
        }
    }
#endif
}