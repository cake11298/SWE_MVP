import * as THREE from 'three';

export class BarEnvironment {
    constructor(scene, interactionSystem = null, physicsSystem = null, cocktailSystem = null) {
        this.scene = scene;
        this.bottles = [];
        this.interactables = []; // 可互動物品列表
        this.glasses = []; // 杯子列表
        this.barTools = {}; // 吧檯工具（shaker, jigger等）
        this.guitar = null; // 吉他物件

        // 系統引用
        this.interaction = interactionSystem;
        this.physics = physicsSystem;
        this.cocktail = cocktailSystem;

        this.createEnvironment();

        // 如果系統已初始化，設置互動物品
        if (this.interaction && this.physics && this.cocktail) {
            this.setupInteractables();
        }
    }

    /**
     * 設置可互動物品（在系統初始化後調用）
     */
    setupInteractables() {
        // 添加吧台碰撞體（允許物品放置）
        this.physics.addStaticBox(
            new THREE.Vector3(0, 1.05, -3),    // 檯面位置
            new THREE.Vector3(12.2, 0.1, 2.2)  // 檯面尺寸
        );

        // 添加酒架碰撞體（3層架子）
        for (let i = 0; i < 3; i++) {
            const shelfY = 1.5 + i * 1.2;
            this.physics.addStaticBox(
                new THREE.Vector3(0, shelfY, -8),     // 酒架位置
                new THREE.Vector3(10, 0.2, 1)          // 酒架尺寸
            );
        }

        // 註冊酒瓶為可互動物品
        this.bottles.forEach((bottle, index) => {
            const bottleType = this.getBottleTypeFromIndex(index);
            this.interaction.registerInteractable(bottle, 'bottle', bottle.position.clone());

            // 添加物理屬性
            this.physics.addCylinderBody(bottle, 0.15, 0.18, 0.85, 0.5, 'glass');

            // 標記酒類型
            bottle.userData.liquorType = bottleType;
        });

        // 註冊杯子為可互動物品
        this.glasses.forEach(glass => {
            this.interaction.registerInteractable(glass, 'glass', glass.position.clone());
            this.physics.addCylinderBody(glass, 0.13, 0.15, 0.6, 0.3, 'glass');

            // 初始化杯子容器
            this.cocktail.initContainer(glass, 300);
        });

        // 註冊 Shaker
        if (this.barTools.shaker) {
            this.interaction.registerInteractable(this.barTools.shaker, 'shaker', this.barTools.shaker.position.clone());
            this.physics.addCylinderBody(this.barTools.shaker, 0.18, 0.22, 0.65, 0.6, 'metal');
            this.cocktail.initContainer(this.barTools.shaker, 500);
        }

        // 註冊 Jigger
        if (this.barTools.jigger) {
            this.interaction.registerInteractable(this.barTools.jigger, 'jigger', this.barTools.jigger.position.clone());
            this.physics.addCylinderBody(this.barTools.jigger, 0.09, 0.13, 0.18, 0.2);
        }

        // 註冊吉他為可互動物品（特殊類型：不會被拾取，而是觸發音樂）
        if (this.guitar) {
            // 計算吉他的世界位置
            const guitarWorldPos = new THREE.Vector3();
            this.guitar.getWorldPosition(guitarWorldPos);
            this.interaction.registerInteractable(this.guitar, 'guitar', guitarWorldPos);
        }

        // 註冊材料瓶為可互動物品
        if (this.ingredientBottles) {
            this.ingredientBottles.forEach(ingredientData => {
                const bottle = ingredientData.mesh;
                // 計算世界位置
                const worldPos = new THREE.Vector3();
                bottle.getWorldPosition(worldPos);

                this.interaction.registerInteractable(bottle, 'bottle', worldPos);
                this.physics.addCylinderBody(bottle, 0.08, 0.1, 0.35, 0.2, 'glass');

                // 標記材料類型
                bottle.userData.liquorType = ingredientData.type;
                bottle.userData.displayName = ingredientData.displayName;
            });
        }
    }

    /**
     * 根據索引獲取酒類型
     */
    getBottleTypeFromIndex(index) {
        const types = ['gin', 'vodka', 'whiskey', 'gin', 'whiskey', 'tequila', 'rum', 'vodka'];
        return types[index % types.length];
    }
    
    createEnvironment() {
        this.createFloor();
        this.createWalls();
        this.createBarCounter();
        this.createLiquorShelf();
        this.createBarStools();
        this.createBarDecoration();
        this.createPremiumBottleDisplay();
        this.createIngredientShelf(); // 添加材料展示櫃
        this.createDrinkingGlasses(); // 添加可互動的杯子
    }
    
    createFloor() {
        const floorGeometry = new THREE.PlaneGeometry(20, 20);
        // 使用更真實的深色木地板紋理
        const floorMaterial = new THREE.MeshPhongMaterial({ 
            color: 0x2c1810,
            shininess: 60,
            specular: 0x111111
        });
        const floor = new THREE.Mesh(floorGeometry, floorMaterial);
        floor.rotation.x = -Math.PI / 2;
        floor.receiveShadow = true;
        this.scene.add(floor);
    }
    
    createWalls() {
        // 使用更溫暖的牆壁顏色
        const wallMaterial = new THREE.MeshPhongMaterial({ 
            color: 0x5d4e37,
            shininess: 5
        });
        
        const wallPositions = [
            // 前牆（原本的後牆，在 z = -10）
            { pos: [0, 5, -10], rot: [0, 0, 0] },
            // 左牆
            { pos: [-10, 5, 0], rot: [0, Math.PI/2, 0] },
            // 右牆
            { pos: [10, 5, 0], rot: [0, -Math.PI/2, 0] },
            // 新增：後牆（在 z = 10）
            { pos: [0, 5, 15], rot: [0, Math.PI, 0] }
        ];
        
        wallPositions.forEach(({ pos, rot }) => {
            const wall = new THREE.Mesh(
                new THREE.PlaneGeometry(20, 10),
                wallMaterial
            );
            wall.position.set(...pos);
            wall.rotation.set(...rot);
            wall.receiveShadow = true;
            this.scene.add(wall);
        });
    }
    
    createBarCounter() {
        const counterGroup = new THREE.Group();
        
        // 吧檯主體（使用更真實的木材顏色）
        const counter = new THREE.Mesh(
            new THREE.BoxGeometry(12, 1, 2),
            new THREE.MeshPhongMaterial({ 
                color: 0x654321,
                shininess: 40,
                specular: 0x222222
            })
        );
        counter.position.set(0, 0.5, -3);
        counter.castShadow = true;
        counter.receiveShadow = true;
        
        // 改進的大理石檯面（更真實的顏色和材質）
        const counterTop = new THREE.Mesh(
            new THREE.BoxGeometry(12.2, 0.1, 2.2),
            new THREE.MeshPhongMaterial({ 
                color: 0xe8e8e8,
                shininess: 150,
                specular: 0x444444
            })
        );
        counterTop.position.set(0, 1.05, -3);
        
        counterGroup.add(counter);
        counterGroup.add(counterTop);
        this.scene.add(counterGroup);
        
        this.createDetailedBarTools();
    }
    
    createLiquorShelf() {
        // 更真實的酒架背板
        const shelfBack = new THREE.Mesh(
            new THREE.BoxGeometry(10, 5, 0.3),
            new THREE.MeshPhongMaterial({ 
                color: 0x4a3c28,
                shininess: 20
            })
        );
        shelfBack.position.set(0, 3, -8.5);
        shelfBack.castShadow = true;
        this.scene.add(shelfBack);
        
        // 建立多層架子與酒瓶
        for (let i = 0; i < 3; i++) {
            const shelf = new THREE.Mesh(
                new THREE.BoxGeometry(10, 0.2, 1),
                new THREE.MeshPhongMaterial({ 
                    color: 0x654321,
                    shininess: 50
                })
            );
            shelf.position.set(0, 1.5 + i * 1.2, -8);
            shelf.castShadow = true;
            this.scene.add(shelf);
            
            // 添加貨架前緣LED燈帶效果
            const ledStrip = new THREE.Mesh(
                new THREE.BoxGeometry(10, 0.02, 0.05),
                new THREE.MeshBasicMaterial({ 
                    color: 0xffffff,
                    transparent: true,
                    opacity: 0.6
                })
            );
            ledStrip.position.set(0, 1.4 + i * 1.2, -7.5);
            this.scene.add(ledStrip);
            
            this.createBottleRow(1.5 + i * 1.2);
        }
    }
    
    createBottleRow(height) {
        const bottleTypes = [
            { color: 0x4a5d23, label: 'TANQUERAY', height: 0.85, shape: 'gin' },
            { color: 0xffffff, label: 'GREY GOOSE', height: 0.9, shape: 'vodka' },
            { color: 0x8b4513, label: 'JAMESON', height: 0.8, shape: 'whiskey' },
            { color: 0x1e3a5f, label: 'BOMBAY', height: 0.85, shape: 'gin' },
            { color: 0x2c1810, label: 'JACK DANIELS', height: 0.82, shape: 'whiskey' },
            { color: 0xb8860b, label: 'PATRON', height: 0.75, shape: 'tequila' },
            { color: 0x8fbc8f, label: 'BACARDI', height: 0.88, shape: 'rum' },
            { color: 0x4169e1, label: 'ABSOLUTE', height: 0.9, shape: 'vodka' }
        ];
        
        for (let i = 0; i < 8; i++) {
            const type = bottleTypes[i % bottleTypes.length];
            const bottle = this.createRealisticBottle(type);
            bottle.position.set(-4 + i * 1.1, height + 0.4, -7.8);
            this.bottles.push(bottle);
            this.scene.add(bottle);
        }
    }
    
