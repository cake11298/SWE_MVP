using UnityEditor;
using UnityEngine;

public class UpgradeMaterialsRunner
{
    public static void Execute()
    {
        URPMaterialUpgrader.UpgradeMaterials();
    }
}
