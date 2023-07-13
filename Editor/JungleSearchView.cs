using System;
using System.Collections.Generic;
using System.Linq;
using Jungle.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Jungle.Editor
{
    public class JungleSearchView : ScriptableObject, ISearchWindowProvider
    {
        #region Variables

        private JungleEditor _jungleEditor;
        private ContextRequest? _contextRequest;
        private struct ContextRequest
        {
            public readonly Type Context;
            public readonly Vector2 DropPosition;

            public ContextRequest(Type context, Vector2 dropPosition)
            {
                Context = context;
                DropPosition = dropPosition;
            }
        }
        
        private struct Group
        {
            public readonly string Name;
            public readonly List<Group> Subgroups;
            public readonly List<Item> Items;

            public Group(string name)
            {
                Name = name;
                Subgroups = new List<Group>();
                Items = new List<Item>();
            }
        }
        
        private struct Item
        {
            public readonly string Name;
            public readonly Type Type;
            
            public Item(string name, Type type)
            {
                Name = name;
                Type = type;
            }
        }

        #endregion
        
        public void SetupContext(Type context, Vector2 dropPosition)
        {
            _contextRequest = new ContextRequest(context, dropPosition);
            SearchWindow.Open(new SearchWindowContext(dropPosition), this);
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext _)
        {
            var allJungleNodeTypes = TypeCache.GetTypesDerivedFrom<JungleNode>();
            var searchTree = new List<SearchTreeEntry>();

            // Build search view without context
            if (_contextRequest == null)
            {
                searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Add Node")));
                
                var groups = new List<Group>();

                foreach (var jungleNodeType in allJungleNodeTypes)
                {
                    var instance = CreateInstance(jungleNodeType) as JungleNode;
                    if (instance == null)
                    {
                        continue;
                    }

                    var group = instance.GetCategory();
                    if (group.Contains(":HIDDEN"))
                    {
                        continue;
                    }

                    var groupList = group.Split('/');
                    foreach (var groupListName in groupList)
                    {
                        var preexistingGroup = groups.FirstOrDefault(g => g.Name == groupListName);
                        if (!preexistingGroup.Equals(new Group()))
                        {
                            //var rebuildGroup = new Group(groupListName, preexistingGroup.Subgroups, preexistingGroup.Items);
                        }
                        else
                        {
                        
                        }
                    }
                }
                
                allJungleNodeTypes.ToList().ForEach(node =>
                {
                    var nodeInstance = CreateInstance(node) as JungleNode;
                    searchTree.Add(new SearchTreeEntry(new GUIContent(nodeInstance.GetTitle(), nodeInstance.GetIcon())));
                });
            }
            // Build search view with context
            else
            {
                var contextType = _contextRequest.Value.Context;

                var mainGroup = new SearchTreeGroupEntry(new GUIContent($"Add Node ({contextType.Name})"));
                searchTree.Add(mainGroup);
                
                var contextualJungleNodeTypes = new List<JungleNode>();
                foreach (var jungleNodeType in allJungleNodeTypes)
                {
                    var instance = CreateInstance(jungleNodeType) as JungleNode;
                    if (instance == null) continue;
                    
                    /*
                    if (instance.GetInput().Type == contextType)
                    {
                        contextualJungleNodeTypes.Add(instance);
                    }
                    */
                }
                
                contextualJungleNodeTypes.ForEach(node =>
                {
                    var entry = CreateSearchTreeEntry(node, 1);
                    if (entry == null)
                    {
                        return;
                    }
                    searchTree.Add(entry);
                });
            }
            
            return searchTree;
        }
        
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var convertedPosition = JungleGraphView.Singleton.viewport.ChangeCoordinatesTo(
                JungleEditor.Singleton.rootVisualElement.parent,
                _contextRequest.Value.DropPosition - JungleEditor.Singleton.position.position);

            var a = JungleGraphView.Singleton.viewTransform.matrix.inverse.MultiplyPoint(_contextRequest.Value.DropPosition);
            
            JungleEditor.Singleton.TryAddNodeToGraph(searchTreeEntry.userData.GetType(), a);
            _contextRequest = null;
            return true;
        }

        private SearchTreeEntry CreateSearchTreeEntry(JungleNode node, int level)
        {
            if (node.GetType() == typeof(StartNode))
            {
                return null;
            }
            
            var entry = new SearchTreeEntry(new GUIContent(node.GetTitle(), node.GetIcon()))
            {
                level = level,
                userData = node
            };
            return entry;
        }
        
        private SearchTreeEntry CreateSearchTreeEntry(Type type, int level)
        {
            if (type == typeof(StartNode))
            {
                return null;
            }

            var node = CreateInstance(type) as JungleNode;
            if (node == null)
            {
                return null;
            }
            var entry = new SearchTreeEntry(new GUIContent(node.GetTitle(), node.GetIcon()))
            {
                level = level,
                userData = type
            };
            return entry;
        }
    }
}
