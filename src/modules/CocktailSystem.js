import * as THREE from 'three';

/**
 * 調酒系統 - 處理倒酒、搖酒、喝酒等調酒邏輯
 */
export default class CocktailSystem {
    constructor(scene, interactionSystem) {
        this.scene = scene;
        this.interaction = interactionSystem;

        // 容器內容追蹤 { container -> { ingredients: [], color, volume } }
        this.containerContents = new Map();

        // 倒酒狀態
        this.isPouringActive = false;
        this.pouringStartTime = 0;
        this.currentPouringBottle = null; // 當前倒酒的酒瓶
        this.originalBottleRotation = null; // 酒瓶原始旋轉
        this.currentPouringAmount = 0; // 當前這次倒酒的累積量
        this.pourProgressHideTimer = null; // 進度條隱藏計時器

        // 喝酒狀態
        this.isDrinking = false;
        this.drinkingStartTime = 0;
        this.currentDrinkingGlass = null; // 當前喝酒的杯子
        this.originalGlassPosition = null; // 杯子原始位置
        this.lastDrinkInfo = null; // 最後一次喝酒的資訊

        // 搖酒狀態
        this.isShakingActive = false;
        this.shakeIntensity = 0;
        this.shakeTime = 0;

        // 粒子系統（用於倒酒效果）
        this.particleSystems = new Map();

        // 酒類資料庫
        this.liquorDatabase = this.initLiquorDatabase();

        // UI 元素：倒酒進度條
        this.pourProgressPanel = document.getElementById('pour-progress-panel');
        this.containerVolumeBar = document.getElementById('container-volume-bar');
        this.containerVolumeText = document.getElementById('container-volume-text');
        this.pourRateBar = document.getElementById('pour-rate-bar');
        this.pourRateText = document.getElementById('pour-rate-text');

        // 倒酒速度（ml/秒）
        this.pourRate = 30; // 調慢速度，讓倒酒更精確
    }

    /**
     * 初始化酒類資料庫
     * @returns {Map} 酒類資料
     */
    initLiquorDatabase() {
        const database = new Map();

        // === 六大基酒 ===
        database.set('vodka', {
            name: '伏特加',
            displayName: 'Vodka',
            color: 0xf0f0f0,
            alcoholContent: 40,
            category: 'base_spirit'
        });

        database.set('gin', {
            name: '琴酒',
            displayName: 'Gin',
            color: 0xe8f4f8,
            alcoholContent: 40,
            category: 'base_spirit'
        });

        database.set('rum', {
            name: '蘭姆酒',
            displayName: 'Rum',
            color: 0xd4a574,
            alcoholContent: 40,
            category: 'base_spirit'
        });

        database.set('whiskey', {
            name: '威士忌',
            displayName: 'Whiskey',
            color: 0xb87333,
            alcoholContent: 40,
            category: 'base_spirit'
        });

        database.set('tequila', {
            name: '龍舌蘭',
            displayName: 'Tequila',
            color: 0xf5deb3,
            alcoholContent: 40,
            category: 'base_spirit'
        });

        database.set('brandy', {
            name: '白蘭地',
            displayName: 'Brandy',
            color: 0x8b4513,
            alcoholContent: 40,
            category: 'base_spirit'
        });

        // === 調味料 ===
        database.set('lemon_juice', {
            name: '檸檬汁',
            displayName: 'Lemon Juice',
            color: 0xfff44f,
            alcoholContent: 0,
            category: 'mixer'
        });

        database.set('lime_juice', {
            name: '萊姆汁',
            displayName: 'Lime Juice',
            color: 0x32cd32,
            alcoholContent: 0,
            category: 'mixer'
        });

        database.set('simple_syrup', {
            name: '糖漿',
            displayName: 'Simple Syrup',
            color: 0xffe4b5,
            alcoholContent: 0,
            category: 'mixer'
        });

        database.set('grenadine', {
            name: '紅石榴糖漿',
            displayName: 'Grenadine',
            color: 0xff0000,
            alcoholContent: 0,
            category: 'mixer'
        });

        database.set('angostura_bitters', {
            name: '安格仕苦精',
            displayName: 'Angostura Bitters',
            color: 0x8b0000,
            alcoholContent: 44.7,
            category: 'mixer'
        });

        // === 果汁類 ===
        database.set('orange_juice', {
            name: '柳橙汁',
            displayName: 'Orange Juice',
            color: 0xffa500,
            alcoholContent: 0,
            category: 'juice'
        });

        database.set('pineapple_juice', {
            name: '鳳梨汁',
            displayName: 'Pineapple Juice',
            color: 0xffeb3b,
            alcoholContent: 0,
            category: 'juice'
        });

        database.set('cranberry_juice', {
            name: '蔓越莓汁',
            displayName: 'Cranberry Juice',
            color: 0xdc143c,
            alcoholContent: 0,
            category: 'juice'
        });

        database.set('tomato_juice', {
            name: '番茄汁',
            displayName: 'Tomato Juice',
            color: 0xff6347,
            alcoholContent: 0,
            category: 'juice'
        });

        database.set('grapefruit_juice', {
            name: '葡萄柚汁',
            displayName: 'Grapefruit Juice',
            color: 0xff69b4,
            alcoholContent: 0,
            category: 'juice'
        });

        // === 其他常見材料 ===
        database.set('soda_water', {
            name: '蘇打水',
            displayName: 'Soda Water',
            color: 0xe0ffff,
            alcoholContent: 0,
            category: 'mixer'
        });

        database.set('tonic_water', {
            name: '通寧水',
            displayName: 'Tonic Water',
            color: 0xf0ffff,
            alcoholContent: 0,
            category: 'mixer'
        });

        database.set('cola', {
            name: '可樂',
            displayName: 'Cola',
            color: 0x3e2723,
            alcoholContent: 0,
            category: 'mixer'
        });

        database.set('liqueur', {
            name: '利口酒',
            displayName: 'Liqueur',
            color: 0xff6b9d,
            alcoholContent: 20,
            category: 'liqueur'
        });

        // === 利口酒和香艾酒類 ===
        database.set('vermouth_dry', {
            name: '不甜香艾酒',
            displayName: 'Dry Vermouth',
            color: 0xe8e8d0,
            alcoholContent: 18,
            category: 'fortified_wine'
        });

        database.set('vermouth_sweet', {
            name: '甜香艾酒',
            displayName: 'Sweet Vermouth',
            color: 0x8b4513,
            alcoholContent: 18,
            category: 'fortified_wine'
        });

        database.set('campari', {
            name: '金巴利',
            displayName: 'Campari',
            color: 0xdc143c,
            alcoholContent: 25,
            category: 'liqueur'
        });

        database.set('triple_sec', {
            name: '橙皮酒',
            displayName: 'Triple Sec',
            color: 0xffa500,
            alcoholContent: 40,
            category: 'liqueur'
        });

        database.set('coconut_cream', {
            name: '椰漿',
            displayName: 'Coconut Cream',
            color: 0xfffaf0,
            alcoholContent: 0,
            category: 'mixer'
        });

        return database;
    }