    createRealisticBottle(type) {
        const bottleGroup = new THREE.Group();
        
        // 根據酒類型選擇不同的瓶型
        let bottleGeometry, neckGeometry;
        
        switch(type.shape) {
            case 'gin':
                bottleGeometry = new THREE.CylinderGeometry(0.15, 0.18, type.height, 8);
                neckGeometry = new THREE.CylinderGeometry(0.08, 0.12, 0.25, 8);
                break;
            case 'vodka':
                bottleGeometry = new THREE.CylinderGeometry(0.14, 0.14, type.height, 8);
                neckGeometry = new THREE.CylinderGeometry(0.07, 0.14, 0.3, 8);
                break;
            case 'whiskey':
                bottleGeometry = new THREE.CylinderGeometry(0.16, 0.2, type.height, 8);
                neckGeometry = new THREE.CylinderGeometry(0.08, 0.16, 0.2, 8);
                break;
            case 'tequila':
                bottleGeometry = new THREE.CylinderGeometry(0.18, 0.15, type.height, 8);
                neckGeometry = new THREE.CylinderGeometry(0.06, 0.15, 0.35, 8);
                break;
            default:
                bottleGeometry = new THREE.CylinderGeometry(0.15, 0.18, type.height, 8);
                neckGeometry = new THREE.CylinderGeometry(0.08, 0.15, 0.25, 8);
        }
        
        // 瓶身（更真實的材質）
        const bottle = new THREE.Mesh(
            bottleGeometry,
            new THREE.MeshPhongMaterial({ 
                color: type.color,
                transparent: type.color === 0xffffff ? false : true,
                opacity: type.color === 0xffffff ? 1 : 0.7,
                shininess: 120,
                specular: 0x333333,
                reflectivity: 0.3
            })
        );
        
        // 瓶頸
        const neck = new THREE.Mesh(
            neckGeometry,
            bottle.material.clone()
        );
        neck.position.y = type.height/2 + 0.125;
        
        // 更真實的瓶蓋
        const cap = new THREE.Mesh(
            new THREE.CylinderGeometry(0.09, 0.09, 0.08, 8),
            new THREE.MeshPhongMaterial({ 
                color: type.shape === 'whiskey' ? 0x2c1810 : 0x888888,
                shininess: 100,
                metalness: 0.7,
                specular: 0x444444
            })
        );
        cap.position.y = type.height/2 + 0.29;
        
        // === 更詳細的標籤系統 ===
        const createBottleLabel = (type) => {
            const labelGroup = new THREE.Group();
            
            // 主標籤背景
            const labelGeometry = new THREE.PlaneGeometry(0.28, 0.45);
            let labelColor, textColor, brandName, subText;
            
            switch(type.shape) {
                case 'gin':
                    labelColor = 0xf5f5dc; // 米白色
                    textColor = 0x0f4d2a; // 深綠色
                    brandName = type.label;
                    subText = 'LONDON DRY GIN\n47% ABV';
                    break;
                case 'vodka':
                    labelColor = 0xe6e6fa; // 淺紫色
                    textColor = 0x191970; // 深藍色
                    brandName = type.label;
                    subText = 'PREMIUM VODKA\n40% ABV';
                    break;
                case 'whiskey':
                    labelColor = 0xffd700; // 金色
                    textColor = 0x8b4513; // 棕色
                    brandName = type.label;
                    subText = 'TENNESSEE WHISKEY\n40% ABV';
                    break;
                case 'tequila':
                    labelColor = 0xfffacd; // 檸檬絲綢色
                    textColor = 0x8b0000; // 暗紅色
                    brandName = type.label;
                    subText = 'SILVER TEQUILA\n38% ABV';
                    break;
                case 'rum':
                    labelColor = 0xf4e4bc; // 淺棕色
                    textColor = 0x654321; // 深棕色
                    brandName = type.label;
                    subText = 'CARIBBEAN RUM\n37.5% ABV';
                    break;
                default:
                    labelColor = 0xf5f5dc;
                    textColor = 0x333333;
                    brandName = type.label;
                    subText = 'PREMIUM SPIRIT\n40% ABV';
            }
            
            const label = new THREE.Mesh(
                labelGeometry,
                new THREE.MeshBasicMaterial({ 
                    color: labelColor,
                    transparent: true,
                    opacity: 0.95
                })
            );
            label.position.set(0.01, 0.1, 0.181);
            label.rotation.y = -0.1;
            
            // 標籤邊框
            const labelBorder = new THREE.Mesh(
                new THREE.PlaneGeometry(0.3, 0.47),
                new THREE.MeshBasicMaterial({ 
                    color: textColor,
                    transparent: true,
                    opacity: 0.8
                })
            );
            labelBorder.position.set(0.01, 0.1, 0.18);
            labelBorder.rotation.y = -0.1;
            
            // 創建品牌Logo區域
            const logoArea = new THREE.Mesh(
                new THREE.PlaneGeometry(0.2, 0.08),
                new THREE.MeshBasicMaterial({ 
                    color: textColor,
                    transparent: true,
                    opacity: 0.9
                })
            );
            logoArea.position.set(0.011, 0.2, 0.182);
            logoArea.rotation.y = -0.1;
            
            // 年份標籤（部分酒類）
            if (type.shape === 'whiskey' || Math.random() > 0.5) {
                const vintageLabel = new THREE.Mesh(
                    new THREE.PlaneGeometry(0.12, 0.05),
                    new THREE.MeshBasicMaterial({ 
                        color: 0x8b0000,
                        transparent: true,
                        opacity: 0.9
                    })
                );
                vintageLabel.position.set(0.011, -0.1, 0.182);
                vintageLabel.rotation.y = -0.1;
                labelGroup.add(vintageLabel);
            }
            
            // 酒標裝飾線條
            for (let i = 0; i < 3; i++) {
                const decorLine = new THREE.Mesh(
                    new THREE.PlaneGeometry(0.24, 0.005),
                    new THREE.MeshBasicMaterial({ 
                        color: textColor,
                        transparent: true,
                        opacity: 0.7
                    })
                );
                decorLine.position.set(0.011, 0.05 - i * 0.08, 0.182);
                decorLine.rotation.y = -0.1;
                labelGroup.add(decorLine);
            }
            
            labelGroup.add(labelBorder);
            labelGroup.add(label);
            labelGroup.add(logoArea);
            
            return labelGroup;
        };
        
        // 液體效果（對於透明瓶子）
        if (type.color !== 0xffffff && type.shape !== 'whiskey') {
            const liquidGeometry = new THREE.CylinderGeometry(0.13, 0.16, type.height * 0.8, 8);
            const liquidColor = type.shape === 'gin' ? 0xf0f8ff : 
                              type.shape === 'tequila' ? 0xfffacd : 
                              type.shape === 'rum' ? 0xdaa520 : 0xf0f8ff;
            
            const liquid = new THREE.Mesh(
                liquidGeometry,
                new THREE.MeshPhongMaterial({
                    color: liquidColor,
                    transparent: true,
                    opacity: 0.6,
                    shininess: 80
                })
            );
            liquid.position.y = -type.height * 0.1;
            bottleGroup.add(liquid);
        }
        
        const bottleLabel = createBottleLabel(type);
        bottleGroup.add(bottleLabel);
        bottleGroup.add(bottle);
        bottleGroup.add(neck);
        bottleGroup.add(cap);
        bottleGroup.castShadow = true;
        bottleGroup.receiveShadow = true;
        
        return bottleGroup;
    }
    
    createDetailedBarTools() {
        // 專業調酒墊（防滑橡膠材質）
        const barMat = new THREE.Mesh(
            new THREE.BoxGeometry(4, 0.03, 2),
            new THREE.MeshPhongMaterial({ 
                color: 0x0f0f0f,
                roughness: 0.9,
                shininess: 5
            })
        );
        barMat.position.set(0, 1.1, -3);
        this.scene.add(barMat);
        
        // 添加墊子上的品牌Logo區域
        const logoArea = new THREE.Mesh(
            new THREE.PlaneGeometry(0.8, 0.3),
            new THREE.MeshBasicMaterial({ 
                color: 0x333333,
                transparent: true,
                opacity: 0.7
            })
        );
        logoArea.position.set(0, 1.1, -4);
        logoArea.rotation.x = -Math.PI / 2;
        this.scene.add(logoArea);
        
        // 高級Boston Shaker套組
        const shakerGroup = new THREE.Group();
        
        // 金屬搖酒器（拋光不銹鋼）
        const shakerBottom = new THREE.Mesh(
            new THREE.CylinderGeometry(0.18, 0.22, 0.65, 12),
            new THREE.MeshPhongMaterial({ 
                color: 0xe5e5e5,
                shininess: 200,
                metalness: 0.9,
                specular: 0x666666
            })
        );
        shakerBottom.position.y = 0.325;
        
        // 玻璃混合杯
        const shakerTop = new THREE.Mesh(
            new THREE.CylinderGeometry(0.15, 0.18, 0.55, 12, 1, true),
            new THREE.MeshPhongMaterial({ 
                color: 0xffffff,
                transparent: true,
                opacity: 0.15,
                shininess: 150,
                specular: 0x888888
            })
        );
        shakerTop.position.y = 0.9;
        
        // 添加搖酒器底部的測量刻度
        for (let i = 1; i <= 4; i++) {
            const scale = new THREE.Mesh(
                new THREE.RingGeometry(0.19, 0.21, 16),
                new THREE.MeshBasicMaterial({ 
                    color: 0x333333,
                    transparent: true,
                    opacity: 0.5
                })
            );
            scale.position.y = 0.1 + i * 0.12;
            scale.rotation.x = Math.PI / 2;
            shakerGroup.add(scale);
        }
        
        shakerGroup.add(shakerBottom);
        shakerGroup.add(shakerTop);
        shakerGroup.position.set(-1.8, 1.10, -2.2);
        shakerGroup.castShadow = true;
        this.scene.add(shakerGroup);

        // 儲存 Shaker 引用
        this.barTools.shaker = shakerGroup;

        // 精密雙頭量酒器
        this.createPrecisionJigger();

        // === 以下工具已暫時註解，簡化吧檯布局 ===
        // 專業螺旋攪拌匙
        // this.createProfessionalBarSpoon();

        // 高級隔冰器
        // this.createPremiumStrainer();

        // 日式調酒杯（裝飾用，已改用可互動的 Mixing Glass）
        // this.createJapaneseMixingGlass();

        // 專業搗棒
        // this.createProfessionalMuddler();

        // 添加其他專業工具
        // this.createAdditionalTools();
    }
    
