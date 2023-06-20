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
        
        private const string TAB_ICON_DARK_FILE_PATH = 
            "Icons/JungleEditorIconDark";
        private const string TAB_ICON_LIGHT_FILE_PATH = 
            "Icons/JungleEditorIconLight";
        public const string STYLE_SHEET_FILE_PATH =
            "Packages/com.jackedupstudios.jungle/Editor/UI/JungleEditorStyle.uss";
        private const int MAXIMUM_DISPLAYED_TREE_NAME = 28;

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
            var icon = EditorGUIUtility.isProSkin 
                ? Resources.Load<Texture>(TAB_ICON_DARK_FILE_PATH) 
                : Resources.Load<Texture>(TAB_ICON_LIGHT_FILE_PATH);
            titleContent = new GUIContent("Jungle Editor*", icon);
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
            window.RepaintGraphView();
            window._inspectorView.UpdateSelection(null);
            return true;
        }

        private void CreateGUI()
        {
            JungleTutorials.TryShowEditorTutorial();
            
            var jungleEditorFilePath = AssetDatabase.GetAssetPath(Resources.Load("JungleEditor"));
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(jungleEditorFilePath);
            visualTreeAsset.CloneTree(rootVisualElement);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_SHEET_FILE_PATH);
            rootVisualElement.styleSheets.Add(styleSheet);
            
            // Inspector view ------------------------------------------------------------------------------------------
            _inspectorView = rootVisualElement.Q<JungleInspectorView>("inspector-view");
            _inspectorView.Initialize(this);
            
            // Search view ---------------------------------------------------------------------------------------------
            _searchView = CreateInstance<JungleSearchView>();
            _searchView.Initialize(this);

            // Graph view ----------------------------------------------------------------------------------------------
            _graphView = rootVisualElement.Q<JungleGraphView>("graph-view");
            _graphView.Initialize(this, _searchView);
            RepaintGraphView();
        }

        private void OnGUI()
        {
            GetGhostEdgeState();
            RepaintNodeViews();
            Repaint();
        }

        private void RepaintGraphView()
        {
            _graphView?.UpdateGraphView();
            RepaintTitle();
            RepaintNodeViews();
        }

        private void RepaintNodeViews()
        {
            _graphView?.UpdateNodeViews();
        }
        
        private void RepaintTitle()
        {
            var titleLabel = rootVisualElement.Q<Label>("tree-name-label");
            if (titleLabel == null || EditTree == null) return;
            // Ensures the name displayed can be no longer than the maximum length
            // If it is too long, this removes the extra text and adds a "..." bit
            titleLabel.text = EditTree.name.Length > MAXIMUM_DISPLAYED_TREE_NAME 
                ? $"{EditTree.name[..(MAXIMUM_DISPLAYED_TREE_NAME - 2)]}..." 
                : EditTree.name;
        }

        private void GetGhostEdgeState()
        {
            if (_graphView == null || _graphView.SelectedNodeView == null)
            {
                return;
            }
return;
            foreach (var outputPortView in _graphView.SelectedNodeView.OutputPortViews)
            {
                if (!outputPortView.edgeConnector.edgeDragHelper.edgeCandidate.isGhostEdge)
                    continue;
                
                Debug.Log("A ghost exists");
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeView"></param>
        public void OnSelectedNode(JungleNodeView nodeView)
        {
            _graphView.UpdateSelected(nodeView);
            _inspectorView.UpdateSelection(nodeView);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void OnDeselectedNode()
        {
            _graphView.UpdateSelected(null);
            _inspectorView.UpdateSelection(null);
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
            OnSelectedNode(nodeView);
            return _graphView != null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="screenMousePosition"></param>
        /// <returns></returns>
        public Vector2 GetMousePosition(Vector2 screenMousePosition)
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
