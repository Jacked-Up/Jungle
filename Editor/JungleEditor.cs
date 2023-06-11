using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    public class JungleEditor : EditorWindow
    {
        #region Variables

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
                if (_editTree != null) return _editTree;
                
                // If no cache is available, we can just look for the last selected for convenience
                var lastEditTreeInstanceID = EditorPrefs.GetInt("Jungle_LastEditTreeInstanceID", -1);
                var jungleTreeAsset = EditorUtility.InstanceIDToObject(lastEditTreeInstanceID) as JungleTree;
                if (jungleTreeAsset == null)
                {
                    return null;
                }
                _editTree = jungleTreeAsset;
                return _editTree;
            }
            private set
            {
                if (value == null)
                {
                    EditorPrefs.SetInt("Jungle_LastEditTreeInstanceID", -1);
                    _editTree = null;
                    return;
                }
                EditorPrefs.SetInt("Jungle_LastEditTreeInstanceID", value.GetInstanceID());
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
            titleContent = new GUIContent("Jungle Editor", icon);
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
            
            // Search view ---------------------------------------------------------------------------------------------
            _searchView = CreateInstance<JungleSearchView>();
            _searchView.Initialize(this);

            // Graph view ----------------------------------------------------------------------------------------------
            _graphView = rootVisualElement.Q<JungleGraphView>("graph-view");
            _graphView.OnNodeSelected = nodeView =>
            {
                _graphView.SelectedNodeView = nodeView;
                _inspectorView.UpdateSelection(nodeView);
            };
            _graphView.Initialize(this, _searchView);
            RepaintGraphView();
        }

        private void OnGUI()
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="graphPosition"></param>
        /// <returns></returns>
        public bool TryAddNodeToGraph(Type nodeType, Vector2 graphPosition)
        {
            _graphView?.CreateNode(nodeType, graphPosition);
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
    }
}
