using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Jungle.Editor
{
    /// <summary>
    /// 
    /// </summary>
    [InitializeOnLoad]
    public static class JungleValidator
    {
        #region Variables

        private const string SEARCH_FILTER 
            = "t:JungleTree";

        #endregion
        
        static JungleValidator()
        {
            CompilationPipeline.compilationFinished += _ =>
            {
                var jungleTrees = GetAllJungleTrees();
                var reports = new List<ValidationReport>();
                foreach (var jungleTree in jungleTrees)
                {
                    var report = Validate(jungleTree);
                    // If the validation did not fail then we dont care to see the report
                    if (!report.Failed)
                    {
                        continue;
                    }
                    reports.Add(report);
                }
            };
        }
        
        public static ValidationReport Validate(JungleTree tree)
        {
            var treeAssetPath = AssetDatabase.GetAssetPath(tree);
            var treeSubAssets = AssetDatabase.LoadAllAssetsAtPath(treeAssetPath);
            var report = new ValidationReport(tree, null, null);
            
            foreach (var subAsset in treeSubAssets)
            {
                // This USUALLY means that the developer deleted the node script
                if (subAsset == null)
                {
                    //report.NodeErrors = true;
                    continue;
                }
                if (subAsset.GetType() != typeof(JungleNode))
                {
                    //report.TreeErrors = true;
                    continue;
                }
                var node = subAsset as JungleNode;
                foreach (var port in node.OutputPorts)
                {
                    foreach (var connection in port.connections)
                    {
                        if (connection.InputInfo.PortType != port.PortType)
                        {
                            //report.NodeErrors = true;
                        }
                    }
                }
            }
            
            return report;
        }

        public static bool AllJungleTreesValid()
        {
            var jungleTrees = GetAllJungleTrees();
            return jungleTrees.All(jungleTree => !Validate(jungleTree).Failed);
        }
        
        public static JungleTree[] GetAllJungleTrees()
        {
            var jungleTreeAssets = new List<JungleTree>();
            
            // This searches through all jungle tree assets inside the project
            // The search filter should be type "t:JungleTree"
            AssetDatabase.FindAssets(SEARCH_FILTER).ToList().ForEach(guid =>
            {
                var guidToAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<JungleTree>(guidToAssetPath);
                if (asset != null)
                {
                    jungleTreeAssets.Add(asset);
                }
            });

            return jungleTreeAssets.ToArray();
        }
    }
    
    [Serializable]
    public struct ValidationReport
    {
        public bool Failed => TreeErrors != null || NodeErrors != null;
            
        public JungleTree Tree; 
        public List<TreeError> TreeErrors;
        public List<NodeError> NodeErrors;

        public ValidationReport(JungleTree tree, List<TreeError> treeErrors,
            List<NodeError> nodeErrors)
        {
            Tree = tree;
            TreeErrors = treeErrors;
            NodeErrors = nodeErrors;
        }
            
        public struct TreeError
        {
            public ErrorType Type;
                
            public enum ErrorType
            {
                MissingOrNullNode
            }
        }
        public struct NodeError
        {
            public JungleNode Node;
            public ErrorType Type;
                
            public enum ErrorType
            {
                ConnectionTypeMismatch
            }
        }
    }

    public class JungleValidatorEditor : EditorWindow
    {
        #region Variables
        
        private static JungleValidatorEditor _instance;
        private ValidationReport[] _reports;

        private string _searchQuery;
        private Vector2 _scrollView;
        
        private bool OnlyShowIssues
        {
            get => EditorPrefs.GetBool("Jungle_OnlyShowIssues", false);
            set => EditorPrefs.SetBool("Jungle_OnlyShowIssues", value);
        }
        
        #endregion

        private void OnEnable()
        {
            _reports = Array.Empty<ValidationReport>();
        }

        [MenuItem("Window/Jungle/Open Validator")]
        private static void OpenWindow()
        {
            var jungleTrees = JungleValidator.GetAllJungleTrees();
            var jungleTreeReports = jungleTrees.Select(JungleValidator.Validate).ToArray();
            ShowReport(jungleTreeReports);
        }

        public static void ShowReport(ValidationReport[] reports)
        {
            _instance = GetWindow<JungleValidatorEditor>
            (
                false,
                "Jungle Validator"
            );
            _instance._reports = reports;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            // A the case a tree no longer exists, we should clear the report cache
            // Usually occurs when the Jungle Tree file is deleted from the project
            foreach (var report in _reports)
            {
                if (report.Tree != null)
                {
                    continue;
                }
                _reports = Array.Empty<ValidationReport>();
                break;
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Button("?", GUILayout.Width(20f));
            GUILayout.Label("Status: No Issues", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            OnlyShowIssues = GUILayout.Toggle(OnlyShowIssues, "Only Show Issues");
            var onlyShowIssues = OnlyShowIssues;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            _searchQuery = GUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField);
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
            GUILayout.Space(2.5f);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), EditorGUIUtility.isProSkin 
                ? new Color(0.7f, 0.7f, 0.7f, 0.5f) 
                : new Color(0.3f, 0.3f, 0.3f, 0.5f));
            GUILayout.Space(2.5f);

            var reportsToShow = new List<ValidationReport>();
            foreach (var report in _reports)
            {
                if (onlyShowIssues && !report.Failed)
                {
                    continue;
                }
                if (report.Tree == null)
                {
                    continue;
                }
                reportsToShow.Add(report);
            }
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                var query = new List<ValidationReport>();
                foreach (var report in reportsToShow)
                {
                    var treeName = report.Tree.name.ToLower().Replace(" ", string.Empty);
                    if (!treeName.Contains(_searchQuery.ToLower().Replace(" ", string.Empty)))
                    {
                        continue;
                    }
                    query.Add(report);
                }
                reportsToShow = query;
            }

            _scrollView = GUILayout.BeginScrollView(_scrollView);
            if (reportsToShow.Count == 0 && !string.IsNullOrEmpty(_searchQuery))
            {
                GUILayout.Label("No Results");
            }
            else if (reportsToShow.Count == 0)
            {
                GUILayout.Label("Nothing to Show...");
            }
            
            for (var i = 0; i < reportsToShow.Count; i++) 
            {
                if (onlyShowIssues && !reportsToShow[i].Failed)
                {
                    continue;
                }
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label($"{(i + 1).ToString()}:");
                GUILayout.Label(reportsToShow[i].Tree.name);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Open", GUILayout.Width(45f)))
                {
                    EditorGUIUtility.PingObject(reportsToShow[i].Tree);
                    Selection.activeObject = reportsToShow[i].Tree;
                }
                GUI.enabled = false;
                if (GUILayout.Button("Fix", GUILayout.Width(42.5f)))
                {
                        
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.Space(2.5f);
            }
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            GUILayout.Space(2.5f);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), EditorGUIUtility.isProSkin 
                ? new Color(0.7f, 0.7f, 0.7f, 0.5f) 
                : new Color(0.3f, 0.3f, 0.3f, 0.5f));
            GUILayout.Space(2.5f);
            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Click refresh to view/update the validation report.", MessageType.Info);
            if (GUILayout.Button("Refresh", GUILayout.Height(37.5f), GUILayout.Width(100f)))
            {
                OpenWindow();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
