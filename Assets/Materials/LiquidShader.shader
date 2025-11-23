Shader "BarSimulator/Liquid"
{
    Properties
    {
        _Color ("Liquid Color", Color) = (1, 1, 1, 0.8)
        _Glossiness ("Smoothness", Range(0, 1)) = 0.9
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 3
        _FresnelIntensity ("Fresnel Intensity", Range(0, 1)) = 0.5

        // 波動效果屬性
        _WobbleX ("Wobble X", Float) = 0
        _WobbleZ ("Wobble Z", Float) = 0
        _WobbleIntensity ("Wobble Intensity", Range(0, 1)) = 0
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5

        // 波紋效果
        _WaveSpeed ("Wave Speed", Float) = 2
        _WaveFrequency ("Wave Frequency", Float) = 3
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        LOD 200

        // 渲染背面
        Cull Front
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade vertex:vert
        #pragma target 3.0

        struct Input
        {
            float3 viewDir;
            float3 worldNormal;
            float3 worldPos;
            float3 localPos;
        };

        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        half _FresnelPower;
        half _FresnelIntensity;

        // 波動變數
        float _WobbleX;
        float _WobbleZ;
        float _WobbleIntensity;
        float _FillAmount;
        float _WaveSpeed;
        float _WaveFrequency;
        float _WaveAmplitude;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            // 儲存本地位置用於波動計算
            o.localPos = v.vertex.xyz;

            // 計算波動效果
            float wobbleAmount = _WobbleIntensity * _WaveAmplitude;

            // 基於時間的波紋
            float time = _Time.y * _WaveSpeed;
            float wave = sin(v.vertex.x * _WaveFrequency + time) * cos(v.vertex.z * _WaveFrequency + time * 0.7);

            // 結合手動波動和自然波紋
            float totalWobble = _WobbleX * v.vertex.x + _WobbleZ * v.vertex.z;
            totalWobble += wave * wobbleAmount;

            // 只影響頂部頂點（Y 軸較高的部分）
            float topFactor = saturate((v.vertex.y + 0.5) * 2);
            v.vertex.y += totalWobble * topFactor * 0.5;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 基礎顏色
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            // Fresnel 效果（邊緣高亮）
            float fresnel = pow(1.0 - saturate(dot(IN.viewDir, IN.worldNormal)), _FresnelPower);

            // 添加基於波動的發光變化
            float wobbleGlow = _WobbleIntensity * 0.3;
            o.Emission = _Color.rgb * fresnel * (_FresnelIntensity + wobbleGlow);

            o.Alpha = _Color.a;
        }
        ENDCG

        // 渲染正面
        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade vertex:vert
        #pragma target 3.0

        struct Input
        {
            float3 viewDir;
            float3 worldNormal;
            float3 worldPos;
            float3 localPos;
        };

        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        half _FresnelPower;
        half _FresnelIntensity;

        // 波動變數
        float _WobbleX;
        float _WobbleZ;
        float _WobbleIntensity;
        float _FillAmount;
        float _WaveSpeed;
        float _WaveFrequency;
        float _WaveAmplitude;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            // 儲存本地位置用於波動計算
            o.localPos = v.vertex.xyz;

            // 計算波動效果
            float wobbleAmount = _WobbleIntensity * _WaveAmplitude;

            // 基於時間的波紋
            float time = _Time.y * _WaveSpeed;
            float wave = sin(v.vertex.x * _WaveFrequency + time) * cos(v.vertex.z * _WaveFrequency + time * 0.7);

            // 結合手動波動和自然波紋
            float totalWobble = _WobbleX * v.vertex.x + _WobbleZ * v.vertex.z;
            totalWobble += wave * wobbleAmount;

            // 只影響頂部頂點（Y 軸較高的部分）
            float topFactor = saturate((v.vertex.y + 0.5) * 2);
            v.vertex.y += totalWobble * topFactor * 0.5;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            float fresnel = pow(1.0 - saturate(dot(IN.viewDir, IN.worldNormal)), _FresnelPower);

            // 添加基於波動的發光變化
            float wobbleGlow = _WobbleIntensity * 0.3;
            o.Emission = _Color.rgb * fresnel * (_FresnelIntensity + wobbleGlow);

            o.Alpha = _Color.a;
        }
        ENDCG
    }

    FallBack "Transparent/Diffuse"
}
