Shader "Custom/EdgeShader" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _NoiseTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_MultColor ("Color Tint Mult", Color) = (1,1,1,1)
		_TintColor ("Color Tint Add", Color) = (0,0,0,0)
        _MultColorActive ("MultColorActive", Int) = 0
        _TintColorActive ("TintColorActive", Int) = 0
        _Side ("Side", Int) = 0
	}
	
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off Lighting Off Cull Off Fog { Mode Off } Blend SrcAlpha OneMinusSrcAlpha
		LOD 110
		
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert_vct
			#pragma fragment frag_mult 
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
            sampler2D _NoiseTex;
			float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            int _MultColorActive;
            int _TintColorActive;
            int _Side;

			float4 _MultColor;
			float4 _TintColor;

			struct vin_vct 
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f_vct
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			v2f_vct vert_vct(vin_vct v)
			{
				v2f_vct o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
				o.texcoord = v.texcoord;
				return o;
			}

			fixed4 frag_mult(v2f_vct i) : COLOR
			{
                //float2 shim = i.texcoord + float2(tex2D(_NoiseTex, i.vertex.xy/300 + float2(_SinTime.w, _CosTime.w)/2).xy)/  40;
				float2 shim = i.texcoord + float2(
					tex2D(_NoiseTex, i.vertex.xy/1000 + (_Side ? float2((_Time.w%50)/50, 0) : -float2((_Time.w%50)/50, 0))).z,
					tex2D(_NoiseTex, i.vertex.xy/1000 + (_Side ? float2(0, (_Time.w%50)/50) : -float2(0, (_Time.w%50)/50))).z
				)/ 1.75;
                fixed4 result = tex2D(_MainTex, shim) * i.color;

                if (_MultColorActive == 1) {
                    result.rgb *= _MultColor.rgb;
                }
                if (_TintColorActive == 1) {
                    result.rgb += _TintColor.rgb;
                }
                return result;
			}
			
			ENDCG
		} 
	}
 
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off Fog { Mode Off }
		LOD 100

		BindChannels 
		{
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
			Bind "Color", color
		}

		Pass 
		{
			Lighting Off
			SetTexture [_MainTex] { combine texture * primary } 
		}
	}
}
