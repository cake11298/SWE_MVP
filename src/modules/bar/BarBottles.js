import * as THREE from 'three';

/**
 * 酒瓶和杯子模組
 * 負責創建酒瓶、杯子等容器
 */
export class BarBottles {
    constructor(scene) {
        this.scene = scene;
        this.bottles = [];
        this.glasses = [];
    }

    /**
     * 創建酒架上的酒瓶
     * @param {Array<number>} shelfHeights - 酒架高度陣列
     */
    createShelfBottles(shelfHeights) {
        shelfHeights.forEach(height => {
            this.createBottleRow(height);
        });
    }

    /**
     * 創建一排酒瓶
     * @param {number} height - 酒瓶放置的高度
     */
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

    /**
     * 創建逼真的酒瓶
     * @param {Object} type - 酒瓶類型配置
     */
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

        // 瓶身(更真實的材質)
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

            // 年份標籤(部分酒類)
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

        // 液體效果(對於透明瓶子)
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

    /**
     * 創建可互動的飲用杯子
     * 在吧檯上放置 3 個 Mixing Glass 風格的杯子
     */
    createDrinkingGlasses() {
        // 放置在 Seaton 的右前方(靠近吧台邊緣),方便玩家拿取
        const glassPositions = [
            { x: -5, z: -2.5 },  // 左邊
            { x: -2.5, z: -2.5 },  // 中間
            { x: -0.5, z: -2.5 }   // 右邊
        ];

        glassPositions.forEach(pos => {
            const glass = this.createMixingGlassForDrinking();
            // 從稍高的位置掉落,讓重力系統生效
            glass.position.set(pos.x, 2, pos.z);
            this.glasses.push(glass);
            this.scene.add(glass);
        });
    }

    /**
     * 創建 Mixing Glass 飲用杯
     */
    createMixingGlassForDrinking() {
    const glassGroup = new THREE.Group();

    // 主杯身(高腰圓柱體,mixing glass特徵)
    const glassBody = new THREE.Mesh(
        new THREE.CylinderGeometry(0.19, 0.16, 0.65, 16, 1, true),
        new THREE.MeshPhongMaterial({
            color: 0xffffff,
            transparent: true,
            opacity: 0.3, // 原本是 0.12，改成 0.3
            shininess: 200,
            specular: 0xaaaaaa,
            side: THREE.DoubleSide
        })
    );
    glassBody.position.y = 0.325;

    // 杯底加厚(mixing glass特徵)
    const glassBase = new THREE.Mesh(
        new THREE.CylinderGeometry(0.16, 0.16, 0.08),
        new THREE.MeshPhongMaterial({
            color: 0xffffff,
            transparent: true,
            opacity: 0.4, // 原本是 0.2，改成 0.4
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
     * 獲取所有創建的酒瓶
     */
    getBottles() {
        return this.bottles;
    }

    /**
     * 獲取所有創建的杯子
     */
    getGlasses() {
        return this.glasses;
    }
}
