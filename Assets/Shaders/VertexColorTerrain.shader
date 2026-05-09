Shader "Custom/VertexColorTerrain"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 vertColor  : COLOR;
                float3 normalWS   : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.normalWS   = TransformObjectToWorldNormal(input.normalOS);
                o.vertColor  = input.color;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 n = normalize(input.normalWS);

                // Main directional light
                Light mainLight = GetMainLight();
                float NdotL = dot(n, mainLight.direction);

                // Half-lambert: wraps lighting so side faces are never fully dark
                float halfLambert = NdotL * 0.5 + 0.5;
                halfLambert = halfLambert * halfLambert; // squared for softer falloff

                // Minimum brightness so walls are always visible
                float lighting = max(halfLambert, 0.35);

                float3 albedo = input.vertColor.rgb * _BaseColor.rgb;
                float3 finalColor = albedo * lighting * mainLight.color;

                // Add ambient
                float3 ambient = SampleSH(n) * albedo * 0.5;
                finalColor += ambient;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // Shadow caster
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            Cull Back
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex sv
            #pragma fragment sf
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 p : POSITION; };
            struct V { float4 p : SV_POSITION; };
            V sv(A i) { V o; o.p = TransformObjectToHClip(i.p.xyz); return o; }
            half4 sf(V i) : SV_Target { return 0; }
            ENDHLSL
        }

        // Depth
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex dv
            #pragma fragment df
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 p : POSITION; };
            struct V { float4 p : SV_POSITION; };
            V dv(A i) { V o; o.p = TransformObjectToHClip(i.p.xyz); return o; }
            half4 df(V i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
