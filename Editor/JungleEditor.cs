using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    public class JungleEditor : EditorWindow
    {
        #region Variables
        
        private const string SEARCH_FILTER = "t:JungleTree";
        private const string TAB_TITLE = "Jungle Editor";
        public const string STYLE_SHEET_FILE_PATH = "Packages/com.jackedupsoftware.jungle/Editor/UI/JungleEditorStyle.uss";

        public JungleTree EditTree
        {
            get
            {
                // We can just return the cache if one is available
                if (_editTree != null)
                {
                    return _editTree;
                }
                
                // If no cache is available, we can just look for the last selected for convenience
                var lastActiveGuid = EditorPrefs.GetString("Jungle_LastEditTreeInstanceID", string.Empty);
                if (string.IsNullOrEmpty(lastActiveGuid))
                {
                    return null;
                }
                var jungleTreeAsset = GetAllJungleTrees().FirstOrDefault(jungleTree =>
                {
                    return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(jungleTree)) == lastActiveGuid;
                });
                if (jungleTreeAsset == null)
                {
                    return null;
                }
                return _editTree = jungleTreeAsset;
            }
            private set
            {
                if (value == null)
                {
                    EditorPrefs.SetString("Jungle_LastEditTreeInstanceID", string.Empty);
                    _editTree = null;
                    return;
                }
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value));
                EditorPrefs.SetString("Jungle_LastEditTreeInstanceID", guid);
                _editTree = value;
            }
        }
        private JungleTree _editTree;
        
        private JungleInspectorView _inspectorView;
        private JungleSearchView _searchView;
        private JungleGraphView _graphView;

        #endregion
        
        private void OnEnable()
        {
            var tabIcon = EditorGUIUtility.IconContent
            (
                EditorGUIUtility.isProSkin
                    ? "d_BlendTree Icon"
                    : "BlendTree Icon"
            );
            tabIcon.text = TAB_TITLE;
            titleContent = tabIcon;
        }
        
        private void CreateGUI()
        {
            JungleTutorials.TryShowEditorTutorial();
            
            var jungleEditorFilePath = AssetDatabase.GetAssetPath(Resources.Load("JungleEditor"));
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(jungleEditorFilePath);
            visualTreeAsset.CloneTree(rootVisualElement);
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_SHEET_FILE_PATH);
            rootVisualElement.styleSheets.Add(styleSheet);
            
            _inspectorView = rootVisualElement.Q<JungleInspectorView>("inspector-view");
            _inspectorView.Initialize();
            
            _searchView = CreateInstance<JungleSearchView>();
            _searchView.Initialize(this);
            
            _graphView = rootVisualElement.Q<JungleGraphView>("graph-view");
            _graphView.Initialize(this, _inspectorView, _searchView);
            _graphView?.UpdateGraphView();
        }
        
        private void Update()
        {
            _graphView?.UpdateNodeViews();
            UpdateGraphHeader();
        }

        [OnOpenAsset]
        public static bool OpenAssetCallback(int _, int __)
        {
            if (Selection.activeObject.GetType() != typeof(JungleTree))
            {
                return false;
            }
            var window = GetWindow<JungleEditor>();
            window.EditTree = Selection.activeObject as JungleTree;
            window._inspectorView.UpdateSelection(null);
            window._graphView?.UpdateGraphView();
            return true;
        }
        
        private void UpdateGraphHeader()
        {
            // Update tree title label
            var titleLabel = rootVisualElement.Q<Label>("tree-name-label");
            if (titleLabel != null && EditTree != null)
            {
                titleLabel.text = JungleGUILayout.ShortenString(EditTree.name, 28);
            }
            
            // Update node selection list label
            var nodeLabel = rootVisualElement.Q<Label>("node-name-label");
            if (nodeLabel != null && _graphView != null && _graphView.SelectedNodeViews.Count > 0)
            {
                nodeLabel.text = string.Empty;
                for (var i = 0; i < 4; i++)
                {
                    if (i + 1 == 4 && _graphView.SelectedNodeViews.Count > 4)
                    {
                        nodeLabel.text += $"{_graphView.SelectedNodeViews.Count - 3} more selected";
                        break;
                    }
                    if (i > _graphView.SelectedNodeViews.Count - 1)
                    {
                        break;
                    }
                    nodeLabel.text += $"{JungleGUILayout.ShortenString(_graphView.SelectedNodeViews[i].Node.name, 32)}\n";
                }
            }
            else if (nodeLabel != null)
            {
                nodeLabel.text = string.Empty;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="graphPosition"></param>
        /// <returns></returns>
        public bool TryAddNodeToGraph(Type nodeType, Vector2 graphPosition)
        {
            var nodeView = _graphView?.CreateNode(nodeType, graphPosition);
            //OnSelectedNode(nodeView);
            return _graphView != null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="screenMousePosition"></param>
        /// <returns></returns>
        public Vector2 MousePositionToGraphViewPosition(Vector2 screenMousePosition)
        {
            var mousePosition = rootVisualElement.ChangeCoordinatesTo(rootVisualElement.parent,
                screenMousePosition - position.position);
            return _graphView.contentViewContainer.WorldToLocal(mousePosition);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
}
