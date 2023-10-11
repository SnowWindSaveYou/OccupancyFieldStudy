using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OccupancyFieldStudy
{
    public class RenderTexture3DToTexture3DFilter
    {
        ComputeShader sliceCS;
        int width, height, depth;
        TextureFormat format = TextureFormat.RFloat;
        RenderTextureFormat formatRT = RenderTextureFormat.RFloat;
        public RenderTexture3DToTexture3DFilter()
        {
            sliceCS = Resources.Load<ComputeShader>("Shaders/SliceRenderTex");
        }

        RenderTexture Copy3DSliceToRenderTexture(RenderTexture source, int layer)
        {

            //TODO RenderTexture.GetTemporary()
            RenderTextureDescriptor desc = new RenderTextureDescriptor(width, height, 0);
            desc.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            desc.colorFormat = formatRT;
            desc.enableRandomWrite = true;
            desc.autoGenerateMips = false;
            RenderTexture render = RenderTexture.GetTemporary(desc);
            //RenderTexture render = new RenderTexture(width, height, 0, formatRT);
            //render.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            //render.enableRandomWrite = true;
            //render.wrapMode = TextureWrapMode.Clamp;
            //render.Create();

            int kernelIndex = sliceCS.FindKernel("CSMain");
            sliceCS.SetTexture(kernelIndex, "voxels", source);
            sliceCS.SetInt("layer", layer);
            sliceCS.SetTexture(kernelIndex, "Result", render);
            sliceCS.Dispatch(kernelIndex, width, height, 1);

            return render;
        }

        Texture2D ConvertFromRenderTexture(RenderTexture rt)
        {
            Texture2D output = new Texture2D(width, height);
            RenderTexture.active = rt;
            output.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            output.Apply();
            return output;
        }

        public Texture3D Save(RenderTexture selectedRenderTexture,
            string name = "unname",
            TextureFormat textureFormat = TextureFormat.RFloat)
        {
            width = selectedRenderTexture.width;
            height = selectedRenderTexture.height;
            depth = selectedRenderTexture.volumeDepth;
            format = textureFormat;

            //Texture3D export = new Texture3D(width, height, depth, format, false);

            RenderTexture[] layers = new RenderTexture[depth];
            for (int i = 0; i < depth; i++)
                layers[i] = Copy3DSliceToRenderTexture(selectedRenderTexture, i);

            Texture2D[] finalSlices = new Texture2D[depth];
            for (int i = 0; i < depth; i++)
                finalSlices[i] = ConvertFromRenderTexture(layers[i]);

            Texture3D output = new Texture3D(width, height, depth, format, true);
            output.filterMode = FilterMode.Trilinear;
            Color[] outputPixels = output.GetPixels();

            for (int k = 0; k < depth; k++)
            {
                Color[] layerPixels = finalSlices[k].GetPixels();
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        outputPixels[i + j * width + k * width * height] = layerPixels[i + j * width];
                    }
            }

            output.SetPixels(outputPixels);
            output.Apply();
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(output, "Assets/" + name + ".tex3d.asset");
#endif
            return output;
        }
    }
}