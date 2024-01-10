Shader "Custom/UI/RoundRect"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("Main Texture", 2D) = "white" {}
        [PerRendererData]
        _Color ("Main Color", Color) = (1,1,1,1)
        _Radius ("Radius", Range(0,0.5)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

	pass{
        CGPROGRAM
       
	   #pragma vertex vert
	   #pragma fragment frag
	   #include "unitycg.cginc"
	   
	   sampler2D _MainTex;
	   fixed _Radius;
	   fixed4 _Color;

	   struct v2f{
            float4 pos:SV_POSITION;
            float2 srcUV:TEXCOORD0;		// ԭ����uv
            float2 adaptUV:TEXCOORD1;	// ����������������uv
	   };

	   
	   v2f vert(appdata_base v){
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.srcUV = v.texcoord;

			// ����uv��Χ��(0,1)��(-0.5,0.5)����ͼƬuvԭ������½ǵ����ĵ�
			o.adaptUV = o.srcUV - fixed2(0.5,0.5);
			return o;
	   }

	   fixed4 frag(v2f i):COLOR
	   {
			fixed4 col = fixed4(0,0,0,0);

			// ���Ȼ����м䲿�֣�������Բ�ǰ뾶����ģ�(adaptUV x y ����ֵС�� 0.5-Բ�ǰ뾶�ڵ�����)
			if(abs(i.adaptUV).x<(0.5-_Radius) || abs(i.adaptUV).y<(0.5-_Radius))
			{
				col =tex2D(_MainTex,i.srcUV);
			}
			else
			{  
				// ����ĸ�Բ�ǲ��֣��൱���� ��0.5-Բ�ǰ뾶��0.5-Բ�ǰ뾶��ΪԲ�ģ��� uv �� Բ�ǰ뾶�ڵ�uv���Ƴ�����
				if(length(abs(i.adaptUV)-fixed2(0.5-_Radius,0.5-_Radius)) < _Radius){
					col = tex2D(_MainTex,i.srcUV);
				}
				else// �����Ĳ��ֺ��Ե�
				{
					discard;
				}
			}
			return col*_Color;
	   }

        ENDCG
    }
	}
    FallBack "Diffuse"
}
