using UnityEngine;

namespace BarSimulator.Data
{
    /// <summary>
    /// 配方成分資料
    /// </summary>
    [System.Serializable]
    public class RecipeIngredient
    {
        [Tooltip("成分名稱")]
        public string name;

        [Tooltip("數量描述 (如: 60ml, 適量)")]
        public string amount;

        /// <summary>
        /// 預設建構子
        /// </summary>
        public RecipeIngredient() { }

        /// <summary>
        /// 完整建構子
        /// </summary>
        public RecipeIngredient(string amount, string name)
        {
            this.amount = amount;
            this.name = name;
        }
    }

    /// <summary>
    /// 雞尾酒配方資料
    /// 參考: CocktailSystem.js getCocktailRecipes() Line 1176-1443
    /// </summary>
    [System.Serializable]
    public class RecipeData
    {
        [Tooltip("雞尾酒名稱")]
        public string name;

        [Tooltip("配方成分")]
        public RecipeIngredient[] ingredients;

        [Tooltip("調製方法")]
        [TextArea(2, 5)]
        public string method;

        /// <summary>
        /// 預設建構子
        /// </summary>
        public RecipeData() { }

        /// <summary>
        /// 完整建構子
        /// </summary>
        public RecipeData(string name, RecipeIngredient[] ingredients, string method)
        {
            this.name = name;
            this.ingredients = ingredients;
            this.method = method;
        }
    }

    /// <summary>
    /// 配方資料庫 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Bar/Recipe Database")]
    public class RecipeDatabase : ScriptableObject
    {
        [Tooltip("所有配方")]
        public RecipeData[] recipes;

        /// <summary>
        /// 根據名稱取得配方
        /// </summary>
        public RecipeData GetRecipe(string name)
        {
            if (recipes == null) return null;

            foreach (var recipe in recipes)
            {
                if (recipe.name.Contains(name))
                    return recipe;
            }
            return null;
        }

        /// <summary>
        /// 初始化預設配方
        /// 參考: CocktailSystem.js getCocktailRecipes()
        /// </summary>
        public void InitializeDefaults()
        {
            recipes = new RecipeData[]
            {
                // Martini
                new RecipeData(
                    "Martini 馬丁尼",
                    new RecipeIngredient[]
                    {
                        new RecipeIngredient("60ml", "琴酒 Gin"),
                        new RecipeIngredient("10ml", "不甜香艾酒 Dry Vermouth")
                    },
                    "Stir（攪拌法）：將材料加冰攪拌後濾入冰鎮馬丁尼杯，可加檸檬皮裝飾。"
                ),

                // Negroni
                new RecipeData(
                    "Negroni 內格羅尼",
                    new RecipeIngredient[]
                    {
                        new RecipeIngredient("30ml", "琴酒 Gin"),
                        new RecipeIngredient("30ml", "金巴利 Campari"),
                        new RecipeIngredient("30ml", "甜香艾酒 Sweet Vermouth")
                    },
                    "Build：將材料倒入裝滿冰塊的古典杯，攪拌均勻，柳橙皮裝飾。"
                ),

                // Margarita
                new RecipeData(
                    "Margarita 瑪格麗特",
                    new RecipeIngredient[]
                    {
                        new RecipeIngredient("50ml", "龍舌蘭 Tequila"),
                        new RecipeIngredient("20ml", "橙皮酒 Triple Sec"),
                        new RecipeIngredient("15ml", "萊姆汁 Lime Juice")
                    },
                    "Shake：加冰搖盪後濾入杯緣抹鹽的杯中，萊姆角裝飾。"
                ),

                // Daiquiri
                new RecipeData(
                    "Daiquiri 黛克瑞",
                    new RecipeIngredient[]
                    {
                        new RecipeIngredient("60ml", "蘭姆酒 Rum"),
                        new RecipeIngredient("20ml", "萊姆汁 Lime Juice"),
                        new RecipeIngredient("10ml", "糖漿 Simple Syrup")
                    },
                    "Shake：加冰搖盪後濾入冰鎮雞尾酒杯。"
                ),

                // Cosmopolitan
                new RecipeData(
                    "Cosmopolitan 柯夢波丹",
                    new RecipeIngredient[]
                    {
                        new RecipeIngredient("40ml", "伏特加 Vodka"),
                        new RecipeIngredient("15ml", "橙皮酒 Triple Sec"),
                        new RecipeIngredient("15ml", "萊姆汁 Lime Juice"),
                        new RecipeIngredient("30ml", "蔓越莓汁 Cranberry Juice")
                    },
                    "Shake：加冰搖盪後濾入馬丁尼杯，萊姆皮或蔓越莓裝飾。"
                ),

                // Mojito
                new RecipeData(
                    "Mojito 莫希托",
                    new RecipeIngredient[]
                    {
                        new RecipeIngredient("45ml", "蘭姆酒 Rum"),
                        new RecipeIngredient("20ml", "萊姆汁 Lime Juice"),
                        new RecipeIngredient("20ml", "糖漿 Simple Syrup"),
                        new RecipeIngredient("適量", "蘇打水 Soda Water")
                    },
                    "Muddle：在杯中壓碎薄荷葉與糖，加冰、蘭姆酒、萊姆汁，上方加蘇打水。"
                ),

                // Piña Colada
                new RecipeData(
                    "Piña Colada 椰林風情",
                    new RecipeIngredient[]
                    {
                        new RecipeIngredient("50ml", "蘭姆酒 Rum"),
                        new RecipeIngredient("30ml", "椰漿 Coconut Cream"),
                        new RecipeIngredient("50ml", "鳳梨汁 Pineapple Juice")
                    },
                    "Blend：與碎冰混合打碎，倒入颶風杯，鳳梨角和櫻桃裝飾。"
                ),

                // Whiskey Sour
                new RecipeData(
                    "Whiskey Sour 威士忌酸酒",
                    new RecipeIngredient[]
                    {
                        new RecipeIngredient("50ml", "威士忌 Whiskey"),
                        new RecipeIngredient("25ml", "檸檬汁 Lemon Juice"),
                        new RecipeIngredient("15ml", "糖漿 Simple Syrup")
                    },
                    "Shake：加冰搖盪後濾入古典杯，可加蛋白增加口感。"
                )
            };
        }
    }
}
