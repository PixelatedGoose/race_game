// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/NewSurfaceShader"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Mask("Mask Texture", 2D) = "white" {}
		_IgnoreColor("Mask Ignore Color", Color) = (0, 0, 0, 1)
		_Art("Art Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags{
			"Queue" = "Transparent"             
			"RenderType" = "Transparent"             
			"PreviewType" = "Plane" 
		}
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert 
			#pragma fragment frag

			uniform sampler2D _MainTex;
			uniform sampler2D _Mask;
			uniform sampler2D _Art;
			uniform float4 _IgnoreColor;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata input)
			{
				v2f output;

				output.vertex = UnityObjectToClipPos(input.vertex);
				output.uv = input.uv;

				return output;
			}

			float4 frag(v2f input) : COLOR
			{
				if (any(tex2D(_Mask, input.uv.xy) != _IgnoreColor)) {
					float4 mainTexColor = tex2D(_MainTex, input.uv.xy);
					if (any(mainTexColor == _IgnoreColor))
						return tex2D(_Art, input.uv.xy);
					else
						return mainTexColor;
				}
				else 
					return float4(0, 0, 0, 0);
			}

			ENDCG
		}
	}
}