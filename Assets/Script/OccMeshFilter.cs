using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccMeshFilter : MonoBehaviour
{
    public OccupancyFieldGraphObject OccGo;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    void handleMeshUpdated()
    {
        meshFilter.mesh = OccGo.GetMesh();
        meshCollider.sharedMesh = meshFilter.mesh;
        //meshFilter.mesh.RecalculateBounds();
        //meshFilter.mesh.RecalculateNormals();
    }

    private void Awake()
    {
        OccGo.OnOccUpdated += handleMeshUpdated;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
