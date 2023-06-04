using System;
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
        /// 
        /// </summary>
        public List<JungleTree> RunningTrees
        {
            get
            {
                var query = new List<JungleTree>();
                foreach (var entry in _running)
                {
                    query.Add(entry.Tree);
                }
                return query;
            }
        }
        
        private List<TreeEntry> _running = new();
        
        #endregion
        
        private void Awake()
        {
            if (Singleton != null)
            {
#if UNITY_EDITOR
                Debug.LogError("[Jungle Runtime] A Jungle runtime was instantiated while another instance already existed.", gameObject);
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
            foreach (var entry in new List<TreeEntry>(_running))
            {
                StopTree(entry.Tree);
            }
        }

        private void Update()
        {
            foreach (var entry in new List<TreeEntry>(_running))
            {
                if (entry.Tree.State == JungleTree.TreeState.Finished)
                {
                    StopTree(entry.Tree);
                    continue;
                }
                entry.Tree.Update();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="linkedScene"></param>
        public void StartTree(JungleTree tree, Scene? linkedScene = null)
        {
            var entry = new TreeEntry(tree, linkedScene);

            _running ??= new List<TreeEntry>();
            if (_running.Contains(entry))
            {
                return;
            }
            _running.Add(entry);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        public void StopTree(JungleTree tree)
        {
            if (_running == null || _running.Count == 0)
            {
                return;
            }
            for (var i = 0; i < _running.Count; i++)
            {
                if (_running[i].Tree != tree)
                {
                    continue;
                }
                _running.RemoveAt(i);
                tree.Stop();
                break;
            }
        }
        
        private void SceneUnloadedCallback(Scene unloadedScene)
        {
            var query = new List<TreeEntry>(_running);
            foreach (var entry in query)
            {
                if (entry.Link == null)
                {
                    continue;
                }
                if (entry.Link == unloadedScene)
                {
                    StopTree(entry.Tree);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct TreeEntry
    {
        /// <summary>
        /// 
        /// </summary>
        public JungleTree Tree { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public Scene? Link { get; private set; }

        public TreeEntry(JungleTree tree, Scene? link = null)
        {
            Tree = tree;
            Link = link;
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
            
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUI.enabled = true;
            GUILayout.Label("Running Tree(s)", EditorStyles.boldLabel, GUILayout.MaxWidth(450));
            GUILayout.Space(5);
            GUI.enabled = false;
            
            if (instance.RunningTrees.Count == 0)
            {
                GUI.enabled = true;
                GUILayout.Label("...");
                GUI.enabled = false;
            }
            
            foreach (var tree in instance.RunningTrees)
            {
                GUILayout.BeginHorizontal();
                GUI.enabled = true;
                if (GUILayout.Button("X"))
                {
                    tree.Stop();
                    continue;
                }
                GUI.enabled = false;
                EditorGUILayout.ObjectField(tree, typeof(JungleNode), GUILayout.MaxWidth(300));
                GUILayout.FlexibleSpace();
                GUILayout.Label($"{Math.Round(tree.PlayTime, 0)}s");
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndVertical();
            Repaint();
        }
    }
#endif
}
