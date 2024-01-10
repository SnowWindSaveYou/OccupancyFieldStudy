using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OccupancyFieldStudy
{
    public class OccProcedualRenderer : MonoBehaviour
    {
        public OccupancyFieldGraphObject OccGo;

        Camera _Camera;
        CommandBuffer _occupancyRenderCmd;
        CameraEvent _occCameraEvent = CameraEvent.BeforeForwardAlpha;
        public Material OccMaterial;

        static readonly int idx_triangles = Shader.PropertyToID("triangles");
        static readonly int idx_localToWorldMat = Shader.PropertyToID("localToWorldMat");
        static readonly int idx_worldToLocalMat = Shader.PropertyToID("worldToLocalMat");

        public void UpdateRender()
        {
            if (!this.isActiveAndEnabled) return;
            _Camera.RemoveCommandBuffer(_occCameraEvent, _occupancyRenderCmd);
            _occupancyRenderCmd.Clear();

            OccMaterial.SetBuffer(idx_triangles, OccGo.RawVerticesBuffer);
            OccMaterial.SetMatrix(idx_localToWorldMat, this.transform.localToWorldMatrix);
            OccMaterial.SetMatrix(idx_worldToLocalMat, this.transform.worldToLocalMatrix);
            _occupancyRenderCmd.DrawProceduralIndirect(
                Matrix4x4.identity, OccMaterial, 0, MeshTopology.Triangles, OccGo.RawArgBuffer);
            _Camera.AddCommandBuffer(_occCameraEvent, _occupancyRenderCmd);
        }
        private void Awake()
        {
            _Camera = Camera.main;
            _occupancyRenderCmd = new CommandBuffer();
            OccMaterial = new Material(Shader.Find("Custom/MeshProcedural"));
            OccGo.OnOccUpdated += UpdateRender;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnEnable()
        {
            if (_occupancyRenderCmd != null)
                _Camera.AddCommandBuffer(_occCameraEvent, _occupancyRenderCmd);
        }

        private void OnDisable()
        {
            if (_occupancyRenderCmd != null && _Camera.isActiveAndEnabled)
                _Camera.RemoveCommandBuffer(_occCameraEvent, _occupancyRenderCmd);
        }

        // Update is called once per frame
        void Update()
        {
            if (this.transform.hasChanged)
            {
                this.transform.hasChanged = false;
                OccMaterial.SetMatrix(idx_localToWorldMat, this.transform.localToWorldMatrix);
                OccMaterial.SetMatrix(idx_worldToLocalMat, this.transform.worldToLocalMatrix);
            }
        }
    }
}