    /**
     * 初始化容器（杯子、Shaker）
     * @param {THREE.Object3D} container - 容器物件
     * @param {number} maxVolume - 最大容量（ml）
     */
    initContainer(container, maxVolume = 300) {
        this.containerContents.set(container, {
            ingredients: [],
            color: 0xffffff,
            volume: 0,
            maxVolume: maxVolume,
            liquidMesh: null
        });

        // 創建液體視覺效果
        this.createLiquidVisual(container);
    }

    /**
     * 創建液體視覺效果
     * @param {THREE.Object3D} container - 容器物件
     */
    createLiquidVisual(container) {
        const contents = this.containerContents.get(container);
        if (!contents) return;

        // 創建液體網格 - 使用圓柱體
        // 根據杯子結構：glassBase 在 y=0.04，高度 0.08，頂部在 y=0.08
        // glassBody 從 y=0 開始，高度 0.65
        // 所以液體應該從 y≈0.08 開始填充
        const liquidGeometry = new THREE.CylinderGeometry(0.15, 0.15, 0.01, 32); // 初始高度很小
        const liquidMaterial = new THREE.MeshPhongMaterial({
            color: contents.color,
            transparent: true,
            opacity: 0.8, // 提高不透明度
            shininess: 100,
            side: THREE.DoubleSide,
            reflectivity: 0.4
        });

        const liquidMesh = new THREE.Mesh(liquidGeometry, liquidMaterial);
        // 將液體放置在杯底上方（從杯底頂部開始）
        liquidMesh.position.set(0, 0.08, 0);
        liquidMesh.visible = false; // 初始隱藏

        container.add(liquidMesh);
        contents.liquidMesh = liquidMesh;
    }

    /**
     * 更新液體視覺效果
     * @param {THREE.Object3D} container - 容器物件
     */
    updateLiquidVisual(container) {
        const contents = this.containerContents.get(container);
        if (!contents || !contents.liquidMesh) return;

        const fillRatio = contents.volume / contents.maxVolume;

        if (fillRatio > 0) {
            // 顯示液體
            contents.liquidMesh.visible = true;

            // 更新高度(根據容量) - 使用實際的圓柱體高度而不是拉伸
            const maxHeight = 0.55; // 杯子可用高度（稍小於實際高度避免溢出）
            const liquidHeight = Math.max(0.01, Math.min(maxHeight * fillRatio, maxHeight)); // 確保有最小高度且不超過杯子高度

            // 計算梯形柱體的半徑(底部大,頂部小,模擬液體表面張力)
            // 隨著液體增加,頂部半徑接近底部半徑
            const bottomRadius = 0.14; // 底部半徑（稍小於杯子以避免穿模）
            const topRadius = 0.12 + (fillRatio * 0.02); // 頂部半徑隨液體量增加而增大

            // 重新創建幾何體以獲得正確的橢圓梯形柱體(避免拉伸變形)
            const newGeometry = new THREE.CylinderGeometry(
                topRadius,      // 頂部半徑
                bottomRadius,   // 底部半徑
                liquidHeight,   // 高度
                32              // 段數(更平滑)
            );
            contents.liquidMesh.geometry.dispose(); // 釋放舊幾何體
            contents.liquidMesh.geometry = newGeometry;

            // 定位液體(底部對齊杯座頂部，從 y=0.08 開始往上填充)
            // 液體底部從 glassBase 頂部 (y=0.08) 開始，液體高度的一半就是中心點
            contents.liquidMesh.position.y = 0.08 + liquidHeight / 2;

            // 更新顏色
            contents.liquidMesh.material.color.setHex(contents.color);
        } else {
            // 隱藏液體
            contents.liquidMesh.visible = false;
        }
    }

    /**
     * 倒酒（從酒瓶到容器）
     * @param {THREE.Object3D} bottle - 酒瓶
     * @param {THREE.Object3D} targetContainer - 目標容器
     * @param {string} liquorType - 酒類類型
     * @param {number} deltaTime - 時間增量
     * @param {THREE.Camera} camera - 相機（可選，用於視角檢測）
     */
    pour(bottle, targetContainer, liquorType, deltaTime, camera = null) {
        const contents = this.containerContents.get(targetContainer);
        if (!contents) return;

        // 檢查容器是否已滿
        if (contents.volume >= contents.maxVolume) {
            console.log('容器已滿！');
            return;
        }

        // 如果提供了相機，檢查距離和視角
        if (camera) {
            const distance = bottle.position.distanceTo(targetContainer.position);

            // 距離必須小於 1.5 米
            if (distance > 1.5) {
                return;
            }

            // 計算相機到杯子的方向
            const cameraToGlass = new THREE.Vector3();
            cameraToGlass.subVectors(targetContainer.position, camera.position).normalize();

            // 計算相機朝向
            const cameraDirection = new THREE.Vector3();
            camera.getWorldDirection(cameraDirection);

            // 計算角度（點積）
            const dotProduct = cameraDirection.dot(cameraToGlass);

            // 視角必須對準杯子（角度小於30度，cos(30°) ≈ 0.866）
            if (dotProduct < 0.85) {
                return;
            }
        }

        // 倒酒速度（ml/秒）- 使用系統設定的速度
        const amountPoured = this.pourRate * deltaTime;

        // 添加酒水（合併同類材料）
        const liquor = this.liquorDatabase.get(liquorType);
        if (liquor) {
            // 檢查是否已經有相同類型的材料
            const existingIngredient = contents.ingredients.find(ing => ing.type === liquorType);

            if (existingIngredient) {
                // 如果已存在，累加數量
                existingIngredient.amount += amountPoured;
            } else {
                // 如果不存在，添加新材料
                contents.ingredients.push({
                    type: liquorType,
                    name: liquor.name,
                    displayName: liquor.displayName || liquor.name,
                    amount: amountPoured,
                    color: liquor.color
                });
            }

            contents.volume += amountPoured;

            // 重新計算混合顏色
            this.updateMixedColor(targetContainer);

            // 更新視覺效果
            this.updateLiquidVisual(targetContainer);

            // 更新倒酒進度條 UI
            this.updatePourProgressUI(targetContainer, amountPoured);

            // 創建倒酒粒子效果和動畫
            if (!this.isPouringActive) {
                this.createPourParticles(bottle, targetContainer);
                this.isPouringActive = true;
                this.currentPouringBottle = bottle;
                this.originalBottleRotation = bottle.rotation.clone();
                this.currentPouringAmount = 0; // 重置倒酒累積量

                // 清除之前的隱藏計時器
                if (this.pourProgressHideTimer) {
                    clearTimeout(this.pourProgressHideTimer);
                    this.pourProgressHideTimer = null;
                }
            }

            // 累積這次倒酒的總量
            this.currentPouringAmount += amountPoured;

            // 倒酒動畫：傾斜酒瓶
            if (this.currentPouringBottle) {
                const targetRotation = Math.PI / 3; // 60度傾斜
                const currentZ = this.currentPouringBottle.rotation.z;
                const lerpSpeed = 3.0 * deltaTime;
                this.currentPouringBottle.rotation.z += (targetRotation - currentZ) * lerpSpeed;
            }
        }
    }

