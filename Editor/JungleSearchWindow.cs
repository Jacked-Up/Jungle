using System.Collections.Generic;
using System.Linq;
using Jungle.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    public class JungleSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        #region Variables

        private JungleEditor _jungleEditor;
        private JungleGraphView _graphView;
        
        private struct CategoryCache
        {
            public string CategoryName;
            public List<JungleNode> Nodes;

            public CategoryCache(string categoryName, JungleNode firstNode)
            {
                CategoryName = categoryName;
                Nodes = new List<JungleNode> {firstNode};
            }
        }

        #endregion

        public void Initialize(JungleEditor jungleEditor, JungleGraphView graphView)
        {
            _jungleEditor = jungleEditor;
            _graphView = graphView;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var nodeTypes = TypeCache.GetTypesDerivedFrom<JungleNode>();
            var categories = new List<CategoryCache>();
            nodeTypes.ToList().ForEach(nodeType =>
            {
                var typeObject = CreateInstance(nodeType) as JungleNode;
                if (typeObject == null || typeObject is RootNode) return;
                var typeCategory = typeObject.Category;
                if (categories.All(category => category.CategoryName != typeCategory))
                {
                    categories.Add(new CategoryCache(typeCategory, typeObject));
                }
                else
                {
                    for (var i = 0; i < categories.Count; i++)
                    {
                        if (categories[i].CategoryName != typeCategory) continue;
                        categories[i].Nodes.Add(typeObject);
                        break;
                    }
                }
            });
            
            var searchTree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"))
            };
            var noCategoryNodes = new List<CategoryCache>();
            categories.ForEach(category =>
            {
                if (string.IsNullOrEmpty(category.CategoryName))
                {
                    noCategoryNodes.Add(category);
                }
                else
                {
                    searchTree.Add(new SearchTreeGroupEntry(new GUIContent(category.CategoryName))
                    {
                        level = 1
                    });
                    category.Nodes.ForEach(node =>
                    {
                        searchTree.Add(new SearchTreeEntry(new GUIContent(node.TitleName))
                        {
                            userData = node,
                            level = 2
                        });
                    });
                }
            });
            noCategoryNodes.ForEach(category =>
            {
                category.Nodes.ForEach(node =>
                {
                    searchTree.Add(new SearchTreeEntry(new GUIContent(node.TitleName))
                    {
                        userData = node,
                        level = 1
                    });
                });
            });
            return searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var graphView = _graphView;
            var window = _jungleEditor;
            var editorWindowMousePosition =
                window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent,
                    context.screenMousePosition - window.position.position);
            var graphViewMousePosition = graphView.contentViewContainer.WorldToLocal(editorWindowMousePosition);
            graphView.CreateNode(searchTreeEntry.userData.GetType(), graphViewMousePosition);
            return true;
        }
    }
}
