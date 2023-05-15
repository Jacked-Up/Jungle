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

        private Tree _activeTree;
        private JungleGraphView _graphView;
        private JungleInspectorView _inspectorView;
        private JungleSearchWindow _searchWindow;
        
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
            if (Selection.activeObject.GetType() != typeof(Tree))
            {
                return false;
            }
            GetWindow<JungleEditor>();
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
            _inspectorView = rootVisualElement.Q<JungleInspectorView>();

            // Graph view ----------------------------------------------------------------------------------------------
            _graphView = rootVisualElement.Q<JungleGraphView>();
            _graphView.OnNodeSelected = nodeView =>
            {
                _graphView.SelectedNodeView = nodeView;
                _inspectorView.UpdateSelection(nodeView);
            };
            _activeTree = Selection.activeObject as Tree;
            PopulateGraphView(_activeTree);

            // Search view ---------------------------------------------------------------------------------------------
            _searchWindow = CreateInstance<JungleSearchWindow>();
            _searchWindow.Initialize(this, _graphView);
            _graphView.SetupSearchWindow(_searchWindow); // Sets up callbacks
        }

        private void OnGUI() { if (_activeTree == null) Close(); }

        private void PopulateGraphView(Tree tree)
        {
            if (tree == null) return;
            var titleLabel = rootVisualElement.Q<Label>("tree-name-label");
            titleLabel.text = tree.name;
            _graphView.PopulateGraphView(tree);
        }
    }
}