    createPrecisionJigger() {
        const jiggerGroup = new THREE.Group();
        
        // 修正 Jigger 設計 - 創建一個整體的雙頭量酒器
        const jiggerBody = new THREE.Mesh(
            new THREE.CylinderGeometry(0.04, 0.04, 0.12, 12),
            new THREE.MeshPhongMaterial({ 
                color: 0xffd700,
                shininess: 150,
                metalness: 0.8
            })
        );
        jiggerBody.position.y = 0.06;
        
        // 大杯（2oz）- 向下開口
        const jiggerLarge = new THREE.Mesh(
            new THREE.ConeGeometry(0.13, 0.18, 12),
            new THREE.MeshPhongMaterial({ 
                color: 0xffd700,
                shininess: 150,
                metalness: 0.8,
                specular: 0x444444
            })
        );
        jiggerLarge.position.y = -0.09;
        
        // 小杯（1oz）- 向上開口
        const jiggerSmall = new THREE.Mesh(
            new THREE.ConeGeometry(0.09, 0.14, 12),
            new THREE.MeshPhongMaterial({ 
                color: 0xffd700,
                shininess: 150,
                metalness: 0.8
            })
        );
        jiggerSmall.position.y = 0.19;
        jiggerSmall.rotation.z = Math.PI;
        
        // 內部刻度線（只在大杯內部）
        for (let i = 1; i <= 2; i++) {
            const innerMark = new THREE.Mesh(
                new THREE.RingGeometry(0.08 + i * 0.02, 0.09 + i * 0.02, 12),
                new THREE.MeshBasicMaterial({ color: 0x888888 })
            );
            innerMark.position.y = -0.15 + i * 0.05;
            innerMark.rotation.x = Math.PI / 2;
            jiggerGroup.add(innerMark);
        }
        
        jiggerGroup.add(jiggerBody);
        jiggerGroup.add(jiggerLarge);
        jiggerGroup.add(jiggerSmall);
        jiggerGroup.position.set(-0.6, 1.18, -2.1); // 提高位置避免穿模
        jiggerGroup.castShadow = true;
        this.scene.add(jiggerGroup);

        // 儲存 Jigger 引用
        this.barTools.jigger = jiggerGroup;
    }
    
    createProfessionalBarSpoon() {
        const spoonGroup = new THREE.Group();
        
        // 螺旋手柄（更精細的螺旋）
        const spoonCurve = new THREE.CatmullRomCurve3([
            new THREE.Vector3(0, 0, 0),
            new THREE.Vector3(0.03, 0.2, 0.015),
            new THREE.Vector3(-0.02, 0.4, -0.01),
            new THREE.Vector3(0.025, 0.6, 0.012),
            new THREE.Vector3(-0.015, 0.8, -0.008),
            new THREE.Vector3(0.02, 1.0, 0.01),
            new THREE.Vector3(0, 1.2, 0)
        ]);
        
        const spoonGeometry = new THREE.TubeGeometry(spoonCurve, 30, 0.018, 10, false);
        const spoonHandle = new THREE.Mesh(
            spoonGeometry,
            new THREE.MeshPhongMaterial({ 
                color: 0xc0c0c0,
                shininess: 180,
                metalness: 0.7
            })
        );
        
        // 更真實的匙頭
        const spoonHead = new THREE.Mesh(
            new THREE.SphereGeometry(0.045, 12, 8),
            new THREE.MeshPhongMaterial({ 
                color: 0xc0c0c0,
                shininess: 180
            })
        );
        spoonHead.scale.set(1, 0.25, 0.8);
        spoonHead.position.y = 1.22;
        
        // 叉子端（另一端）
        const forkTines = new THREE.Group();
        const tineGeometry = new THREE.CylinderGeometry(0.005, 0.005, 0.08);
        const tineMaterial = new THREE.MeshPhongMaterial({ color: 0xc0c0c0, shininess: 180 });
        
        for (let i = -1; i <= 1; i++) {
            const tine = new THREE.Mesh(tineGeometry, tineMaterial);
            tine.position.set(i * 0.015, 0.04, 0);
            forkTines.add(tine);
        }
        
        forkTines.position.y = -0.04;
        
        spoonGroup.add(spoonHandle);
        spoonGroup.add(spoonHead);
        spoonGroup.add(forkTines);
        spoonGroup.position.set(0.6, 1.18, -2.3); // 提高位置避免穿模
        spoonGroup.rotation.z = -Math.PI / 15;
        spoonGroup.castShadow = true;
        this.scene.add(spoonGroup);
    }
    
    createPremiumStrainer() {
        const strainerGroup = new THREE.Group();
        
        // 主體圓盤
        const strainerPlate = new THREE.Mesh(
            new THREE.CylinderGeometry(0.22, 0.22, 0.025, 20),
            new THREE.MeshPhongMaterial({ 
                color: 0xd0d0d0,
                shininess: 150,
                metalness: 0.6
            })
        );
        
        // 濾孔
        for (let ring = 0; ring < 3; ring++) {
            const radius = 0.05 + ring * 0.05;
            const holes = 6 + ring * 4;
            
            for (let i = 0; i < holes; i++) {
                const angle = (i / holes) * Math.PI * 2;
                const hole = new THREE.Mesh(
                    new THREE.CylinderGeometry(0.008, 0.008, 0.03),
                    new THREE.MeshBasicMaterial({ color: 0x000000 })
                );
                hole.position.set(
                    Math.cos(angle) * radius,
                    0,
                    Math.sin(angle) * radius
                );
                strainerGroup.add(hole);
            }
        }
        
        // 彈簧圈（更精細）
        const springGeometry = new THREE.TorusGeometry(0.2, 0.012, 6, 24);
        const spring = new THREE.Mesh(
            springGeometry,
            new THREE.MeshPhongMaterial({ 
                color: 0xb0b0b0,
                shininess: 120
            })
        );
        spring.rotation.x = Math.PI / 2;
        spring.position.y = -0.015;
        
        // 人體工學把手
        const handleGeometry = new THREE.BoxGeometry(0.35, 0.025, 0.08);
        const handle = new THREE.Mesh(
            handleGeometry,
            new THREE.MeshPhongMaterial({ 
                color: 0xd0d0d0,
                shininess: 150
            })
        );
        handle.position.set(0.28, 0, 0);
        
        // 把手紋理
        const gripTexture = new THREE.Mesh(
            new THREE.BoxGeometry(0.25, 0.03, 0.06),
            new THREE.MeshPhongMaterial({ 
                color: 0x999999,
                roughness: 0.8
            })
        );
        gripTexture.position.set(0.28, 0.001, 0);
        
        strainerGroup.add(strainerPlate);
        strainerGroup.add(spring);
        strainerGroup.add(handle);
        strainerGroup.add(gripTexture);
        strainerGroup.position.set(1.6, 1.20, -2.3); // 提高位置避免穿模
        strainerGroup.castShadow = true;
        this.scene.add(strainerGroup);
    }
    
    createJapaneseMixingGlass() {
        const mixingGlass = new THREE.Mesh(
            new THREE.CylinderGeometry(0.19, 0.16, 0.65, 16, 1, true),
            new THREE.MeshPhongMaterial({ 
                color: 0xffffff,
                transparent: true,
                opacity: 0.12,
                shininess: 200,
                specular: 0xaaaaaa,
                side: THREE.DoubleSide
            })
        );
        
        // 杯底加厚
        const glassBase = new THREE.Mesh(
            new THREE.CylinderGeometry(0.16, 0.16, 0.08),
            new THREE.MeshPhongMaterial({ 
                color: 0xffffff,
                transparent: true,
                opacity: 0.2,
                shininess: 200
            })
        );
        glassBase.position.y = 0.04;
        
        const glassGroup = new THREE.Group();
        glassGroup.add(mixingGlass);
        glassGroup.add(glassBase);
        glassGroup.position.set(2.6, 1.39, -2.4);
        glassGroup.castShadow = true;
        this.scene.add(glassGroup);
    }
    
    createProfessionalMuddler() {
        const muddlerGroup = new THREE.Group();
        
        // 手柄（楓木紋理）
        const muddlerBody = new THREE.Mesh(
            new THREE.CylinderGeometry(0.035, 0.042, 0.85, 12),
            new THREE.MeshPhongMaterial({ 
                color: 0xa0522d,
                shininess: 40,
                specular: 0x222222
            })
        );
        muddlerBody.position.y = 0.425;
        
        // 搗碎端（不銹鋼）
        const muddlerHead = new THREE.Mesh(
            new THREE.CylinderGeometry(0.055, 0.055, 0.12),
            new THREE.MeshPhongMaterial({ 
                color: 0xc0c0c0,
                shininess: 150,
                metalness: 0.8
            })
        );
        muddlerHead.position.y = 0.91;
        
        // 手柄紋理環
        for (let i = 0; i < 8; i++) {
            const ring = new THREE.Mesh(
                new THREE.TorusGeometry(0.04, 0.003, 4, 12),
                new THREE.MeshPhongMaterial({ color: 0x8b4513 })
            );
            ring.position.y = 0.1 + i * 0.1;
            ring.rotation.x = Math.PI / 2;
            muddlerGroup.add(ring);
        }
        
        muddlerGroup.add(muddlerBody);
        muddlerGroup.add(muddlerHead);
        muddlerGroup.position.set(-2.6, 1.18, -2.4); // 提高位置避免穿模
        muddlerGroup.rotation.z = Math.PI / 12;
        muddlerGroup.castShadow = true;
        this.scene.add(muddlerGroup);
    }
    
