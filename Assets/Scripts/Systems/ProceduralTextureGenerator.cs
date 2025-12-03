using UnityEngine;

namespace BarSimulator.Systems
{
    /// <summary>
    /// 程序化紋理生成器 - 使用 Perlin Noise 算法生成 PBR 基礎貼圖
    /// 解決 GameObject.CreatePrimitive 紫色材質問題
    /// </summary>
    public static class ProceduralTextureGenerator
    {
        #region 木紋紋理 (Wood Texture)

        /// <summary>
        /// 生成木紋紋理 - 深淺相間的木紋效果
        /// </summary>
        /// <param name="width">紋理寬度</param>
        /// <param name="height">紋理高度</param>
        /// <param name="baseColor">基礎顏色（深色木材）</param>
        /// <param name="grainColor">木紋顏色（淺色木材）</param>
        /// <param name="grainScale">木紋密度（越小越密集）</param>
        /// <param name="grainStrength">木紋強度（0-1）</param>
        /// <returns>生成的木紋紋理</returns>
        public static Texture2D GenerateWoodTexture(
            int width = 512,
            int height = 512,
            Color? baseColor = null,
            Color? grainColor = null,
            float grainScale = 0.05f,
            float grainStrength = 0.4f)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, true);

            // 預設顏色：深褐色木材
            Color base_c = baseColor ?? new Color(0.3f, 0.15f, 0.05f);
            Color grain_c = grainColor ?? new Color(0.5f, 0.25f, 0.1f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 使用 Perlin Noise 生成橫向木紋（主要沿 X 軸變化）
                    float xCoord = x * grainScale;
                    float yCoord = y * grainScale * 0.2f; // Y 軸變化較小，形成橫向紋理

                    float noise = Mathf.PerlinNoise(xCoord, yCoord);

                    // 添加第二層細節噪聲
                    float detailNoise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.3f;
                    noise = Mathf.Clamp01(noise + detailNoise);

                    // 使用正弦函數強化木紋條紋效果
                    float woodPattern = Mathf.Abs(Mathf.Sin(noise * Mathf.PI * 8f));

                    // 混合基礎色和木紋色
                    Color pixelColor = Color.Lerp(base_c, grain_c, woodPattern * grainStrength);

                    // 添加隨機變化增加真實感
                    float randomVariation = Random.Range(-0.05f, 0.05f);
                    pixelColor.r = Mathf.Clamp01(pixelColor.r + randomVariation);
                    pixelColor.g = Mathf.Clamp01(pixelColor.g + randomVariation);
                    pixelColor.b = Mathf.Clamp01(pixelColor.b + randomVariation);

                    texture.SetPixel(x, y, pixelColor);
                }
            }

            texture.Apply();
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.name = "ProceduralWood";

            return texture;
        }

        #endregion

        #region 地磚紋理 (Tile Texture)

        /// <summary>
        /// 生成地磚紋理 - 有縫隙的棋盤格效果
        /// </summary>
        /// <param name="width">紋理寬度</param>
        /// <param name="height">紋理高度</param>
        /// <param name="tileColor">地磚顏色</param>
        /// <param name="groutColor">縫隙顏色</param>
        /// <param name="tileSize">地磚大小（像素）</param>
        /// <param name="groutSize">縫隙寬度（像素）</param>
        /// <returns>生成的地磚紋理</returns>
        public static Texture2D GenerateTileTexture(
            int width = 512,
            int height = 512,
            Color? tileColor = null,
            Color? groutColor = null,
            int tileSize = 128,
            int groutSize = 4)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, true);

            // 預設顏色：淺灰色地磚，深灰色縫隙
            Color tile_c = tileColor ?? new Color(0.85f, 0.85f, 0.8f);
            Color grout_c = groutColor ?? new Color(0.3f, 0.3f, 0.3f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 計算當前像素在地磚格子中的位置
                    int tileX = x % tileSize;
                    int tileY = y % tileSize;

                    bool isGrout = (tileX < groutSize || tileY < groutSize);

                    if (isGrout)
                    {
                        // 縫隙區域
                        texture.SetPixel(x, y, grout_c);
                    }
                    else
                    {
                        // 地磚區域 - 添加細微的 Perlin Noise 變化
                        float noise = Mathf.PerlinNoise(x * 0.02f, y * 0.02f);
                        Color pixelColor = tile_c;

                        // 輕微的顏色變化（模擬石材紋理）
                        float variation = (noise - 0.5f) * 0.1f;
                        pixelColor.r = Mathf.Clamp01(pixelColor.r + variation);
                        pixelColor.g = Mathf.Clamp01(pixelColor.g + variation);
                        pixelColor.b = Mathf.Clamp01(pixelColor.b + variation);

                        texture.SetPixel(x, y, pixelColor);
                    }
                }
            }

            texture.Apply();
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.name = "ProceduralTile";

            return texture;
        }

        #endregion

        #region 金屬紋理 (Metal Texture)

        /// <summary>
        /// 生成金屬紋理 - 帶有雜訊的灰色金屬效果
        /// </summary>
        /// <param name="width">紋理寬度</param>
        /// <param name="height">紋理高度</param>
        /// <param name="baseColor">基礎金屬顏色</param>
        /// <param name="roughness">粗糙度（0-1，越高越粗糙）</param>
        /// <param name="brushedEffect">是否啟用拉絲效果</param>
        /// <returns>生成的金屬紋理</returns>
        public static Texture2D GenerateMetalTexture(
            int width = 512,
            int height = 512,
            Color? baseColor = null,
            float roughness = 0.3f,
            bool brushedEffect = true)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, true);

            // 預設顏色：不銹鋼灰色
            Color base_c = baseColor ?? new Color(0.7f, 0.7f, 0.7f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = base_c;

                    if (brushedEffect)
                    {
                        // 拉絲金屬效果（橫向條紋）
                        float brushNoise = Mathf.PerlinNoise(x * 0.5f, y * 0.01f);
                        float brushPattern = Mathf.Abs(Mathf.Sin(brushNoise * Mathf.PI * 50f));

                        // 添加拉絲紋理
                        float brushIntensity = brushPattern * 0.15f;
                        pixelColor.r = Mathf.Clamp01(pixelColor.r + brushIntensity);
                        pixelColor.g = Mathf.Clamp01(pixelColor.g + brushIntensity);
                        pixelColor.b = Mathf.Clamp01(pixelColor.b + brushIntensity);
                    }

                    // 添加粗糙度噪聲
                    float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                    float variation = (noise - 0.5f) * roughness * 0.2f;

                    pixelColor.r = Mathf.Clamp01(pixelColor.r + variation);
                    pixelColor.g = Mathf.Clamp01(pixelColor.g + variation);
                    pixelColor.b = Mathf.Clamp01(pixelColor.b + variation);

                    // 添加細微的隨機噪點（模擬金屬表面瑕疵）
                    if (Random.value < 0.01f)
                    {
                        pixelColor *= Random.Range(0.85f, 1.15f);
                    }

                    texture.SetPixel(x, y, pixelColor);
                }
            }

            texture.Apply();
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.name = "ProceduralMetal";

            return texture;
        }

        #endregion

        #region 玻璃紋理 (Glass Texture)

        /// <summary>
        /// 生成玻璃紋理 - 半透明、帶有邊緣高光的效果
        /// 註：此紋理主要用於 Albedo，透明度需在 Material 中設定
        /// </summary>
        /// <param name="width">紋理寬度</param>
        /// <param name="height">紋理高度</param>
        /// <param name="tintColor">玻璃著色</param>
        /// <param name="clarity">清晰度（0-1，越高越清澈）</param>
        /// <returns>生成的玻璃紋理</returns>
        public static Texture2D GenerateGlassTexture(
            int width = 512,
            int height = 512,
            Color? tintColor = null,
            float clarity = 0.95f)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, true);

            // 預設顏色：輕微藍色調的透明玻璃
            Color tint_c = tintColor ?? new Color(0.95f, 0.98f, 1f, 0.1f);

            // 計算紋理中心點
            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
            float maxDistance = Mathf.Sqrt(center.x * center.x + center.y * center.y);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = tint_c;

                    // 計算距離中心的距離（用於邊緣高光）
                    Vector2 pos = new Vector2(x, y);
                    float distanceFromCenter = Vector2.Distance(pos, center);
                    float normalizedDistance = distanceFromCenter / maxDistance;

                    // 邊緣高光效果（Fresnel 近似）
                    float edgeHighlight = Mathf.Pow(normalizedDistance, 2f) * 0.3f;

                    // 添加極細微的噪聲（模擬玻璃內部的微小氣泡或瑕疵）
                    float noise = Mathf.PerlinNoise(x * 0.02f, y * 0.02f);
                    float imperfection = (1f - clarity) * (noise - 0.5f) * 0.05f;

                    pixelColor.r = Mathf.Clamp01(pixelColor.r + edgeHighlight + imperfection);
                    pixelColor.g = Mathf.Clamp01(pixelColor.g + edgeHighlight + imperfection);
                    pixelColor.b = Mathf.Clamp01(pixelColor.b + edgeHighlight + imperfection);

                    // 玻璃的透明度根據清晰度調整
                    pixelColor.a = tint_c.a * clarity;

                    // 極少數像素模擬氣泡
                    if (Random.value < 0.0005f)
                    {
                        pixelColor.a *= 0.5f;
                    }

                    texture.SetPixel(x, y, pixelColor);
                }
            }

            texture.Apply();
            texture.wrapMode = TextureWrapMode.Clamp; // 玻璃通常不重複
            texture.name = "ProceduralGlass";

            return texture;
        }

        #endregion

        #region 法線貼圖生成 (Normal Map Generation)

        /// <summary>
        /// 從灰階高度圖生成法線貼圖
        /// </summary>
        /// <param name="heightMap">高度圖紋理</param>
        /// <param name="strength">法線強度</param>
        /// <returns>生成的法線貼圖</returns>
        public static Texture2D GenerateNormalMap(Texture2D heightMap, float strength = 1f)
        {
            int width = heightMap.width;
            int height = heightMap.height;

            Texture2D normalMap = new Texture2D(width, height, TextureFormat.RGB24, true);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Sobel 濾波器計算梯度
                    float tl = heightMap.GetPixel(x - 1, y + 1).grayscale;
                    float t = heightMap.GetPixel(x, y + 1).grayscale;
                    float tr = heightMap.GetPixel(x + 1, y + 1).grayscale;
                    float l = heightMap.GetPixel(x - 1, y).grayscale;
                    float r = heightMap.GetPixel(x + 1, y).grayscale;
                    float bl = heightMap.GetPixel(x - 1, y - 1).grayscale;
                    float b = heightMap.GetPixel(x, y - 1).grayscale;
                    float br = heightMap.GetPixel(x + 1, y - 1).grayscale;

                    // 計算 X 和 Y 方向的梯度
                    float dx = (tr + 2f * r + br) - (tl + 2f * l + bl);
                    float dy = (bl + 2f * b + br) - (tl + 2f * t + tr);

                    // 構建法線向量
                    Vector3 normal = new Vector3(-dx * strength, -dy * strength, 1f).normalized;

                    // 轉換到 0-1 範圍（法線貼圖格式）
                    Color normalColor = new Color(
                        normal.x * 0.5f + 0.5f,
                        normal.y * 0.5f + 0.5f,
                        normal.z * 0.5f + 0.5f
                    );

                    normalMap.SetPixel(x, y, normalColor);
                }
            }

            normalMap.Apply();
            normalMap.name = "ProceduralNormal";

            return normalMap;
        }

        #endregion

        #region 工具方法 (Utility Methods)

        /// <summary>
        /// 生成簡單的棋盤格測試紋理
        /// </summary>
        public static Texture2D GenerateCheckerboard(int size = 512, int checkerSize = 64)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGB24, false);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isWhite = ((x / checkerSize) + (y / checkerSize)) % 2 == 0;
                    Color color = isWhite ? Color.white : Color.black;
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.name = "Checkerboard";

            return texture;
        }

        /// <summary>
        /// 生成純色紋理
        /// </summary>
        public static Texture2D GenerateSolidColor(Color color, int size = 64)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGB24, false);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            texture.name = "SolidColor";

            return texture;
        }

        #endregion
    }
}
