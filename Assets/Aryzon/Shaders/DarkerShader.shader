Shader "Aryzon/DarkerEffect" {
	Properties {
		_MainTex ("RGB", 2D) = "" {}
        _TintColor("Tint Color", Color) = (1,1,1,1)
	}
	Subshader {
		Cull Off ZTest Always ZWrite Off
        ColorMask RGB


		Pass {    
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
            float4 _TintColor;

			float4 frag (v2f_img i) : SV_Target
			{
                float4 color = (1,1,1,1);
                fixed4 col = tex2D(_MainTex, i.uv) + _TintColor - color;

				return col;
			}
	ENDCG 
		}
	}
	Fallback off
}