    createAdditionalTools() {
        // 檸檬切片器
        const citrusZester = new THREE.Group();
        const zesterHandle = new THREE.Mesh(
            new THREE.CylinderGeometry(0.02, 0.02, 0.4),
            new THREE.MeshPhongMaterial({ color: 0x333333 })
        );
        const zesterHead = new THREE.Mesh(
            new THREE.BoxGeometry(0.12, 0.02, 0.08),
            new THREE.MeshPhongMaterial({ color: 0xc0c0c0, shininess: 150 })
        );
        zesterHead.position.set(0, 0.21, 0);
        citrusZester.add(zesterHandle);
        citrusZester.add(zesterHead);
        citrusZester.position.set(0.2, 1.38, -2.8); // 提高位置避免穿模
        citrusZester.rotation.z = Math.PI / 6;
        this.scene.add(citrusZester);
        
        // 開瓶器
        const bottleOpener = new THREE.Group();
        const openerBody = new THREE.Mesh(
            new THREE.BoxGeometry(0.3, 0.02, 0.06),
            new THREE.MeshPhongMaterial({ 
                color: 0xc0c0c0,
                shininess: 150,
                metalness: 0.8
            })
        );
        
        const openerHook = new THREE.Mesh(
            new THREE.TorusGeometry(0.04, 0.01, 4, 8, Math.PI),
            new THREE.MeshPhongMaterial({ 
                color: 0xc0c0c0,
                shininess: 150
            })
        );
        openerHook.position.set(0.12, 0, 0);
        openerHook.rotation.z = Math.PI / 2;
        
        bottleOpener.add(openerBody);
        bottleOpener.add(openerHook);
        bottleOpener.position.set(-0.2, 1.20, -2.8); // 提高位置避免穿模
        this.scene.add(bottleOpener);
        
        // 雞尾酒挑選工具
        const cocktailPicks = new THREE.Group();
        for (let i = 0; i < 5; i++) {
            const pick = new THREE.Mesh(
                new THREE.CylinderGeometry(0.002, 0.002, 0.15),
                new THREE.MeshPhongMaterial({ color: 0x8b4513 })
            );
            pick.position.set(i * 0.02 - 0.04, 0.075, 0);
            
            const pickHead = new THREE.Mesh(
                new THREE.SphereGeometry(0.01),
                new THREE.MeshPhongMaterial({ color: 0xff6347 })
            );
            pickHead.position.set(i * 0.02 - 0.04, 0.15, 0);
            
            cocktailPicks.add(pick);
            cocktailPicks.add(pickHead);
        }
        
        const pickHolder = new THREE.Mesh(
            new THREE.CylinderGeometry(0.08, 0.08, 0.03),
            new THREE.MeshPhongMaterial({ color: 0x333333 })
        );
        
        cocktailPicks.add(pickHolder);
        cocktailPicks.position.set(3.2, 1.20, -2.6); // 提高位置避免穿模
        this.scene.add(cocktailPicks);
    }
    
    createBarStools() {
        for (let i = 0; i < 4; i++) {
            const stool = new THREE.Group();
            
            // 更舒適的軟墊座椅
            const seatPadding = new THREE.Mesh(
                new THREE.CylinderGeometry(0.42, 0.37, 0.08),
                new THREE.MeshPhongMaterial({ 
                    color: 0x722f37,
                    shininess: 30
                })
            );
            seatPadding.position.y = 0.82;
            
            const seatBase = new THREE.Mesh(
                new THREE.CylinderGeometry(0.4, 0.35, 0.05),
                new THREE.MeshPhongMaterial({ 
                    color: 0x333333,
                    shininess: 80
                })
            );
            seatBase.position.y = 0.775;
            
            // 更穩固的椅腿設計
            const centerPost = new THREE.Mesh(
                new THREE.CylinderGeometry(0.06, 0.06, 0.75),
                new THREE.MeshPhongMaterial({ 
                    color: 0x2c2c2c,
                    shininess: 100,
                    metalness: 0.6
                })
            );
            centerPost.position.y = 0.375;
            
            // 腳踏環
            const footRest = new THREE.Mesh(
                new THREE.TorusGeometry(0.25, 0.02, 8, 16),
                new THREE.MeshPhongMaterial({ 
                    color: 0x2c2c2c,
                    shininess: 100
                })
            );
            footRest.position.y = 0.25;
            footRest.rotation.x = Math.PI / 2;
            
            // 底座
            const base = new THREE.Mesh(
                new THREE.CylinderGeometry(0.3, 0.3, 0.05),
                new THREE.MeshPhongMaterial({ 
                    color: 0x2c2c2c,
                    shininess: 100
                })
            );
            base.position.y = 0.025;
            
            stool.add(seatPadding);
            stool.add(seatBase);
            stool.add(centerPost);
            stool.add(footRest);
            stool.add(base);
            stool.position.set(-3 + i * 2, 0, 0);
            stool.castShadow = true;
            this.scene.add(stool);
        }
    }
    
    createBarDecoration() {
        // 天花板裝飾
        this.createCeilingDecor();
        
        // 牆壁裝飾
        this.createWallDecor();
        
        // 地面細節
        this.createFloorDetails();
        
        // 酒吧氛圍物件
        this.createAtmosphereItems();
        this.createGreenPlants();
        this.createMusicCorner();
    }
    
    createGreenPlants() {
        // 角落大型龜背竹
        const monsteraGroup = new THREE.Group();
        
        // 花盆
        const pot1 = new THREE.Mesh(
            new THREE.CylinderGeometry(0.8, 0.6, 0.6),
            new THREE.MeshPhongMaterial({ 
                color: 0x654321,
                shininess: 40
            })
        );
        pot1.position.y = 0.3;
        
        // 土壤
        const soil1 = new THREE.Mesh(
            new THREE.CylinderGeometry(0.75, 0.75, 0.05),
            new THREE.MeshPhongMaterial({ 
                color: 0x3e2723,
                roughness: 0.9
            })
        );
        soil1.position.y = 0.58;
        
        // 龜背竹葉子（多片大型葉片）
        for (let i = 0; i < 8; i++) {
            const leafGroup = new THREE.Group();
            
            // 主葉片
            const leaf = new THREE.Mesh(
                new THREE.SphereGeometry(0.6, 8, 6),
                new THREE.MeshPhongMaterial({ 
                    color: 0x2e7d32,
                    shininess: 30,
                    side: THREE.DoubleSide
                })
            );
            leaf.scale.set(1, 0.02, 1.2);
            
            // 葉脈紋理
            const vein = new THREE.Mesh(
                new THREE.PlaneGeometry(1, 0.02),
                new THREE.MeshPhongMaterial({ 
                    color: 0x1b5e20,
                    transparent: true,
                    opacity: 0.8
                })
            );
            vein.position.y = 0.01;
            
            // 葉柄
            const stem = new THREE.Mesh(
                new THREE.CylinderGeometry(0.02, 0.04, 0.5),
                new THREE.MeshPhongMaterial({ color: 0x4caf50 })
            );
            stem.position.y = -0.25;
            stem.rotation.x = Math.PI / 6;
            
            leafGroup.add(leaf);
            leafGroup.add(vein);
            leafGroup.add(stem);
            
            // 隨機分佈葉子
            const angle = (i / 8) * Math.PI * 2 + Math.random() * 0.5;
            const height = 0.8 + Math.random() * 1.2;
            const distance = 0.3 + Math.random() * 0.4;
            
            leafGroup.position.set(
                Math.cos(angle) * distance,
                height,
                Math.sin(angle) * distance
            );
            leafGroup.rotation.set(
                Math.random() * 0.5 - 0.25,
                angle + Math.random() * 0.5,
                Math.random() * 0.3 - 0.15
            );
            
            monsteraGroup.add(leafGroup);
        }
        
        monsteraGroup.add(pot1);
        monsteraGroup.add(soil1);
        monsteraGroup.position.set(-8, 0, 7);
        monsteraGroup.castShadow = true;
        this.scene.add(monsteraGroup);
        
    
        // 吊掛的常春藤
        const hangingPlant = new THREE.Group();
        
        // 吊盆
        const hangingPot = new THREE.Mesh(
            new THREE.SphereGeometry(0.4, 10, 6),
            new THREE.MeshPhongMaterial({ 
                color: 0x795548,
                shininess: 40
            })
        );
        hangingPot.scale.set(1, 0.8, 1);
        
        // 吊繩
        const rope = new THREE.Mesh(
            new THREE.CylinderGeometry(0.02, 0.02, 2),
            new THREE.MeshPhongMaterial({ color: 0x5d4037 })
        );
        rope.position.y = 1;
        
        // 下垂的藤蔓
        for (let i = 0; i < 6; i++) {
            const vine = new THREE.Group();
            
            for (let j = 0; j < 15; j++) {
                const vineLeaf = new THREE.Mesh(
                    new THREE.SphereGeometry(0.12, 6, 4),
                    new THREE.MeshPhongMaterial({ 
                        color: 0x4caf50,
                        shininess: 25
                    })
                );
                vineLeaf.scale.set(1.2, 0.02, 0.8);
                vineLeaf.position.set(
                    Math.sin(j * 0.3) * 0.1,
                    -j * 0.15,
                    Math.cos(j * 0.3) * 0.1
                );
                vine.add(vineLeaf);
            }
            
            const vineAngle = (i / 6) * Math.PI * 2;
            vine.position.set(
                Math.cos(vineAngle) * 0.3,
                0,
                Math.sin(vineAngle) * 0.3
            );
            
            hangingPlant.add(vine);
        }
        
        hangingPlant.add(hangingPot);
        hangingPlant.add(rope);
        hangingPlant.position.set(-7, 6, -7);
        this.scene.add(hangingPlant);
    }

