using System;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jungle.Nodes.Scene
{
    [Node(
        TitleName = "Unload Scene",
        Category = "Scene",
        Color = Color.Orange,
        InputPortName = "Unload",
        OutputPortNames = new []{ "Unloaded" }
    )]
    public class UnloadSceneNode : JungleNode
    {
        #region Variables

        [SerializeField] 
        private string sceneToUnload;

        [NonSerialized]
        private AsyncOperation operation;
        
        #endregion
        
        public override void Initialize(in object inputValue)
        {
            
        }

        public override bool Execute(out PortCall[] call)
        {
            call = Array.Empty<PortCall>();
            if (operation == null)
            {
                operation = SceneManager.UnloadSceneAsync(sceneToUnload);
            }

            if (operation.isDone)
            {
                call = new[]
                {
                    new PortCall(0, new None())
                };
                return true;
            }
            return false;
        }
    }
}