    /**
     * 停止倒酒
     */
    stopPouring() {
        // 恢復酒瓶旋轉
        if (this.currentPouringBottle && this.originalBottleRotation) {
            this.currentPouringBottle.rotation.copy(this.originalBottleRotation);
        }

        this.isPouringActive = false;
        this.currentPouringBottle = null;
        this.originalBottleRotation = null;
        this.removePourParticles();

        // 延遲5秒後隱藏倒酒進度條
        if (this.pourProgressPanel) {
            // 清除之前的計時器（如果有）
            if (this.pourProgressHideTimer) {
                clearTimeout(this.pourProgressHideTimer);
            }

            // 設置新的計時器，5秒後隱藏
            this.pourProgressHideTimer = setTimeout(() => {
                if (this.pourProgressPanel) {
                    this.pourProgressPanel.style.display = 'none';
                }
                this.currentPouringAmount = 0; // 重置累積量
            }, 5000);
        }
    }

    /**
     * 從 Shaker 倒酒到其他容器
     * @param {THREE.Object3D} shaker - Shaker 物件
     * @param {THREE.Object3D} targetContainer - 目標容器
     * @param {number} deltaTime - 時間增量
     */
    pourFromShaker(shaker, targetContainer, deltaTime) {
        const shakerContents = this.containerContents.get(shaker);
        const targetContents = this.containerContents.get(targetContainer);

        if (!shakerContents || !targetContents) return;
        if (shakerContents.volume <= 0) return; // Shaker 是空的
        if (targetContents.volume >= targetContents.maxVolume) return; // 目標容器已滿

        // 倒酒速度（ml/秒）- 使用系統設定的速度
        const amountToPour = Math.min(
            this.pourRate * deltaTime,
            shakerContents.volume, // Shaker 剩餘量
            targetContents.maxVolume - targetContents.volume // 目標容器剩餘空間
        );

        // 轉移材料
        shakerContents.ingredients.forEach(ingredient => {
            // 計算這個材料應該轉移的比例
            const ratio = amountToPour / shakerContents.volume;
            const transferAmount = ingredient.amount * ratio;

            // 從 Shaker 減少
            ingredient.amount -= transferAmount;

            // 添加到目標容器
            const existingIngredient = targetContents.ingredients.find(
                ing => ing.type === ingredient.type
            );

            if (existingIngredient) {
                existingIngredient.amount += transferAmount;
            } else {
                targetContents.ingredients.push({
                    type: ingredient.type,
                    name: ingredient.name,
                    displayName: ingredient.displayName,
                    amount: transferAmount,
                    color: ingredient.color
                });
            }
        });

        // 更新體積
        shakerContents.volume -= amountToPour;
        targetContents.volume += amountToPour;

        // 清理 Shaker 中量為 0 的材料
        shakerContents.ingredients = shakerContents.ingredients.filter(
            ing => ing.amount > 0.01
        );

        // 更新顏色和視覺效果
        this.updateMixedColor(shaker);
        this.updateMixedColor(targetContainer);
        this.updateLiquidVisual(shaker);
        this.updateLiquidVisual(targetContainer);

        // 更新進度條 UI
        this.updatePourProgressUI(targetContainer, amountToPour);

        // 創建倒酒效果
        if (!this.isPouringActive) {
            this.createPourParticles(shaker, targetContainer);
            this.isPouringActive = true;
            this.currentPouringBottle = shaker;
            this.originalBottleRotation = shaker.rotation.clone();
            this.currentPouringAmount = 0;

            if (this.pourProgressHideTimer) {
                clearTimeout(this.pourProgressHideTimer);
                this.pourProgressHideTimer = null;
            }
        }

        this.currentPouringAmount += amountToPour;

        // 傾斜動畫
        if (this.currentPouringBottle) {
            const targetRotation = Math.PI / 3;
            const currentZ = this.currentPouringBottle.rotation.z;
            const lerpSpeed = 3.0 * deltaTime;
            this.currentPouringBottle.rotation.z += (targetRotation - currentZ) * lerpSpeed;
        }
    }

    /**
     * 更新倒酒進度條 UI
     * @param {THREE.Object3D} targetContainer - 目標容器
     * @param {number} amountPoured - 倒出的量（ml）
     */
    updatePourProgressUI(targetContainer, amountPoured) {
        if (!this.pourProgressPanel) return;

        const contents = this.containerContents.get(targetContainer);
        if (!contents) return;

        // 顯示進度條面板
        this.pourProgressPanel.style.display = 'block';

        // 更新杯子容量進度條
        const volumePercentage = (contents.volume / contents.maxVolume) * 100;
        if (this.containerVolumeBar) {
            this.containerVolumeBar.style.width = `${Math.min(volumePercentage, 100)}%`;
        }
        if (this.containerVolumeText) {
            this.containerVolumeText.textContent = `${Math.round(contents.volume)}/${contents.maxVolume}ml`;
        }

        // 更新倒入量進度條（顯示這次倒酒的累積總量）
        const totalPouredPercentage = (this.currentPouringAmount / contents.maxVolume) * 100;
        if (this.pourRateBar) {
            this.pourRateBar.style.width = `${Math.min(totalPouredPercentage, 100)}%`;
        }
        if (this.pourRateText) {
            this.pourRateText.textContent = `${Math.round(this.currentPouringAmount)}ml`;
        }
    }