    createMusicCorner() {
        const musicSetup = new THREE.Group();
        
        // 電吉他架
        const guitarStand = new THREE.Group();
        
        // 支架底座
        const standBase = new THREE.Mesh(
            new THREE.CylinderGeometry(0.3, 0.3, 0.05),
            new THREE.MeshPhongMaterial({ 
                color: 0x2c2c2c,
                shininess: 80,
                metalness: 0.7
            })
        );
        standBase.position.y = 0.025;
        
        // 支架主桿
        const standPole = new THREE.Mesh(
            new THREE.CylinderGeometry(0.03, 0.03, 1.5),
            new THREE.MeshPhongMaterial({ 
                color: 0x2c2c2c,
                shininess: 80
            })
        );
        standPole.position.y = 0.75;
        
        // 吉他支撐臂
        const supportArm = new THREE.Mesh(
            new THREE.CylinderGeometry(0.02, 0.02, 0.4),
            new THREE.MeshPhongMaterial({ color: 0x2c2c2c })
        );
        supportArm.position.set(0.15, 1.3, 0);
        supportArm.rotation.z = Math.PI / 6;
        
        guitarStand.add(standBase);
        guitarStand.add(standPole);
        guitarStand.add(supportArm);
        
        // 電吉他本體
        const guitar = new THREE.Group();
        
        // 吉他琴身
        const guitarBody = new THREE.Mesh(
            new THREE.BoxGeometry(0.8, 0.05, 1.2),
            new THREE.MeshPhongMaterial({ 
                color: 0x8b0000,
                shininess: 100,
                specular: 0x444444
            })
        );
        guitarBody.position.y = 1;
        
        // 琴頸
        const guitarNeck = new THREE.Mesh(
            new THREE.BoxGeometry(0.15, 0.04, 1.8),
            new THREE.MeshPhongMaterial({ 
                color: 0x8b4513,
                shininess: 80
            })
        );
        guitarNeck.position.set(0, 1.02, 1.5);
        
        // 琴頭
        const guitarHead = new THREE.Mesh(
            new THREE.BoxGeometry(0.2, 0.05, 0.4),
            new THREE.MeshPhongMaterial({ 
                color: 0x8b4513,
                shininess: 80
            })
        );
        guitarHead.position.set(0, 1.02, 2.6);
        
        // 弦鈕（調音鈕）
        for (let i = 0; i < 6; i++) {
            const tuningPeg = new THREE.Mesh(
                new THREE.CylinderGeometry(0.02, 0.02, 0.08),
                new THREE.MeshPhongMaterial({ 
                    color: 0xc0c0c0,
                    shininess: 150
                })
            );
            tuningPeg.position.set(
                (i % 2 === 0 ? -0.08 : 0.08),
                1.02,
                2.45 + (Math.floor(i / 2)) * 0.08
            );
            tuningPeg.rotation.z = Math.PI / 2;
            guitar.add(tuningPeg);
        }
        
        // 吉他弦
        for (let i = 0; i < 6; i++) {
            const string = new THREE.Mesh(
                new THREE.CylinderGeometry(0.001, 0.001, 2.2),
                new THREE.MeshBasicMaterial({ color: 0xc0c0c0 })
            );
            string.position.set(
                -0.05 + i * 0.02,
                1.025,
                1.6
            );
            string.rotation.x = Math.PI / 2;
            guitar.add(string);
        }
        
        // 拾音器
        for (let i = 0; i < 3; i++) {
            const pickup = new THREE.Mesh(
                new THREE.BoxGeometry(0.12, 0.01, 0.8),
                new THREE.MeshPhongMaterial({ 
                    color: 0x1a1a1a,
                    shininess: 60
                })
            );
            pickup.position.set(0, 1.025, 0.4 + i * 0.3);
            guitar.add(pickup);
        }
        
        // 控制旋鈕
        for (let i = 0; i < 4; i++) {
            const knob = new THREE.Mesh(
                new THREE.CylinderGeometry(0.03, 0.03, 0.02),
                new THREE.MeshPhongMaterial({ 
                    color: 0x333333,
                    shininess: 80
                })
            );
            knob.position.set(
                -0.2 + i * 0.08,
                1.03,
                -0.3
            );
            guitar.add(knob);
        }
        
        guitar.add(guitarBody);
        guitar.add(guitarNeck);
        guitar.add(guitarHead);
        guitar.rotation.z = -Math.PI / 12;
        guitar.position.set(0.15, 0, 0);
        
        guitarStand.add(guitar);
        guitarStand.position.set(7, 0, -6);
        musicSetup.add(guitarStand);

        // 保存吉他引用供後續互動使用
        this.guitar = guitar;
        
        // 音箱系統
        const ampSystem = new THREE.Group();
        
        // 主音箱
        const mainAmp = new THREE.Mesh(
            new THREE.BoxGeometry(1.2, 1.5, 0.8),
            new THREE.MeshPhongMaterial({ 
                color: 0x1a1a1a,
                shininess: 40
            })
        );
        mainAmp.position.y = 0.75;
        
        // 音箱網格
        const speakerGrill = new THREE.Mesh(
            new THREE.PlaneGeometry(1, 1.2),
            new THREE.MeshPhongMaterial({ 
                color: 0x333333,
                transparent: true,
                opacity: 0.8
            })
        );
        speakerGrill.position.set(0, 0.75, 0.41);
        
        // 喇叭單體
        for (let i = 0; i < 4; i++) {
            const speaker = new THREE.Mesh(
                new THREE.CylinderGeometry(0.15, 0.15, 0.05),
                new THREE.MeshPhongMaterial({ 
                    color: 0x2c2c2c,
                    shininess: 20
                })
            );
            speaker.position.set(
                (i % 2 === 0 ? -0.25 : 0.25),
                0.4 + Math.floor(i / 2) * 0.7,
                0.42
            );
            speaker.rotation.x = Math.PI / 2;
            ampSystem.add(speaker);
            
            // 喇叭錐盆
            const cone = new THREE.Mesh(
                new THREE.CylinderGeometry(0.08, 0.12, 0.03),
                new THREE.MeshPhongMaterial({ 
                    color: 0x4a4a4a,
                    shininess: 10
                })
            );
            cone.position.set(
                (i % 2 === 0 ? -0.25 : 0.25),
                0.4 + Math.floor(i / 2) * 0.7,
                0.44
            );
            cone.rotation.x = Math.PI / 2;
            ampSystem.add(cone);
        }
        
        // 控制面板
        const controlPanel = new THREE.Mesh(
            new THREE.PlaneGeometry(0.8, 0.3),
            new THREE.MeshPhongMaterial({ 
                color: 0x444444,
                shininess: 60
            })
        );
        controlPanel.position.set(0, 1.3, 0.41);
        
        // 控制旋鈕
        for (let i = 0; i < 6; i++) {
            const ampKnob = new THREE.Mesh(
                new THREE.CylinderGeometry(0.03, 0.03, 0.02),
                new THREE.MeshPhongMaterial({ 
                    color: 0x666666,
                    shininess: 100
                })
            );
            ampKnob.position.set(
                -0.3 + i * 0.12,
                1.35,
                0.42
            );
            ampSystem.add(ampKnob);
            
            // 刻度指示
            const indicator = new THREE.Mesh(
                new THREE.BoxGeometry(0.01, 0.005, 0.015),
                new THREE.MeshBasicMaterial({ color: 0xffffff })
            );
            indicator.position.set(
                -0.3 + i * 0.12,
                1.37,
                0.425
            );
            ampSystem.add(indicator);
        }
        
        // LED 指示燈
        const powerLED = new THREE.Mesh(
            new THREE.SphereGeometry(0.015),
            new THREE.MeshBasicMaterial({ 
                color: 0x00ff00,
                transparent: true,
                opacity: 0.8
            })
        );
        powerLED.position.set(0.35, 1.35, 0.42);
        ampSystem.add(powerLED);
        
        // 品牌Logo區域
        const logo = new THREE.Mesh(
            new THREE.PlaneGeometry(0.3, 0.08),
            new THREE.MeshBasicMaterial({ 
                color: 0xffd700,
                transparent: true,
                opacity: 0.9
            })
        );
        logo.position.set(0, 1.45, 0.41);
        
        ampSystem.add(mainAmp);
        ampSystem.add(speakerGrill);
        ampSystem.add(controlPanel);
        ampSystem.add(logo);
        ampSystem.position.set(8, 0, -8);
        
        // 音箱線材
        const cable = new THREE.Group();
        
        // 創建彎曲的電線
        const cableCurve = new THREE.CatmullRomCurve3([
            new THREE.Vector3(7.2, 0.5, -6),
            new THREE.Vector3(7.5, 0.3, -6.5),
            new THREE.Vector3(7.8, 0.2, -7),
            new THREE.Vector3(8, 0.2, -7.5)
        ]);
        
        const cableGeometry = new THREE.TubeGeometry(cableCurve, 20, 0.02, 8, false);
        const cableWire = new THREE.Mesh(
            cableGeometry,
            new THREE.MeshPhongMaterial({ 
                color: 0x1a1a1a,
                shininess: 20
            })
        );
        
        cable.add(cableWire);
        
        // 連接頭
        const connector = new THREE.Mesh(
            new THREE.CylinderGeometry(0.03, 0.03, 0.08),
            new THREE.MeshPhongMaterial({ 
                color: 0x333333,
                shininess: 80
            })
        );
        connector.position.set(7.2, 0.5, -6);
        connector.rotation.z = Math.PI / 2;
        cable.add(connector);
        
        musicSetup.add(ampSystem);
        musicSetup.add(cable);
        
        // 副音箱（監聽音箱）
        const monitorSpeaker = new THREE.Mesh(
            new THREE.BoxGeometry(0.6, 0.8, 0.5),
            new THREE.MeshPhongMaterial({ 
                color: 0x2c2c2c,
                shininess: 60
            })
        );
        monitorSpeaker.position.set(6, 0.4, -7.5);
        
        const monitorGrill = new THREE.Mesh(
            new THREE.PlaneGeometry(0.4, 0.6),
            new THREE.MeshPhongMaterial({ 
                color: 0x444444,
                transparent: true,
                opacity: 0.7
            })
        );
        monitorGrill.position.set(6, 0.4, -7.24);
        
        const monitorCone = new THREE.Mesh(
            new THREE.CylinderGeometry(0.12, 0.12, 0.03),
            new THREE.MeshPhongMaterial({ 
                color: 0x1a1a1a,
                shininess: 10
            })
        );
        monitorCone.position.set(6, 0.4, -7.23);
        monitorCone.rotation.x = Math.PI / 2;
        
        musicSetup.add(monitorSpeaker);
        musicSetup.add(monitorGrill);
        musicSetup.add(monitorCone);
        
        musicSetup.castShadow = true;
        this.scene.add(musicSetup);
        
        // 地毯裝飾
        const musicCarpet = new THREE.Mesh(
            new THREE.PlaneGeometry(4, 3),
            new THREE.MeshPhongMaterial({ 
                color: 0x4a148c,
                shininess: 10,
                roughness: 0.9
            })
        );
        musicCarpet.rotation.x = -Math.PI / 2;
        musicCarpet.position.set(7, 0.01, -7);
        this.scene.add(musicCarpet);
    }

