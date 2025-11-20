import * as THREE from 'three';

/**
 * 吧檯工具模組
 * 負責創建 Shaker、Jigger 等調酒工具
 * 根據需求,只保留核心工具:Shaker、Jigger、調酒墊、收銀機
 */
export class BarTools {
    constructor(scene) {
        this.scene = scene;
        this.barTools = {};
    }

    /**
     * 創建所有吧檯工具
     */
    createAll() {
        this.createBarMat();        // 調酒墊
        this.createShaker();         // Shaker(雪克杯)
        this.createJigger();         // Jigger(量酒器)
        this.createCashRegister();   // 收銀機
    }

    /**
     * 創建專業調酒墊(防滑橡膠材質)
     */
    createBarMat() {
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
    }

    /**
     * 創建高級Boston Shaker套組
     */
    createShaker() {
        const shakerGroup = new THREE.Group();

        // 金屬搖酒器(拋光不銹鋼)
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
        shakerGroup.position.set(1.5, 1.10, -2.2);
        shakerGroup.castShadow = true;
        this.scene.add(shakerGroup);

        // 儲存 Shaker 引用
        this.barTools.shaker = shakerGroup;
    }

    /**
     * 創建精密雙頭量酒器
     */
    createJigger() {
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

        // 大杯(2oz) - 向下開口
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

        // 小杯(1oz) - 向上開口
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

        // 內部刻度線(只在大杯內部)
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
        jiggerGroup.position.set(-0.6, 1.18, -2.1);
        jiggerGroup.castShadow = true;
        this.scene.add(jiggerGroup);

        // 儲存 Jigger 引用
        this.barTools.jigger = jiggerGroup;
    }

    /**
     * 創建收銀機
     */
    createCashRegister() {
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
    }

    /**
     * 獲取吧檯工具引用
     */
    getTools() {
        return this.barTools;
    }
}
