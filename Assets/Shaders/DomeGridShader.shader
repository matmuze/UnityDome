Shader "Custom/DomeGridShader" {
	Properties {
		_MainTex("Base (RGB)", 2D) = "white" {}
		_FOV("FOV", Range(0.01, 360.0)) = 180.0
	}
	SubShader {
		Tags {"Queue"="Overlay"}

		Lighting Off
        Cull Off
			ZWrite On

        Pass {
            CGPROGRAM
	        #pragma vertex vert  
	        #pragma fragment frag 
         
			struct vertexInput {
				float4 vert : POSITION;
				float4 norm : NORMAL;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 norm : NORMAL;
			};

			uniform sampler2D _MainTex;
			uniform float _FOV;

			vertexOutput vert(vertexInput input) 
			{
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP, input.vert);
				output.norm = input.norm;				
				return output;
			}
			float4 frag(vertexOutput input) : COLOR
			{
				float3 vec = input.norm;
				float dir = atan2(-vec.z, vec.x);
				float r = (1.0-asin(vec.y)/(3.1415926535897932384626433832795/2.0)) * 180.0/_FOV;
				if (r > 1.0)
					discard;
				return tex2D(_MainTex, float2(r*cos(dir)/2.0+0.5, r*sin(dir)/2.0+0.5));	
			}

	        ENDCG
        }
    }
}
