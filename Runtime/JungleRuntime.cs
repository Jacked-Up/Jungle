using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle
{
    /// <summary>
    /// 
    /// </summary>
    [DisallowMultipleComponent] [AddComponentMenu("")]
    public class JungleRuntime : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// Jungle runtime mono behaviour reference
        /// </summary>
        public static JungleRuntime Singleton
        {
            get;
            private set;
        }
        
        /// <summary>
        /// List of executing node trees. (Includes both persistent and non-persistent trees)
        /// </summary>
        public List<JungleTree> RunningTrees
        {
            get
            {
                var combinedList = new List<JungleTree>();
                _persistentRunningTrees ??= new List<JungleTree>();
                combinedList.AddRange(_persistentRunningTrees);
                _nonPersistentRunningTrees ??= new List<JungleTree>();
                combinedList.AddRange(_nonPersistentRunningTrees);
                return combinedList;
            }
        }

        private List<JungleTree> _persistentRunningTrees = new();
        private List<JungleTree> _nonPersistentRunningTrees = new();
        private List<Scene> _sceneQuery = new();
        
        #endregion
        
        private void Awake()
        {
            if (Singleton != null)
            {
#if UNITY_EDITOR
                Debug.LogError("[Jungle Runtime] A Jungle runtime was instantiated while another instance already existed");
#endif
                enabled = false;
                return;
            }
            Singleton = this;
        }

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += SceneUnloadedCallback;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= SceneUnloadedCallback;
            foreach (var runningTree in RunningTrees)
            {
                StopTree(runningTree);
            }
        }

        private void Update()
        {
            var query = new List<JungleTree>();
            foreach (var nodeTree in RunningTrees)
            {
                if (nodeTree.State == JungleTree.TreeState.Finished)
                {
                    query.Add(nodeTree);
                    continue;
                }
                nodeTree.Update();
            }
            query.ForEach(nodeTree =>
            {
                StopTree(nodeTree);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public bool PlayTree(JungleTree tree)
        {
            _persistentRunningTrees ??= new List<JungleTree>();
            if (RunningTrees.Contains(tree))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[Jungle Runtime] Attempt to start {tree.name} but it was already running");
#endif
                return false;
            }
            _persistentRunningTrees.Add(tree);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="linkedScene"></param>
        /// <returns></returns>
        public bool PlayTree(JungleTree tree, Scene linkedScene)
        {
            _nonPersistentRunningTrees ??= new List<JungleTree>();
            _sceneQuery ??= new List<Scene>();
            _nonPersistentRunningTrees.Add(tree);
            _sceneQuery.Add(linkedScene);
            return true;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public bool StopTree(JungleTree tree)
        {
            _persistentRunningTrees ??= new List<JungleTree>();
            _nonPersistentRunningTrees ??= new List<JungleTree>();
            if (!RunningTrees.Contains(tree))
            {
                return false;
            }
            if (_persistentRunningTrees.Contains(tree))
            {
                _persistentRunningTrees.Remove(tree);
            }
            else if (_nonPersistentRunningTrees.Contains(tree))
            {
                _nonPersistentRunningTrees.Remove(tree);
            }
            tree.Stop();
            return true;
        }
        
        private void SceneUnloadedCallback(Scene unloadedScene)
        {
            _nonPersistentRunningTrees ??= new List<JungleTree>();
            _sceneQuery ??= new List<Scene>();
            
            var query = new List<JungleTree>();
            var query2 = new List<Scene>();
            foreach (var scene in _sceneQuery)
            {
                if (scene != unloadedScene) continue;
                var index = _sceneQuery.IndexOf(scene);
                query.Add(_nonPersistentRunningTrees[index]);
                query2.Add(scene);
            }
            foreach (var tree in query)
            {
                var stopped = StopTree(tree);
#if UNITY_EDITOR
                if (stopped)
                {
                    Debug.Log($"[{tree.name}] Tree was stopped prematurely");
                }
#endif
                _nonPersistentRunningTrees.Remove(tree);
            }
            foreach (var request in query2)
            {
                _sceneQuery.Remove(request);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(JungleRuntime))]
    public class JungleRuntimeEditor : Editor
    {
        #region Varaibles

        private JungleRuntime instance;

        #endregion

        private void OnEnable()
        {
            instance = target as JungleRuntime;
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            foreach (var tree in instance.RunningTrees)
            {
                //EditorGUILayout.ObjectField(tree, typeof(JungleNode));
            }
            Repaint();
        }
    }
#endif
}
