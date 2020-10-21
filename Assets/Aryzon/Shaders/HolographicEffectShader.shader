Shader "Aryzon/HolographicEffect" {
	Properties {
		_MainTex ("RGB", 2D) = "" {}
	}
	Subshader {
		Cull Off ZTest Always ZWrite Off
        ColorMask RGBA

    

		Pass {    
        Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;

			float4 frag (v2f_img i) : SV_Target
			{
				half2 uv = i.uv;
				float4 screen = tex2D(_MainTex, uv);

                float highest = screen.r;
                if (screen.g > screen.r) {
                    highest = screen.g;
                    if (screen.b > screen.g) {
                        highest = screen.b;
                    }
                } else if (screen.b > screen.r) {
                    highest = screen.b;
                }

                screen.r = screen.r / highest;
                screen.g = screen.g / highest;
                screen.b = screen.b / highest;

                screen.a = highest;

				return screen;
			}
	ENDCG 
		}
	}
	Fallback off
}