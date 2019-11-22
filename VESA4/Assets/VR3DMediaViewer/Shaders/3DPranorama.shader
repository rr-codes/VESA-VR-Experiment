Shader "Hidden/VR3D/3DPanorama"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Rotation ("Rotation", Float ) = 0
		[MaterialToggle] _OneEightyDegrees ("180", Float ) = 0
		[MaterialToggle] _OneEightyClamp ("180 Clamp", Float ) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				//UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;			
			float _Rotation;
			fixed _OneEightyDegrees;
			fixed _OneEightyClamp;
			
			v2f vert (appdata v)
			{
				v2f o;				
                o.vertex = UnityObjectToClipPos(v.vertex);				
                o.uv = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 coords = normalize(_WorldSpaceCameraPos.xyz - i.uv.xyz);
				coords *= -1.0;
												
				float a = atan2(coords.z, coords.x);
				float b = acos(coords.y);
				float c = 0.5 / UNITY_PI;
				float d = 1 / UNITY_PI;
				float stretch = (_OneEightyDegrees == 1 ? 2 : 1);

				float2 coords2 = float2(0.5, 1) - float2(a, b) * float2(c, d);
								
				float2 panoTexCoords = (coords2 + 
				float4(_Rotation / 360, 0, stretch, 1).xy) * 
				float4(_Rotation / 360, 1, stretch, 1).zw;
				
				// sample the texture
				float4 col;
				if (_OneEightyDegrees == 1 && _OneEightyClamp == 1)
				{				
					if (panoTexCoords.x >= 0 && panoTexCoords.x < 1)				
						col = tex2Dlod(_MainTex, float4(panoTexCoords.xy * _MainTex_ST.xy + _MainTex_ST.zw, 0, 0));
					else
						col = float4(0, 0, 0, 1);
				}
				else
					col = tex2Dlod(_MainTex, float4(panoTexCoords.xy * _MainTex_ST.xy + _MainTex_ST.zw, 0, 0));

				return col;
			}
			ENDCG
		}
	}
}
