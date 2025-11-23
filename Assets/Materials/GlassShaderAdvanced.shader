Shader "BarSimulator/GlassAdvanced"
{
    Properties
    {
        // Basic properties
        _Color ("Tint Color", Color) = (1, 1, 1, 0.08)
        _Glossiness ("Smoothness", Range(0, 1)) = 0.98
        _Metallic ("Metallic", Range(0, 1)) = 0.0

        // Fresnel
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 4
        _FresnelIntensity ("Fresnel Intensity", Range(0, 1)) = 0.9

        // Refraction
        _RefractionStrength ("Refraction Strength", Range(0, 0.2)) = 0.05
        _RefractionChromatic ("Chromatic Aberration", Range(0, 0.1)) = 0.01

        // Thickness simulation
        _ThicknessMap ("Thickness Map", 2D) = "white" {}
        _ThicknessScale ("Thickness Scale", Range(0, 2)) = 0.5
        _ThicknessColor ("Thickness Tint", Color) = (0.9, 0.95, 1, 1)

        // Specular
        _SpecularIntensity ("Specular Intensity", Range(0, 3)) = 1.5
        _SpecularPower ("Specular Power", Range(1, 256)) = 64

        // Rim lighting
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 8)) = 3
        _RimIntensity ("Rim Intensity", Range(0, 1)) = 0.3

        // Environment reflection
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 0.3
        _ReflectionBlur ("Reflection Blur", Range(0, 8)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+100"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        LOD 400

        // Grab pass for refraction
        GrabPass { "_GrabTexture" }

        // Inner surface (back face) - rendered first
        Pass
        {
            Name "InnerSurface"
            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "UnityStandardBRDF.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 grabPos : TEXCOORD3;
                float2 uv : TEXCOORD4;
                float3 reflectionDir : TEXCOORD5;
            };

            fixed4 _Color;
            half _FresnelPower;
            half _FresnelIntensity;
            float _RefractionStrength;
            float _RefractionChromatic;

            sampler2D _ThicknessMap;
            float4 _ThicknessMap_ST;
            float _ThicknessScale;
            fixed4 _ThicknessColor;

            float _SpecularIntensity;
            float _SpecularPower;

            fixed4 _RimColor;
            float _RimPower;
            float _RimIntensity;

            float _ReflectionStrength;
            float _ReflectionBlur;

            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(-v.normal); // Flip for back face
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.uv = TRANSFORM_TEX(v.uv, _ThicknessMap);
                o.reflectionDir = reflect(-o.viewDir, o.worldNormal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Thickness
                float thickness = tex2D(_ThicknessMap, i.uv).r * _ThicknessScale;

                // Refraction with chromatic aberration
                float2 baseOffset = i.worldNormal.xz * _RefractionStrength;
                float2 grabUV = i.grabPos.xy / i.grabPos.w;

                // Sample with slight chromatic offset
                float2 redUV = grabUV + baseOffset * (1.0 + _RefractionChromatic);
                float2 greenUV = grabUV + baseOffset;
                float2 blueUV = grabUV + baseOffset * (1.0 - _RefractionChromatic);

                fixed4 refractedColor;
                refractedColor.r = tex2D(_GrabTexture, redUV).r;
                refractedColor.g = tex2D(_GrabTexture, greenUV).g;
                refractedColor.b = tex2D(_GrabTexture, blueUV).b;
                refractedColor.a = 1;

                // Fresnel
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, i.worldNormal)), _FresnelPower);

                // Environment reflection
                half4 reflection = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, i.reflectionDir, _ReflectionBlur);
                reflection.rgb = DecodeHDR(reflection, unity_SpecCube0_HDR);

                // Base glass color
                fixed4 glassColor = _Color;

                // Apply thickness tinting
                glassColor.rgb = lerp(glassColor.rgb, _ThicknessColor.rgb, thickness);

                // Combine refraction and tint
                fixed4 finalColor = refractedColor * glassColor;

                // Add reflection
                finalColor.rgb = lerp(finalColor.rgb, reflection.rgb, fresnel * _ReflectionStrength);

                // Fresnel edge highlight
                finalColor.rgb += fresnel * _FresnelIntensity * 0.5;

                // Specular highlight
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDir = normalize(lightDir + i.viewDir);
                float spec = pow(max(0, dot(i.worldNormal, halfDir)), _SpecularPower) * _SpecularIntensity;
                finalColor.rgb += spec * 0.5;

                // Alpha based on fresnel and thickness
                finalColor.a = lerp(_Color.a, 0.4, fresnel) + thickness * 0.2;

                return finalColor;
            }
            ENDCG
        }

        // Outer surface (front face)
        Pass
        {
            Name "OuterSurface"
            Cull Back
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "UnityStandardBRDF.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 grabPos : TEXCOORD3;
                float2 uv : TEXCOORD4;
                float3 reflectionDir : TEXCOORD5;
            };

            fixed4 _Color;
            half _FresnelPower;
            half _FresnelIntensity;
            float _RefractionStrength;
            float _RefractionChromatic;

            sampler2D _ThicknessMap;
            float4 _ThicknessMap_ST;
            float _ThicknessScale;
            fixed4 _ThicknessColor;

            float _SpecularIntensity;
            float _SpecularPower;

            fixed4 _RimColor;
            float _RimPower;
            float _RimIntensity;

            float _ReflectionStrength;
            float _ReflectionBlur;

            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.uv = TRANSFORM_TEX(v.uv, _ThicknessMap);
                o.reflectionDir = reflect(-o.viewDir, o.worldNormal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Thickness
                float thickness = tex2D(_ThicknessMap, i.uv).r * _ThicknessScale;

                // Refraction with chromatic aberration
                float2 baseOffset = i.worldNormal.xz * _RefractionStrength;
                float2 grabUV = i.grabPos.xy / i.grabPos.w;

                float2 redUV = grabUV + baseOffset * (1.0 + _RefractionChromatic);
                float2 greenUV = grabUV + baseOffset;
                float2 blueUV = grabUV + baseOffset * (1.0 - _RefractionChromatic);

                fixed4 refractedColor;
                refractedColor.r = tex2D(_GrabTexture, redUV).r;
                refractedColor.g = tex2D(_GrabTexture, greenUV).g;
                refractedColor.b = tex2D(_GrabTexture, blueUV).b;
                refractedColor.a = 1;

                // Fresnel
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, i.worldNormal)), _FresnelPower);

                // Environment reflection
                half4 reflection = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, i.reflectionDir, _ReflectionBlur);
                reflection.rgb = DecodeHDR(reflection, unity_SpecCube0_HDR);

                // Base glass color
                fixed4 glassColor = _Color;
                glassColor.rgb = lerp(glassColor.rgb, _ThicknessColor.rgb, thickness);

                // Combine
                fixed4 finalColor = refractedColor * glassColor;

                // Add reflection
                finalColor.rgb = lerp(finalColor.rgb, reflection.rgb, fresnel * _ReflectionStrength);

                // Rim lighting
                float rim = pow(1.0 - saturate(dot(i.viewDir, i.worldNormal)), _RimPower);
                finalColor.rgb += _RimColor.rgb * rim * _RimIntensity;

                // Fresnel edge highlight
                finalColor.rgb += fresnel * _FresnelIntensity;

                // Specular highlight
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDir = normalize(lightDir + i.viewDir);
                float spec = pow(max(0, dot(i.worldNormal, halfDir)), _SpecularPower) * _SpecularIntensity;
                finalColor.rgb += spec;

                // Alpha
                finalColor.a = lerp(_Color.a, 0.6, fresnel) + thickness * 0.2;

                return finalColor;
            }
            ENDCG
        }
    }

    FallBack "BarSimulator/Glass"
}
