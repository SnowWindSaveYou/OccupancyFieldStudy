using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OccupancyFieldStudy
{
    public class SDFToOccTex : MonoBehaviour
    {
        public OccupancyFieldGraphObject OccGo;
        public Texture3D InputTexture;
        ComputeShader _SDFToOccCS;

        public bool autoOnStart = false;
        // Start is called before the first frame update
        void Start()
        {
            _SDFToOccCS = Resources.Load<ComputeShader>("Shaders/SDFToOccComputeShader");
            if (autoOnStart)
            {
                ProcessTransfer();
                OccGo.UpdateOccRenderer();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ProcessTransfer()
        {
            int kernel_main = _SDFToOccCS.FindKernel("CSMain");

            _SDFToOccCS.SetTexture(kernel_main, "ResultTex", OccGo.OccupanceTex);
            _SDFToOccCS.SetTexture(kernel_main, "InputTex", InputTexture);

            _SDFToOccCS.Dispatch(kernel_main, OccGo.VoxelSize / 8, OccGo.VoxelSize / 8, OccGo.VoxelSize / 8);
        }
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(SDFToOccTex))]
    class SDFToOccTexEditor : Editor
    {
        SDFToOccTex that;
        private void OnEnable()
        {
            that = (SDFToOccTex)target;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Process"))
            {
                that.ProcessTransfer();
                that.OccGo.UpdateOccRenderer();
                Debug.Log("Process Done ");
            }
        }

    }
#endif
}