    /**
     * 創建倒酒粒子效果
     * @param {THREE.Object3D} bottle - 酒瓶
     * @param {THREE.Object3D} target - 目標容器
     */
    createPourParticles(bottle, target) {
        const particleCount = 200; // 增加粒子數量
        const particles = new THREE.BufferGeometry();
        const positions = new Float32Array(particleCount * 3);
        const velocities = []; // 儲存粒子速度

        // 計算從瓶口到杯子的方向
        const bottlePos = bottle.position.clone();
        bottlePos.y -= 0.3; // 瓶口位置
        const targetPos = target.position.clone();

        for (let i = 0; i < particleCount; i++) {
            // 初始化粒子在瓶口附近
            const spread = 0.05;
            positions[i * 3] = bottlePos.x + (Math.random() - 0.5) * spread;
            positions[i * 3 + 1] = bottlePos.y;
            positions[i * 3 + 2] = bottlePos.z + (Math.random() - 0.5) * spread;

            // 計算速度（朝向杯子）
            const velocity = new THREE.Vector3(
                (targetPos.x - bottlePos.x) + (Math.random() - 0.5) * 0.2,
                -1.0, // 向下
                (targetPos.z - bottlePos.z) + (Math.random() - 0.5) * 0.2
            );
            velocities.push(velocity);
        }

        particles.setAttribute('position', new THREE.BufferAttribute(positions, 3));

        const particleMaterial = new THREE.PointsMaterial({
            color: 0xaaccff, // 淡藍色，更明顯
            size: 0.05, // 增大粒子尺寸
            transparent: true,
            opacity: 0.8
        });

        const particleSystem = new THREE.Points(particles, particleMaterial);
        particleSystem.userData.velocities = velocities; // 儲存速度
        particleSystem.userData.bottlePosition = bottlePos;

        this.scene.add(particleSystem);
        this.particleSystems.set('pour', particleSystem);
    }

    /**
     * 移除倒酒粒子效果
     */
    removePourParticles() {
        const particleSystem = this.particleSystems.get('pour');
        if (particleSystem) {
            this.scene.remove(particleSystem);
            this.particleSystems.delete('pour');
        }
    }

    /**
     * 搖酒（Shaker）
     * @param {THREE.Object3D} shaker - 搖酒器
     * @param {number} deltaTime - 時間增量
     */
    shake(shaker, deltaTime) {
        const contents = this.containerContents.get(shaker);
        if (!contents || contents.volume === 0) {
            console.log('Shaker 是空的！');
            return;
        }

        this.isShakingActive = true;
        this.shakeTime += deltaTime;

        // 搖晃強度（正弦波動）
        this.shakeIntensity = Math.sin(this.shakeTime * 20) * 0.05;

        // 應用搖晃旋轉
        shaker.rotation.z = this.shakeIntensity;
        shaker.rotation.x = Math.sin(this.shakeTime * 15) * 0.03;

        // 搖晃會混合酒水，增強顏色混合
        if (this.shakeTime > 2) {
            this.enhanceMixing(shaker);
        }
    }

    /**
     * 停止搖酒
     * @param {THREE.Object3D} shaker - 搖酒器
     */
    stopShaking(shaker) {
        this.isShakingActive = false;
        this.shakeTime = 0;

        // 重置旋轉
        shaker.rotation.z = 0;
        shaker.rotation.x = 0;
    }

    /**
     * 增強混合效果
     * @param {THREE.Object3D} container - 容器
     */
    enhanceMixing(container) {
        const contents = this.containerContents.get(container);
        if (!contents) return;

        // 混合後顏色更均勻
        this.updateMixedColor(container);
        this.updateLiquidVisual(container);
    }

    /**
     * 更新混合顏色
     * @param {THREE.Object3D} container - 容器
     */
    updateMixedColor(container) {
        const contents = this.containerContents.get(container);
        if (!contents || contents.ingredients.length === 0) return;

        let r = 0, g = 0, b = 0;
        let totalAmount = 0;

        // 加權平均計算混合顏色
        contents.ingredients.forEach(ingredient => {
            const color = new THREE.Color(ingredient.color);
            const weight = ingredient.amount;

            r += color.r * weight;
            g += color.g * weight;
            b += color.b * weight;
            totalAmount += weight;
        });

        if (totalAmount > 0) {
            r /= totalAmount;
            g /= totalAmount;
            b /= totalAmount;

            const mixedColor = new THREE.Color(r, g, b);
            contents.color = mixedColor.getHex();
        }
    }

    /**
     * 喝掉飲品
     * @param {THREE.Object3D} container - 容器
     * @param {boolean} startAnimation - 是否開始喝酒動畫
     * @returns {Object} 飲品資訊
     */
    drink(container, startAnimation = true) {
        const contents = this.containerContents.get(container);
        if (!contents || contents.volume === 0) {
            console.log('杯子是空的！');
            return null;
        }

        if (startAnimation && !this.isDrinking) {
            // 開始喝酒動畫
            this.isDrinking = true;
            this.drinkingStartTime = Date.now();
            this.currentDrinkingGlass = container;
            this.originalGlassPosition = container.position.clone();
            return null; // 動畫中先不返回資訊
        }

        // 獲取飲品資訊（非動畫模式）
        const drinkInfo = {
            volume: contents.volume,
            ingredients: [...contents.ingredients],
            color: contents.color,
            name: this.identifyCocktail(contents)
        };

        // 清空容器
        contents.ingredients = [];
        contents.volume = 0;
        contents.color = 0xffffff;

        // 更新視覺
        this.updateLiquidVisual(container);

        console.log(`你喝了 ${drinkInfo.name}！`);
        return drinkInfo;
    }

    /**
     * 取得並清除最後一次喝酒的資訊
     * @returns {Object|null} 飲品資訊
     */
    getLastDrinkInfo() {
        const info = this.lastDrinkInfo;
        this.lastDrinkInfo = null;
        return info;
    }

