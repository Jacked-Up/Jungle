using System;
using System.Collections.Generic;
using System.Linq;
using Jungle.Nodes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Jungle
{
    /// <summary>
    /// Main Jungle Tree class.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "My Jungle Tree 0", menuName = "Jungle Tree", order = 81)]
    public class JungleTree : ScriptableObject
    {
        #region Variables
        
        /// <summary>
        /// Array list of all associated nodes.
        /// </summary>
        [HideInInspector]
        public JungleNode[] nodes = Array.Empty<JungleNode>();
        
        /// <summary>
        /// List of all currently running nodes.
        /// </summary>
        public List<JungleNode> RunningNodes
        {
            get;
            private set; 
        } = new();

        /// <summary>
        /// The amount of time in seconds the Jungle Tree has been running.
        /// </summary>
        public float PlayTime
        {
            get;
            private set;
        }

        /// <summary>
        /// The Jungle Trees run state.
        /// </summary>
        public StateFlag State
        {
            get; 
            private set;
        } = StateFlag.Ready;
        
        private ActionsList _revertActions = new();
        private float _startPlayTime;

        /// <summary>
        /// 
        /// </summary>
        public struct StartResult
        {
            public ErrorFlag Error;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public struct StopResult
        {
            public ErrorFlag Error;
        }
        
        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum ErrorFlag
        {
            /// <summary>
            /// 
            /// </summary>
            AlreadyRunning = 0,
            /// <summary>
            /// 
            /// </summary>
            IsNotRunning = 1,
            /// <summary>
            /// 
            /// </summary>
            NotInPlayMode = 2,
            /// <summary>
            /// 
            /// </summary>
            NoNodes = 3,
            /// <summary>
            /// 
            /// </summary>
            NoRuntimeSingletonInstance = 4,
            /// <summary>
            /// 
            /// </summary>
            None = 99
        }
        
        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum StateFlag
        {
            /// <summary>
            /// Describes a node that has never been run and is not currently running.
            /// </summary>
            Ready = 0,
            /// <summary>
            /// Describes a node that is currently running.
            /// </summary>
            Running = 1,
            /// <summary>
            /// Describes a node that is not currently running and has run at some point.
            /// </summary>
            Finished = 0
        }
        
        #endregion

        /// <summary>
        /// Starts the execution of the Jungle Tree.
        /// </summary>
        public StartResult Start()
        {
            var result = new StartResult
            {
                Error = ErrorFlag.None
            };
            
            // Must be in play mode in order to start a Jungle Tree
            if (Application.isPlaying)
            {
#if UNITY_EDITOR
                Debug.LogFormat
                (LogType.Error, LogOption.NoStacktrace, this,
                    $"[{name}] Failed to start Jungle Tree because the editor is not in play-mode."
                );
#endif
                result.Error = ErrorFlag.NotInPlayMode;
                return result;
            }
            // Jungle Tree must not already be running to start
            if (State == StateFlag.Running)
            {
#if UNITY_EDITOR
                Debug.LogFormat
                (LogType.Warning, LogOption.NoStacktrace, this,
                    $"[{name}] Could not start Jungle Tree because it is already running."
                );
#endif
                result.Error = ErrorFlag.AlreadyRunning;
                return result;
            }
            // Jungle Tree must have nodes to start
            if (nodes == null || nodes.Length == 0)
            {
#if UNITY_EDITOR
                Debug.LogFormat
                (LogType.Error, LogOption.NoStacktrace, this,
                    $"[{name}] Failed to start Jungle Tree because it has no nodes."
                );
#endif
                result.Error = ErrorFlag.NoNodes;
                return result;
            }
            
            _startPlayTime = Time.unscaledTime;
            RunningNodes = new List<JungleNode>
            {
                // Find the index of the start node and add to execution list
                nodes[nodes.ToList().IndexOf(nodes.First(node => node is StartNode))]
            };
            RunningNodes[0].Initialize(new None());
            State = StateFlag.Running;

            // Jungle Runtime singleton instance must exist to start
            if (JungleRuntime.Singleton == null)
            {
#if UNITY_EDITOR
                Debug.LogFormat
                (LogType.Error, LogOption.NoStacktrace, this,
                    $"[{name}] Failed to start Jungle Tree because there wasn't a Jungle Runtime " +
                    "instance in the scene."
                );
#endif
                result.Error = ErrorFlag.NoRuntimeSingletonInstance;
                return result;
            }
            
            JungleRuntime.Singleton.StartTree(this);
            return result;
        }
        
        /// <summary>
        /// Stops the execution of the Jungle Tree.
        /// </summary>
        public void Stop()
        {
            if (State != StateFlag.Running)
            {
                return;
            }

            PlayTime = 0f;
            RunningNodes = new List<JungleNode>();
            State = StateFlag.Finished;
            
            // Invoke revert methods
            _revertActions?.InvokeAll();
            _revertActions = new ActionsList();
            
            JungleRuntime.Singleton.StopTree(this);
        }
        
        /// <summary>
        /// Executes all listening nodes in the node tree and removes nodes which have finished execution
        /// </summary>
        public void Update()
        {
            if (State == StateFlag.Finished)
            {
                return;
            }
            PlayTime = Time.unscaledTime - _startPlayTime;
            
            var query = new List<JungleNode>(RunningNodes);
            foreach (var node in RunningNodes)
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
                if (finished)
                {
                    query.Remove(node);
                }
            }
            // Populate executing nodes with new query ONLY if it has changed
            // I believe this prevents the list from redundantly reallocating new memory. I could be wrong
            if (!RunningNodes.Equals(query)) RunningNodes = query;
            if (RunningNodes.Count == 0) Stop();
        }

        /// <summary>
        /// Adds an action to be invoked when the Jungle Tree execution ends or is stopped.
        /// </summary>
        /// <param name="action">Action to add to the invoke list.</param>
        public void AddRevertAction(Action action)
        {
            _revertActions ??= new ActionsList();
            _revertActions.AddAction(action);
        }
        
        /// <summary>
        /// Removed an action from the revert invoke list.
        /// </summary>
        /// <param name="action">Action to be removed from the invoke list.</param>
        public void RemoveRevertAction(Action action)
        {
            _revertActions?.RemoveAction(action);
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
            node.name = $"{nodeType.Name}_{i.ToString()}";
            
            // Build node and populate graph view properties
            node.Tree = this;
            node.NodeProperties = new NodeProperties
            {
                guid = GUID.Generate().ToString(),
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
            node.name = $"{original.GetType().Name}_{i.ToString()}";
            
            // Build node and populate graph view properties
            node.Tree = this;
            node.NodeProperties = new NodeProperties
            {
                guid = GUID.Generate().ToString(),
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
        
        public JungleTreeEditorData editorData;
        
        public delegate void JungleTreeValidateCallback();
        public event JungleTreeValidateCallback OnJungleTreeValidate;
        private void OnValidate()
        {
            OnJungleTreeValidate?.Invoke();
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
    public class JungleTreeEditor : Editor
    {
        #region Variables
        
        private GUIContent PlayIcon =>
            _playIcon ??= EditorGUIUtility.IconContent
            (
                EditorGUIUtility.isProSkin
                    ? "PlayButton On@2x"
                    : "PlayButton@2x"
            );
        private GUIContent _playIcon;
        
        private GUIContent StopIcon =>
            _stopIcon ??= EditorGUIUtility.IconContent
            (
                EditorGUIUtility.isProSkin
                    ? "PauseButton On@2x"
                    : "PauseButton@2x"
            );
        private GUIContent _stopIcon;
        
        private JungleTree instance;
        
        #endregion

        private void OnEnable()
        {
            instance = target as JungleTree;
        }

        public override void OnInspectorGUI()
        {
            // This is literally just a description field. Yo mama
            GUILayout.Label("Description:");
            instance.editorData.description ??= "\n\n";
            var description = GUILayout.TextArea(instance.editorData.description, 500);
            if (instance.editorData.description != description)
            {
                instance.editorData.description = description;
                EditorUtility.SetDirty(instance);
            }
            
            GUILayout.Space(2f);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), EditorGUIUtility.isProSkin 
                ? new Color(0.7f, 0.7f, 0.7f, 0.5f) 
                : new Color(0.3f, 0.3f, 0.3f, 0.5f));
            GUILayout.Space(2f);
            
            // Start, stop, and status information box
            GUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.enabled = Application.isPlaying;
                GUILayout.BeginHorizontal();
                    switch (instance.State)
                    {
                        case JungleTree.StateFlag.Ready or JungleTree.StateFlag.Finished 
                        when GUILayout.Button(PlayIcon, GUILayout.Width(65f), GUILayout.ExpandHeight(true)):
                            instance.Start();
                            break;
                        case JungleTree.StateFlag.Running 
                        when GUILayout.Button(StopIcon, GUILayout.Width(65f), GUILayout.ExpandHeight(true)):
                            instance.Stop();
                            break;
                    }
                    GUILayout.BeginVertical();
                        var state = instance.State is JungleTree.StateFlag.Ready or JungleTree.StateFlag.Finished
                            ? "Ready"
                            : "Running";
                        GUILayout.Label($"State: {state}");
                        GUILayout.Label($"Time: {Math.Round(instance.PlayTime, 1)}s");
                    GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            if (!Application.isPlaying)
            {
                GUI.enabled = true;
                EditorGUILayout.HelpBox("Jungle Trees can only be debugged in play mode.", MessageType.Info);
            }
            
            Repaint();
        }
    }
    
    [Serializable]
    public struct JungleTreeEditorData
    {
        public Vector3 lastViewPosition;
        public string description;
    }
    
#endif
}
