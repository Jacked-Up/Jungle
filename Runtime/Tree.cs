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
    public class Tree : ScriptableObject
    {
        #region Variables
        
        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public Node rootNode;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public Node[] nodes = Array.Empty<Node>();

        /// <summary>
        /// 
        /// </summary>
        public List<Node> ExecutingNodes { get; private set; } = new();

        /// <summary>
        /// 
        /// </summary>
        public Scene[] PrerequisiteScenes
        {
            get => prerequisiteScenes;
            set => prerequisiteScenes = value;
        }
        [SerializeField] [HideInInspector]
        private Scene[] prerequisiteScenes = Array.Empty<Scene>();
        
        /// <summary>
        /// 
        /// </summary>
        public Scene RequisiteScene { get; private set; }

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
        
        #endregion

        /// <summary>
        /// Initializes the node tree for execution
        /// </summary>
        public void Play()
        {
            if (State == TreeState.Running) return;
            if (!JungleRuntime.Singleton.PlayTree(this))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{name}] Failed to initialize and play");
#endif
                return;
            }
            HandlePlay();
        }

        /// <summary>
        /// Initializes the node tree for execution and links to a scene
        /// </summary>
        /// <param name="linkedScene">If the linked scene is unloaded while the tree is executing, execution will
        /// automatically halt</param>
        public void Play(Scene linkedScene)
        {
            if (State == TreeState.Running)
            {
                return;
            }
            if (!JungleRuntime.Singleton.PlayTree(this, linkedScene))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{name}] Failed to initialize and play");
#endif
                return;
            }
            HandlePlay();
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
        }
        
        /// <summary>
        /// Executes all listening nodes in the node tree and removes nodes which have finished execution
        /// </summary>
        public void Update()
        {
            if (State == TreeState.Finished) return;
            var query = new List<Node>(ExecutingNodes);
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
                                           $"out of the index range");
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
            if (ExecutingNodes.Count == 0) State = TreeState.Finished;
        }

        private void HandlePlay()
        {
            PlayTime = Time.unscaledTime;
            ExecutingNodes = new List<Node> {rootNode};
            ExecutingNodes[0].Initialize(true);
            State = TreeState.Running;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public Node CreateNode(Type nodeType, Vector2 position)
        {
            var node = CreateInstance(nodeType) as Node;
            if (node == null)
            {
                Debug.LogError($"[Tree] [{name}] Failed to instance node of type {nodeType} because it doesn't " +
                               "inherit from type node");
                return null;
            }
            
            // Create unique file name
            var i = 0;
            var path = $"{AssetDatabase.GetAssetPath(this)}/{name}_{nodeType.Name}_{i.ToString()}.asset";
            while (AssetDatabase.LoadAssetAtPath(path, typeof(Node)) != null) i++;
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
            var nodesList = new List<Node>();
            nodes ??= Array.Empty<Node>();
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
        public Node DuplicateNode(Node original)
        {
            var node = Instantiate(original);
            if (node == null) return null;
            
            // Create unique file name
            var i = 0;
            var path = $"{AssetDatabase.GetAssetPath(this)}/{name}_{original.GetType().Name}_{i.ToString()}.asset";
            while (AssetDatabase.LoadAssetAtPath(path, typeof(Node)) != null) i++;
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
        public void DeleteNode(Node node)
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
        public void ConnectNodes(Node node, Node connect, byte portIndex)
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
        public void DisconnectNodes(Node node, Node disconnect, byte portIndex)
        {
            Undo.RecordObject(node, $"Removed edge from {node.name}");
            node.RemoveConnection(disconnect, portIndex);
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Tree))]
    public class TreeEditor : Editor
    {
        #region Variables

        private static bool _prerequisiteFoldoutOpen = true;
        private static bool _informationFoldoutOpen = true;
        private static bool _debugFoldDownOpen;
        private List<string> sceneLinkOptions;
        private int selectedSceneIndex;
        private Tree instance;

        private SerializedProperty _prerequisiteScenes;
        
        #endregion

        private void OnEnable()
        {
            instance = target as Tree;
            _prerequisiteScenes = serializedObject.FindProperty("prerequisiteScenes");
        }

        public override void OnInspectorGUI()
        {
            _prerequisiteFoldoutOpen = EditorGUILayout.Foldout(_prerequisiteFoldoutOpen, new GUIContent("Prerequisites"));
            if (_prerequisiteFoldoutOpen)
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(_prerequisiteScenes);
                serializedObject.ApplyModifiedProperties();
            }
            
            GUILayout.Space(2.5f);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), EditorGUIUtility.isProSkin 
                ? new Color(0.7f, 0.7f, 0.7f, 0.5f) 
                : new Color(0.3f, 0.3f, 0.3f, 0.5f));
            GUILayout.Space(2.5f);
            
            _informationFoldoutOpen = EditorGUILayout.Foldout(_informationFoldoutOpen, new GUIContent("Information"));
            if (_informationFoldoutOpen)
            {
                GUI.enabled = false;
                EditorGUILayout.LabelField($"Node Count: {instance.nodes?.Length ?? 0}");
                
                var treeStatus = instance.State is Tree.TreeState.Ready or Tree.TreeState.Finished
                    ? "Ready"
                    : "Running";
                EditorGUILayout.LabelField($"Tree Status: {treeStatus}");
                
                if (instance.PlayTime != 0f)
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
            
            _debugFoldDownOpen = EditorGUILayout.Foldout(_debugFoldDownOpen, new GUIContent("Debug"));
            if (_debugFoldDownOpen)
            {
                GUI.enabled = Application.isPlaying;
                sceneLinkOptions = new List<string>() {"None"};
                var sceneCount = SceneManager.sceneCount;
                var sceneNames = new string[sceneCount];
                for (var i = 0; i < sceneCount; i++) 
                {
                    sceneLinkOptions.Add(SceneManager.GetSceneAt(i).name);
                }
                selectedSceneIndex = EditorGUILayout.Popup("Linked Scene", selectedSceneIndex, sceneLinkOptions.ToArray());
            
                GUILayout.BeginHorizontal();
                GUI.enabled = instance.State != Tree.TreeState.Running && Application.isPlaying;
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
                GUI.enabled = instance.State == Tree.TreeState.Running && Application.isPlaying;
                if (GUILayout.Button("Stop"))
                {
                    instance.Stop();
                }
                GUILayout.EndHorizontal();
                if (!Application.isPlaying)
                {
                    GUI.enabled = true;
                    EditorGUILayout.HelpBox("You can only debug node trees while in play mode", MessageType.Info);
                }
            }

            Repaint();
        }
    }
#endif
}