    /**
     * 更新喝酒動畫
     */
    updateDrinkingAnimation() {
        if (!this.isDrinking || !this.currentDrinkingGlass) return;

        const elapsedTime = (Date.now() - this.drinkingStartTime) / 1000;

        if (elapsedTime < 1.0) {
            // 動畫進行中：將杯子向上移動並稍微傾斜
            const progress = elapsedTime;
            this.currentDrinkingGlass.position.y = this.originalGlassPosition.y + 0.2 * progress;
            this.currentDrinkingGlass.rotation.x = -Math.PI / 6 * progress; // 傾斜30度
        } else {
            // 動畫結束：重置位置並實際消耗飲品
            this.currentDrinkingGlass.position.copy(this.originalGlassPosition);
            this.currentDrinkingGlass.rotation.x = 0;

            // 實際消耗飲品
            const contents = this.containerContents.get(this.currentDrinkingGlass);
            if (contents && contents.volume > 0) {
                const drinkInfo = {
                    volume: contents.volume,
                    ingredients: [...contents.ingredients],
                    color: contents.color,
                    name: this.identifyCocktail(contents)
                };

                // 清空容器
                contents.ingredients = [];
                contents.volume = 0;
                contents.color = 0xffffff;
                this.updateLiquidVisual(this.currentDrinkingGlass);

                // 儲存飲品資訊供外部使用
                this.lastDrinkInfo = drinkInfo;
            }

            this.isDrinking = false;
            this.currentDrinkingGlass = null;
        }
    }

    /**
     * 識別雞尾酒
     * @param {Object} contents - 容器內容
     * @returns {string} 雞尾酒名稱
     */
    identifyCocktail(contents) {
        const ingredients = contents.ingredients;
        const types = ingredients.map(ing => ing.type);

        // 獲取每種材料的量
        const getAmount = (type) => {
            const ing = ingredients.find(i => i.type === type);
            return ing ? ing.amount : 0;
        };

        // === 經典調酒識別（精確配方） ===

        // Martini（馬丁尼）：Gin + Dry Vermouth (2:1 到 3:1)
        // 可以加少量檸檬汁或糖漿
        if (types.includes('gin') && types.includes('vermouth_dry')) {
            const ginAmount = getAmount('gin');
            const vermouthAmount = getAmount('vermouth_dry');
            const ratio = ginAmount / vermouthAmount;

            // 檢查是否有其他不允許的基酒或果汁
            const hasOtherSpirits = types.some(t =>
                ['vodka', 'rum', 'whiskey', 'tequila', 'brandy', 'campari'].includes(t)
            );
            const hasJuice = types.some(t =>
                ['orange_juice', 'cranberry_juice', 'pineapple_juice', 'tomato_juice', 'grapefruit_juice'].includes(t)
            );

            // 允許的額外材料
            const allowedExtras = ['lemon_juice', 'lime_juice', 'simple_syrup'];
            const hasOnlyAllowedExtras = types.filter(t =>
                t !== 'gin' && t !== 'vermouth_dry'
            ).every(t => allowedExtras.includes(t));

            if (!hasOtherSpirits && !hasJuice && hasOnlyAllowedExtras && ratio >= 2 && ratio <= 3) {
                return '馬丁尼 (Martini) ✨';
            }
        }

        // Vodka Martini（伏特加馬丁尼）：Vodka + Dry Vermouth
        if (types.includes('vodka') && types.includes('vermouth_dry')) {
            const vodkaAmount = getAmount('vodka');
            const vermouthAmount = getAmount('vermouth_dry');
            const ratio = vodkaAmount / vermouthAmount;

            const hasOtherSpirits = types.some(t =>
                ['gin', 'rum', 'whiskey', 'tequila', 'brandy', 'campari'].includes(t)
            );

            if (!hasOtherSpirits && ratio >= 2 && ratio <= 3) {
                return '伏特加馬丁尼 (Vodka Martini) ✨';
            }
        }

        // Negroni（內格羅尼）：Gin + Campari + Sweet Vermouth (1:1:1)
        if (types.includes('gin') && types.includes('campari') && types.includes('vermouth_sweet')) {
            const ginAmount = getAmount('gin');
            const campariAmount = getAmount('campari');
            const vermouthAmount = getAmount('vermouth_sweet');

            // 檢查比例是否接近 1:1:1（允許 ±30% 誤差）
            const avgAmount = (ginAmount + campariAmount + vermouthAmount) / 3;
            const isBalanced =
                Math.abs(ginAmount - avgAmount) / avgAmount < 0.3 &&
                Math.abs(campariAmount - avgAmount) / avgAmount < 0.3 &&
                Math.abs(vermouthAmount - avgAmount) / avgAmount < 0.3;

            if (isBalanced) {
                return '內格羅尼 (Negroni) ✨';
            }
        }

        // Margarita（瑪格麗特）：Tequila + Triple Sec + Lime Juice
        if (types.includes('tequila') && types.includes('triple_sec') && types.includes('lime_juice')) {
            return '瑪格麗特 (Margarita) ✨';
        }

        // Daiquiri（黛克瑞）：Rum + Lime Juice + Simple Syrup
        if (types.includes('rum') && types.includes('lime_juice') && types.includes('simple_syrup')) {
            return '黛克瑞 (Daiquiri) ✨';
        }

        // Piña Colada（椰林風情）：Rum + Pineapple Juice + Coconut Cream
        if (types.includes('rum') && types.includes('pineapple_juice') && types.includes('coconut_cream')) {
            return '椰林風情 (Piña Colada) ✨';
        }

        // Cosmopolitan（柯夢波丹）：Vodka + Triple Sec + Cranberry Juice + Lime Juice
        if (types.includes('vodka') && types.includes('triple_sec') && types.includes('cranberry_juice') && types.includes('lime_juice')) {
            return '柯夢波丹 (Cosmopolitan) ✨';
        }

        // Mojito（莫希托）：Rum + Lime Juice + Simple Syrup (+ optional soda water)
        if (types.includes('rum') && types.includes('lime_juice') && types.includes('simple_syrup')) {
            return '莫希托 (Mojito) ✨';
        }

        // === 簡單配方匹配 ===
        if (types.includes('vodka') && types.length === 1) {
            return '伏特加純飲';
        } else if (types.includes('gin') && types.length === 1) {
            return '琴酒純飲';
        } else if (types.includes('rum') && types.includes('liqueur')) {
            return '熱帶雞尾酒';
        } else if (types.includes('vodka') && types.includes('liqueur')) {
            return '性感海灘';
        } else if (types.length > 2) {
            return '特調混酒';
        } else if (types.length === 2) {
            return '雙料調酒';
        } else {
            return '未知飲品';
        }
    }

