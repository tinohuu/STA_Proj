Shader "Custom/SpriteDropShadow"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_ShadowColor("Shadow", Color) = (0,0,0,1)
		_ShadowOffset("ShadowOffset", Vector) = (0,-0.1,0,0)
		radius("Radius", Range(0,50)) = 15
		resolution("Resolution", float) = 800
		hstep("HorizontalStep", Range(0,1)) = 0.5
		vstep("VerticalStep", Range(0,1)) = 0.5
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			// draw shadow
			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;
				fixed4 _ShadowColor;
				float4 _ShadowOffset;

				float radius;
				float resolution;

				//the direction of our blur
				//hstep (1.0, 0.0) -> x-axis blur
				//vstep(0.0, 1.0) -> y-axis blur
				//for example horizontaly blur equal:
				//float hstep = 1;
				//float vstep = 0;
				float hstep;
				float vstep;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex + _ShadowOffset);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _ShadowColor;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif
					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);
					color.rgb = _ShadowColor.rgb;

					/*#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
					#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED*/

					return color;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					float2 uv = IN.texcoord;

					float blur = radius / resolution / 4;



					float alpha = 0;
					alpha += tex2D(_MainTex, uv + blur * float2(1, 1)).a;
					alpha += tex2D(_MainTex, uv + blur * float2(-1, 1)).a;
					alpha += tex2D(_MainTex, uv + blur * float2(1, -1)).a;
					alpha += tex2D(_MainTex, uv + blur * float2(-1, -1)).a;
					alpha *= 0.2;


					/*c.a += tex2D(_MainTex, float2(tc.x - 4.0 * blur * hstep, tc.y - 4.0 * blur * vstep)).a * 0.0162162162;
					c.a += tex2D(_MainTex, float2(tc.x - 3.0 * blur * hstep, tc.y - 3.0 * blur * vstep)).a * 0.0540540541;
					c.a += tex2D(_MainTex, float2(tc.x - 2.0 * blur * hstep, tc.y - 2.0 * blur * vstep)).a * 0.1216216216;
					c.a += tex2D(_MainTex, float2(tc.x - 1.0 * blur * hstep, tc.y - 1.0 * blur * vstep)).a * 0.1945945946;

					c.a += tex2D(_MainTex, float2(tc.x, tc.y)).r * 0.2270270270;
					c.a += tex2D(_MainTex, float2(tc.x + 1.0 * blur * hstep, tc.y + 1.0 * blur * vstep)).a * 0.1945945946;
					c.a += tex2D(_MainTex, float2(tc.x + 2.0 * blur * hstep, tc.y + 2.0 * blur * vstep)).a * 0.1216216216;
					c.a += tex2D(_MainTex, float2(tc.x + 3.0 * blur * hstep, tc.y + 3.0 * blur * vstep)).a * 0.0540540541;
					c.a += tex2D(_MainTex, float2(tc.x + 4.0 * blur * hstep, tc.y + 4.0 * blur * vstep)).a * 0.0162162162;*/

					fixed4 c = fixed4(_ShadowColor.rgb, alpha * _ShadowColor.a);
					c.rgb *= c.a;

					return c;
				}
			ENDCG
			}

			// draw real sprite
			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);

					#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
					#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}