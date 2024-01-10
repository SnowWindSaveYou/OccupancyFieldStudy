using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace OccupancyFieldStudy
{
    public class OccupancyFieldGraphObject : MonoBehaviour
    {
        public int VoxelSize = 64;
        public RenderTexture OccupanceTex;
        public Action OnOccUpdated;



        private ComputeBuffer _rawArgBuffer;
        private ComputeBuffer _rawVerticesBuffer;
        public ComputeBuffer RawArgBuffer { get {
                if (_rawArgBuffer == null)
                {
                    _rawArgBuffer = BufferManager.Instance.GetBuffer("McArgBuffer", 4, sizeof(int), ComputeBufferType.IndirectArguments);
                }
                return _rawArgBuffer; 
            } }
        public ComputeBuffer RawVerticesBuffer { get {
                if (_rawVerticesBuffer == null)
                {
                    int count = Math.Min((VoxelSize* VoxelSize* VoxelSize) / 5, MarchingCubeFilter.MAX_VTX);//TODO
                    _rawVerticesBuffer = BufferManager.Instance.GetBuffer("McVerticesBuffer", count, sizeof(float) * 18, ComputeBufferType.Append);
                }
                return _rawVerticesBuffer; 
            } }

        MarchingCubeFilter.MarchingCubeInputParam _mcInput;
        public MarchingCubeFilter.MarchingCubeOutput _mcOutput;

        public void UpdateOccRenderer()
        {
            if (_mcInput == null)
            {
                _mcInput = new MarchingCubeFilter.MarchingCubeInputParam()
                {
                    gridSize = new int[3] { VoxelSize, VoxelSize, VoxelSize },
                    isoLevel = 0.5f,
                    inputTex = OccupanceTex
                };
            }
            if (_mcOutput == null)
            {
                _mcOutput = new MarchingCubeFilter.MarchingCubeOutput()
                {
                    outputVertexBuffer = RawVerticesBuffer,
                    outputArgBuffer = RawArgBuffer
                };
            }
            MarchingCubeFilter.Instance.CalcMarchingCube(_mcInput,_mcOutput);
            OnOccUpdated?.Invoke();
        }

        void Awake()
        {

            if (OccupanceTex == null)
            {
                OccupanceTex = new RenderTexture(VoxelSize, VoxelSize, 0, RenderTextureFormat.RFloat);
                OccupanceTex.dimension = TextureDimension.Tex3D;
                OccupanceTex.volumeDepth = VoxelSize;
                OccupanceTex.enableRandomWrite = true;
                OccupanceTex.Create();
            }
        }
        private void OnDestroy()
        {
            OccupanceTex.Release();
        }
        private void Start()
        {
            this.UpdateOccRenderer();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(OccupancyFieldGraphObject))]
    class OccupancyFieldGraphObjectEditor : Editor
    {
        OccupancyFieldGraphObject that;
        private void OnEnable()
        {
            that = (OccupancyFieldGraphObject)target;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Update Renderer"))
            {
                that.UpdateOccRenderer();
                Debug.Log("Update Done ");
            }
        }

    }
#endif
}