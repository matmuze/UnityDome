Shader "Custom/BakeStencilMask"
{	
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM

			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			struct appdata 
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				sample float4 pos : SV_POSITION;
				sample float2 uv : TEXCOORD0;
				sample float2 screenUV : TEXCOORD1;
			};
			
			
			RWTexture2D<float4> _StencilMaskTex : register(u1);
			sampler2D _OpacityMaskTex;
			float4x4 _OrthoMatrix;
			int _Width;
			int _Height;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(_OrthoMatrix, float4(v.vertex.xy, 0, 1));
				o.screenUV = v.vertex.xy;
				o.uv = v.uv;
				return o;
			}

			void frag(v2f i, uint iSample : SV_SAMPLEINDEX) 
			{		
				float opacity = tex2D(_OpacityMaskTex, i.screenUV).r;
				
				if(opacity >= 0.05)
				{
					//_StencilMaskTex[floor(i.screenUV * float2(_Width, _Height))] = float4(1,1,1,1);	
					_StencilMaskTex[ceil(i.uv * float2(_Width, _Height))] = float4(1,1,1,1);		
					_StencilMaskTex[floor(i.uv * float2(_Width, _Height))] = float4(1,1,1,1);				
					_StencilMaskTex[round(i.uv * float2(_Width, _Height))] = float4(1,1,1,1);		
				}						
			}
			ENDCG
		}
	}
}