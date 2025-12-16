using UnityEngine;
using System.Collections.Generic;

namespace BarSimulator.Player
{
    /// <summary>
    /// 为可互动物品添加高亮效果
    /// </summary>
    public class InteractionHighlight : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.3f); // Yellow with transparency
        [SerializeField] private float highlightIntensity = 1.5f;
        
        private GameObject currentHighlightedObject;
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Renderer, Material[]> highlightMaterials = new Dictionary<Renderer, Material[]>();

        /// <summary>
        /// 高亮指定的物件
        /// </summary>
        public void HighlightObject(GameObject obj)
        {
            if (obj == currentHighlightedObject)
                return;

            // 清除之前的高亮
            ClearHighlight();

            if (obj == null)
                return;

            currentHighlightedObject = obj;

            // 获取所有 Renderer 组件（包括子物件）
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                // 保存原始材质
                Material[] originalMats = renderer.materials;
                originalMaterials[renderer] = originalMats;

                // 创建高亮材质
                Material[] highlightMats = new Material[originalMats.Length];
                for (int i = 0; i < originalMats.Length; i++)
                {
                    highlightMats[i] = new Material(originalMats[i]);
                    
                    // 增加发光效果
                    if (highlightMats[i].HasProperty("_EmissionColor"))
                    {
                        highlightMats[i].EnableKeyword("_EMISSION");
                        highlightMats[i].SetColor("_EmissionColor", highlightColor * highlightIntensity);
                    }
                    
                    // 如果材质支持颜色，稍微调整颜色
                    if (highlightMats[i].HasProperty("_Color"))
                    {
                        Color originalColor = highlightMats[i].GetColor("_Color");
                        highlightMats[i].SetColor("_Color", Color.Lerp(originalColor, highlightColor, 0.3f));
                    }
                }

                highlightMaterials[renderer] = highlightMats;
                renderer.materials = highlightMats;
            }
        }

        /// <summary>
        /// 清除高亮效果
        /// </summary>
        public void ClearHighlight()
        {
            if (currentHighlightedObject == null)
                return;

            // 恢复原始材质
            foreach (var kvp in originalMaterials)
            {
                Renderer renderer = kvp.Key;
                if (renderer != null)
                {
                    renderer.materials = kvp.Value;
                }
            }

            // 清理高亮材质
            foreach (var kvp in highlightMaterials)
            {
                Material[] mats = kvp.Value;
                foreach (Material mat in mats)
                {
                    if (mat != null)
                    {
                        Destroy(mat);
                    }
                }
            }

            originalMaterials.Clear();
            highlightMaterials.Clear();
            currentHighlightedObject = null;
        }

        private void OnDestroy()
        {
            ClearHighlight();
        }
    }
}