    createCeilingDecor() {
        // 工業風懸吊燈具
        for (let i = 0; i < 3; i++) {
            const lampGroup = new THREE.Group();
            
            // 燈罩
            const lampShade = new THREE.Mesh(
                new THREE.ConeGeometry(0.4, 0.3, 8, 1, true),
                new THREE.MeshPhongMaterial({ 
                    color: 0x2c2c2c,
                    side: THREE.DoubleSide
                })
            );
            lampShade.position.y = -0.15;
            
            // 燈泡
            const bulb = new THREE.Mesh(
                new THREE.SphereGeometry(0.08),
                new THREE.MeshBasicMaterial({ 
                    color: 0xfff8dc,
                    transparent: true,
                    opacity: 0.8
                })
            );
            bulb.position.y = -0.1;
            
            // 懸吊線
            const wire = new THREE.Mesh(
                new THREE.CylinderGeometry(0.005, 0.005, 2),
                new THREE.MeshBasicMaterial({ color: 0x333333 })
            );
            wire.position.y = 1;
            
            lampGroup.add(lampShade);
            lampGroup.add(bulb);
            lampGroup.add(wire);
            lampGroup.position.set(-4 + i * 4, 7, -1);
            this.scene.add(lampGroup);
        }
        
        // 移除原來的裸露管線（它穿過了整個地圖）
        // 改為添加天花板橫樑裝飾
        const ceilingBeam = new THREE.Mesh(
            new THREE.BoxGeometry(12, 0.2, 0.3),
            new THREE.MeshPhongMaterial({ 
                color: 0x4a3c28,
                shininess: 20
            })
        );
        ceilingBeam.position.set(0, 8.5, -3);
        this.scene.add(ceilingBeam);
    }
    
    createWallDecor() {
        const menuGroup = new THREE.Group();
        
        // 黑板背景
        const chalkboard = new THREE.Mesh(
            new THREE.PlaneGeometry(3.5, 4),
            new THREE.MeshPhongMaterial({ 
                color: 0x1a2f1a,
                shininess: 5,
                roughness: 0.9
            })
        );
        
        // 黑板框架
        const chalkboardFrame = new THREE.Mesh(
            new THREE.PlaneGeometry(3.7, 4.2),
            new THREE.MeshPhongMaterial({ 
                color: 0x8b4513,
                shininess: 40
            })
        );
        chalkboardFrame.position.z = -0.01;
        
        // 標題區域
        const titleArea = new THREE.Mesh(
            new THREE.PlaneGeometry(3, 0.6),
            new THREE.MeshBasicMaterial({ 
                color: 0xffd700,
                transparent: true,
                opacity: 0.8
            })
        );
        titleArea.position.set(0, 1.5, 0.01);
        
        // 分類標題背景
        const categories = [
            { name: '經典調酒', y: 0.8, color: 0xff6b6b },
            { name: '分子創意', y: 0.2, color: 0x4ecdc4 },
            { name: '無酒精', y: -0.4, color: 0x45b7d1 },
            { name: '限定特調', y: -1.0, color: 0x96ceb4 }
        ];
        
        categories.forEach(category => {
            const categoryBg = new THREE.Mesh(
                new THREE.PlaneGeometry(3, 0.4),
                new THREE.MeshBasicMaterial({ 
                    color: category.color,
                    transparent: true,
                    opacity: 0.3
                })
            );
            categoryBg.position.set(0, category.y, 0.005);
            menuGroup.add(categoryBg);
            
            // 分類標題文字區域
            const categoryTitle = new THREE.Mesh(
                new THREE.PlaneGeometry(1.5, 0.15),
                new THREE.MeshBasicMaterial({ 
                    color: 0xffffff,
                    transparent: true,
                    opacity: 0.9
                })
            );
            categoryTitle.position.set(-0.8, category.y + 0.08, 0.01);
            menuGroup.add(categoryTitle);
        });
        
        // 創意菜單項目
        const menuItems = [
            // 經典調酒
            { name: 'Old Fashioned 懷舊經典', price: '$280', y: 0.65, creativity: '威士忌 × 苦精 × 時光' },
            { name: 'Negroni 義式苦甜', price: '$320', y: 0.5, creativity: '琴酒 × 金巴利 × 甜苦艾' },
            
            // 分子創意
            { name: '液氮馬丁尼', price: '$450', y: 0.05, creativity: '-196°C 的浪漫邂逅' },
            { name: '煙燻威士忌球', price: '$520', y: -0.1, creativity: '威士忌包裹在煙霧中的驚喜' },
            
            // 無酒精
            { name: '植感綠洲', price: '$180', y: -0.55, creativity: '薄荷 × 檸檬草 × 氣泡' },
            { name: '紫蘇莓果氣泡', price: '$200', y: -0.7, creativity: '日式紫蘇遇見北歐莓果' },
            
            // 限定特調
            { name: 'NCU 校園回憶', price: '$399', y: -1.15, creativity: '只有我們懂的青春滋味' },
            { name: '中大湖畔夕陽', price: '$350', y: -1.3, creativity: '橙色漸層如湖光山色' }
        ];
        
        menuItems.forEach(item => {
            // 項目名稱背景
            const itemBg = new THREE.Mesh(
                new THREE.PlaneGeometry(2.8, 0.12),
                new THREE.MeshBasicMaterial({ 
                    color: 0xffffff,
                    transparent: true,
                    opacity: 0.1
                })
            );
            itemBg.position.set(0.1, item.y, 0.008);
            menuGroup.add(itemBg);
            
            // 價格標籤
            const priceTag = new THREE.Mesh(
                new THREE.PlaneGeometry(0.6, 0.08),
                new THREE.MeshBasicMaterial({ 
                    color: 0xffd700,
                    transparent: true,
                    opacity: 0.8
                })
            );
            priceTag.position.set(1.2, item.y, 0.009);
            menuGroup.add(priceTag);
            
            // 創意描述區域
            if (item.creativity) {
                const creativityArea = new THREE.Mesh(
                    new THREE.PlaneGeometry(2.5, 0.04),
                    new THREE.MeshBasicMaterial({ 
                        color: 0x98fb98,
                        transparent: true,
                        opacity: 0.6
                    })
                );
                creativityArea.position.set(0, item.y - 0.08, 0.007);
                menuGroup.add(creativityArea);
            }
        });
        
        // 特殊裝飾元素
        // 粉筆灰塵效果
        for (let i = 0; i < 20; i++) {
            const chalkDust = new THREE.Mesh(
                new THREE.SphereGeometry(0.005 + Math.random() * 0.01),
                new THREE.MeshBasicMaterial({ 
                    color: 0xffffff,
                    transparent: true,
                    opacity: 0.3 + Math.random() * 0.4
                })
            );
            chalkDust.position.set(
                -1.5 + Math.random() * 3,
                -1.8 + Math.random() * 0.3,
                0.01
            );
            menuGroup.add(chalkDust);
        }
        
        // 黑板邊角裝飾
        const cornerDecors = [
            { x: -1.6, y: 1.8 }, { x: 1.6, y: 1.8 },
            { x: -1.6, y: -1.8 }, { x: 1.6, y: -1.8 }
        ];
        
        cornerDecors.forEach(corner => {
            const decor = new THREE.Mesh(
                new THREE.RingGeometry(0.08, 0.12, 8),
                new THREE.MeshBasicMaterial({ 
                    color: 0xffd700,
                    transparent: true,
                    opacity: 0.7
                })
            );
            decor.position.set(corner.x, corner.y, 0.01);
            menuGroup.add(decor);
        });
        
        // 今日特價標籤
        const specialOfferBanner = new THREE.Mesh(
            new THREE.PlaneGeometry(1, 0.3),
            new THREE.MeshBasicMaterial({ 
                color: 0xff4757,
                transparent: true,
                opacity: 0.9
            })
        );
        specialOfferBanner.position.set(1.3, -1.7, 0.02);
        specialOfferBanner.rotation.z = -Math.PI / 12;
        
        const specialText = new THREE.Mesh(
            new THREE.PlaneGeometry(0.8, 0.15),
            new THREE.MeshBasicMaterial({ 
                color: 0xffffff,
                transparent: true,
                opacity: 0.95
            })
        );
        specialText.position.set(1.3, -1.7, 0.021);
        specialText.rotation.z = -Math.PI / 12;

        menuGroup.add(chalkboardFrame);
        menuGroup.add(chalkboard);
        menuGroup.add(titleArea);
        menuGroup.add(specialOfferBanner);
        menuGroup.add(specialText);
        menuGroup.position.set(8, 5, -9.9);
        //this.scene.add(menuGroup);


        // 復古酒類海報
        for (let i = 0; i < 3; i++) {
            const poster = new THREE.Mesh(
                new THREE.PlaneGeometry(1.2, 1.6),
                new THREE.MeshPhongMaterial({ 
                    color: i === 0 ? 0x8b0000 : i === 1 ? 0x006400 : 0x000080,
                    shininess: 10
                })
            );
            poster.position.set(-6 + i * 6, 4, -9.9);
            this.scene.add(poster);
            
            // 海報框架
            const frame = new THREE.Mesh(
                new THREE.PlaneGeometry(1.3, 1.7),
                new THREE.MeshPhongMaterial({ color: 0x8b4513 })
            );
            frame.position.set(-6 + i * 6, 4, -9.95);
            this.scene.add(frame);
        }
        
        // 霓虹燈招牌
        const neonSign = new THREE.Group();
        
        // 背板
        const signBack = new THREE.Mesh(
            new THREE.PlaneGeometry(3, 1),
            new THREE.MeshBasicMaterial({ color: 0x1a1a1a })
        );
        
        // 霓虹管
        const neonTube = new THREE.Mesh(
            new THREE.TorusGeometry(0.8, 0.03, 8, 24),
            new THREE.MeshBasicMaterial({ 
                color: 0xff1493,
                transparent: true,
                opacity: 0.8
            })
        );
        neonTube.position.z = 0.02;
        
        neonSign.add(signBack);
        neonSign.add(neonTube);
        neonSign.position.set(0, 8, -9.9);
        this.scene.add(neonSign);
    }
    