    /**
     * 清空容器
     * @param {THREE.Object3D} container - 容器
     */
    emptyContainer(container) {
        const contents = this.containerContents.get(container);
        if (!contents) return;

        contents.ingredients = [];
        contents.volume = 0;
        contents.color = 0xffffff;

        this.updateLiquidVisual(container);
    }

    /**
     * 獲取容器資訊
     * @param {THREE.Object3D} container - 容器
     * @returns {Object|null} 容器內容
     */
    getContainerInfo(container) {
        return this.containerContents.get(container) || null;
    }

    /**
     * 檢查容器是否為空
     * @param {THREE.Object3D} container - 容器
     * @returns {boolean}
     */
    isEmpty(container) {
        const contents = this.containerContents.get(container);
        return !contents || contents.volume === 0;
    }

    /**
     * 檢查容器是否已滿
     * @param {THREE.Object3D} container - 容器
     * @returns {boolean}
     */
    isFull(container) {
        const contents = this.containerContents.get(container);
        return contents && contents.volume >= contents.maxVolume;
    }

    /**
     * 獲取配方建議
     * @returns {Array} 配方列表
     */
    getRecipeSuggestions() {
        return [
            {
                name: 'Mojito',
                ingredients: ['rum', 'liqueur'],
                description: '清新的古巴經典'
            },
            {
                name: 'Cosmopolitan',
                ingredients: ['vodka', 'liqueur'],
                description: '優雅的城市風情'
            },
            {
                name: 'Old Fashioned',
                ingredients: ['whiskey', 'liqueur'],
                description: '經典威士忌調酒'
            },
            {
                name: 'Margarita',
                ingredients: ['tequila', 'liqueur'],
                description: '墨西哥風味雞尾酒'
            }
        ];
    }

    /**
     * 顯示容器成分信息（UI）
     * @param {THREE.Object3D} container - 容器
     */
    showContainerInfo(container) {
        const contents = this.containerContents.get(container);
        const infoDiv = document.getElementById('container-info');

        if (!contents || !infoDiv) return;

        if (contents.volume > 0) {
            // 構建成分列表
            const ingredientListHTML = contents.ingredients.map(ing => {
                const liquor = this.liquorDatabase.get(ing.type);
                return `
                    <div class="ingredient-item">
                        <span class="ingredient-name">${liquor ? liquor.name : ing.name}</span>
                        <span class="ingredient-amount">${Math.round(ing.amount)} ml</span>
                    </div>
                `;
            }).join('');

            // 計算酒精濃度
            const alcoholContent = this.calculateAlcoholContent(contents);

            // 識別雞尾酒
            const cocktailName = this.identifyCocktail(contents);

            infoDiv.innerHTML = `
                <h3>${cocktailName}</h3>
                <div class="ingredient-list">
                    ${ingredientListHTML}
                </div>
                <div class="volume-info">
                    總容量: ${Math.round(contents.volume)} / ${contents.maxVolume} ml<br>
                    酒精濃度: ${alcoholContent.toFixed(1)}%
                </div>
            `;
            infoDiv.classList.add('visible');
        } else {
            infoDiv.classList.remove('visible');
        }
    }

    /**
     * 計算容器內的平均酒精濃度
     * @param {Object} contents - 容器內容
     * @returns {number} 酒精濃度（百分比）
     */
    calculateAlcoholContent(contents) {
        if (!contents || contents.volume === 0) return 0;

        let totalAlcohol = 0; // 總酒精量（ml）

        // 計算每種材料的酒精含量
        contents.ingredients.forEach(ing => {
            const liquor = this.liquorDatabase.get(ing.type);
            if (liquor && liquor.alcoholContent) {
                // 酒精含量 = 材料量 * 酒精濃度
                totalAlcohol += ing.amount * (liquor.alcoholContent / 100);
            }
        });

        // 平均酒精濃度 = 總酒精量 / 總容量 * 100%
        const averageAlcoholContent = (totalAlcohol / contents.volume) * 100;

        return averageAlcoholContent;
    }

    /**
     * 隱藏容器成分信息
     */
    hideContainerInfo() {
        const infoDiv = document.getElementById('container-info');
        if (infoDiv) {
            infoDiv.classList.remove('visible');
        }
    }

    /**
     * 更新系統（每幀調用）
     * @param {number} deltaTime - 時間增量
     */
    update(deltaTime) {
        // 更新倒酒粒子動畫
        if (this.isPouringActive) {
            const particleSystem = this.particleSystems.get('pour');
            if (particleSystem) {
                const positions = particleSystem.geometry.attributes.position.array;
                const velocities = particleSystem.userData.velocities;
                const bottlePos = particleSystem.userData.bottlePosition;

                for (let i = 0; i < positions.length / 3; i++) {
                    const idx = i * 3;
                    const velocity = velocities[i];

                    // 應用速度
                    positions[idx] += velocity.x * deltaTime * 2;
                    positions[idx + 1] += velocity.y * deltaTime * 2;
                    positions[idx + 2] += velocity.z * deltaTime * 2;

                    // 重置粒子（當粒子落得太低時）
                    if (positions[idx + 1] < bottlePos.y - 1.0) {
                        const spread = 0.05;
                        positions[idx] = bottlePos.x + (Math.random() - 0.5) * spread;
                        positions[idx + 1] = bottlePos.y;
                        positions[idx + 2] = bottlePos.z + (Math.random() - 0.5) * spread;
                    }
                }

                particleSystem.geometry.attributes.position.needsUpdate = true;
            }
        }

        // 更新喝酒動畫
        this.updateDrinkingAnimation();
    }

