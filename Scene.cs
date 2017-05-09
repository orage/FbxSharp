﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FbxSharp
{
    public class Scene : FbxDocument
    {
        public Scene(string name="")
            : base(name)
        {
            RootNode = new FbxNode();

            Poses = SrcObjects.CreateCollectionView<Pose>();
            Materials = SrcObjects.CreateCollectionView<SurfaceMaterial>();
            Textures = SrcObjects.CreateCollectionView<Texture>();
            Nodes = SrcObjects.CreateCollectionView<FbxNode>();

            SrcObjects.Add(new FbxGlobalSettings());
            SetAnimationEvaluator(new FbxAnimEvaluator());
        }

        public override void ConnectSrcObject(FbxObject fbxObject/*, Connection.EType type = Connection.EType.None*/)
        {
            base.ConnectSrcObject(fbxObject/*, type*/);

            if (fbxObject is FbxAnimStack && this.GetCurrentAnimationStack() == null)
            {
                this.SetCurrentAnimationStack((FbxAnimStack)fbxObject);
            }

            foreach (var srcobj in fbxObject.SrcObjects)
            {
                if (!SrcObjects.Contains(srcobj))
                    this.ConnectSrcObject(srcobj);
            }

            foreach (Property prop in fbxObject.Properties)
            {
                foreach (var srcobj in prop.SrcObjects)
                {
                    if (!SrcObjects.Contains(srcobj))
                        this.ConnectSrcObject(srcobj);
                }
            }
        }


        #region Pose Management

        public readonly CollectionView<Pose> Poses;

        public int GetPoseCount()
        {
            return Poses.Count;
        }

        public Pose GetPose(int pIndex)
        {
            return Poses[pIndex];
        }

        public bool AddPose(Pose pPose)
        {
            if (this.Poses.Contains(pPose)) return false;

            this.ConnectSrcObject(pPose);

            return true;
        }

        public bool RemovePose(Pose pPose)
        {
            return this.DisconnectSrcObject(pPose);
        }

        public bool RemovePose(int pIndex)
        {
            return RemovePose(Poses[pIndex]);
        }

        #endregion

        #region Global Settings

        public FbxGlobalSettings GetGlobalSettings()
        {
            return (FbxGlobalSettings)SrcObjects.FirstOrDefault(x => x is FbxGlobalSettings);
        }

        #endregion

        #region Scene Animation Evaluation

        public FbxAnimStack CurrentAnimationStack;

        public void SetCurrentAnimationStack(FbxAnimStack pAnimStack)
        {
            CurrentAnimationStack = pAnimStack;
        }

        public FbxAnimStack GetCurrentAnimationStack()
        {
            return CurrentAnimationStack;
        }

        public void SetAnimationEvaluator(FbxAnimEvaluator pEvaluator)
        {
            var current = GetAnimationEvaluator();
            if (current != null)
            {
                SrcObjects.Remove(current);
            }
            SrcObjects.Add(pEvaluator);
        }

        public FbxAnimEvaluator GetAnimationEvaluator()
        {
            return (FbxAnimEvaluator)SrcObjects.FirstOrDefault(x => x is FbxAnimEvaluator);
        }

        #endregion

        #region Material Access

        public readonly CollectionView<SurfaceMaterial> Materials;

        public int GetMaterialCount()
        {
            return Materials.Count;
        }

        public SurfaceMaterial GetMaterial(int pIndex)
        {
            return Materials[pIndex];
        }

        public SurfaceMaterial GetMaterial(string pName)
        {
            return Materials.FirstOrDefault(m => m.Name == pName);
        }

        public bool AddMaterial(SurfaceMaterial pMaterial)
        {
            ConnectSrcObject(pMaterial);
            return true;
        }

        public bool RemoveMaterial(SurfaceMaterial pMaterial)
        {
            return DisconnectSrcObject(pMaterial);
        }

        #endregion

        #region Texture Access

        public readonly CollectionView<Texture> Textures;

        public int GetTextureCount()
        {
            return Textures.Count;
        }

        public Texture GetTexture(int pIndex)
        {
            return Textures[pIndex];
        }

        public Texture GetTexture(string pName)
        {
            return Textures.FirstOrDefault(t => t.Name == pName);
        }

        public bool AddTexture(Texture pTexture)
        {
            ConnectSrcObject(pTexture);
            return true;
        }

        public bool RemoveTexture(Texture pTexture)
        {
            return DisconnectSrcObject(pTexture);
        }

        #endregion

        #region Node Tree Access

        public CollectionView<FbxNode> Nodes;

        FbxNode _rootNode;
        public FbxNode RootNode {
            get { return _rootNode; }
            protected set
            {
                if (value != _rootNode)
                {
                    if (_rootNode != null)
                    {
                        this.DisconnectSrcObject(_rootNode);
                    }

                    _rootNode = value;

                    if (_rootNode != null)
                    {
                        this.ConnectSrcObject(_rootNode);
                    }
                }
            }
        }

        public FbxNode GetRootNode()
        {
            return RootNode;
        }

        #endregion

        #region Node Access

        public int GetNodeCount()
        {
            return Nodes.Count;
        }

        public FbxNode GetNode(int pIndex)
        {
            return Nodes[pIndex];
        }

        public bool AddNode(FbxNode pNode)
        {
            if (!Nodes.Contains(pNode))
            {
                ConnectSrcObject(pNode);
                foreach (var child in pNode.ChildNodes)
                {
                    AddNode(child);
                }
                return true;
            }

            return false;
        }

        public bool RemoveNode(FbxNode pNode)
        {
            if (Nodes.Contains(pNode))
            {
                DisconnectSrcObject(pNode);
                foreach (var child in pNode.ChildNodes)
                {
                    RemoveNode(child);
                }
                return true;
            }

            return false;
        }

        public int GetCurveOnSurfaceCount()
        {
            throw new NotImplementedException();
        }

        public FbxNode FindNodeByName(string pName)
        {
            return Nodes.FirstOrDefault(n => n.Name == pName);
        }

        #endregion

        public override string GetNameSpacePrefix()
        {
            return "Scene::";
        }
    }
}

