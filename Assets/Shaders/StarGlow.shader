Shader "Custom/StarGlow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
		
		_Color ("Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Blend SrcAlpha OneMinusSrcAlpha

		Tags
		{
			"Queue" = "Transparent"
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			bool isBeam (float ang)
			{
				float PI = 3.14159265f;
				int NUM_BEAMS = 5;
				float BEAM_WIDTH = PI / 6;

				ang += fmod(_Time.y, 2 * PI);

				if (ang > PI) {
					ang -= 2 * PI;
				}

				//ang between -PI and PI
				for (int i = 0; i < NUM_BEAMS + 1; i++) {
					float beamCenter = i * 2 * PI / NUM_BEAMS - PI;
					float beamStart = beamCenter - BEAM_WIDTH / 2;
					float beamEnd = beamCenter + BEAM_WIDTH / 2;
					if (beamStart < ang && ang < beamEnd) {
						return true;
					}
				}
				return false;
			}

			fixed4 _Color;

			fixed4 frag (v2f i) : SV_Target
			{
				float ALPHA_SCALE = 0.8;
				float ALPHA_NOBEAM_OFFSET = 0.1;
				float ALPHA_NOBEAM_SCALE = 0.8;

				float2 uv = i.uv;
				float y = uv.y - 0.5;
				float x = uv.x - 0.5;
				float ang = atan2(y, x);

				float maxDist = 0.5; //from center to middle of side
				float dist = sqrt(pow(x, 2) + pow(y, 2)) / maxDist;
				float alpha = 1 - dist;
				//if not in beam region, decrease alpha by a bit
				if (!isBeam(ang)) {
					alpha = alpha * ALPHA_NOBEAM_SCALE - ALPHA_NOBEAM_OFFSET;
				}

				alpha *= ALPHA_SCALE;

				fixed4 col = fixed4(_Color.r, _Color.g, _Color.b, _Color.a * alpha);
				return col;
			}
			ENDCG
		}
	}
}
