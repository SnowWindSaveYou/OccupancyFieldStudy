using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MarchingCubeFilter
{
    struct Vertex
    {
        public Vector3 vPosition;
        public Vector3 vNormal;
    };


    public Texture inputDataTex;
    public Vector3Int gridSize;
    public float isoLevel = 0;
    public int[] resolutionScale = { 1, 1, 1 };


    public ComputeBuffer outputVertexBuffer;
    public ComputeBuffer outputArgBuffer;
    public Matrix4x4 MeshTransformMatrix = Matrix4x4.identity;
    public int sampleType = 0;//0 defualt, 1 truncated plane sdf


    private ComputeShader MarchingCubesCS;
    private int kernelMC;

    private ComputeBuffer countBuffer;
    private int countBufferLength = 0;
    private int triengleCount = 0;
    private int kernelTriTOVtx;

    int idx_layerStart;
    int idx_layerEnd;
    int idx_triangleRW;
    int idx_densityTexture;
    int idx_gridSize;
    int idx_isoLevel;
    int idx_resolutionScale;
    int idx_meshTransformMatrix;
    int idx_sampleType;
    int idx_maskBuffer;

    const int vertexcountbuffersize = 512;// 
    int MAX_VTX = 7000000;

    public MarchingCubeFilter()
    {
        MarchingCubesCS = Resources.Load<ComputeShader>("Shaders/MarchingCubesOpt");
        kernelMC = MarchingCubesCS.FindKernel("MarchingCubes");

        outputArgBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        countBuffer = new ComputeBuffer(vertexcountbuffersize, 4);
        kernelTriTOVtx = MarchingCubesCS.FindKernel("TriToVtx");
        MarchingCubesCS.SetBuffer(kernelTriTOVtx, "argBuffer", outputArgBuffer);

        idx_layerStart = Shader.PropertyToID("_layerStart");
        idx_layerEnd = Shader.PropertyToID("_layerEnd");
        idx_triangleRW = Shader.PropertyToID("triangleRW");
        idx_densityTexture = Shader.PropertyToID("_densityTexture");
        idx_gridSize = Shader.PropertyToID("_gridSize");
        idx_isoLevel = Shader.PropertyToID("_isoLevel");
        idx_resolutionScale = Shader.PropertyToID("_resolutionScale");
        idx_meshTransformMatrix = Shader.PropertyToID("_meshTransformMatrix");
        idx_sampleType = Shader.PropertyToID("_sampleType");
        idx_maskBuffer = Shader.PropertyToID("_MaskBuffer");

    }
    ~MarchingCubeFilter()
    {
        if (outputVertexBuffer != null) outputVertexBuffer.Release();
        countBuffer.Release();
        outputArgBuffer.Release();
    }

    public void CalcMarchingCube()
    {
        MarchingCubesCS.SetTexture(kernelMC, idx_densityTexture, inputDataTex);
        MarchingCubesCS.SetInts(idx_gridSize, new int[3] { gridSize.x, gridSize.y, gridSize.z });
        MarchingCubesCS.SetFloat(idx_isoLevel, isoLevel);
        MarchingCubesCS.SetInts(idx_resolutionScale, resolutionScale);
        //MarchingCubesCS.SetMatrix(idx_meshTransformMatrix, MeshTransformMatrix);
        MarchingCubesCS.SetInt(idx_sampleType, sampleType);

        if (outputVertexBuffer == null)
        {
            int count = Math.Min((gridSize.x * gridSize.y * gridSize.z) / 5, MAX_VTX);//TODO
            outputVertexBuffer = new ComputeBuffer(count, sizeof(float) * 18, ComputeBufferType.Append);
            countBufferLength = count;
        }

        outputVertexBuffer.SetCounterValue(0);
        MarchingCubesCS.SetBuffer(kernelMC, idx_triangleRW, outputVertexBuffer);

        // 计算顶点
        MarchingCubesCS.Dispatch(kernelMC, Mathf.CeilToInt(gridSize.x / resolutionScale[0] / 8.0f),
                                            Mathf.CeilToInt(gridSize.y / resolutionScale[1] / 8.0f),
                                            Mathf.CeilToInt((gridSize.z / resolutionScale[2]) / 8.0f));

        int[] args = new int[] { 0, 1, 0, 0 };
        outputArgBuffer.SetData(args);
        ComputeBuffer.CopyCount(outputVertexBuffer, outputArgBuffer, 0);

        //outputArgBuffer.GetData(args);
        //Debug.Log("vertices Count: " + args[0]);
        MarchingCubesCS.Dispatch(kernelTriTOVtx, 1, 1, 1);// for render
    }


    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;//
        int[] args = new int[] { 0, 1, 0, 0 };
        outputArgBuffer.GetData(args);
        int vertexsCount = args[0];

        if (vertexsCount < 3) return mesh;
        if (vertexsCount >= MAX_VTX)
        {
            Debug.LogError("over max allowed vertices " + vertexsCount);
            return mesh;
        }
        Vertex[] vertexsArg = new Vertex[vertexsCount];
        outputVertexBuffer.GetData(vertexsArg, 0, 0, vertexsCount);//TODO


        Dictionary<Vector3, int> vertexsDict = new Dictionary<Vector3, int>();
        //Dictionary<Vector3, Vector3> normalsDict = new Dictionary<Vector3, Vector3>();

        // 收集所有顶点
        int vid = 0;
        for (int i = 0; i < vertexsCount; ++i)
        {
            var currPos = MeshTransformMatrix.MultiplyPoint(vertexsArg[i].vPosition);
            //var currPos = vertexsArg[i].vPosition;
            Vector3 position = new Vector3(
                (float)Math.Round(currPos.x, 2),
                (float)Math.Round(currPos.y, 2),
                (float)Math.Round(currPos.z, 2)
                );
            //Vector3 position = currPos;
            if (!vertexsDict.ContainsKey(position))
            {
                vertexsDict.Add(position, vid++);
                //normalsDict.Add(position, MeshTransformMatrix.inverse.MultiplyVector(vertexsArg[i].vNormal).normalized);
            }
            //else
            //{
            //    normalsDict[position] =  (normalsDict[position]+MeshTransformMatrix.inverse.MultiplyVector(vertexsArg[i].vNormal).normalized)*0.5f;
            //}
        }
        //Debug.Log("[MarchingCube] get vertexs count" + vertexsDict.Count.ToString());


        // 得到顶点数量后再构建三角形减少内存重新分配消耗
        Vector3[] vertexs = new Vector3[vertexsDict.Count];
        Vector3[] normals = new Vector3[vertexsDict.Count];
        int[] triengles = new int[vertexsCount];
        //int[] triangleVertexIds = new int[3];

        for (int i = 0; i < vertexsCount; ++i)
        {
            var currPos = MeshTransformMatrix.MultiplyPoint(vertexsArg[i].vPosition);
            //var currPos = vertexsArg[i].vPosition;
            Vector3 position = new Vector3(
                (float)Math.Round(currPos.x, 2),
                (float)Math.Round(currPos.y, 2),
                (float)Math.Round(currPos.z, 2)
                );
            //Vector3 position = currPos;
            if (!vertexsDict.ContainsKey(position))
            {
                Debug.LogError("Vertex not found");
                return mesh;
            }
            int idx = vertexsDict[position];
            vertexs[idx] = position;
            normals[idx] = MeshTransformMatrix.inverse.MultiplyVector(vertexsArg[i].vNormal).normalized;
            //normals[idx] = normalsDict[position];
            triengles[i] = idx;
        }

        mesh.SetVertices(vertexs);
        mesh.SetTriangles(triengles, 0);// idx是短整型
        mesh.SetNormals(normals);
        //mesh.SetNormals(normals);
        //mesh.RecalculateNormals();
        //mesh.RecalculateBounds();
        //mesh.OptimizeReorderVertexBuffer();
        return mesh;
    }


}
