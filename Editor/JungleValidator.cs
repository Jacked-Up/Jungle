using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Jungle.Editor
{
    [InitializeOnLoad]
    public static class JungleValidator
    {
        #region Variables

        private const string SEARCH_FILTER 
            = "t:JungleTree";

        #endregion
        
        static JungleValidator()
        {
            CompilationPipeline.compilationFinished += _ =>
            {
                var jungleTrees = GetAllJungleTrees();
                var reports = new List<ValidationReport>();
                foreach (var jungleTree in jungleTrees)
                {
                    var report = Validate(jungleTree);
                    // If the validation did not fail then we dont care to see the report
                    if (!report.Failed)
                    {
                        continue;
                    }
                    reports.Add(report);
                }
            };
        }
        
        public static ValidationReport Validate(JungleTree tree)
        {
            var treeAssetPath = AssetDatabase.GetAssetPath(tree);
            var treeSubAssets = AssetDatabase.LoadAllAssetsAtPath(treeAssetPath);
            var report = new ValidationReport(tree, null, null);
            
            foreach (var subAsset in treeSubAssets)
            {
                // This USUALLY means that the developer deleted the node script
                if (subAsset == null)
                {
                    //report.NodeErrors = true;
                    continue;
                }
                if (subAsset.GetType() != typeof(JungleNode))
                {
                    
                    //report.TreeErrors = true;
                    continue;
                }
                var node = subAsset as JungleNode;
                foreach (var port in node.OutputPorts)
                {
                    foreach (var connection in port.connections)
                    {
                        if (connection.InputInfo.PortType != port.PortType)
                        {
                            //report.NodeErrors = true;
                        }
                    }
                }
            }
            
            return report;
        }

        [MenuItem("Window/Jungle/Validate All Jungle Trees")]
        public static void ValidateAll()
        {
            
        }

        public static bool AllJungleTreesValid()
        {
            var jungleTrees = GetAllJungleTrees();
            return jungleTrees.All(jungleTree => !Validate(jungleTree).Failed);
        }
        
        public static List<JungleTree> GetAllJungleTrees()
        {
            var jungleTreeAssets = new List<JungleTree>();
            
            // This searches through all jungle tree assets inside the project
            // The search filter should be type "t:JungleTree"
            AssetDatabase.FindAssets(SEARCH_FILTER).ToList().ForEach(guid =>
            {
                var guidToAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<JungleTree>(guidToAssetPath);
                if (asset != null)
                {
                    jungleTreeAssets.Add(asset);
                }
            });

            return jungleTreeAssets;
        }
    }
    
    [Serializable]
    public struct ValidationReport
    {
        public bool Failed => TreeErrors != null || NodeErrors != null;
            
        public readonly JungleTree Tree; 
        public readonly List<TreeError> TreeErrors;
        public readonly List<NodeError> NodeErrors;

        public ValidationReport(JungleTree tree, List<TreeError> treeErrors,
            List<NodeError> nodeErrors)
        {
            Tree = tree;
            TreeErrors = treeErrors;
            NodeErrors = nodeErrors;
        }
            
        public struct TreeError
        {
            public ErrorType Type;
                
            public enum ErrorType
            {
                MissingOrNullNode
            }
        }
        public struct NodeError
        {
            public JungleNode Node;
            public ErrorType Type;
                
            public enum ErrorType
            {
                ConnectionTypeMismatch
            }
        }
    }
}
