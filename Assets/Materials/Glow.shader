// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Glow"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_GlowTex("Glow Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Interval("Interval", Float) = 5
		_Duration("Duration", Float) = 5
		_Scale("Scale", Float) = 1
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
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
					float2 worldUV : TEXCOORD1;
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
					OUT.worldUV = mul(unity_ObjectToWorld, IN.vertex).xy;
					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				sampler2D _GlowTex;
				float4 _MainTex_ST;
				float4 _GlowTex_ST;
				float _AlphaSplitEnabled;
				float _Interval;
				float _Duration;
				float _Scale;
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
					float2 guv = IN.texcoord;
					guv.x /= _Scale;
					float offset = fmod(_Time.y, _Interval) / _Duration - 2;
					guv.x -= offset;

					fixed4 gc = tex2D(_GlowTex, guv);
					gc *= c.a;
					c = gc * gc.a + (1 - gc.a) * c;
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}