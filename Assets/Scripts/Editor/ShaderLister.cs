using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ShaderLister
{
    public static void ListShaders()
    {
        var shaderInfo = ShaderUtil.GetAllShaderInfo();
        Debug.Log($"Found {shaderInfo.Length} shaders.");
        foreach (var info in shaderInfo)
        {
            if (info.name.Contains("Universal Render Pipeline") || info.name.Contains("Skybox"))
            {
                Debug.Log("Shader: " + info.name);
            }
        }
    }
}
