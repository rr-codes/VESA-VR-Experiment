Shader "Hidden/VR3D/3DFormatConversion"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LeftColor ("Left Color", Color) = (1,1,1,1)
		_RightColor ("Right Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
				
		// Horizontal Interlaced to Top/Bottom
		Pass 
		{  
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// We get the vertical index of the pixel that should be displayed at this text cord.				
				uint currentYPixel = (i.uv.y * _MainTex_TexelSize.w);
					
				float4 color = 0;

				// If were trying to display a pixel on the right half we get the pixel data from a different row.
				if (i.uv.y >= 0.5)
				{
					uint currentOffsetYPixel = (currentYPixel - (_MainTex_TexelSize.w / 2));						
				
					float currentTexelOfImageHalf = (_MainTex_TexelSize.y * currentOffsetYPixel);

					color = tex2D(_MainTex, fixed2(i.uv.x, (i.uv.y - 0.5) + currentTexelOfImageHalf));
				}
				else
				{
					uint currentOffsetYPixel = currentYPixel + 1;
				
					float currentTexelOfImageHalf = (_MainTex_TexelSize.y * currentOffsetYPixel);

					color = tex2D(_MainTex, fixed2(i.uv.x, i.uv.y + currentTexelOfImageHalf));
				}
				
				return color;
			}
			ENDCG
		}

		// Vertical Interlaced to Side-by-Side
		Pass
		{  
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;				
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{	
				// We get the vertical index of the pixel that should be displayed at this text cord.					
				uint currentXPixel = (i.uv.x * _MainTex_TexelSize.z);
					
				float4 color = 0;

				// If were trying to display a pixel on the right half we get the pixel data from a different row.
				if (i.uv.x >= 0.5)
				{
					uint currentOffsetXPixel = (currentXPixel - (_MainTex_TexelSize.z / 2));
					
					float currentTexelOfImageHalf = (_MainTex_TexelSize.x * currentOffsetXPixel);

					color = tex2D(_MainTex, fixed2((i.uv.x - 0.5) + currentTexelOfImageHalf, i.uv.y));
				}
				else
				{
					uint currentOffsetXPixel = currentXPixel + 1;
					
					float currentTexelOfImageHalf = (_MainTex_TexelSize.x * currentOffsetXPixel);

					color = tex2D(_MainTex, fixed2(i.uv.x + currentTexelOfImageHalf, i.uv.y));
				}

				return color;
			}
			ENDCG
		}

		// Checkerboard to Side-by-Side
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// We get the vertical index of the pixel that should be displayed at this text cord.
				uint currentXPixel = (i.uv.x * _MainTex_TexelSize.z);
				uint currentYPixel = (i.uv.y * _MainTex_TexelSize.w);
										
				float4 color = 0;

				// If were trying to display a pixel on the right half we get the pixel data from a different row.
				if (i.uv.x >= 0.5)
				{	
					if (currentYPixel % 2 == 0)
					{
						uint currentOffsetXPixel = (currentXPixel - (_MainTex_TexelSize.z / 2)) + 1;
						
						float currentTexelOfImageHalf = (_MainTex_TexelSize.x * currentOffsetXPixel);

						color = tex2D(_MainTex, fixed2((i.uv.x - 0.5) + currentTexelOfImageHalf, i.uv.y));
							
					}
					else
					{
						uint currentOffsetXPixel = (currentXPixel - (_MainTex_TexelSize.z / 2));
						
						float currentTexelOfImageHalf = (_MainTex_TexelSize.x * currentOffsetXPixel);

						color = tex2D(_MainTex, fixed2((i.uv.x - 0.5) + currentTexelOfImageHalf, i.uv.y));
					}
				}
				else
				{
					if (currentYPixel % 2 == 0)
					{
						uint currentOffsetXPixel = currentXPixel;
						
						float currentTexelOfImageHalf = (_MainTex_TexelSize.x * currentOffsetXPixel);

						color = tex2D(_MainTex, fixed2(i.uv.x + currentTexelOfImageHalf, i.uv.y));							
					}
					else
					{
						uint currentOffsetXPixel = currentXPixel + 1;
						
						float currentTexelOfImageHalf = (_MainTex_TexelSize.x * currentOffsetXPixel);

						color = tex2D(_MainTex, fixed2(i.uv.x + currentTexelOfImageHalf, i.uv.y));
					}
				}

				return color;
			}
			ENDCG
		}

		// Anaglyph to Side-by-Side
		Pass
		{  
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			fixed4 _LeftColor;
			fixed4 _RightColor;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				if (i.uv.x >= 0.5)
				{	
					float4 color = tex2D(_MainTex, fixed2((i.uv.x - 0.5) * 2, i.uv.y)) * _RightColor;
					return color;
				}
				else
				{
					float4 color = tex2D(_MainTex, fixed2(i.uv.x * 2, i.uv.y)) * _LeftColor;
					return color;
				}
			}
			ENDCG
		}
	}
}
