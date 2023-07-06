using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle.Nodes.Scene
{
    [Node(
        Title = "Reload Scene",
        Category = "Scene",
        Color = Color.Orange,
        InputPortName = "Reload",
        InputPortType = typeof(UnityEngine.SceneManagement.Scene),
        OutputPortNames = new []{ "Reloaded" },
        OutputPortTypes = new []{ typeof(UnityEngine.SceneManagement.Scene) }
    )]
    public class ReloadSceneNode : JungleNode
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
                    $"[{name}] Failed to unload scene because the input scene by name \"{_scene.name}\" was invalid.");
            }
#endif
        }

        public override bool Execute(out PortCall[] call)
        {
            call = Array.Empty<PortCall>();
            operation ??= SceneManager.UnloadSceneAsync(_scene);
            if (!operation.isDone)
            {
                return false;
            }
            call = new[]
            {
                new PortCall(0, new None())
            };
            return true;
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(ReloadSceneNode))]
    public class ReloadSceneNodeEditor : UnityEditor.Editor
    {
        #region Variables

        

        #endregion

        public override void OnInspectorGUI()
        {
            
        }
    }
#endif
}