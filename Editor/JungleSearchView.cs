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

        public static JungleSearchView Instance
        {
            get;
            private set;
        }
        
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
            Instance = this;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var nodeTypes = TypeCache.GetTypesDerivedFrom<JungleNode>();
            var categories = new List<CategoryCache>();
            var groupEntries = new List<SearchTreeEntry>(); // Store group entries separately

            nodeTypes.ToList().ForEach(nodeType =>
            {
                var typeObject = CreateInstance(nodeType) as JungleNode;
                if (typeObject == null || typeObject is StartNode) return;
                var typeCategory = typeObject.GetCategory();
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

            var searchTree = new List<SearchTreeEntry>();
            var noCategoryNodes = new List<JungleNode>();

            categories.ForEach(category =>
            {
                if (string.IsNullOrEmpty(category.CategoryName))
                {
                    noCategoryNodes.AddRange(category.Nodes);
                }
                else
                {
                    var categoryNames = category.CategoryName.Split('/');
                    var currentGroup = searchTree;
                    var groupName = "";

                    for (var i = 0; i < categoryNames.Length; i++)
                    {
                        groupName += categoryNames[i];

                        var existingGroup = FindGroupEntry(currentGroup, groupName);
                        if (existingGroup == null)
                        {
                            var newGroup = new SearchTreeGroupEntry(new GUIContent(categoryNames[i]))
                            {
                                level = i + 1
                            };

                            currentGroup.Add(newGroup);
                            groupEntries.Add(newGroup);
                            //currentGroup = newGroup.children; // Use the children list of the group entry
                        }
                        else
                        {
                            //currentGroup = existingGroup.children; // Use the children list of the existing group entry
                        }

                        groupName += "/";
                    }

                    category.Nodes.ForEach(node =>
                    {
                        currentGroup.Add(new SearchTreeEntry(new GUIContent(node.GetTitle(), node.GetIcon()))
                        {
                            userData = node,
                            level = categoryNames.Length + 1
                        });
                    });
                }
            });

            // Add group entries to the search tree before individual entries
            searchTree.AddRange(groupEntries);

            // Add individual entries after group entries
            noCategoryNodes.ForEach(node =>
            {
                searchTree.Add(new SearchTreeEntry(new GUIContent(node.GetTitle()))
                {
                    userData = node,
                    level = 1
                });
            });

            return searchTree;
        }
        
        private SearchTreeEntry FindGroupEntry(List<SearchTreeEntry> searchTree, string groupName)
        {
            foreach (var entry in searchTree)
            {
                if (entry is SearchTreeGroupEntry groupEntry && groupEntry.name == groupName)
                {
                    return groupEntry;
                }
            }
            return null;
        }
        
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var mousePosition = _jungleEditor.MousePositionToGraphViewPosition(context.screenMousePosition);
            return _jungleEditor.TryAddNodeToGraph(searchTreeEntry.userData.GetType(), mousePosition);
        }
    }
}
