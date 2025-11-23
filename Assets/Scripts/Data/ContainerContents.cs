using System.Collections.Generic;
using UnityEngine;

namespace BarSimulator.Data
{
    /// <summary>
    /// 成分資料結構
    /// 參考: CocktailSystem.js containerContents
    /// </summary>
    [System.Serializable]
    public class Ingredient
    {
        [Tooltip("成分類型識別碼")]
        public string type;

        [Tooltip("成分中文名稱")]
        public string name;

        [Tooltip("成分英文顯示名稱")]
        public string displayName;

        [Tooltip("成分數量 (ml)")]
        public float amount;

        [Tooltip("成分顏色")]
        public Color color;

        /// <summary>
        /// 預設建構子
        /// </summary>
        public Ingredient() { }

        /// <summary>
        /// 完整建構子
        /// </summary>
        public Ingredient(string type, string name, string displayName, float amount, Color color)
        {
            this.type = type;
            this.name = name;
            this.displayName = displayName;
            this.amount = amount;
            this.color = color;
        }

        /// <summary>
        /// 從 LiquorData 建立 Ingredient
        /// </summary>
        public static Ingredient FromLiquor(LiquorData liquor, float amount)
        {
            return new Ingredient(
                liquor.id,
                liquor.nameZH,
                liquor.displayName,
                amount,
                liquor.color
            );
        }

        /// <summary>
        /// 複製成分
        /// </summary>
        public Ingredient Clone()
        {
            return new Ingredient(type, name, displayName, amount, color);
        }
    }

    /// <summary>
    /// 容器內容追蹤資料結構
    /// 參考: CocktailSystem.js containerContents Map
    /// </summary>
    [System.Serializable]
    public class ContainerContents
    {
        [Tooltip("成分列表")]
        public List<Ingredient> ingredients = new List<Ingredient>();

        [Tooltip("混合後顏色")]
        public Color mixedColor = Color.white;

        [Tooltip("當前容量 (ml)")]
        public float volume;

        [Tooltip("最大容量 (ml)")]
        public float maxVolume;

        /// <summary>
        /// 預設建構子
        /// </summary>
        public ContainerContents()
        {
            maxVolume = 300f;
        }

        /// <summary>
        /// 指定最大容量的建構子
        /// </summary>
        public ContainerContents(float maxVolume)
        {
            this.maxVolume = maxVolume;
        }

        /// <summary>
        /// 檢查容器是否已滿
        /// </summary>
        public bool IsFull => volume >= maxVolume;

        /// <summary>
        /// 檢查容器是否為空
        /// </summary>
        public bool IsEmpty => volume <= 0f || ingredients.Count == 0;

        /// <summary>
        /// 剩餘空間
        /// </summary>
        public float RemainingSpace => maxVolume - volume;

        /// <summary>
        /// 填充比例 (0-1)
        /// </summary>
        public float FillRatio => maxVolume > 0 ? Mathf.Clamp01(volume / maxVolume) : 0f;

        /// <summary>
        /// 添加成分（會合併同類型成分）
        /// 參考: CocktailSystem.js pour() Line 407-426
        /// </summary>
        public void AddIngredient(Ingredient ingredient)
        {
            // 檢查是否已有相同類型的成分
            var existing = ingredients.Find(i => i.type == ingredient.type);

            if (existing != null)
            {
                // 合併數量
                existing.amount += ingredient.amount;
            }
            else
            {
                // 添加新成分
                ingredients.Add(ingredient.Clone());
            }

            volume += ingredient.amount;

            // 更新混合顏色
            UpdateMixedColor();
        }

        /// <summary>
        /// 添加酒類
        /// </summary>
        public void AddLiquor(LiquorData liquor, float amount)
        {
            AddIngredient(Ingredient.FromLiquor(liquor, amount));
        }

        /// <summary>
        /// 更新混合顏色
        /// 參考: CocktailSystem.js updateMixedColor() Line 739-765
        /// </summary>
        public void UpdateMixedColor()
        {
            if (ingredients.Count == 0 || volume <= 0)
            {
                mixedColor = new Color(1f, 1f, 1f, 0f);
                return;
            }

            Vector4 rgba = Vector4.zero;
            float totalAmount = 0f;

            // 加權平均計算混合顏色（包含透明度）
            foreach (var ingredient in ingredients)
            {
                float weight = ingredient.amount;
                rgba.x += ingredient.color.r * weight;
                rgba.y += ingredient.color.g * weight;
                rgba.z += ingredient.color.b * weight;
                // 使用成分顏色的透明度，如果沒有則使用預設值
                float alpha = ingredient.color.a > 0 ? ingredient.color.a : 0.8f;
                rgba.w += alpha * weight;
                totalAmount += weight;
            }

            if (totalAmount > 0)
            {
                rgba /= totalAmount;
            }

            // 確保透明度在合理範圍內（0.6-0.9 for nice liquid appearance）
            float finalAlpha = Mathf.Clamp(rgba.w, 0.6f, 0.9f);
            mixedColor = new Color(rgba.x, rgba.y, rgba.z, finalAlpha);
        }

        /// <summary>
        /// 計算酒精濃度
        /// 參考: CocktailSystem.js calculateAlcoholContent() Line 1103-1121
        /// </summary>
        public float CalculateAlcoholContent(LiquorDatabase database)
        {
            if (database == null || volume <= 0) return 0f;

            float totalAlcohol = 0f;

            foreach (var ingredient in ingredients)
            {
                var liquor = database.GetLiquor(ingredient.type);
                if (liquor != null && liquor.alcoholContent > 0)
                {
                    totalAlcohol += ingredient.amount * (liquor.alcoholContent / 100f);
                }
            }

            return (totalAlcohol / volume) * 100f;
        }

        /// <summary>
        /// 清空容器
        /// 參考: CocktailSystem.js emptyContainer() Line 985-994
        /// </summary>
        public void Clear()
        {
            ingredients.Clear();
            volume = 0f;
            mixedColor = new Color(1f, 1f, 1f, 0f);
        }

        /// <summary>
        /// 取得成分類型列表
        /// </summary>
        public List<string> GetIngredientTypes()
        {
            var types = new List<string>();
            foreach (var ingredient in ingredients)
            {
                if (!types.Contains(ingredient.type))
                {
                    types.Add(ingredient.type);
                }
            }
            return types;
        }

        /// <summary>
        /// 取得特定成分的數量
        /// </summary>
        public float GetIngredientAmount(string type)
        {
            var ingredient = ingredients.Find(i => i.type == type);
            return ingredient?.amount ?? 0f;
        }

        /// <summary>
        /// 複製容器內容
        /// </summary>
        public ContainerContents Clone()
        {
            var clone = new ContainerContents(maxVolume)
            {
                volume = volume,
                mixedColor = mixedColor
            };

            foreach (var ingredient in ingredients)
            {
                clone.ingredients.Add(ingredient.Clone());
            }

            return clone;
        }
    }
}
