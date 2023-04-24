#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    public class JungleInspectorView : VisualElement
    {
        #region Variables

        private UnityEditor.Editor _editor;

        private JungleEditor _jungleEditor;
        
        #endregion

        public JungleInspectorView() {}
        
        public new class UxmlFactory : UxmlFactory<JungleInspectorView, UxmlTraits> {}

        public void SetJungleEditor(JungleEditor editor)
        {
            _jungleEditor = editor;
        }
        
        public void UpdateSelection(JungleNodeView nodeView)
        {
            Clear();
            Object.DestroyImmediate(_editor);
            _editor = UnityEditor.Editor.CreateEditor(nodeView.Node);
            var container = new IMGUIContainer(() =>
            {
                if (_editor.target != null)
                {
                    _editor.OnInspectorGUI();
                }
                else
                {
                    _jungleEditor.Close();
                }
            });
            Add(container);
        }
    }
}
#endif