    createFloorDetails() {
        // 地磚縫隙
        for (let x = -10; x <= 10; x += 2) {
            const grout = new THREE.Mesh(
                new THREE.PlaneGeometry(0.05, 20),
                new THREE.MeshPhongMaterial({ color: 0x1a1a1a })
            );
            grout.rotation.x = -Math.PI / 2;
            grout.position.set(x, 0.002, 0);
            this.scene.add(grout);
        }
        
        for (let z = -10; z <= 10; z += 2) {
            const grout = new THREE.Mesh(
                new THREE.PlaneGeometry(20, 0.05),
                new THREE.MeshPhongMaterial({ color: 0x1a1a1a })
            );
            grout.rotation.x = -Math.PI / 2;
            grout.position.set(0, 0.002, z);
            this.scene.add(grout);
        }
        
        // 腳墊區域
        const footMat = new THREE.Mesh(
            new THREE.PlaneGeometry(8, 1.5),
            new THREE.MeshPhongMaterial({ 
                color: 0x4a4a4a,
                roughness: 0.8
            })
        );
        footMat.rotation.x = -Math.PI / 2;
        footMat.position.set(0, 0.005, 1);
        this.scene.add(footMat);
    }
    
    createAtmosphereItems() {
        // 垃圾桶
        const trashCan = new THREE.Mesh(
            new THREE.CylinderGeometry(0.3, 0.3, 0.6),
            new THREE.MeshPhongMaterial({ 
                color: 0x2c2c2c,
                shininess: 40
            })
        );
        trashCan.position.set(7, 0.3, -2);
        this.scene.add(trashCan);
        
        // 收銀機
        const cashRegister = new THREE.Group();
        
        const registerBody = new THREE.Mesh(
            new THREE.BoxGeometry(0.6, 0.3, 0.4),
            new THREE.MeshPhongMaterial({ 
                color: 0x1a1a1a,
                shininess: 60
            })
        );
        registerBody.position.y = 0.15;
        
        const registerScreen = new THREE.Mesh(
            new THREE.PlaneGeometry(0.2, 0.15),
            new THREE.MeshBasicMaterial({ 
                color: 0x00ff00,
                transparent: true,
                opacity: 0.7
            })
        );
        registerScreen.position.set(0, 0.2, 0.21);
        
        cashRegister.add(registerBody);
        cashRegister.add(registerScreen);
        cashRegister.position.set(5, 1.06, -3);
        this.scene.add(cashRegister);
        
        // 菜單架
        const menuBoard = new THREE.Group();
        
        const boardBack = new THREE.Mesh(
            new THREE.PlaneGeometry(2, 3),
            new THREE.MeshPhongMaterial({ color: 0x2c2c2c })
        );
        
        // 菜單項目
        for (let i = 0; i < 8; i++) {
            const menuItem = new THREE.Mesh(
                new THREE.PlaneGeometry(1.8, 0.2),
                new THREE.MeshBasicMaterial({ 
                    color: 0xfff8dc,
                    transparent: true,
                    opacity: 0.9
                })
            );
            menuItem.position.set(0, 1.2 - i * 0.3, 0.01);
            menuBoard.add(menuItem);
        }
        
        menuBoard.add(boardBack);
        menuBoard.position.set(-8, 6, -9.9);
        this.scene.add(menuBoard);
        
        // 冰桶
        const iceBucket = new THREE.Mesh(
            new THREE.CylinderGeometry(0.25, 0.3, 0.4),
            new THREE.MeshPhongMaterial({ 
                color: 0xc0c0c0,
                shininess: 120,
                metalness: 0.8
            })
        );
        iceBucket.position.set(-3.5, 1.26, -3);
        this.scene.add(iceBucket);
        
        // 冰塊效果
        for (let i = 0; i < 6; i++) {
            const ice = new THREE.Mesh(
                new THREE.BoxGeometry(0.06, 0.06, 0.06),
                new THREE.MeshPhongMaterial({ 
                    color: 0xf0f8ff,
                    transparent: true,
                    opacity: 0.6,
                    shininess: 200
                })
            );
            ice.position.set(
                -3.5 + (Math.random() - 0.5) * 0.3,
                1.35 + Math.random() * 0.2,
                -3 + (Math.random() - 0.5) * 0.3
            );
            ice.rotation.set(
                Math.random() * Math.PI,
                Math.random() * Math.PI,
                Math.random() * Math.PI
            );
            this.scene.add(ice);
        }
    }
    
    createPremiumBottleDisplay() {
        const displayCase = new THREE.Group();
        
        // 透明玻璃展示櫃 - 修改為真正透明
        const glassCase = new THREE.Mesh(
            new THREE.BoxGeometry(4, 2.5, 1.2, 1, 1, 1),
            new THREE.MeshPhongMaterial({ 
                color: 0xffffff, // 改為白色
                transparent: true,
                opacity: 0.1, // 更透明
                shininess: 200,
                side: THREE.DoubleSide
            })
        );
        glassCase.position.y = 1.25;
        
        // 展示櫃框架 - 改為更亮的金屬色
        const caseFrame = new THREE.Mesh(
            new THREE.BoxGeometry(4.1, 2.6, 1.3),
            new THREE.MeshPhongMaterial({ 
                color: 0xc0c0c0, // 改為銀色
                shininess: 120,
                metalness: 0.8
            })
        );
        caseFrame.position.y = 1.25;
        
        // 展示層板 - 改為更透明的玻璃
        for (let i = 0; i < 3; i++) {
            const shelf = new THREE.Mesh(
                new THREE.BoxGeometry(3.8, 0.05, 1),
                new THREE.MeshPhongMaterial({ 
                    color: 0xffffff,
                    transparent: true,
                    opacity: 0.15, // 降低不透明度
                    shininess: 200
                })
            );
            shelf.position.set(0, 0.3 + i * 0.7, 0);
            displayCase.add(shelf);
            
            // LED 燈帶 - 增強亮度
            const ledStrip = new THREE.Mesh(
                new THREE.BoxGeometry(3.6, 0.02, 0.1),
                new THREE.MeshBasicMaterial({ 
                    color: 0x87ceeb,
                    transparent: true,
                    opacity: 0.9 // 增加亮度
                })
            );
            ledStrip.position.set(0, 0.32 + i * 0.7, -0.45);
            displayCase.add(ledStrip);
        }
        
        // 精品酒款展示
        const premiumBottles = [
            { name: 'Macallan 25', color: 0x8b4513, height: 0.9, x: -1.3, y: 1.6, rarity: '珍藏' },
            { name: 'Dom Pérignon', color: 0x2d4a2d, height: 0.85, x: -0.4, y: 1.6, rarity: '香檳' },
            { name: 'Hennessy XO', color: 0x4a2c17, height: 0.8, x: 0.5, y: 1.6, rarity: '干邑' },
            { name: 'Grey Goose Magnum', color: 0xe6e6fa, height: 0.95, x: 1.4, y: 1.6, rarity: '大瓶裝' },
            
            { name: 'Hibiki 21', color: 0xdaa520, height: 0.85, x: -1.1, y: 0.9, rarity: '日威' },
            { name: 'Johnnie Blue', color: 0x191970, height: 0.9, x: 0, y: 0.9, rarity: '藍牌' },
            { name: 'Rémy XO', color: 0x8b4513, height: 0.82, x: 1.1, y: 0.9, rarity: '特級' },
            
            { name: 'Armand de Brignac', color: 0xffd700, height: 0.88, x: -0.7, y: 0.2, rarity: '黑桃A' },
            { name: 'Crystal Head', color: 0xffffff, height: 0.75, x: 0.7, y: 0.2, rarity: '水晶骷髏' }
        ];
        
        premiumBottles.forEach(bottle => {
            const bottleGroup = new THREE.Group();
            
            // 瓶身
            const bottleBody = new THREE.Mesh(
                new THREE.CylinderGeometry(0.12, 0.15, bottle.height, 8),
                new THREE.MeshPhongMaterial({ 
                    color: bottle.color,
                    transparent: bottle.color === 0xffffff,
                    opacity: bottle.color === 0xffffff ? 0.3 : 0.8,
                    shininess: 150,
                    metalness: 0.3
                })
            );
            
            // 瓶蓋
            const cap = new THREE.Mesh(
                new THREE.CylinderGeometry(0.08, 0.08, 0.06),
                new THREE.MeshPhongMaterial({ 
                    color: bottle.name.includes('Dom') ? 0xffd700 : 0x2c2c2c,
                    shininess: 120,
                    metalness: 0.8
                })
            );
            cap.position.y = bottle.height/2 + 0.03;
    
            bottleGroup.add(bottleBody);
            bottleGroup.add(cap);
            bottleGroup.position.set(bottle.x, bottle.y, 0.2);
            
            
            displayCase.add(bottleGroup);
        });
        
        //displayCase.add(caseFrame);
        displayCase.add(glassCase);
        
        // 設定位置並旋轉90度
        displayCase.position.set(9.5, 0, 9);
        displayCase.rotation.y = Math.PI / -2; // 繞Y軸旋轉90度
        
        displayCase.castShadow = true;
        displayCase.receiveShadow = true;

        this.scene.add(displayCase);
    }

