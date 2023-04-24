using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    public class JungleEditor : EditorWindow
    {
        #region Variables

        public JungleInspectorView JungleInspectorView
        {
            get;
            private set;
        }
        
        private JungleGraphView _jungleGraphView;
        
        #endregion

        [OnOpenAsset]
        public static bool OpenAssetCallback(int instanceId, int line)
        {
            if (Selection.activeObject is not NodeTree)
            {
                return false;
            }
            var editorWindow = GetWindow<JungleEditor>();
            editorWindow.titleContent = new GUIContent("Jungle Editor");
            return true;
        }

        public void CreateGUI()
        {
            var jungleEditorFilePath = AssetDatabase.GetAssetPath(Resources.Load("JungleEditor"));
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(jungleEditorFilePath);
            visualTreeAsset.CloneTree(rootVisualElement);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.jackedupstudios.jungle/Editor/UI/JungleEditorStyle.uss");
            rootVisualElement.styleSheets.Add(styleSheet);

            _jungleGraphView = rootVisualElement.Q<JungleGraphView>();
            _jungleGraphView.SetJungleEditor(this);
            _jungleGraphView.EditorWindow = this;
            JungleInspectorView = rootVisualElement.Q<JungleInspectorView>();
            JungleInspectorView.SetJungleEditor(this);
            
            _jungleGraphView.OnNodeSelected = NodeSelectionChangedCallback;
            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            var nodeTree = Selection.activeObject as NodeTree;
            if (nodeTree != null && AssetDatabase.CanOpenAssetInEditor(nodeTree.GetInstanceID()))
            {
                _jungleGraphView.PopulateView(nodeTree);
            }
        }

        private void NodeSelectionChangedCallback(JungleNodeView nodeView)
        {
            JungleInspectorView.UpdateSelection(nodeView);
            _jungleGraphView.SelectedNodeViewCallback(nodeView, this);
        }
    }
}
