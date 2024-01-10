using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//using 
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OccupancyFieldStudy {


    public class OccFFDModifier : MonoBehaviour
    {

        public GameObject FFDHandlerTemplate;
        public OccEditManager oem;
        public ComputeShader FFDProcessCS;
        public Transform CursorTransform;

        public RenderTexture FFDTex;
        public ComputeBuffer FFDHandlerBuffer;

        public List<FFDHandler> FFDHandlers;

        public bool enableSetupHandler = false;

        RenderTexture GetFFDTex()
        {
            if (FFDTex != null) return FFDTex;
            var occTex = oem.targetOccObj.OccupanceTex;
            FFDTex = new RenderTexture(occTex.width, occTex.height, 0,RenderTextureFormat.RGB111110Float);//format
            FFDTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            FFDTex.volumeDepth = occTex.volumeDepth;
            FFDTex.enableRandomWrite = true;
            return FFDTex;
        }

        void SetupFFDHandler()
        {
            var p = GameObject.Instantiate(FFDHandlerTemplate);
            p.transform.SetParent(oem.targetOccObj.transform,true);
            p.transform.position = CursorTransform.position;



            var h = p.GetComponent<FFDHandler>();
            h.InitPos = h.transform.localPosition;
            h.OnTransformChanged += HandleFFDHandlerUpdated;
            FFDHandlers.Add(h);
        }

        void HandleFFDHandlerUpdated()
        {
            if (FFDHandlers.Count < 2) return;//TODO
            var occ = oem.targetOccObj;
            if (FFDHandlerBuffer==null || FFDHandlerBuffer.count != FFDHandlers.Count)
            {
                if (FFDHandlerBuffer != null) FFDHandlerBuffer.Release();
                 FFDHandlerBuffer = new ComputeBuffer(FFDHandlers.Count, sizeof(float) * 6);
                oem.CopyTex(occ.OccupanceTex, oem.GetTempTex());
                this.enableSetupHandler = false;
            }
            var ffdArr = FFDHandlers.Select(o => o.GetStruct()).ToArray();
            FFDHandlerBuffer.SetData(ffdArr);



            
            var gridSize = Vector3.one * occ.VoxelSize;
            int kernel = FFDProcessCS.FindKernel("CalcFFD");
            FFDProcessCS.SetBuffer(kernel,"_FFDHandlerBuffer", FFDHandlerBuffer);
            FFDProcessCS.SetInt("_FFDHandlerBufferCount", FFDHandlerBuffer.count);
            FFDProcessCS.SetTexture(kernel, "_FFDTex", GetFFDTex());
            FFDProcessCS.SetTexture(kernel, "_DataTex", oem.GetTempTex());
            FFDProcessCS.SetTexture(kernel, "_TargetTex", occ.OccupanceTex);

            FFDProcessCS.SetVector("_gridSize", gridSize);

            FFDProcessCS.Dispatch(kernel, Mathf.CeilToInt(gridSize.x / 8.0f), Mathf.CeilToInt(gridSize.y / 8.0f), Mathf.CeilToInt(gridSize.z / 8.0f));
            Debug.Log("FFD Updated " + FFDHandlerBuffer.count);
            oem.targetOccObj.UpdateOccRenderer();
        }
        void RefreshFFD()
        {
            
        }


        // Start is called before the first frame update
        void Start()
        {
            FFDHandlers = new List<FFDHandler>();

        }

        // Update is called once per frame
        void Update()
        {
            if (enableSetupHandler&& Input.GetKeyDown(KeyCode.Mouse1))
            {
                SetupFFDHandler();
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(OccFFDModifier))]
    class OccFFDModifierEditor : Editor
    {
        OccFFDModifier that;
        private void OnEnable()
        {
            that = (OccFFDModifier)target;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Reset"))
            {
                that.FFDHandlers.ForEach((p) => { p.transform.localPosition = p.InitPos; });

            }

        }

    }
#endif
}