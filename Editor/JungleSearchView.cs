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

        public static JungleSearchView Instance
        {
            get;
            private set;
        }
        
        private JungleEditor _jungleEditor;

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

        public void Initialize(JungleEditor editor)
        {
            _jungleEditor = editor;
            Instance = this;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext _)
        {
            var jungleNodeTypes = TypeCache.GetTypesDerivedFrom<JungleNode>();
            var groups = new List<Group>();

            foreach (var jungleNodeType in jungleNodeTypes)
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
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var mousePosition = _jungleEditor.MousePositionToGraphViewPosition(context.screenMousePosition);
            return _jungleEditor.TryAddNodeToGraph(searchTreeEntry.userData.GetType(), mousePosition);
        }
    }
}
