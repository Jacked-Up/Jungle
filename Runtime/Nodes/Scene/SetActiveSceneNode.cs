using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Scene
{
    [NodeProperties(
        Title = "Set Active Scene",
        Category = "Scene",
        Color = Orange,
        InputPortName = "Set Active",
        InputPortType = typeof(UnityEngine.SceneManagement.Scene),
        OutputPortNames = new []{ "Next" },
        OutputPortTypes = new []{ typeof(None) }
    )]
    public class SetActiveSceneNode : JungleNode
    {
        #region Variables

        [NonSerialized] 
        private UnityEngine.SceneManagement.Scene _scene;
        
        [NonSerialized]
        private AsyncOperation operation;
        
        #endregion
        
        public override void Initialize(in object inputValue)
        {
            _scene = (UnityEngine.SceneManagement.Scene) inputValue;
#if UNITY_EDITOR
            if (!_scene.IsValid())
            {
                Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, Tree,
                    $"[{name}] Failed to set active scene because the input scene by name \"{_scene.name}\" was invalid.");
            }
#endif
            
            var result = SceneManager.SetActiveScene(_scene);
#if UNITY_EDITOR
            if (!result)
            {
                Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, Tree,
                    $"[{name}] Failed to set active scene.");
            }
#endif
        }

        public override bool Execute(out PortCall[] call)
        {
            call = new[]
            {
                new PortCall(0, new None())
            };
            return true;
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(SetActiveSceneNode))]
    public class SetActiveSceneNodeEditor : UnityEditor.Editor
    {
        #region Variables

        

        #endregion

        public override void OnInspectorGUI()
        {
            
        }
    }
#endif
}