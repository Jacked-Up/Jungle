using System.Collections.Generic;
using System.Linq;
using Jungle.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Jungle.Editor
{
    public class JungleSearchView : ScriptableObject, ISearchWindowProvider
    {
        #region Variables

        private const string TITLE = "Add Node to Tree";

        private JungleEditor _jungleEditor;
        
        private struct CategoryCache
        {
            public readonly string CategoryName;
            public readonly List<JungleNode> Nodes;

            public CategoryCache(string categoryName, JungleNode firstNode)
            {
                CategoryName = categoryName;
                Nodes = new List<JungleNode> {firstNode};
            }
        }

        #endregion

        public void Initialize(JungleEditor editor)
        {
            _jungleEditor = editor;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var nodeTypes = TypeCache.GetTypesDerivedFrom<JungleNode>();
            var categories = new List<CategoryCache>();
            nodeTypes.ToList().ForEach(nodeType =>
            {
                var typeObject = CreateInstance(nodeType) as JungleNode;
                if (typeObject == null || typeObject is StartNode) return;
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
                new SearchTreeGroupEntry(new GUIContent(TITLE))
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
            var mousePosition = _jungleEditor.GetMousePosition(context.screenMousePosition);
            //return _jungleEditor.TryAddNodeToGraph(searchTreeEntry.userData.GetType(), mousePosition);
            return true;
        }
    }
}
