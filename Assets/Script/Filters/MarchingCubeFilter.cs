using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using OccupancyFieldStudy.Utility;
using UnityEngine.Rendering;

namespace OccupancyFieldStudy
{
    public class MarchingCubeFilter
    {
        static MarchingCubeFilter _instance;
        public static MarchingCubeFilter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MarchingCubeFilter();
                }
                return _instance;
            }
        }

        public class MarchingCubeInputParam
        {
            public Texture inputTex;
            public int[] gridSize;
            public float isoLevel = 0;
            public int[] resolutionScale = new int[3] { 1, 1, 1 };
            public int sampleType = 0;
        }

        public class MarchingCubeOutput
        {
            public ComputeBuffer outputVertexBuffer;
            public ComputeBuffer outputArgBuffer;
        }

        struct Vertex
        {
            public Vector3 vPosition;
            public Vector3 vNormal;
        };
        private ComputeShader MarchingCubesCS;
        private int kernelMC;
        private int kernelTriTOVtx;

        static readonly int idx_layerStart = Shader.PropertyToID("_layerStart");
        static readonly int idx_layerEnd = Shader.PropertyToID("_layerEnd");
        static readonly int idx_triangleRW = Shader.PropertyToID("triangleRW");
        static readonly int idx_densityTexture = Shader.PropertyToID("_densityTexture");
        static readonly int idx_gridSize = Shader.PropertyToID("_gridSize");
        static readonly int idx_isoLevel = Shader.PropertyToID("_isoLevel");
        static readonly int idx_resolutionScale = Shader.PropertyToID("_resolutionScale");
        static readonly int idx_meshTransformMatrix = Shader.PropertyToID("_meshTransformMatrix");
        static readonly int idx_sampleType = Shader.PropertyToID("_sampleType");
        static readonly int idx_maskBuffer = Shader.PropertyToID("_MaskBuffer");
        static readonly int idx_argBuffer = Shader.PropertyToID("argBuffer");


        public const int MAX_VTX = 7000000;

        public MarchingCubeFilter()
        {
            MarchingCubesCS = Resources.Load<ComputeShader>("Shaders/MarchingCubesOptShader");
            kernelMC = MarchingCubesCS.FindKernel("MarchingCubes");
            kernelTriTOVtx = MarchingCubesCS.FindKernel("TriToVtx");
        }
        ~MarchingCubeFilter()
        {

        }

        public void CalcMarchingCube(MarchingCubeInputParam input,MarchingCubeOutput output)
        {
            MarchingCubesCS.SetTexture(kernelMC, idx_densityTexture, input.inputTex);
            MarchingCubesCS.SetInts(idx_gridSize, input.gridSize);
            MarchingCubesCS.SetFloat(idx_isoLevel, input.isoLevel);
            MarchingCubesCS.SetInts(idx_resolutionScale, input.resolutionScale);
            MarchingCubesCS.SetInt(idx_sampleType, input.sampleType);

            if (output.outputVertexBuffer == null|| output.outputArgBuffer == null)
            {
                Debug.LogError("Buffer Not Exitst");
            }

            output.outputVertexBuffer.SetCounterValue(0);
            MarchingCubesCS.SetBuffer(kernelMC, idx_triangleRW, output.outputVertexBuffer);

            // 计算顶点
            MarchingCubesCS.Dispatch(kernelMC, Mathf.CeilToInt(input.gridSize[0] / input.resolutionScale[0] / 8.0f),
                                                Mathf.CeilToInt(input.gridSize[1] / input.resolutionScale[1] / 8.0f),
                                                Mathf.CeilToInt(input.gridSize[2] / input.resolutionScale[2] / 8.0f));

            int[] args = new int[] { 0, 1, 0, 0 };
            output.outputArgBuffer.SetData(args);
            ComputeBuffer.CopyCount(output.outputVertexBuffer, output.outputArgBuffer, 0);
            MarchingCubesCS.SetBuffer(kernelTriTOVtx, idx_argBuffer, output.outputArgBuffer);
            //outputArgBuffer.GetData(args);
            //Debug.Log("vertices Count: " + args[0]);
            MarchingCubesCS.Dispatch(kernelTriTOVtx, 1, 1, 1);// for render
        }


        public Mesh GetMesh(MarchingCubeOutput outputResult, Matrix4x4 MeshTransformMatrix,
            ref Vector3[] verticesList, ref Vector3[] normalsList, ref int[] indeciesList)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;//
            int[] args = new int[] { 0, 1, 0, 0 };
            outputResult.outputArgBuffer.GetData(args);
            int rawVertexsCount = args[0];

            if (rawVertexsCount < 3) return mesh;
            if (rawVertexsCount >= MAX_VTX)
            {
                Debug.LogError("over max allowed vertices " + rawVertexsCount);
                return mesh;
            }
            if (rawVertexsCount >= outputResult.outputVertexBuffer.count)
            {
                Debug.LogError("over max allowed verticesBuffer count "+ rawVertexsCount + " > " + outputResult.outputVertexBuffer.count);
                return mesh;
            }
            Vertex[] vertexsArg = new Vertex[rawVertexsCount];
            outputResult.outputVertexBuffer.GetData(vertexsArg, 0, 0, rawVertexsCount);//TODO

            Dictionary<Vector3, int> vertexsDict = new Dictionary<Vector3, int>();
            // 收集所有顶点
            int vid = 0;
            for (int i = 0; i < rawVertexsCount; ++i)
            {
                var currPos = MeshTransformMatrix.MultiplyPoint(vertexsArg[i].vPosition);
                Vector3 position = currPos.Round(5);
                if (!vertexsDict.ContainsKey(position))
                {
                    vertexsDict.Add(position, vid++);
                }
            }


            // 得到顶点数量后再构建三角形减少内存重新分配消耗
            int verticesCount = vertexsDict.Count;
            if (verticesList == null || verticesCount > verticesList.Count())
            {
                verticesList = new Vector3[verticesCount];
                normalsList = new Vector3[verticesCount];

            }
            else
            {
                Array.Fill(normalsList, Vector3.zero, 0, verticesList.Count());
            }
            if (indeciesList==null|| rawVertexsCount > indeciesList.Count())
            {
                indeciesList = new int[rawVertexsCount];
            }
            int trienglesCount = rawVertexsCount / 3;
            int j = 0;
            for (int i = 0; i < trienglesCount; i++)
            {
                int ii = i * 3;
                var a = MeshTransformMatrix.MultiplyPoint(vertexsArg[ii].vPosition).Round(5);
                var b = MeshTransformMatrix.MultiplyPoint(vertexsArg[ii + 1].vPosition).Round(5);
                var c = MeshTransformMatrix.MultiplyPoint(vertexsArg[ii + 2].vPosition).Round(5);

                var an = MeshTransformMatrix.MultiplyVector(vertexsArg[ii].vNormal);
                var bn = MeshTransformMatrix.MultiplyVector(vertexsArg[ii + 1].vNormal);
                var cn = MeshTransformMatrix.MultiplyVector(vertexsArg[ii + 2].vNormal);

                int a_idx = vertexsDict[a];
                int b_idx = vertexsDict[b];
                int c_idx = vertexsDict[c];

                if (a_idx == b_idx || b_idx == c_idx || c_idx == a_idx)
                {
                    continue;
                }
                int jj = j * 3;// jump over 
                indeciesList[jj] = a_idx;
                indeciesList[jj + 1] = b_idx;
                indeciesList[jj + 2] = c_idx;

                verticesList[a_idx] = a;
                verticesList[b_idx] = b;
                verticesList[c_idx] = c;

                normalsList[a_idx] = (normalsList[a_idx] + an) / 2.0f;
                normalsList[b_idx] = (normalsList[b_idx] + bn) / 2.0f;
                normalsList[c_idx] = (normalsList[c_idx] + cn) / 2.0f;
                j++;
            }

            mesh.SetVertices(verticesList, 0, vertexsDict.Count);
            mesh.SetNormals(normalsList, 0, vertexsDict.Count);
            mesh.SetTriangles(indeciesList, 0, j * 3, 0);// idx是短整型
            return mesh;
        }
    }
}