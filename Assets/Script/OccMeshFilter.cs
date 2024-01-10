using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OccupancyFieldStudy
{
    public class OccMeshFilter : MonoBehaviour
    {
        public OccupancyFieldGraphObject OccGo;
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;

        Vector3[] verticesList;
        Vector3[] normalsList;
        int[] indeciesList;

        void handleMeshUpdated()
        {
            meshFilter.mesh = MarchingCubeFilter.Instance.GetMesh(OccGo._mcOutput,Matrix4x4.identity,ref verticesList, ref normalsList, ref indeciesList);
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
}