using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    public class JungleEditor : EditorWindow
    {
        #region Variables

        public const string STYLE_SHEET_PATH =
            "Packages/com.jackedupstudios.jungle/Editor/UI/JungleEditorStyle.uss";

        private NodeTree _activeNodeTree;
        private JungleGraphView _graphView;
        private JungleInspectorView _inspectorView;
        private JungleSearchWindow _searchWindow;
        
        #endregion

        [OnOpenAsset]
        public static bool OpenAssetCallback(int _, int __)
        {
            var selected = Selection.activeObject;
            if (selected is not NodeTree)
            {
                return false;
            }
            
            var window = GetWindow<JungleEditor>();
            window.titleContent = new GUIContent("Jungle Editor");

            if (selected is NodeTree nodeTree)
            {
                window.PopulateGraphView(nodeTree);
                return true;
            }
            return true;
        }

        private void CreateGUI()
        {
            var jungleEditorFilePath = AssetDatabase.GetAssetPath(Resources.Load("JungleEditor"));
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(jungleEditorFilePath);
            visualTreeAsset.CloneTree(rootVisualElement);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_SHEET_PATH);
            rootVisualElement.styleSheets.Add(styleSheet);
            
            // Inspector view ------------------------------------------------------------------------------------------
            _inspectorView = rootVisualElement.Q<JungleInspectorView>();

            // Graph view ----------------------------------------------------------------------------------------------
            _graphView = rootVisualElement.Q<JungleGraphView>();
            _graphView.OnNodeSelected = nodeView =>
            {
                _graphView.UpdateSelection(nodeView);
                _inspectorView.UpdateSelection(nodeView);
            };
            _activeNodeTree = Selection.activeObject as NodeTree;
            PopulateGraphView(_activeNodeTree);

            // Search view ---------------------------------------------------------------------------------------------
            _searchWindow = CreateInstance<JungleSearchWindow>();
            _searchWindow.Initialize(this, _graphView);
            _graphView.SetupSearchWindow(_searchWindow); // Sets up callbacks
        }

        private void OnGUI() { if (_activeNodeTree == null) Close(); }

        private void PopulateGraphView(NodeTree nodeTree)
        {
            if (nodeTree == null) return;
            if (AssetDatabase.CanOpenAssetInEditor(nodeTree.GetInstanceID()))
            {
                var titleLabel = rootVisualElement.Q<Label>("tree-name-label");
                titleLabel.text = nodeTree.name;
                _graphView.PopulateView(nodeTree);
            }
            else
            {
                Debug.LogError("[Jungle Editor] Failed to open requested node tree");
            }
        }
    }
}
