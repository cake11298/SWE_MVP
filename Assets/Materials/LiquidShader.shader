Shader "BarSimulator/Liquid"
{
    Properties
    {
        _Color ("Liquid Color", Color) = (1, 1, 1, 0.8)
        _Glossiness ("Smoothness", Range(0, 1)) = 0.9
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 3
        _FresnelIntensity ("Fresnel Intensity", Range(0, 1)) = 0.5
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
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

        struct Input
        {
            float3 viewDir;
            float3 worldNormal;
        };

        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        half _FresnelPower;
        half _FresnelIntensity;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 基礎顏色
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            // Fresnel 效果（邊緣高亮）
            float fresnel = pow(1.0 - saturate(dot(IN.viewDir, IN.worldNormal)), _FresnelPower);
            o.Emission = _Color.rgb * fresnel * _FresnelIntensity;

            o.Alpha = _Color.a;
        }
        ENDCG

        // 渲染正面
        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

        struct Input
        {
            float3 viewDir;
            float3 worldNormal;
        };

        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        half _FresnelPower;
        half _FresnelIntensity;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            float fresnel = pow(1.0 - saturate(dot(IN.viewDir, IN.worldNormal)), _FresnelPower);
            o.Emission = _Color.rgb * fresnel * _FresnelIntensity;

            o.Alpha = _Color.a;
        }
        ENDCG
    }

    FallBack "Transparent/Diffuse"
}