    /**
     * 獲取經典調酒食譜列表（IBA經典調酒）
     * @returns {Array} 食譜列表
     */
    getCocktailRecipes() {
        return [
            // === Unforgettable 經典不朽調酒 ===
            {
                name: 'Martini 馬丁尼',
                ingredients: [
                    { amount: '60ml', name: '琴酒 Gin' },
                    { amount: '10ml', name: '不甜香艾酒 Dry Vermouth' }
                ],
                method: 'Stir（攪拌法）：將材料加冰攪拌後濾入冰鎮馬丁尼杯，可加檸檬皮裝飾。'
            },
            {
                name: 'Vodka Martini 伏特加馬丁尼',
                ingredients: [
                    { amount: '60ml', name: '伏特加 Vodka' },
                    { amount: '10ml', name: '不甜香艾酒 Dry Vermouth' }
                ],
                method: 'Stir：將材料加冰攪拌後濾入冰鎮馬丁尼杯，檸檬皮或橄欖裝飾。'
            },
            {
                name: 'Negroni 內格羅尼',
                ingredients: [
                    { amount: '30ml', name: '琴酒 Gin' },
                    { amount: '30ml', name: '金巴利 Campari' },
                    { amount: '30ml', name: '甜香艾酒 Sweet Vermouth' }
                ],
                method: 'Build：將材料倒入裝滿冰塊的古典杯，攪拌均勻，柳橙皮裝飾。'
            },
            {
                name: 'Margarita 瑪格麗特',
                ingredients: [
                    { amount: '50ml', name: '龍舌蘭 Tequila' },
                    { amount: '20ml', name: '橙皮酒 Triple Sec' },
                    { amount: '15ml', name: '萊姆汁 Lime Juice' }
                ],
                method: 'Shake：加冰搖盪後濾入杯緣抹鹽的杯中，萊姆角裝飾。'
            },
            {
                name: 'Daiquiri 黛克瑞',
                ingredients: [
                    { amount: '60ml', name: '蘭姆酒 Rum' },
                    { amount: '20ml', name: '萊姆汁 Lime Juice' },
                    { amount: '10ml', name: '糖漿 Simple Syrup' }
                ],
                method: 'Shake：加冰搖盪後濾入冰鎮雞尾酒杯。'
            },
            {
                name: 'Cosmopolitan 柯夢波丹',
                ingredients: [
                    { amount: '40ml', name: '伏特加 Vodka' },
                    { amount: '15ml', name: '橙皮酒 Triple Sec' },
                    { amount: '15ml', name: '萊姆汁 Lime Juice' },
                    { amount: '30ml', name: '蔓越莓汁 Cranberry Juice' }
                ],
                method: 'Shake：加冰搖盪後濾入馬丁尼杯，萊姆皮或蔓越莓裝飾。'
            },
            {
                name: 'Mojito 莫希托',
                ingredients: [
                    { amount: '45ml', name: '蘭姆酒 Rum' },
                    { amount: '20ml', name: '萊姆汁 Lime Juice' },
                    { amount: '20ml', name: '糖漿 Simple Syrup' },
                    { amount: '適量', name: '蘇打水 Soda Water' }
                ],
                method: 'Muddle：在杯中壓碎薄荷葉與糖，加冰、蘭姆酒、萊姆汁，上方加蘇打水。'
            },
            {
                name: 'Piña Colada 椰林風情',
                ingredients: [
                    { amount: '50ml', name: '蘭姆酒 Rum' },
                    { amount: '30ml', name: '椰漿 Coconut Cream' },
                    { amount: '50ml', name: '鳳梨汁 Pineapple Juice' }
                ],
                method: 'Blend：與碎冰混合打碎，倒入颶風杯，鳳梨角和櫻桃裝飾。'
            },

            // === Contemporary Classics 當代經典調酒 ===
            {
                name: 'Whiskey Sour 威士忌酸酒',
                ingredients: [
                    { amount: '50ml', name: '威士忌 Whiskey' },
                    { amount: '25ml', name: '檸檬汁 Lemon Juice' },
                    { amount: '15ml', name: '糖漿 Simple Syrup' }
                ],
                method: 'Shake：加冰搖盪後濾入古典杯，可加蛋白增加口感。'
            },
            {
                name: 'Gin Tonic 琴湯尼',
                ingredients: [
                    { amount: '50ml', name: '琴酒 Gin' },
                    { amount: '適量', name: '通寧水 Tonic Water' }
                ],
                method: 'Build：在高球杯中加冰塊和琴酒，補滿通寧水，萊姆角裝飾。'
            },
            {
                name: 'Cuba Libre 自由古巴',
                ingredients: [
                    { amount: '50ml', name: '蘭姆酒 Rum' },
                    { amount: '10ml', name: '萊姆汁 Lime Juice' },
                    { amount: '適量', name: '可樂 Cola' }
                ],
                method: 'Build：在高球杯中加冰塊、蘭姆酒和萊姆汁，補滿可樂。'
            },
            {
                name: 'Bloody Mary 血腥瑪麗',
                ingredients: [
                    { amount: '45ml', name: '伏特加 Vodka' },
                    { amount: '90ml', name: '番茄汁 Tomato Juice' },
                    { amount: '15ml', name: '檸檬汁 Lemon Juice' },
                    { amount: '少許', name: '安格仕苦精 Angostura Bitters' }
                ],
                method: 'Roll：在雪克杯中倒入材料與冰塊，來回倒入另一個杯子混合。'
            },
            {
                name: 'Sea Breeze 海風',
                ingredients: [
                    { amount: '40ml', name: '伏特加 Vodka' },
                    { amount: '60ml', name: '蔓越莓汁 Cranberry Juice' },
                    { amount: '30ml', name: '葡萄柚汁 Grapefruit Juice' }
                ],
                method: 'Build：在裝滿冰塊的高球杯中依序倒入材料，攪拌均勻。'
            },
            {
                name: 'Tequila Sunrise 龍舌蘭日出',
                ingredients: [
                    { amount: '45ml', name: '龍舌蘭 Tequila' },
                    { amount: '90ml', name: '柳橙汁 Orange Juice' },
                    { amount: '15ml', name: '紅石榴糖漿 Grenadine' }
                ],
                method: 'Build：在高球杯中加冰、龍舌蘭和柳橙汁，最後慢慢倒入紅石榴糖漿形成漸層。'
            },

            // === 更多經典調酒 ===
            {
                name: 'Screwdriver 螺絲起子',
                ingredients: [
                    { amount: '50ml', name: '伏特加 Vodka' },
                    { amount: '100ml', name: '柳橙汁 Orange Juice' }
                ],
                method: 'Build：在裝滿冰塊的高球杯中倒入伏特加，補滿柳橙汁，攪拌均勻。'
            },
            {
                name: 'Mai Tai 邁泰',
                ingredients: [
                    { amount: '40ml', name: '蘭姆酒 Rum' },
                    { amount: '20ml', name: '橙皮酒 Triple Sec' },
                    { amount: '15ml', name: '萊姆汁 Lime Juice' },
                    { amount: '10ml', name: '糖漿 Simple Syrup' }
                ],
                method: 'Shake：加冰搖盪後濾入裝滿碎冰的古典杯，薄荷和萊姆裝飾。'
            },
            {
                name: 'Sidecar 側車',
                ingredients: [
                    { amount: '50ml', name: '白蘭地 Brandy' },
                    { amount: '20ml', name: '橙皮酒 Triple Sec' },
                    { amount: '20ml', name: '檸檬汁 Lemon Juice' }
                ],
                method: 'Shake：加冰搖盪後濾入杯緣抹糖的雞尾酒杯。'
            },
            {
                name: 'Manhattan 曼哈頓',
                ingredients: [
                    { amount: '50ml', name: '威士忌 Whiskey' },
                    { amount: '20ml', name: '甜香艾酒 Sweet Vermouth' },
                    { amount: '2滴', name: '安格仕苦精 Angostura Bitters' }
                ],
                method: 'Stir：將材料加冰攪拌後濾入馬丁尼杯，櫻桃裝飾。'
            },
            {
                name: 'Americano 美國佬',
                ingredients: [
                    { amount: '30ml', name: '金巴利 Campari' },
                    { amount: '30ml', name: '甜香艾酒 Sweet Vermouth' },
                    { amount: '適量', name: '蘇打水 Soda Water' }
                ],
                method: 'Build：在裝滿冰塊的古典杯中倒入金巴利和甜香艾酒，補滿蘇打水。'
            },
            {
                name: 'Gimlet 琴蕾',
                ingredients: [
                    { amount: '60ml', name: '琴酒 Gin' },
                    { amount: '15ml', name: '萊姆汁 Lime Juice' },
                    { amount: '10ml', name: '糖漿 Simple Syrup' }
                ],
                method: 'Shake：加冰搖盪後濾入冰鎮雞尾酒杯，萊姆角裝飾。'
            },
            {
                name: 'French 75 法式75',
                ingredients: [
                    { amount: '30ml', name: '琴酒 Gin' },
                    { amount: '15ml', name: '檸檬汁 Lemon Juice' },
                    { amount: '10ml', name: '糖漿 Simple Syrup' },
                    { amount: '適量', name: '香檳（可用蘇打水代替）' }
                ],
                method: 'Shake前三種材料後濾入香檳杯，補滿香檳或蘇打水。'
            },
            {
                name: 'Aperol Spritz 艾普羅氣泡酒',
                ingredients: [
                    { amount: '60ml', name: '利口酒 Liqueur' },
                    { amount: '90ml', name: '氣泡酒（可用蘇打水代替）' },
                    { amount: '30ml', name: '蘇打水 Soda Water' }
                ],
                method: 'Build：在裝滿冰塊的酒杯中依序倒入材料，柳橙片裝飾。'
            },
            {
                name: 'Moscow Mule 莫斯科騾子',
                ingredients: [
                    { amount: '45ml', name: '伏特加 Vodka' },
                    { amount: '15ml', name: '萊姆汁 Lime Juice' },
                    { amount: '適量', name: '薑汁汽水（可用蘇打水+糖漿代替）' }
                ],
                method: 'Build：在銅製馬克杯或高球杯中加冰、伏特加和萊姆汁，補滿薑汁汽水。'
            },
            {
                name: 'Caipirinha 卡琵莉亞',
                ingredients: [
                    { amount: '60ml', name: '蘭姆酒 Rum（傳統用 Cachaça）' },
                    { amount: '半顆', name: '萊姆 Lime' },
                    { amount: '2茶匙', name: '糖 Sugar' }
                ],
                method: 'Muddle：在古典杯中壓碎萊姆塊與糖，加入碎冰和蘭姆酒，攪拌均勻。'
            },
            {
                name: 'Long Island Iced Tea 長島冰茶',
                ingredients: [
                    { amount: '15ml', name: '伏特加 Vodka' },
                    { amount: '15ml', name: '蘭姆酒 Rum' },
                    { amount: '15ml', name: '琴酒 Gin' },
                    { amount: '15ml', name: '龍舌蘭 Tequila' },
                    { amount: '15ml', name: '橙皮酒 Triple Sec' },
                    { amount: '25ml', name: '檸檬汁 Lemon Juice' },
                    { amount: '30ml', name: '糖漿 Simple Syrup' },
                    { amount: '適量', name: '可樂 Cola' }
                ],
                method: 'Shake前七種材料後濾入裝滿冰塊的柯林斯杯，上方補可樂，檸檬片裝飾。'
            },
            {
                name: 'Vesper 薇絲朋',
                ingredients: [
                    { amount: '45ml', name: '琴酒 Gin' },
                    { amount: '15ml', name: '伏特加 Vodka' },
                    { amount: '7.5ml', name: '不甜香艾酒 Dry Vermouth' }
                ],
                method: 'Shake：加冰搖盪後濾入馬丁尼杯，檸檬皮裝飾。（007電影中詹姆士龐德的最愛）'
            },
            {
                name: 'Tom Collins 湯姆柯林斯',
                ingredients: [
                    { amount: '45ml', name: '琴酒 Gin' },
                    { amount: '30ml', name: '檸檬汁 Lemon Juice' },
                    { amount: '15ml', name: '糖漿 Simple Syrup' },
                    { amount: '適量', name: '蘇打水 Soda Water' }
                ],
                method: 'Shake前三種材料後濾入裝滿冰塊的柯林斯杯，補滿蘇打水，檸檬片和櫻桃裝飾。'
            },
            {
                name: 'Bramble 荊棘',
                ingredients: [
                    { amount: '40ml', name: '琴酒 Gin' },
                    { amount: '25ml', name: '檸檬汁 Lemon Juice' },
                    { amount: '10ml', name: '糖漿 Simple Syrup' },
                    { amount: '15ml', name: '紅石榴糖漿 Grenadine' }
                ],
                method: 'Shake前三種材料後倒入裝滿碎冰的古典杯，淋上紅石榴糖漿形成漸層，黑莓裝飾。'
            }
        ];
    }
}