    /**
     * 創建材料展示櫃（參考旻偉附近的高級展示櫃樣式）
     */
    createIngredientShelf() {
        const displayCase = new THREE.Group();

        // 透明玻璃展示櫃外殼
        const glassCase = new THREE.Mesh(
            new THREE.BoxGeometry(3, 2.5, 1.0),
            new THREE.MeshPhongMaterial({
                color: 0xffffff,
                transparent: true,
                opacity: 0.1, // 非常透明
                shininess: 200,
                side: THREE.DoubleSide
            })
        );
        glassCase.position.y = 1.25;
        displayCase.add(glassCase);

        // 銀色金屬框架
        const caseFrame = new THREE.Mesh(
            new THREE.BoxGeometry(3.1, 2.6, 1.1),
            new THREE.MeshPhongMaterial({
                color: 0xc0c0c0, // 銀色
                shininess: 120,
                metalness: 0.8,
                transparent: true,
                opacity: 0.3
            })
        );
        caseFrame.position.y = 1.25;
        displayCase.add(caseFrame);

        // 3層透明玻璃架子，帶LED燈帶
        for (let i = 0; i < 3; i++) {
            // 玻璃層板
            const shelf = new THREE.Mesh(
                new THREE.BoxGeometry(2.8, 0.05, 0.8),
                new THREE.MeshPhongMaterial({
                    color: 0xffffff,
                    transparent: true,
                    opacity: 0.15,
                    shininess: 200
                })
            );
            shelf.position.set(0, 0.4 + i * 0.7, 0);
            displayCase.add(shelf);

            // LED 燈帶（藍白色光暈）
            const ledStrip = new THREE.Mesh(
                new THREE.BoxGeometry(2.6, 0.02, 0.1),
                new THREE.MeshBasicMaterial({
                    color: 0x87ceeb, // 天藍色
                    transparent: true,
                    opacity: 0.9
                })
            );
            ledStrip.position.set(0, 0.42 + i * 0.7, -0.35);
            displayCase.add(ledStrip);
        }

        // 添加三個小綠色盆栽裝飾（參考旻偉附近的風格）
        const plantPositions = [
            { x: -1.3, y: 0.05, z: 0.3 },
            { x: 0, y: 0.05, z: 0.3 },
            { x: 1.3, y: 0.05, z: 0.3 }
        ];

        plantPositions.forEach(pos => {
            const plantGroup = new THREE.Group();

            // 小花盆
            const pot = new THREE.Mesh(
                new THREE.CylinderGeometry(0.15, 0.12, 0.2),
                new THREE.MeshPhongMaterial({
                    color: 0x654321,
                    shininess: 40
                })
            );
            pot.position.y = 0.1;

            // 綠色植物葉子
            for (let i = 0; i < 5; i++) {
                const leaf = new THREE.Mesh(
                    new THREE.SphereGeometry(0.08, 6, 4),
                    new THREE.MeshPhongMaterial({
                        color: 0x2e7d32,
                        shininess: 30
                    })
                );
                leaf.scale.set(1, 0.1, 1.5);
                const angle = (i / 5) * Math.PI * 2;
                leaf.position.set(
                    Math.cos(angle) * 0.1,
                    0.25 + Math.random() * 0.1,
                    Math.sin(angle) * 0.1
                );
                leaf.rotation.y = angle;
                plantGroup.add(leaf);
            }

            plantGroup.add(pot);
            plantGroup.position.set(pos.x, pos.y, pos.z);
            displayCase.add(plantGroup);
        });

        // 調酒材料清單（經典調酒常用材料）
        this.ingredientBottles = [];
        const ingredients = [
            // 第一層：利口酒類
            { type: 'vermouth_dry', name: 'Dry Vermouth', displayName: '不甜香艾酒', color: 0xe8e8d0, x: -1.0, y: 0.5 },
            { type: 'vermouth_sweet', name: 'Sweet Vermouth', displayName: '甜香艾酒', color: 0x8b4513, x: -0.35, y: 0.5 },
            { type: 'campari', name: 'Campari', displayName: '金巴利', color: 0xdc143c, x: 0.35, y: 0.5 },
            { type: 'triple_sec', name: 'Triple Sec', displayName: '橙皮酒', color: 0xffa500, x: 1.0, y: 0.5 },

            // 第二層：果汁和糖漿
            { type: 'lemon_juice', name: 'Lemon Juice', displayName: '檸檬汁', color: 0xfff44f, x: -1.0, y: 1.2 },
            { type: 'lime_juice', name: 'Lime Juice', displayName: '萊姆汁', color: 0xbfff00, x: -0.35, y: 1.2 },
            { type: 'simple_syrup', name: 'Simple Syrup', displayName: '糖漿', color: 0xffffff, x: 0.35, y: 1.2 },
            { type: 'grenadine', name: 'Grenadine', displayName: '紅石榴糖漿', color: 0xff1493, x: 1.0, y: 1.2 },

            // 第三層：特殊材料
            { type: 'pineapple_juice', name: 'Pineapple Juice', displayName: '鳳梨汁', color: 0xffff66, x: -1.0, y: 1.9 },
            { type: 'coconut_cream', name: 'Coconut Cream', displayName: '椰漿', color: 0xfffaf0, x: -0.35, y: 1.9 },
            { type: 'orange_juice', name: 'Orange Juice', displayName: '柳橙汁', color: 0xff8c00, x: 0.35, y: 1.9 },
            { type: 'cranberry_juice', name: 'Cranberry Juice', displayName: '蔓越莓汁', color: 0xdc143c, x: 1.0, y: 1.9 }
        ];

        ingredients.forEach(ingredient => {
            const bottle = this.createIngredientBottle(ingredient);
            bottle.position.set(ingredient.x, ingredient.y, 0);
            displayCase.add(bottle);

            // 保存到陣列以便後續註冊為可互動物品
            this.ingredientBottles.push({
                mesh: bottle,
                type: ingredient.type,
                name: ingredient.name,
                displayName: ingredient.displayName
            });
        });

        // 設定位置：在 Seaton 曦樂（-2, 0, -5）的左邊，靠近吧台
        displayCase.position.set(-4.8, 0, -4.5);
        displayCase.rotation.y = Math.PI / 6; // 稍微旋轉，面向玩家

        displayCase.castShadow = true;
        displayCase.receiveShadow = true;

        this.scene.add(displayCase);
    }

    /**
     * 創建材料瓶
     */
    createIngredientBottle(ingredient) {
        const bottleGroup = new THREE.Group();

        // 瓶身（較小的瓶子）
        const bottleBody = new THREE.Mesh(
            new THREE.CylinderGeometry(0.08, 0.1, 0.35, 8),
            new THREE.MeshPhongMaterial({
                color: ingredient.color,
                transparent: true,
                opacity: 0.7,
                shininess: 100
            })
        );
        bottleBody.position.y = 0.175;

        // 瓶蓋
        const cap = new THREE.Mesh(
            new THREE.CylinderGeometry(0.06, 0.06, 0.04),
            new THREE.MeshPhongMaterial({
                color: 0x2c2c2c,
                shininess: 80
            })
        );
        cap.position.y = 0.37;

        // 標籤
        const label = new THREE.Mesh(
            new THREE.BoxGeometry(0.14, 0.12, 0.01),
            new THREE.MeshPhongMaterial({
                color: 0xffffff,
                shininess: 10
            })
        );
        label.position.set(0, 0.15, 0.095);

        bottleGroup.add(bottleBody);
        bottleGroup.add(cap);
        bottleGroup.add(label);

        bottleGroup.castShadow = true;
        bottleGroup.receiveShadow = true;

        return bottleGroup;
    }

    /**
     * 創建可互動的杯子
     */
    createDrinkingGlasses() {
        // 在吧檯上放置 3 個 Mixing Glass 風格的杯子
        // 放置在 Seaton 的右前方（靠近吧台邊緣），方便玩家拿取
        const glassPositions = [
            { x: -3.2, z: -2.5 },  // 左邊
            { x: -2.5, z: -2.5 },  // 中間
            { x: -1.8, z: -2.5 }   // 右邊
        ];

        glassPositions.forEach(pos => {
            const glass = this.createMixingGlassForDrinking();
            // 從稍高的位置掉落，讓重力系統生效
            glass.position.set(pos.x, 2.5, pos.z);
            this.glasses.push(glass);
            this.scene.add(glass);
        });
    }

    /**
     * 創建 Mixing Glass 風格的可互動杯子（類似日式調酒杯）
     */
    createMixingGlassForDrinking() {
        const glassGroup = new THREE.Group();

        // 主杯身（高腰圓柱體，mixing glass特徵）
        const glassBody = new THREE.Mesh(
            new THREE.CylinderGeometry(0.19, 0.16, 0.65, 16, 1, true),
            new THREE.MeshPhongMaterial({
                color: 0xffffff,
                transparent: true,
                opacity: 0.12,
                shininess: 200,
                specular: 0xaaaaaa,
                side: THREE.DoubleSide
            })
        );
        glassBody.position.y = 0.325;

        // 杯底加厚（mixing glass特徵）
        const glassBase = new THREE.Mesh(
            new THREE.CylinderGeometry(0.16, 0.16, 0.08),
            new THREE.MeshPhongMaterial({
                color: 0xffffff,
                transparent: true,
                opacity: 0.2,
                shininess: 200
            })
        );
        glassBase.position.y = 0.04;

        glassGroup.add(glassBody);
        glassGroup.add(glassBase);
        glassGroup.castShadow = true;
        glassGroup.userData.interactable = true;

        return glassGroup;
    }

    /**
     * 創建逼真的玻璃杯
     */
    createRealisticGlass() {
        const glassGroup = new THREE.Group();

        // 杯身（圓柱體）
        const glassBody = new THREE.Mesh(
            new THREE.CylinderGeometry(0.13, 0.15, 0.6, 16, 1, true),
            new THREE.MeshPhongMaterial({
                color: 0xffffff,
                transparent: true,
                opacity: 0.2,
                shininess: 150,
                specular: 0xffffff,
                reflectivity: 0.8
            })
        );
        glassBody.position.y = 0.3;

        // 杯底
        const glassBottom = new THREE.Mesh(
            new THREE.CircleGeometry(0.15, 16),
            new THREE.MeshPhongMaterial({
                color: 0xffffff,
                transparent: true,
                opacity: 0.3,
                shininess: 150
            })
        );
        glassBottom.rotation.x = -Math.PI / 2;
        glassBottom.position.y = 0.01;

        // 杯緣（厚度）
        const glassRim = new THREE.Mesh(
            new THREE.TorusGeometry(0.13, 0.02, 8, 16),
            new THREE.MeshPhongMaterial({
                color: 0xffffff,
                transparent: true,
                opacity: 0.3,
                shininess: 150
            })
        );
        glassRim.position.y = 0.6;
        glassRim.rotation.x = Math.PI / 2;

        glassGroup.add(glassBody);
        glassGroup.add(glassBottom);
        glassGroup.add(glassRim);
        glassGroup.castShadow = true;
        glassGroup.receiveShadow = true;

        return glassGroup;
    }
}