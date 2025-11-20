import * as THREE from 'three';

/**
 * 吧台基礎結構模組
 * 負責創建地板、牆壁、吧檯、酒架等基礎結構
 */
export class BarStructure {
    constructor(scene) {
        this.scene = scene;
    }

    /**
     * 創建所有基礎結構
     */
    createAll() {
        this.createFloor();
        this.createWalls();
        this.createBarCounter();
        this.createLiquorShelf();
    }

    /**
     * 創建地板
     */
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

    /**
     * 創建牆壁
     */
    createWalls() {
        // 使用更溫暖的牆壁顏色
        const wallMaterial = new THREE.MeshPhongMaterial({
            color: 0x5d4e37,
            shininess: 5
        });

        const wallPositions = [
            // 前牆（原本的後牆,在 z = -10）
            { pos: [0, 5, -10], rot: [0, 0, 0] },
            // 左牆
            { pos: [-10, 5, 0], rot: [0, Math.PI/2, 0] },
            // 右牆
            { pos: [10, 5, 0], rot: [0, -Math.PI/2, 0] },
            // 新增:後牆(在 z = 10)
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

    /**
     * 創建吧檯
     */
    createBarCounter() {
        const counterGroup = new THREE.Group();

        // 吧檯主體(使用更真實的木材顏色)
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

        // 改進的大理石檯面(更真實的顏色和材質)
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
    }

    /**
     * 創建酒架(注意:現在只創建架子結構,不創建酒瓶)
     */
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

        // 建立多層架子
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
        }

        return { shelfHeights: [1.5, 2.7, 3.9] }; // 返回架子高度供酒瓶模組使用
    }
}
