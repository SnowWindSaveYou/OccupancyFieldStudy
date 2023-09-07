using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class OccupancyFieldGraphObject : MonoBehaviour
{
    public int VoxelSize = 64;
    public RenderTexture OccupanceTex;
    public Action OnOccUpdated;

    MarchingCubeFilter _mcFilter;

    public ComputeBuffer RawArgBuffer { get => _mcFilter.outputArgBuffer; }
    public ComputeBuffer RawVerticesBuffer { get => _mcFilter.outputVertexBuffer; }
    public Mesh GetMesh() { return _mcFilter.GetMesh(); }
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
        if (GUILayout.Button("Update"))
        {
            that.UpdateOccRenderer();
            Debug.Log("Update Done ");
        }
    }

}
#endif