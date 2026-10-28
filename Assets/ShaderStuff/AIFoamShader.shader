Shader "Custom/AIFoamShader"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.1, 0.4, 0.5, 0.8)
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamDistance ("Foam Distance", Float) = 0.5
        _NoiseTex ("Foam Noise (RGB)", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 10.0
        _FoamSpeed ("Foam Scroll Speed", Vector) = (0.1, 0.1, 0, 0)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard alpha vertex:vert

        sampler2D _CameraDepthTexture;
        sampler2D _NoiseTex;

        fixed4 _Color;
        fixed4 _FoamColor;
        float _FoamDistance;
        float _NoiseScale;
        float2 _FoamSpeed;
        struct Input
        {
            float4 screenPos;
            float2 uv_NoiseTex;
        };

        void vert (inout appdata_full v)
        {
            // Optional: Add basic vertex wave displacement here if desired
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 1. Get screen space coordinates
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

            // 2. Sample raw depth and convert to linear eye depth
            float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
            float sceneDepth = LinearEyeDepth(rawDepth);
            float surfaceDepth = LinearEyeDepth(IN.screenPos.z / IN.screenPos.w);

            // 3. Difference between background geometry and water surface
            float depthDifference = sceneDepth - surfaceDepth;

            // 4. Generate animated UV for foam noise
            float2 noiseUV = (screenUV * _NoiseScale) + (_Time.y * _FoamSpeed);
            float noise = tex2D(_NoiseTex, noiseUV).r;

            // 5. Calculate foam factor based on depth difference and noise
            float foamFactor = saturate(1.0 - (depthDifference / _FoamDistance));
            foamFactor = step(1.0 - foamFactor, noise); // Crisp stylized foam or smooth with smoothstep

            // Alternatively, smooth foam:
            // float foam = saturate((_FoamDistance - depthDifference + noise * 0.2) / _FoamDistance);

            // 6. Blend water color and foam color
            fixed4 finalColor = lerp(_Color, _FoamColor, saturate(foamFactor));

            o.Albedo = finalColor.rgb;
            o.Alpha = finalColor.a;
            o.Metallic = 0.1;
            o.Smoothness = 0.8;
        }
        ENDCG
    }
    FallBack "Transparent/VertexLit"
}