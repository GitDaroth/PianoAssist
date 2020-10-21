Shader "PianoAssist/LineShader"
{
	Properties
	{
		_Color ("_Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_FadeOutStart("_FadeOutStart", Float) = 0.0
		_FadeOutEnd("_FadeOutEnd", Float) = 1.0
	}
	SubShader
	{
		Tags {"Queue" = "Transparent"
			  "RenderType" = "Transparent" 
			  "IgnoreProjector" = "True"}

		LOD 100

		ZWrite On

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 worldPos : TEXCOORD0;
			};

			float4 _Color;
			float _FadeOutStart;
			float _FadeOutEnd;

			v2f vert(float4 vertPos : POSITION, out float4 outPos : SV_POSITION)
			{
				v2f o;
				o.worldPos = mul(unity_ObjectToWorld, vertPos);;
				outPos = UnityObjectToClipPos(vertPos);
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
				fixed4 color = _Color;

				if (i.worldPos.y > 0.0)
					color.a = 0.0;
				else
					color.a = 1.0 - clamp((-i.worldPos.y - _FadeOutStart) / (_FadeOutEnd - _FadeOutStart), 0.0, 1.0);

				return color;
			}
			ENDCG
		}
	}
}
