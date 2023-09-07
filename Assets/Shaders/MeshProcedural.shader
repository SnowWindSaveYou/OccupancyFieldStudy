//Blinn-Phone光照模型
Shader "Custom/MeshProcedural"
{
    Properties
    {
		[Toggle] _EnableClip ("Enable clip", float) = 0
        _ClipPlanePoint("Point on Clip Plane", Vector) = (0, 0, 0, 1)
        _ClipPlaneNormal ("Normal of Clip Plane", Vector) = (0, 1, 0, 0)
        _Color("Color", Color) = (1,1,1,1)
        _Specular("Specular",Color)=(0.5,0.5,0.5,1)
        _Gloss("Gloss",Range(8.0,256))=20
        _Index("RoiId",int)=0
    }
    SubShader
    {
	//Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Tags {"IgnoreProjector"="True" "RenderType"="Opaque"}
        //ZWrite Off     
        Cull Back
        //ColorMask 0
        ZTest LEqual
        // Blend SrcAlpha OneMinusSrcAlpha         
        Pass
        {
            //Tags {"LightMode" = "ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 			#pragma target 5.0
			#pragma require randomwrite
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile __ GHOST


            struct Vertex
			{
				float3 vPosition;
				float3 vNormal;
			};
            struct Triangle{ Vertex v[3];};
            uniform StructuredBuffer<Triangle> triangles;
            uniform float4x4 localToWorldMat;
            uniform float4x4 worldToLocalMat;


            //定义变量
            fixed4 _Color;
            fixed4 _Specular;
            float _Gloss;
            fixed4 _ClipPlanePoint;
			fixed4 _ClipPlaneNormal;
			float _EnableClip; 
            uint _Index;
            int3 _VolumeDim;
            int _IsCurrent;
            matrix _LocalToVolumeMatrix;

            float3 _MPRCenterVolume;

            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL; 
                uint vid:SV_VERTEXID;
            };
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal:TEXCOORD0;   
                float3 worldPos:TEXCOORD1;   
                float3 localPos:TEXCOORD2;
                //float3 volumePos:TEXCOORD5;
            };
 
            v2f vert (uint id : SV_VertexID)
            {
            	uint pid = id / 3;
				uint vid = id % 3;

                a2v v;
                v.vertex = float4(triangles[pid].v[vid].vPosition, 1);
                v.normal = triangles[pid].v[vid].vNormal;
                v.vid = vid;

                v2f o;
                o.worldPos=mul(localToWorldMat,v.vertex);
                o.pos = mul(UNITY_MATRIX_VP, float4(o.worldPos,1));
                o.worldNormal = mul(transpose(worldToLocalMat),v.normal.xyz);
                o.localPos = v.vertex;
                //o.volumePos = mul(_LocalToVolumeMatrix,v.vertex);
                return o;
            }

            [earlydepthstencil]
            fixed4 frag(v2f i, uint uCoverage : SV_COVERAGE) : SV_Target
            {
                float4 color = float4(0,0,0,0);
                fixed3 viewDir=normalize(UnityWorldSpaceViewDir(i.worldPos));//世界空间下的视角方向
                fixed3 worldNormal=normalize(i.worldNormal);
                fixed3 ambient=UNITY_LIGHTMODEL_AMBIENT.xyz;//得到环境光的颜色和强度信息
                fixed3 worldNormalInv = -worldNormal;
                fixed3 worldLightDir=normalize(UnityWorldSpaceLightDir(i.worldPos));//世界空间下的光照方向
 
                float4 col = _Color;
                float lightAttention = 1.0;

                fixed3 diffuse=_LightColor0.rgb*col.rgb*saturate(max(dot(worldNormal,worldLightDir), dot(worldNormalInv, worldLightDir))* 0.5 + 0.5)*lightAttention;
                fixed3 halfDir=normalize(worldLightDir+viewDir);
                float specularTerm = pow(max(dot(worldNormalInv,halfDir),dot(worldNormal,halfDir)),_Gloss)*lightAttention;
                fixed3 specular=_LightColor0.rgb*_Specular.rgb*specularTerm;
#if GHOST
               float VdN =dot(viewDir,worldNormal);
               if(VdN>0)
                    col.a =pow( (1-VdN),2*(1-col.a)+1);

                if(VdN<0)
                    col.a =pow( (1-abs(VdN)),2*(1-col.a)+1);
#endif


                float viewAttent = 1;
                if(dot(viewDir,worldNormal)<0){
                    viewAttent = 0.5;
                }

                color = fixed4((color.rgb+ ambient + diffuse*viewAttent + specular).xyz, col.a);
                return color;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}

