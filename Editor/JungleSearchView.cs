using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
            //_contextRequest = new ContextRequest(context, dropPosition);
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext _)
        {
            var allJungleNodeTypes = TypeCache.GetTypesDerivedFrom<JungleNode>();
            var searchTree = new List<SearchTreeEntry>();
            allJungleNodeTypes.ToList().ForEach(node =>
            {
                var nodeInstance = CreateInstance(node) as JungleNode;
                searchTree.Add(new SearchTreeEntry(new GUIContent(nodeInstance.GetIcon(), nodeInstance.GetTitle())));
            });
            
            /*
            // Build search view without context
            if (_contextRequest == null)
            {
                allJungleNodeTypes.ToList().ForEach(node =>
                {
                    var nodeInstance = CreateInstance(node) as JungleNode;
                    searchTree.Add(new SearchTreeEntry(new GUIContent(nodeInstance.GetIcon(), nodeInstance.GetTitle())));
                });
            }
            // Build search view with context
            else
            {
                var contextType = _contextRequest.Value.Context;
                var contextualJungleNodeTypes = new List<JungleNode>();
                foreach (var jungleNodeType in allJungleNodeTypes)
                {
                    var instance = CreateInstance(jungleNodeType) as JungleNode;
                    if (instance == null) continue;
                    
                    if (instance.GetInput().PortType == contextType)
                    {
                        contextualJungleNodeTypes.Add(instance);
                    }
                }
                
                contextualJungleNodeTypes.ForEach(node =>
                {
                    searchTree.Add(new SearchTreeEntry(new GUIContent(node.GetIcon(), node.GetTitle())));
                });
            }
            */

            _contextRequest = null;
            return searchTree;
            /*
            var groups = new List<Group>();

            foreach (var jungleNodeType in allJungleNodeTypes)
            {
                var instance = CreateInstance(jungleNodeType) as JungleNode;
                if (instance == null)
                {
                    continue;
                }

                var group = instance.GetGroup();
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
            
            var searchTree = new List<SearchTreeEntry>();
            return searchTree;
            */
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var mousePosition = _jungleEditor.MousePositionToGraphViewPosition(context.screenMousePosition);
            return _jungleEditor.TryAddNodeToGraph(searchTreeEntry.userData.GetType(), mousePosition);
        }
    }
}
