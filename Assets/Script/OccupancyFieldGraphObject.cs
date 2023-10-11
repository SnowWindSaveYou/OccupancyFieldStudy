using System;
using System.Collections;
using System.Collections.Generic;
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

        MarchingCubeFilter _mcFilter;

        public ComputeBuffer RawArgBuffer { get => _mcFilter.outputArgBuffer; }
        public ComputeBuffer RawVerticesBuffer { get => _mcFilter.outputVertexBuffer; }

        Vector3[] verticesList;
        Vector3[] normalsList;
        int[] indeciesList;
        public Mesh GetMesh() { return _mcFilter.GetMesh(ref verticesList, ref normalsList,ref indeciesList); }
        public void UpdateOccRenderer()
        {
            _mcFilter.gridSize = new Vector3Int(VoxelSize, VoxelSize, VoxelSize);
            _mcFilter.isoLevel = 0.5f;
            _mcFilter.inputDataTex = OccupanceTex;
            _mcFilter.CalcMarchingCube();
            OnOccUpdated?.Invoke();
        }
        void Awake()
        {
            verticesList = new Vector3[3];
            normalsList = new Vector3[3];
            indeciesList = new int[3];
            _mcFilter = new MarchingCubeFilter();
            if (OccupanceTex == null)
            {
                OccupanceTex = new RenderTexture(VoxelSize, VoxelSize, 0, RenderTextureFormat.RFloat);
                OccupanceTex.dimension = TextureDimension.Tex3D;
                OccupanceTex.volumeDepth = VoxelSize;
                OccupanceTex.enableRandomWrite = true;
                OccupanceTex.Create();
            }
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

            //if (GUILayout.Button("Save Tex"))
            //{
            //    //var saverer = new RenderTexture3DToTexture3DFilter();
            //    //saverer.Save(that.OccupanceTex,that.name);
            //    AssetDatabase.CreateAsset(that.OccupanceTex, "Assets/" + that.name + ".tex3d.asset");
            //    Debug.Log("Saved Tex");
            //}
        }

    }
#endif
}