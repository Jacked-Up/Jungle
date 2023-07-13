using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle
{
    /// <summary>
    /// System for handling Jungles runtime logic.
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
            get;
            private set;
        } = new();
        
        #endregion
        
        private void Awake()
        {
            if (Singleton != null)
            {
                enabled = false;
                return;
            }
            Singleton = this;
        }
        
        private void OnDisable()
        {
            foreach (var tree in new List<JungleTree>(RunningTrees))
            {
                StopTree(tree);
            }
        }
        
        private void Update()
        {
            foreach (var tree in new List<JungleTree>(RunningTrees))
            {
                if (tree.State == JungleTree.StateFlag.Finished)
                {
                    StopTree(tree);
                    continue;
                }
                tree.Update();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        internal void StartTree(JungleTree tree)
        {
            RunningTrees ??= new List<JungleTree>();
            RunningTrees.Add(tree);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        internal void StopTree(JungleTree tree)
        {
            RunningTrees ??= new List<JungleTree>();
            RunningTrees.Remove(tree);
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
