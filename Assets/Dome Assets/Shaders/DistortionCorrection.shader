//Shader "Custom/DistortionCorrection"
//{
// //Properties {
// //       _MainTex ("Base (RGB)", 2D) = "white" {}
// //   }
   
//    SubShader {
//        Tags { "Queue" = "Geometry" }
       
//        Pass {
//            GLSLPROGRAM
           
//            #ifdef VERTEX
           
//			uniform mat4 _OrthoMatrix;
//            varying vec2 TextureCoordinate;
           
//            void main()
//            {
//                gl_Position = _OrthoMatrix * gl_Vertex;
//                TextureCoordinate = gl_MultiTexCoord0.xy;
//            }
           
//            #endif
           
//            #ifdef FRAGMENT
                       
//            uniform sampler2D _SourceTex;
//            varying vec2 TextureCoordinate;
           
//            void main()
//            {
//                //gl_FragColor = vec4(1,0,0,0); //texture2D(_MainTex, TextureCoordinate);
//                gl_FragColor = texture2D(_SourceTex, TextureCoordinate);
//            }
           
//            #endif
           
//            ENDGLSL
//        }
//    }
//}

Shader "Custom/DistortionCorrection"
{	
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
		
			ZTest Always

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
			};
			
			uniform sampler2D _SourceTex;
			uniform float4x4 _OrthoMatrix;
			uniform float4 _SourceTex_TexelSize;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(_OrthoMatrix, float4(v.vertex.xy, 0, 1));
				o.uv = v.uv;
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{	
				if (_SourceTex_TexelSize.y < 0) i.uv.y = 1 - i.uv.y;			
				half4 c = tex2D(_SourceTex, (i.uv));
				return c;
			}
			ENDCG
		}
	}
}