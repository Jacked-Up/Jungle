using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
#endif

namespace Jungle
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable] [CreateAssetMenu(fileName = "Node Tree", menuName = "Jungle Node Tree")]
    public class JungleTree : ScriptableObject
    {
        #region Variables
        
        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public JungleNode rootNode;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public JungleNode[] nodes = Array.Empty<JungleNode>();

        /// <summary>
        /// 
        /// </summary>
        public List<JungleNode> ExecutingNodes { get; private set; } = new();

        /// <summary>
        /// 
        /// </summary>
        public float PlayTime { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public TreeState State { get; private set; } = TreeState.Ready;
        /// <summary>
        /// Status flag of the node tree
        /// </summary>
        public enum TreeState
        {
            /// <summary>
            /// Describes a node that has never been run and is not currently running
            /// </summary>
            Ready,
            /// <summary>
            /// Describes a node that is currently running
            /// </summary>
            Running,
            /// <summary>
            /// Describes a node that is not currently running and has run at some point
            /// </summary>
            Finished
        }
        
        private ActionsList revertActions;
        
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="link"></param>
        public void Play(Scene? link = null)
        {
            if (State == TreeState.Running)
            {
                return;
            }

            if (JungleRuntime.Singleton.RunningTrees.Contains(this))
            {
                return;
            }
            JungleRuntime.Singleton.StartTree(this, link);
            
            PlayTime = Time.unscaledTime;
            ExecutingNodes = new List<JungleNode>
            {
                rootNode
            };
            // The root node is at index zero
            ExecutingNodes[0].Initialize(new None());
            State = TreeState.Running;
        }

        /// <summary>
        /// Stops the execution of the node tree
        /// </summary>
        public void Stop()
        {
            if (State != TreeState.Running)
            {
                return;
            }
            JungleRuntime.Singleton.StopTree(this);
            PlayTime = 0f;
            State = TreeState.Finished;
            
            // Invoke revert methods
            revertActions?.InvokeAll();
            revertActions = new ActionsList();
        }
        
        /// <summary>
        /// Executes all listening nodes in the node tree and removes nodes which have finished execution
        /// </summary>
        public void Update()
        {
            if (State == TreeState.Finished) return;
            var query = new List<JungleNode>(ExecutingNodes);
            foreach (var node in ExecutingNodes)
            {
                var finished = node.Execute(out var portCalls);
                foreach (var call in portCalls)
                {
                    if (call.PortIndex > node.OutputPorts.Length - 1)
                    {
#if UNITY_EDITOR
                        if (node.OutputPorts.Length != 0)
                        {
                            Debug.LogError($"[{name}] {node.name} attempted to call an output port that is " +
                                           "out of the index range.");
                        }
#endif
                        continue;
                    }
                    foreach (var connection in node.OutputPorts[call.PortIndex].connections)
                    {
                        connection.Initialize(call.Value);
                        query.Add(connection);
                    }
                }
                // Remove from query if the node is finished executing
                if (finished) query.Remove(node);
            }
            // Populate executing nodes with new query ONLY if it has changed
            // I believe this prevents the list from redundantly reallocating new memory. I could be wrong
            if (!ExecutingNodes.Equals(query)) ExecutingNodes = query;
            if (ExecutingNodes.Count == 0) Stop();
        }

        /// <summary>
        /// Adds an action to be invoked when the Jungle Tree execution ends or is stopped.
        /// </summary>
        /// <param name="action">Action to add to the invoke list.</param>
        public void AddRevertAction(Action action)
        {
            revertActions ??= new ActionsList();
            revertActions.AddAction(action);
        }
        
        /// <summary>
        /// Removed an action from the revert invoke list.
        /// </summary>
        /// <param name="action">Action to be removed from the invoke list.</param>
        public void RemoveRevertAction(Action action)
        {
            revertActions?.RemoveAction(action);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public JungleNode CreateNode(Type nodeType, Vector2 position)
        {
            var node = CreateInstance(nodeType) as JungleNode;
            if (node == null)
            {
                Debug.LogError($"[Tree] [{name}] Failed to instance node of type {nodeType} because it doesn't " +
                               "inherit from type node");
                return null;
            }
            
            // Create unique file name
            var i = 0;
            var path = $"{AssetDatabase.GetAssetPath(this)}/{name}_{nodeType.Name}_{i.ToString()}.asset";
            while (AssetDatabase.LoadAssetAtPath(path, typeof(JungleNode)) != null) i++;
            node.name = $"{name}_{nodeType.Name}_{i.ToString()}";
            
            // Build node and populate graph view properties
            node.tree = this;
            node.NodeProperties = new NodeProperties
            {
                guid = GUID.Generate().ToString(),
                notes = string.Empty,
                position = position
            };
            
            // Add new node instance to list
            Undo.RecordObject(this, $"Added {node.name} to tree");
            var nodesList = new List<JungleNode>();
            nodes ??= Array.Empty<JungleNode>();
            nodesList.AddRange(nodes.ToList());
            nodesList.Add(node);
            nodes = nodesList.ToArray();
            
            // Add new node instance to this node tree asset
            AssetDatabase.AddObjectToAsset(node, this);
            Undo.RegisterCreatedObjectUndo(node, $"Added {node.name} to tree");
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            
            return node;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public JungleNode DuplicateNode(JungleNode original)
        {
            var node = Instantiate(original);
            if (node == null) return null;

            // Delete all existing connections
            foreach (var port in node.OutputPorts)
            {
                var portList = node.OutputPorts.ToList();
                foreach (var connection in port.connections)
                {
                    node.RemoveConnection(connection, (byte)portList.IndexOf(port));
                }
            }
            
            // Create unique file name
            var i = 0;
            var path = $"{AssetDatabase.GetAssetPath(this)}/{name}_{original.GetType().Name}_{i.ToString()}.asset";
            while (AssetDatabase.LoadAssetAtPath(path, typeof(JungleNode)) != null) i++;
            node.name = $"{name}_{original.GetType().Name}_{i.ToString()}";
            
            // Build node and populate graph view properties
            node.tree = this;
            node.NodeProperties = new NodeProperties
            {
                guid = GUID.Generate().ToString(),
                notes = node.NodeProperties.notes,
                position = original.NodeProperties.position + new Vector2(35, 35)
            };
            
            // Add new node instance to list
            Undo.RecordObject(this, $"Added {node.name} to tree");
            var query = nodes.ToList();
            query.Add(node);
            nodes = query.ToArray();
            
            // Add new node instance to this node tree asset
            AssetDatabase.AddObjectToAsset(node, this);
            Undo.RegisterCreatedObjectUndo(node, $"Added {node.name} to tree");
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            
            return node;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void DeleteNode(JungleNode node)
        {
            var query = nodes.ToList();
            if (!query.Contains(node)) return;
            Undo.RecordObject(this, $"Deleted {node.name} from tree");
            query.Remove(node);
            Undo.DestroyObjectImmediate(node);
            nodes = query.ToArray();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="connect"></param>
        /// <param name="portIndex"></param>
        public void ConnectNodes(JungleNode node, JungleNode connect, byte portIndex)
        {
            Undo.RecordObject(node, $"Added edge to {node.name}");
            node.MakeConnection(connect, portIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="disconnect"></param>
        /// <param name="portIndex"></param>
        public void DisconnectNodes(JungleNode node, JungleNode disconnect, byte portIndex)
        {
            Undo.RecordObject(node, $"Removed edge from {node.name}");
            node.RemoveConnection(disconnect, portIndex);
        }
#endif
    }

    /// <summary>
    /// Class for managing and invoking a list of actions.
    /// </summary>
    public class ActionsList
    {
        /// <summary>
        /// Invoke list.
        /// </summary>
        public List<Action> Actions { get; private set; }

        /// <summary>
        /// Adds a method to the invoke list.
        /// </summary>
        /// <param name="method">Method to add.</param>
        public void AddAction(Action method)
        {
            Actions ??= new List<Action>();
            if (Actions.Contains(method))
            {
                RemoveAction(method);
            }
            Actions.Add(method);
        }

        /// <summary>
        /// Removes a method from the invoke list.
        /// </summary>
        /// <param name="method">Method to remove.</param>
        public void RemoveAction(Action method)
        {
            Actions ??= new List<Action>();
            if (!Actions.Contains(method))
            {
                return;
            }
            Actions.Remove(method);
        }
        
        /// <summary>
        /// Invokes all of the methods.
        /// </summary>
        public void InvokeAll()
        {
            Actions ??= new List<Action>();
            foreach (var method in Actions)
            {
                method?.Invoke();
            }
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(JungleTree))]
    public class TreeEditor : Editor
    {
        #region Variables

        private static bool InformationFoldoutOpen
        {
            get => EditorPrefs.GetBool("JungleInfoFoldoutOpen", true);
            set => EditorPrefs.SetBool("JungleInfoFoldoutOpen", value);
        }
        
        private static bool DebugFoldDownOpen
        {
            get => EditorPrefs.GetBool("JungleDebugFoldoutOpen", true);
            set => EditorPrefs.SetBool("JungleDebugFoldoutOpen", value);
        }
        
        private List<string> sceneLinkOptions;
        private int selectedSceneIndex;
        private JungleTree instance;
        
        #endregion

        private void OnEnable()
        {
            instance = target as JungleTree;
        }

        public override void OnInspectorGUI()
        {
            InformationFoldoutOpen = EditorGUILayout.Foldout(InformationFoldoutOpen, new GUIContent("Information"));
            if (InformationFoldoutOpen)
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField($"Node Count: {instance.nodes?.Length ?? 0}");
                
                var treeStatus = instance.State is JungleTree.TreeState.Ready or JungleTree.TreeState.Finished
                    ? "Ready"
                    : "Running";
                EditorGUILayout.LabelField($"Tree Status: {treeStatus}");
                
                if (instance.State == JungleTree.TreeState.Running)
                {
                    EditorGUILayout.LabelField($"Play Time: {Math.Round(Time.unscaledTime - instance.PlayTime, 1)}s");
                }
                else
                {
                    EditorGUILayout.LabelField("Play Time: 0s");
                }
                GUI.enabled = true;
            }
            
            GUILayout.Space(2.5f);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), EditorGUIUtility.isProSkin 
                ? new Color(0.7f, 0.7f, 0.7f, 0.5f) 
                : new Color(0.3f, 0.3f, 0.3f, 0.5f));
            GUILayout.Space(2.5f);
            
            DebugFoldDownOpen = EditorGUILayout.Foldout(DebugFoldDownOpen, new GUIContent("Debug"));
            if (DebugFoldDownOpen)
            {
                GUI.enabled = Application.isPlaying;
                sceneLinkOptions = new List<string>()
                {
                    "None"
                };
                var sceneCount = SceneManager.sceneCount;
                var sceneNames = new string[sceneCount];
                for (var i = 0; i < sceneCount; i++) 
                {
                    sceneLinkOptions.Add(SceneManager.GetSceneAt(i).name);
                }
                selectedSceneIndex = EditorGUILayout.Popup("Linked Scene", selectedSceneIndex, sceneLinkOptions.ToArray());
            
                GUILayout.BeginHorizontal();
                GUI.enabled = instance.State != JungleTree.TreeState.Running && Application.isPlaying;
                if (GUILayout.Button("Play"))
                {
                    // Condition simply checks if a scene is selected and calls the proper method
                    if (selectedSceneIndex == 0)
                    {
                        instance.Play();
                    }
                    else
                    {
                        instance.Play(SceneManager.GetSceneAt(selectedSceneIndex - 1));
                    }
                }
                GUI.enabled = instance.State == JungleTree.TreeState.Running && Application.isPlaying;
                if (GUILayout.Button("Stop"))
                {
                    instance.Stop();
                }
                GUILayout.EndHorizontal();
                if (!Application.isPlaying)
                {
                    GUI.enabled = true;
                    EditorGUILayout.HelpBox("You can only debug Jungle Trees while the editor" +
                                            " is in play mode.", MessageType.Info);
                }
            }

            Repaint();
        }
    }
#endif
}
