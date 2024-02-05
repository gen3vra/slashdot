Shader "Custom/ColorShiftGradient"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (1, 0, 0, 1) // Default to red
        _Color2 ("Color 2", Color) = (0, 0, 1, 1) // Default to blue
        _Speed ("Color Shift Speed", Range(0.1, 10)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        struct Input
        {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        fixed4 _Color1;
        fixed4 _Color2;
        float _Speed;

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = lerp(_Color1, _Color2, 0.5 + 0.5 * sin(_Time.y * _Speed)); // Smoothly interpolate between colors
            c.rgb *= tex2D(_MainTex, IN.uv_MainTex).rgb; // Preserve texture color
            o.Albedo = c.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
