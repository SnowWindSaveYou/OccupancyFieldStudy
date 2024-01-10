using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeHelper : MonoSingleton<VolumeHelper>
{
    ComputeShader _volumeHelperCS;
    ComputeBuffer _minMaxBuffer;

    static readonly int idx_InputTex = Shader.PropertyToID("_InputTex");
    static readonly int idx_OutputTex = Shader.PropertyToID("_OutputTex");
    static readonly int idx_OutputIntsBuffer = Shader.PropertyToID("_OutputIntsBuffer");
    static readonly int idx_SrcBeginPos = Shader.PropertyToID("_SrcBeginPos");
    static readonly int idx_DstBeginPos = Shader.PropertyToID("_DstBeginPos");
    static readonly int idx_VolumeDim = Shader.PropertyToID("_VolumeDim");
    int kernel_FindNonZero;
    int kernel_CopyTo;
    int kernel_Clear;

    public override void Init()
    {
        base.Init();
        _minMaxBuffer = new ComputeBuffer(6, sizeof(int));
        _volumeHelperCS = Resources.Load<ComputeShader>("Shaders/VolumeHelperComputeShader");
        kernel_FindNonZero = _volumeHelperCS.FindKernel("FindNonZero");
        kernel_CopyTo = _volumeHelperCS.FindKernel("CopyTo");
        kernel_Clear = _volumeHelperCS.FindKernel("Clear");
    }
    private void OnDestroy()
    {
        _minMaxBuffer.Release();
    }
    public void CropVolume(RenderTexture sourceTex,SubVolumeEntry subVolume)
    {
        // get nonzero bbox
        int[] minmax = new int[6] { 2048, 2048, 2048, 0, 0, 0 };
        _minMaxBuffer.SetData(minmax);
        _volumeHelperCS.SetTexture(kernel_FindNonZero, idx_InputTex, sourceTex);
        _volumeHelperCS.SetBuffer(kernel_FindNonZero, idx_OutputIntsBuffer, _minMaxBuffer);
        Dispatch888(kernel_FindNonZero, sourceTex.width, sourceTex.height, sourceTex.volumeDepth);
        _minMaxBuffer.GetData(minmax);

        // copy to subVolume
        int[] rangeDim = new int[3] { minmax[3] - minmax[0], minmax[4] - minmax[1], minmax[5] - minmax[2] };
        if (rangeDim[0]<=0|| rangeDim[1] <= 0|| rangeDim[2] <= 0)
        {
            Debug.Log("the volume is empty");
            return;
        }
            
        Debug.Log(rangeDim);
        int[] beginPos = new int[3] { minmax[0], minmax[1], minmax[2] };

        if ( subVolume.volume ==null
            || subVolume.dimention[0] <= rangeDim[0]
            || subVolume.dimention[1] <= rangeDim[1]
            || subVolume.dimention[2] <= rangeDim[2])
        {
            RenderTexture volume =(RenderTexture) subVolume.volume;
            if (volume != null)
                volume.Release();
            volume = new RenderTexture(rangeDim[0], rangeDim[1], 0,RenderTextureFormat.RHalf);
            volume.volumeDepth = rangeDim[2];
            volume.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            volume.enableRandomWrite = true;
            volume.Create();
            subVolume.volume = volume;
            subVolume.dimention = rangeDim;
        }
        subVolume.position = beginPos;


        int[] dstPos = new int[3] { 0,0,0 };
        _volumeHelperCS.SetInts(idx_VolumeDim,rangeDim);
        _volumeHelperCS.SetInts(idx_SrcBeginPos, beginPos);
        _volumeHelperCS.SetInts(idx_DstBeginPos, dstPos);
        _volumeHelperCS.SetTexture(kernel_CopyTo, idx_InputTex, sourceTex);
        _volumeHelperCS.SetTexture(kernel_CopyTo, idx_OutputTex, subVolume.volume);
        Dispatch888(kernel_CopyTo, rangeDim[0], rangeDim[1], rangeDim[2]);
    }

    public void PasteVolume(RenderTexture dstTex, SubVolumeEntry subVolume )
    {
        if(subVolume.volume==null
            || subVolume.dimention[0] <=0
            || subVolume.dimention[1] <=0
            || subVolume.dimention[2] <= 0)
        {
            Debug.Log("Paste Volume is Empty");
            return;
        }
            
        int[] beginPos = new int[3] { 0, 0, 0 };

        _volumeHelperCS.SetInts(idx_VolumeDim, subVolume.dimention);
        _volumeHelperCS.SetInts(idx_SrcBeginPos, beginPos);
        _volumeHelperCS.SetInts(idx_DstBeginPos, subVolume.position);
        _volumeHelperCS.SetTexture(kernel_CopyTo, idx_InputTex, subVolume.volume);
        _volumeHelperCS.SetTexture(kernel_CopyTo, idx_OutputTex, dstTex);
        Dispatch888(kernel_CopyTo, subVolume.dimention[0], subVolume.dimention[1], subVolume.dimention[2]);
    }
    public void ClearVolume(RenderTexture dstTex)
    {
        if (dstTex == null) return;
        _volumeHelperCS.SetTexture(kernel_Clear, idx_OutputTex, dstTex);
        Dispatch888(kernel_Clear,dstTex.width,dstTex.height,dstTex.volumeDepth);
    }


    void Dispatch888(int kernel,int x, int y, int z)
    {
        _volumeHelperCS.Dispatch(kernel, Mathf.CeilToInt(x / 8.0f), Mathf.CeilToInt(y / 8.0f), Mathf.CeilToInt(z / 8.0f));
    }
}
