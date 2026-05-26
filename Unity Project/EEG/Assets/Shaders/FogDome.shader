Shader "Custom/FogDome"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.5, 0.6, 0.7, 1)
        _HorizonColor ("Horizon Color", Color) = (0.7, 0.8, 0.85, 1)
        _FogHeight ("Fog Height", Range(-1, 1)) = 0.0
        _FogFalloff ("Fog Falloff", Range(0.1, 10)) = 2.0
        _AlphaPower ("Alpha Falloff Power", Range(0.5, 8)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Front
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };
            
            half4 _FogColor;
            half4 _HorizonColor;
            float _FogHeight;
            float _FogFalloff;
            float _AlphaPower;
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.viewDir = normalize(TransformObjectToWorld(IN.positionOS.xyz) - _WorldSpaceCameraPos);
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                float heightFactor = saturate((IN.viewDir.y - _FogHeight) * _FogFalloff);
                
                half3 col = lerp(_FogColor.rgb, _HorizonColor.rgb, heightFactor);
                float alpha = 1.0 - pow(heightFactor, _AlphaPower);
                
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}