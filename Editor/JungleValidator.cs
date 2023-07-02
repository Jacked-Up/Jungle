using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Jungle.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public static class JungleValidator
    {
        #region Variables



        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static ValidationReport Validate(JungleTree tree)
        {
            var report = new ValidationReport(tree, null, null);
            
            var assetPath = AssetDatabase.GetAssetPath(tree);
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (var asset in subAssets)
            {
                // We can ignore the tree asset
                if (asset == tree)
                {
                    continue;
                }
                
                // This USUALLY means that the developer deleted the node script
                if (asset == null)
                {
                    report.nodeIssues ??= new List<string>();
                    report.nodeIssues.Add("Script for node is missing/deleted");
                    continue;
                }
                if (!asset.GetType().IsSubclassOf(typeof(JungleNode)))
                {
                    report.nodeIssues ??= new List<string>();
                    report.nodeIssues.Add($"{asset.name} does not inherit from the correct base class");
                    continue;
                }
                var jungleNode = asset as JungleNode;
                if (jungleNode == null) continue;
                
                jungleNode.Validate(false, out var issues);
                if (issues != null && issues.Count != 0)
                {
                    report.nodeIssues ??= new List<string>();
                    foreach (var issue in issues)
                    {
                        report.nodeIssues.Add($"[{jungleNode.name}] {issue}");
                    }
                }
                if (jungleNode.Tree == null)
                {
                    report.nodeIssues.Add($"{jungleNode.name} does not have a Jungle Tree");
                }
                
                var validNames = jungleNode.OutputInfo.Count(info => info.PortName is not "ERROR" or null);
                var validTypes = jungleNode.OutputInfo.Count(info => info.PortType != typeof(Error));

                if (validNames < validTypes)
                {
                    report.nodeIssues ??= new List<string>();
                    report.nodeIssues.Add($"{jungleNode.GetType()} has declared more output port types than " +
                                          "output port names");
                }
                else if (validNames > validTypes)
                {
                    report.nodeIssues ??= new List<string>();
                    report.nodeIssues.Add($"{jungleNode.GetType()} has declared more output port names than " +
                                          "output port types");
                }
            }
            
            return report;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        public static void AutoFix(JungleTree tree)
        {
            foreach (var node in tree.nodes)
            {
                node.Validate(true, out var data);
            }
        }
    }
    
    [Serializable]
    public struct ValidationReport
    {
        public bool Failed => treeIssues != null || nodeIssues != null;
            
        public JungleTree tree; 
        public List<string> treeIssues;
        public List<string> nodeIssues;

        public ValidationReport(JungleTree tree, List<string> treeIssues, List<string> nodeIssues)
        {
            this.tree = tree;
            this.treeIssues = treeIssues;
            this.nodeIssues = nodeIssues;
        }
    }

    public class JungleValidatorEditor : EditorWindow
    {
        #region Variables
        
        private static JungleValidatorEditor _instance;
        private ValidationReport[] _reports;
        
        private string _searchQuery;
        private Vector2 _scrollView;
        private int _openFoldout = -1;
        private bool _readyForRefresh;
        private float _lastAssetCheckTime;
        
        private bool OnlyShowIssues
        {
            get => EditorPrefs.GetBool("Jungle_OnlyShowIssues", true);
            set => EditorPrefs.SetBool("Jungle_OnlyShowIssues", value);
        }
        
        private bool AutoRefresh
        {
            get => EditorPrefs.GetBool("Jungle_AutoRefresh", true);
            set => EditorPrefs.SetBool("Jungle_AutoRefresh", value);
        }
        
        private bool CollapseIssues
        {
            get => EditorPrefs.GetBool("Jungle_CollapseIssues", true);
            set => EditorPrefs.SetBool("Jungle_CollapseIssues", value);
        }
        
        private bool ShowAutoFixDialog
        {
            get => EditorPrefs.GetBool("Jungle_ShowAutoFixDialog", true);
            set => EditorPrefs.SetBool("Jungle_ShowAutoFixDialog", value);
        }
        
        #endregion

        private void OnEnable()
        {
            _reports = Array.Empty<ValidationReport>();
            _openFoldout = -1;
        }

        [MenuItem("Window/Jungle/Open Jungle Validator")]
        public static void OpenWindow()
        {
            _instance = GetWindow<JungleValidatorEditor>
            (
                false,
                "Jungle Validator"
            );
            RefreshReports();
        }

        private static void RefreshReports()
        {
            _instance = GetWindow<JungleValidatorEditor>
            (
                false,
                "Jungle Validator"
            );
            var jungleTrees = JungleEditor.GetAllJungleTrees();
            var reports = jungleTrees.Select(JungleValidator.Validate).ToArray();
            _instance._reports = reports;
        }

        private void OnGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("You can only validate Jungle Trees and Jungle Nodes in edit mode.",
                    MessageType.Info);
                return;
            }
            
            GUILayout.BeginVertical();

            var newJungleTreeAssets = false;
            if (!_readyForRefresh)
            {
                // This check waits at least 1/4th of a second before performing an asset check
                // Otherwise the check would be performed wayyyyy too frequently
                if (EditorApplication.timeSinceStartup - _lastAssetCheckTime > 0.25f)
                {
                    newJungleTreeAssets = _reports.Length != JungleEditor.GetAllJungleTrees().Length;
                    _lastAssetCheckTime = (float) EditorApplication.timeSinceStartup;
                }
            }
            
            // A the case a tree no longer exists, we should refresh the report cache
            // Usually occurs when the Jungle Tree file is deleted from the project
            if (_reports.Any(report => report.tree == null))
            {
                RefreshReports();
            }
            else if (newJungleTreeAssets && AutoRefresh)
            {
                RefreshReports();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Button("?", GUILayout.Width(20f));
            
            GUI.enabled = _reports.Any(report => report.Failed);
            if (GUILayout.Button("Auto-Fix All"))
            {
                if (ShowAutoFixDialog)
                {
                    var decision = EditorUtility.DisplayDialogComplex("Jungle Validator",
                        $"Are you sure you want to auto-fix all Jungle Trees?" +
                        "\n\nThis could cause irreversible damage.",
                        "Yes", "No", "Yes, Don't Ask Again");
                    if (decision == 2)
                    {
                        ShowAutoFixDialog = false;
                    }
                }
                foreach (var report in _reports)
                {
                    JungleValidator.AutoFix(report.tree);
                }
                RefreshReports();
            }
            GUI.enabled = true;
            
            GUILayout.FlexibleSpace();
            CollapseIssues = GUILayout.Toggle(CollapseIssues, "Collapse Issues");
            var collapseIssues = CollapseIssues;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            _searchQuery = GUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField, GUILayout.MaxWidth(300f));
            GUILayout.FlexibleSpace();
            OnlyShowIssues = GUILayout.Toggle(OnlyShowIssues, "Issues Only", GUILayout.Width(82.5f));
            var onlyShowIssues = OnlyShowIssues;
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
                if (report.tree == null)
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
                    var treeName = report.tree.name.ToLower().Replace(" ", string.Empty);
                    if (!treeName.Contains(_searchQuery.ToLower().Replace(" ", string.Empty)))
                    {
                        continue;
                    }
                    query.Add(report);
                }
                reportsToShow = query;
            }

            _scrollView = GUILayout.BeginScrollView(_scrollView);
            
            if (_reports == null || _reports.Length == 0)
            {
                GUILayout.Label("No reports to show. Try refreshing?", EditorStyles.boldLabel);
            }
            else if ((reportsToShow.Count == 0 && !string.IsNullOrEmpty(_searchQuery)) 
                     || (reportsToShow.Count == 0 && onlyShowIssues))
            {
                GUILayout.Label("No results", EditorStyles.boldLabel);
            }
            
            var okStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal =
                {
                    background = MakeBackgroundTexture(EditorGUIUtility.isProSkin 
                        ? new Color(0f, 1f, 0.25f, 0.125f)
                        : new Color(0.5f, 1f, 0f, 0.35f))
                }
            };
            var issueStyle = new GUIStyle(EditorStyles.helpBox)
            {
                normal =
                {
                    background = MakeBackgroundTexture(EditorGUIUtility.isProSkin 
                        ? new Color(1f, 0f, 0f, 0.125f)
                        : new Color(1f, 0f, 0f, 0.35f))
                }
            };
            for (var i = 0; i < reportsToShow.Count; i++) 
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal(!reportsToShow[i].Failed ? okStyle : issueStyle);
                
                if (_openFoldout == -1 && reportsToShow[i].Failed)
                {
                    _openFoldout = i;
                }
                var foldout = EditorGUILayout.Foldout(_openFoldout == i, $" {reportsToShow[i].tree.name}");
                if (foldout && _openFoldout != i)
                {
                    _openFoldout = i;
                }
                else if (!foldout && _openFoldout == i)
                {
                    _openFoldout = -1;
                }
                
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Select", GUILayout.Width(52.5f)))
                {
                    EditorGUIUtility.PingObject(reportsToShow[i].tree);
                    Selection.activeObject = reportsToShow[i].tree;
                }
                GUI.enabled = reportsToShow[i].Failed;
                if (GUILayout.Button("Fix", GUILayout.Width(42.5f)))
                {
                    if (ShowAutoFixDialog)
                    {
                        var decision = EditorUtility.DisplayDialogComplex("Jungle Validator",
                            $"Are you sure you want to auto-fix {reportsToShow[i].tree.name}?" +
                            "\n\nThis could cause irreversible damage.",
                            "Yes", "No", "Yes, Don't Ask Again");
                        if (decision == 1)
                        {
                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                            continue;
                        }
                        if (decision == 2)
                        {
                            ShowAutoFixDialog = false;
                        }
                    }
                    JungleValidator.AutoFix(reportsToShow[i].tree);
                    RefreshReports();
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                if (_openFoldout == i)
                {
                    if (!reportsToShow[i].Failed)
                    {
                        GUILayout.Label("- No issues detected");
                    }
                    else
                    {
                        if (reportsToShow[i].treeIssues != null && reportsToShow[i].treeIssues.Count != 0)
                        {
                            GUILayout.Label("Tree Issues:");
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(5);
                            GUILayout.BeginVertical();
                            foreach (var issue in reportsToShow[i].treeIssues)
                            {
                                GUILayout.Label($"- {issue}", EditorStyles.wordWrappedLabel);
                            }
                            GUILayout.EndVertical();
                            GUILayout.EndHorizontal();
                        }
                        if (reportsToShow[i].nodeIssues != null && reportsToShow[i].nodeIssues.Count != 0)
                        {
                            GUILayout.Label("Node Issues:");
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(5);
                            GUILayout.BeginVertical();
                            for (var j = 0; j < reportsToShow[i].nodeIssues.Count; j++)
                            {
                                var issue = reportsToShow[i].nodeIssues[j];
                                var issueCopies = 0;
                                if (collapseIssues)
                                {
                                    issueCopies = reportsToShow[i].nodeIssues.Count(nodeIssue => issue == nodeIssue);
                                    if (issueCopies > 1)
                                    {
                                        var firstCopyIndex = reportsToShow[i].nodeIssues
                                            .IndexOf(reportsToShow[i].nodeIssues.First(iss => iss == issue));
                                        if (firstCopyIndex < j) continue;
                                    }
                                }
                                GUILayout.BeginHorizontal();
                                GUILayout.Label($"- {issue}", EditorStyles.wordWrappedLabel);
                                if (collapseIssues && issueCopies > 1)
                                {
                                    GUILayout.FlexibleSpace();
                                    GUILayout.Label($"({issueCopies})", EditorStyles.boldLabel);
                                }
                                GUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                GUILayout.EndVertical();
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
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            AutoRefresh = GUILayout.Toggle(AutoRefresh, "Auto Refresh");
            GUI.enabled = !AutoRefresh;
            if (GUILayout.Button("Refresh", GUILayout.Height(21f), GUILayout.Width(100f)))
            {
                OpenWindow();
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        
        private Texture2D MakeBackgroundTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
