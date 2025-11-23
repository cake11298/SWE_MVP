Shader "BarSimulator/LiquidAdvanced"
{
    Properties
    {
        // Basic properties
        _Color ("Liquid Color", Color) = (1, 1, 1, 0.8)
        _Glossiness ("Smoothness", Range(0, 1)) = 0.95
        _Metallic ("Metallic", Range(0, 1)) = 0.0

        // Fresnel
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 3
        _FresnelIntensity ("Fresnel Intensity", Range(0, 1)) = 0.5

        // Wave effects
        _WobbleX ("Wobble X", Float) = 0
        _WobbleZ ("Wobble Z", Float) = 0
        _WobbleIntensity ("Wobble Intensity", Range(0, 1)) = 0
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        _WaveSpeed ("Wave Speed", Float) = 2
        _WaveFrequency ("Wave Frequency", Float) = 3
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.1)) = 0.02

        // Refraction
        _RefractionStrength ("Refraction Strength", Range(0, 0.5)) = 0.1
        _RefractionDistortion ("Refraction Distortion", Range(0, 1)) = 0.3

        // Bubbles
        _BubbleColor ("Bubble Color", Color) = (1, 1, 1, 0.3)
        _BubbleIntensity ("Bubble Intensity", Range(0, 1)) = 0
        _BubbleSize ("Bubble Size", Range(0.1, 10)) = 3
        _BubbleSpeed ("Bubble Speed", Range(0.1, 5)) = 1
        _BubbleDensity ("Bubble Density", Range(1, 20)) = 8

        // Foam (for beer/soda)
        _FoamColor ("Foam Color", Color) = (1, 1, 0.9, 0.8)
        _FoamThickness ("Foam Thickness", Range(0, 0.3)) = 0
        _FoamNoise ("Foam Noise", Range(0, 1)) = 0.5

        // Layering
        _LayerEnabled ("Layer Enabled", Float) = 0
        _Layer2Color ("Layer 2 Color", Color) = (1, 0.5, 0, 0.8)
        _Layer2Height ("Layer 2 Height", Range(0, 1)) = 0.5
        _LayerBlend ("Layer Blend", Range(0, 0.5)) = 0.1

        // Specular highlight
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1
        _SpecularPower ("Specular Power", Range(1, 128)) = 32
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        LOD 300

        // Grab pass for refraction
        GrabPass { "_GrabTexture" }

        // Back face pass
        Pass
        {
            Name "BackFace"
            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

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
                float3 localPos : TEXCOORD3;
                float4 grabPos : TEXCOORD4;
                float2 uv : TEXCOORD5;
            };

            fixed4 _Color;
            half _Glossiness;
            half _FresnelPower;
            half _FresnelIntensity;

            float _WobbleX;
            float _WobbleZ;
            float _WobbleIntensity;
            float _FillAmount;
            float _WaveSpeed;
            float _WaveFrequency;
            float _WaveAmplitude;

            float _RefractionStrength;
            float _RefractionDistortion;

            fixed4 _BubbleColor;
            float _BubbleIntensity;
            float _BubbleSize;
            float _BubbleSpeed;
            float _BubbleDensity;

            fixed4 _FoamColor;
            float _FoamThickness;
            float _FoamNoise;

            float _LayerEnabled;
            fixed4 _Layer2Color;
            float _Layer2Height;
            float _LayerBlend;

            float _SpecularIntensity;
            float _SpecularPower;

            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;

            // Simple noise function
            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Smooth noise
            float smoothNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Bubble pattern
            float bubblePattern(float3 p, float time)
            {
                float2 uv = p.xz * _BubbleDensity;
                uv.y += time * _BubbleSpeed;

                float2 cell = floor(uv);
                float2 cellUV = frac(uv);

                float bubble = 0;
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 cellPos = cell + neighbor;

                        // Random position within cell
                        float2 bubblePos = noise(cellPos) * 0.5 + 0.25;
                        bubblePos += neighbor;

                        // Animate Y position
                        bubblePos.y += frac(time * _BubbleSpeed * (0.5 + noise(cellPos + 100))) * 2;

                        float dist = length(cellUV - bubblePos);
                        float size = (0.05 + noise(cellPos + 200) * 0.05) / _BubbleSize;
                        bubble = max(bubble, smoothstep(size, size * 0.1, dist));
                    }
                }

                // Fade bubbles at top and bottom
                float heightFade = smoothstep(0, 0.2, p.y) * smoothstep(1, 0.8, p.y);
                return bubble * heightFade;
            }

            v2f vert (appdata v)
            {
                v2f o;

                // Store local position
                o.localPos = v.vertex.xyz;
                o.uv = v.uv;

                // Wave calculation
                float wobbleAmount = _WobbleIntensity * _WaveAmplitude;
                float time = _Time.y * _WaveSpeed;
                float wave = sin(v.vertex.x * _WaveFrequency + time) * cos(v.vertex.z * _WaveFrequency + time * 0.7);

                // Combined wobble
                float totalWobble = _WobbleX * v.vertex.x + _WobbleZ * v.vertex.z;
                totalWobble += wave * wobbleAmount;

                // Only affect top vertices
                float topFactor = saturate((v.vertex.y + 0.5) * 2);
                v.vertex.y += totalWobble * topFactor * 0.5;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(-v.normal); // Flip for back face
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.grabPos = ComputeGrabScreenPos(o.pos);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize height for effects
                float normalizedHeight = saturate((i.localPos.y + 0.5) * _FillAmount);

                // Base color with layering
                fixed4 liquidColor = _Color;
                if (_LayerEnabled > 0.5)
                {
                    float layerMix = smoothstep(_Layer2Height - _LayerBlend, _Layer2Height + _LayerBlend, normalizedHeight);
                    liquidColor = lerp(_Layer2Color, _Color, layerMix);
                }

                // Refraction
                float2 refractionOffset = i.worldNormal.xz * _RefractionStrength;
                refractionOffset += sin(_Time.y * 2 + i.worldPos.x * 3) * _RefractionDistortion * 0.01;
                float2 grabUV = i.grabPos.xy / i.grabPos.w + refractionOffset;
                fixed4 refractedColor = tex2D(_GrabTexture, grabUV);

                // Fresnel
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, i.worldNormal)), _FresnelPower);

                // Bubbles
                float bubbles = 0;
                if (_BubbleIntensity > 0)
                {
                    bubbles = bubblePattern(i.localPos, _Time.y) * _BubbleIntensity;
                }

                // Foam at top
                float foam = 0;
                if (_FoamThickness > 0)
                {
                    float foamStart = 1.0 - _FoamThickness;
                    if (normalizedHeight > foamStart)
                    {
                        float foamFactor = (normalizedHeight - foamStart) / _FoamThickness;
                        float foamNoise = smoothNoise(i.localPos.xz * 10 + _Time.y * 0.5) * _FoamNoise;
                        foam = saturate(foamFactor + foamNoise);
                    }
                }

                // Combine colors
                fixed4 finalColor = liquidColor;

                // Add refraction tint
                finalColor.rgb = lerp(finalColor.rgb, refractedColor.rgb * liquidColor.rgb, 0.3);

                // Add bubbles
                finalColor.rgb = lerp(finalColor.rgb, _BubbleColor.rgb, bubbles * _BubbleColor.a);

                // Add foam
                finalColor.rgb = lerp(finalColor.rgb, _FoamColor.rgb, foam * _FoamColor.a);

                // Fresnel emission
                float wobbleGlow = _WobbleIntensity * 0.3;
                finalColor.rgb += liquidColor.rgb * fresnel * (_FresnelIntensity + wobbleGlow);

                // Specular highlight
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDir = normalize(lightDir + i.viewDir);
                float spec = pow(max(0, dot(i.worldNormal, halfDir)), _SpecularPower) * _SpecularIntensity;
                finalColor.rgb += spec;

                finalColor.a = liquidColor.a + foam * 0.3;

                return finalColor;
            }
            ENDCG
        }

        // Front face pass
        Pass
        {
            Name "FrontFace"
            Cull Back
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

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
                float3 localPos : TEXCOORD3;
                float4 grabPos : TEXCOORD4;
                float2 uv : TEXCOORD5;
            };

            fixed4 _Color;
            half _Glossiness;
            half _FresnelPower;
            half _FresnelIntensity;

            float _WobbleX;
            float _WobbleZ;
            float _WobbleIntensity;
            float _FillAmount;
            float _WaveSpeed;
            float _WaveFrequency;
            float _WaveAmplitude;

            float _RefractionStrength;
            float _RefractionDistortion;

            fixed4 _BubbleColor;
            float _BubbleIntensity;
            float _BubbleSize;
            float _BubbleSpeed;
            float _BubbleDensity;

            fixed4 _FoamColor;
            float _FoamThickness;
            float _FoamNoise;

            float _LayerEnabled;
            fixed4 _Layer2Color;
            float _Layer2Height;
            float _LayerBlend;

            float _SpecularIntensity;
            float _SpecularPower;

            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;

            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float smoothNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float bubblePattern(float3 p, float time)
            {
                float2 uv = p.xz * _BubbleDensity;
                uv.y += time * _BubbleSpeed;

                float2 cell = floor(uv);
                float2 cellUV = frac(uv);

                float bubble = 0;
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 cellPos = cell + neighbor;

                        float2 bubblePos = noise(cellPos) * 0.5 + 0.25;
                        bubblePos += neighbor;
                        bubblePos.y += frac(time * _BubbleSpeed * (0.5 + noise(cellPos + 100))) * 2;

                        float dist = length(cellUV - bubblePos);
                        float size = (0.05 + noise(cellPos + 200) * 0.05) / _BubbleSize;
                        bubble = max(bubble, smoothstep(size, size * 0.1, dist));
                    }
                }

                float heightFade = smoothstep(0, 0.2, p.y) * smoothstep(1, 0.8, p.y);
                return bubble * heightFade;
            }

            v2f vert (appdata v)
            {
                v2f o;

                o.localPos = v.vertex.xyz;
                o.uv = v.uv;

                float wobbleAmount = _WobbleIntensity * _WaveAmplitude;
                float time = _Time.y * _WaveSpeed;
                float wave = sin(v.vertex.x * _WaveFrequency + time) * cos(v.vertex.z * _WaveFrequency + time * 0.7);

                float totalWobble = _WobbleX * v.vertex.x + _WobbleZ * v.vertex.z;
                totalWobble += wave * wobbleAmount;

                float topFactor = saturate((v.vertex.y + 0.5) * 2);
                v.vertex.y += totalWobble * topFactor * 0.5;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.grabPos = ComputeGrabScreenPos(o.pos);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float normalizedHeight = saturate((i.localPos.y + 0.5) * _FillAmount);

                // Base color with layering
                fixed4 liquidColor = _Color;
                if (_LayerEnabled > 0.5)
                {
                    float layerMix = smoothstep(_Layer2Height - _LayerBlend, _Layer2Height + _LayerBlend, normalizedHeight);
                    liquidColor = lerp(_Layer2Color, _Color, layerMix);
                }

                // Refraction
                float2 refractionOffset = i.worldNormal.xz * _RefractionStrength;
                refractionOffset += sin(_Time.y * 2 + i.worldPos.x * 3) * _RefractionDistortion * 0.01;
                float2 grabUV = i.grabPos.xy / i.grabPos.w + refractionOffset;
                fixed4 refractedColor = tex2D(_GrabTexture, grabUV);

                // Fresnel
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, i.worldNormal)), _FresnelPower);

                // Bubbles
                float bubbles = 0;
                if (_BubbleIntensity > 0)
                {
                    bubbles = bubblePattern(i.localPos, _Time.y) * _BubbleIntensity;
                }

                // Foam
                float foam = 0;
                if (_FoamThickness > 0)
                {
                    float foamStart = 1.0 - _FoamThickness;
                    if (normalizedHeight > foamStart)
                    {
                        float foamFactor = (normalizedHeight - foamStart) / _FoamThickness;
                        float foamNoise = smoothNoise(i.localPos.xz * 10 + _Time.y * 0.5) * _FoamNoise;
                        foam = saturate(foamFactor + foamNoise);
                    }
                }

                // Combine
                fixed4 finalColor = liquidColor;
                finalColor.rgb = lerp(finalColor.rgb, refractedColor.rgb * liquidColor.rgb, 0.3);
                finalColor.rgb = lerp(finalColor.rgb, _BubbleColor.rgb, bubbles * _BubbleColor.a);
                finalColor.rgb = lerp(finalColor.rgb, _FoamColor.rgb, foam * _FoamColor.a);

                // Fresnel
                float wobbleGlow = _WobbleIntensity * 0.3;
                finalColor.rgb += liquidColor.rgb * fresnel * (_FresnelIntensity + wobbleGlow);

                // Specular
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDir = normalize(lightDir + i.viewDir);
                float spec = pow(max(0, dot(i.worldNormal, halfDir)), _SpecularPower) * _SpecularIntensity;
                finalColor.rgb += spec;

                finalColor.a = liquidColor.a + foam * 0.3;

                return finalColor;
            }
            ENDCG
        }
    }

    FallBack "BarSimulator/Liquid"
}
