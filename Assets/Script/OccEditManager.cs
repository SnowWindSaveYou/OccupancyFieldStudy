using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class OccEditManager : MonoBehaviour
{

    public enum EditToolType
    {
        Grab,
        Growth,
        Shink,
        Draw,
        Toward
    }

    public OccupancyFieldGraphObject targetOccObj;

    public Vector3 CursorRadius = Vector3.zero;
    public Vector3 CursorPos = Vector3.zero;
    public float Intensity = 0.25f;
    Vector3 CursorPrePos = Vector3.zero;

    bool didInitEdit = false;
    public bool refineAfterEdit = false;

    public EditToolType currentEditToolType = EditToolType.Grab;


    public void SetCursorSize(float r)
    {
        CursorRadius = 1.0f / targetOccObj.transform.lossyScale.x * targetOccObj.VoxelSize * r * Vector3.one;
        Shader.SetGlobalVector(idx_CursorRadius, CursorRadius);
    }
    public void SetCursorPos(Vector3 worldPos)
    {
        CursorPrePos = CursorPos;
        CursorPos = (targetOccObj.transform.worldToLocalMatrix.MultiplyPoint(worldPos) + 0.5f * Vector3.one) * targetOccObj.VoxelSize;
        Shader.SetGlobalVector(idx_CursorPos, CursorPos);

    }

    RenderTexture _TempOccTex;
    ComputeShader _EditOccCS;
    int kernel_dragFlow;
    int kernel_expandFlow;
    int kernel_shinkFlow;
    int kernel_drawFlow;
    int kernel_towardFlow;
    int kernel_copy;
    int kernel_refine;


    int idx_VoxelOffset;
    int idx_VoxelDim;
    int idx_CursorPos;
    int idx_CursorPrePos;
    int idx_CursorRadius;
    int idx_Intensity;

    int idx_OccTex;
    int idx_InputOccTex;


    public void handleEditBegin()
    {
        InitEdit();
    }
    public void handleEditEnd()
    {

    }
    public void handleEditProcessing()
    {
        if (CursorPrePos == CursorPos) return;
        var tempTex = GetTempTex();
        var OccTex = targetOccObj.OccupanceTex;

        //targetOccObj
        _EditOccCS.SetTexture(kernel_copy, idx_InputOccTex, OccTex);
        _EditOccCS.SetTexture(kernel_copy, idx_OccTex, tempTex);
        DispatchTexSize(_EditOccCS, kernel_copy);
        SetCursorBuffer(_EditOccCS);

        if (currentEditToolType == EditToolType.Draw)
        {
            _EditOccCS.SetTexture(kernel_drawFlow, idx_InputOccTex, tempTex);
            _EditOccCS.SetTexture(kernel_drawFlow, idx_OccTex, OccTex);
            DispatchTexSize(_EditOccCS, kernel_drawFlow);
        }

        else if (currentEditToolType == EditToolType.Grab)
        {
            _EditOccCS.SetTexture(kernel_dragFlow, idx_InputOccTex, tempTex);
            _EditOccCS.SetTexture(kernel_dragFlow, idx_OccTex, OccTex);
            DispatchTexSize(_EditOccCS, kernel_dragFlow);
        }

        else if (currentEditToolType == EditToolType.Growth)
        {
            _EditOccCS.SetTexture(kernel_expandFlow, idx_InputOccTex, tempTex);
            _EditOccCS.SetTexture(kernel_expandFlow, idx_OccTex, OccTex);
            DispatchTexSize(_EditOccCS, kernel_expandFlow);
        }
        else if (currentEditToolType == EditToolType.Shink)
        {
            _EditOccCS.SetTexture(kernel_shinkFlow, idx_InputOccTex, tempTex);
            _EditOccCS.SetTexture(kernel_shinkFlow, idx_OccTex, OccTex);
            DispatchTexSize(_EditOccCS, kernel_shinkFlow);
        }
        else if (currentEditToolType == EditToolType.Toward)
        {
            _EditOccCS.SetTexture(kernel_towardFlow, idx_InputOccTex, tempTex);
            _EditOccCS.SetTexture(kernel_towardFlow, idx_OccTex, OccTex);
            DispatchTexSize(_EditOccCS, kernel_towardFlow);
        }
        if (refineAfterEdit)
            RefineOccTex();
        targetOccObj.UpdateOccRenderer();
    }

    public void RefineOccTex()
    {
        _EditOccCS.SetTexture(kernel_refine, idx_OccTex, targetOccObj.OccupanceTex);
        DispatchTexSize(_EditOccCS, kernel_refine);
    }


    void SetCursorBuffer(ComputeShader computeShader)
    {
        computeShader.SetFloat(idx_Intensity, Intensity);
        computeShader.SetVector(idx_CursorPos, CursorPos);
        computeShader.SetVector(idx_CursorRadius, CursorRadius);

        computeShader.SetVector(idx_CursorPrePos, CursorPrePos);
    }

    void DispatchTexSize(ComputeShader computeShader, int kernel)
    {
        var OccTex = targetOccObj.OccupanceTex;
        computeShader.Dispatch(kernel, Mathf.CeilToInt(OccTex.width / 8.0f), Mathf.CeilToInt(OccTex.height / 8.0f), Mathf.CeilToInt(OccTex.volumeDepth / 8.0f));
    }

    void DispatchCursorSize(ComputeShader computeShader, int kernel)
    {//TODO
        computeShader.Dispatch(kernel, Mathf.CeilToInt(targetOccObj.VoxelSize / 8.0f), Mathf.CeilToInt(targetOccObj.VoxelSize / 8.0f), Mathf.CeilToInt(targetOccObj.VoxelSize / 8.0f));
    }

    RenderTexture GetTempTex()
    {
        if (_TempOccTex == null)
        {
            _TempOccTex = new RenderTexture(targetOccObj.OccupanceTex.width, targetOccObj.OccupanceTex.height, 0, RenderTextureFormat.RFloat);
            _TempOccTex.dimension = TextureDimension.Tex3D;
            _TempOccTex.volumeDepth = targetOccObj.OccupanceTex.volumeDepth;
            _TempOccTex.enableRandomWrite = true;
            _TempOccTex.Create();
        }
        return _TempOccTex;
    }

    void InitShaders()
    {
        _EditOccCS = Resources.Load<ComputeShader>("Shaders/OccEditComputeShader");
        kernel_dragFlow = _EditOccCS.FindKernel("DragFlow");
        kernel_expandFlow = _EditOccCS.FindKernel("ExpandFlow");
        kernel_shinkFlow = _EditOccCS.FindKernel("ShinkFlow");
        kernel_copy = _EditOccCS.FindKernel("CopyTex");
        kernel_refine = _EditOccCS.FindKernel("RefineOccTex");
        kernel_drawFlow = _EditOccCS.FindKernel("DrawFlow");
        kernel_towardFlow = _EditOccCS.FindKernel("TowardFlow");

        idx_VoxelOffset = Shader.PropertyToID("_VoxelOffset");
        idx_VoxelDim = Shader.PropertyToID("_VoxelDim");
        idx_CursorPos = Shader.PropertyToID("_CursorPos");
        idx_CursorPrePos = Shader.PropertyToID("_CursorPrePos");
        idx_CursorRadius = Shader.PropertyToID("_CursorRadius");
        idx_Intensity = Shader.PropertyToID("_Intensity");

        idx_OccTex = Shader.PropertyToID("_OccTex");
        idx_InputOccTex = Shader.PropertyToID("_InputOccTex");

    }

    void InitEdit()
    {
        if (didInitEdit) return;
        didInitEdit = true;
        var OccTex = targetOccObj.OccupanceTex;
        Shader.SetGlobalVector(idx_VoxelDim, (Vector3)new Vector3Int(OccTex.width, OccTex.height, OccTex.volumeDepth));
        _EditOccCS.SetInts(idx_VoxelDim, new int[3] { OccTex.width, OccTex.height, OccTex.volumeDepth });


    }

    void Awake()
    {
        InitShaders();
    }
    private void Start()
    {

    }


    void Update()
    {

    }


}


#if UNITY_EDITOR
[CustomEditor(typeof(OccEditManager))]
class OccEditManagerEditor : Editor
{
    OccEditManager that;
    private void OnEnable()
    {
        that = (OccEditManager)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //if (GUILayout.Button("ToggleExpand"))
        //{
        //    that.ToggleExpand();
        //    Debug.Log("Update Done ");
        //}
        if (GUILayout.Button("RefineOccTex"))
        {
            that.RefineOccTex();
            that.targetOccObj.UpdateOccRenderer();
        }
    }

}
#endif