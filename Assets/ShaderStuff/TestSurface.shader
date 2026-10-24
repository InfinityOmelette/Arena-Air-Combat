Shader "Custom/TestSurface"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.0, 0.4, 0.6, 0.6)
        _MainTex ("Normal Map", 2D) = "bump" {}
        _Speed ("Wave Speed", Float) = 0.5
        _Smoothness ("Smoothness", Range(0,1)) = 0.9
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        sampler2D _MainTex;
        fixed4 _Color;
        float _Speed;
        float _Smoothness;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 scrolledUV = IN.uv_MainTex + float2(_Time.y * _Speed * 0.1, _Time.y * _Speed * 0.05);
            o.Normal = UnpackNormal(tex2D(_MainTex, scrolledUV));
            o.Albedo = _Color.rgb;
            o.Smoothness = _Smoothness;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}