import * as THREE from 'three';

export class RetirementLounge {
    constructor(scene) {
        this.scene = scene;
        this.furniture = [];
        this.decorations = [];
        this.createLounge();
    }
    
    createLounge() {
        this.createExtendedRoomStructure();
        this.createExtendedFloor();
        this.createLoungeLighting();
        this.createSimplifiedFurniture();
        this.createMinimalDecorations();
    }
    
    createExtendedRoomStructure() {
        // 擴展房間的牆壁（從主房間向後延伸）
        const wallMaterial = new THREE.MeshPhongMaterial({ 
            color: 0x5d4e37, // 與主房間一致的顏色
            shininess: 10
        });
        
        // 後牆（主房間最後面的牆）
        const backWall = new THREE.Mesh(
            new THREE.PlaneGeometry(20, 10),
            wallMaterial
        );
        backWall.position.set(0, 5, 15);
        backWall.receiveShadow = true;
        this.scene.add(backWall);
        
        // 擴展左牆
        const leftWallExtension = new THREE.Mesh(
            new THREE.PlaneGeometry(5, 10), // 只延伸5單位
            wallMaterial
        );
        leftWallExtension.position.set(-10, 5, 12.5);
        leftWallExtension.rotation.y = Math.PI / 2;
        leftWallExtension.receiveShadow = true;
        this.scene.add(leftWallExtension);

        // 擴展右牆
        const rightWallExtension = new THREE.Mesh(
            new THREE.PlaneGeometry(5, 10),
            wallMaterial
        );
        rightWallExtension.position.set(10, 5, 12.5);  // 調整位置  
        rightWallExtension.rotation.y = -Math.PI / 2;
        rightWallExtension.receiveShadow = true;
        this.scene.add(rightWallExtension);
        
        // 添加天花板
        const ceiling = new THREE.Mesh(
            new THREE.PlaneGeometry(30, 30),
            new THREE.MeshPhongMaterial({ 
                color: 0x4a3c28,
                shininess: 5
            })
        );
        ceiling.rotation.x = Math.PI / 2;
        ceiling.position.set(0, 10, 0);
        ceiling.receiveShadow = true;
        this.scene.add(ceiling);
    }
    
    createExtendedFloor() {
        // 擴展地板（向後延伸）
        const extendedFloor = new THREE.Mesh(
            new THREE.PlaneGeometry(20, 10), // 向後延伸10單位
            new THREE.MeshPhongMaterial({ 
                color: 0x2c1810, // 與主房間地板一致
                shininess: 60
            })
        );
        extendedFloor.rotation.x = -Math.PI / 2;
        extendedFloor.position.set(0, 0, 10)
        extendedFloor.receiveShadow = true;
        this.scene.add(extendedFloor);
        
        // 休閒區地毯
        const carpet = new THREE.Mesh(
            new THREE.PlaneGeometry(12, 6),
            new THREE.MeshPhongMaterial({ 
                color: 0x8B0000,
                shininess: 5,
                roughness: 0.9
            })
        );
        carpet.rotation.x = -Math.PI / 2;
        carpet.position.set(0, 0.01, 10);
        this.scene.add(carpet);
    }
    
    createLoungeLighting() {
        /*
        // 休閒區主要照明
        const mainLoungeLight = new THREE.PointLight(0xFFE4B5, 1.5, 12);
        mainLoungeLight.position.set(0, 6, 12);
        mainLoungeLight.castShadow = true;
        mainLoungeLight.shadow.mapSize.width = 1024;
        mainLoungeLight.shadow.mapSize.height = 1024;
        this.scene.add(mainLoungeLight);
        
        // 角落溫馨燈光
        const cornerLight1 = new THREE.PointLight(0xFFD4A3, 0.8, 8);
        cornerLight1.position.set(-6, 4, 14);
        this.scene.add(cornerLight1);
        
        const cornerLight2 = new THREE.PointLight(0xFFD4A3, 0.8, 8);
        cornerLight2.position.set(6, 4, 14);
        this.scene.add(cornerLight2);
        */
    }
    
    
    createSimplifiedFurniture() {
        // 只保留一個沙發
        this.createLazySofa(-8, 0, 10, Math.PI*0.5);
        
        // 只保留一個咖啡桌
        this.createCoffeeTable();
        
        // 只保留一個電視
        this.createTV();
        
        // 書架放在角落
        this.createBookshelf();
    }
    
    createLazySofa(x, y, z, rotation) {
        const sofaGroup = new THREE.Group();
        
        // 沙發座墊
        const seatCushion = new THREE.Mesh(
            new THREE.BoxGeometry(3, 0.8, 1.5), // 稍微放大
            new THREE.MeshPhongMaterial({ 
                color: 0x8B4513,
                shininess: 60
            })
        );
        seatCushion.position.y = 0.4;
        seatCushion.castShadow = true;
        
        // 靠背
        const backrest = new THREE.Mesh(
            new THREE.BoxGeometry(3, 1.2, 0.3),
            new THREE.MeshPhongMaterial({ 
                color: 0x654321,
                shininess: 60
            })
        );
        backrest.position.set(0, 0.9, -0.6);
        backrest.castShadow = true;
        
        // 扶手
        const armrest1 = new THREE.Mesh(
            new THREE.BoxGeometry(0.3, 0.8, 1.5),
            new THREE.MeshPhongMaterial({ 
                color: 0x654321,
                shininess: 60
            })
        );
        armrest1.position.set(-1.65, 0.6, 0);
        armrest1.castShadow = true;
        
        const armrest2 = new THREE.Mesh(
            new THREE.BoxGeometry(0.3, 0.8, 1.5),
            new THREE.MeshPhongMaterial({ 
                color: 0x654321,
                shininess: 60
            })
        );
        armrest2.position.set(1.65, 0.6, 0);
        armrest2.castShadow = true;
        
        sofaGroup.add(seatCushion);
        sofaGroup.add(backrest);
        sofaGroup.add(armrest1);
        sofaGroup.add(armrest2);
        
        sofaGroup.position.set(x, y, z);
        sofaGroup.rotation.y = rotation;
        
        this.furniture.push(sofaGroup);
        this.scene.add(sofaGroup);
    }
    
    createBookshelf() {
        const shelfGroup = new THREE.Group();
        
        // 書架主體
        const shelfFrame = new THREE.Mesh(
            new THREE.BoxGeometry(0.4, 3, 2),
            new THREE.MeshPhongMaterial({ 
                color: 0xffffff,
                shininess: 40
            })
        );
        shelfFrame.position.y = 1.5;
        shelfFrame.castShadow = true;
        
        // 書架層板
        for (let i = 0; i < 4; i++) {
            const shelf = new THREE.Mesh(
                new THREE.BoxGeometry(0.35, 0.05, 1.9),
                new THREE.MeshPhongMaterial({ 
                    color: 0x654321,
                    shininess: 50
                })
            );
            shelf.position.set(0, 0.3 + i * 0.7, 0);
            shelfGroup.add(shelf);
            
            // 書本（簡化版本）
            for (let j = 0; j < 4; j++) {
                const book = new THREE.Mesh(
                    new THREE.BoxGeometry(0.1, 0.6, 0.2),
                    new THREE.MeshPhongMaterial({ 
                        color: Math.random() * 0xffffff,
                        shininess: 20
                    })
                );
                book.position.set(
                    0.12,
                    0.6 + i * 0.7,
                    -0.6 + j * 0.3
                );
                shelfGroup.add(book);
            }
        }
        
        //shelfGroup.add(shelfFrame);
        shelfGroup.position.set(-9.5, 0, 14); // 放在角落
        
        this.furniture.push(shelfGroup);
        this.scene.add(shelfGroup);
    }
    
    createTV() {
        const tvGroup = new THREE.Group();
        
        // 電視櫃
        const tvStand = new THREE.Mesh(
            new THREE.BoxGeometry(3.5, 0.8, 1),
            new THREE.MeshPhongMaterial({ 
                color: 0x2C2C2C,
                shininess: 80
            })
        );
        tvStand.position.y = 0.4;
        tvStand.castShadow = true;
        
        // 電視螢幕
        const tvScreen = new THREE.Mesh(
            new THREE.BoxGeometry(3, 1.8, 0.1),
            new THREE.MeshPhongMaterial({ 
                color: 0x000000,
                shininess: 200
            })
        );
        tvScreen.position.set(0, 1.6, 0.45);
        tvScreen.castShadow = true;
        
        // 螢幕邊框
        const tvFrame = new THREE.Mesh(
            new THREE.BoxGeometry(3.1, 1.9, 0.12),
            new THREE.MeshPhongMaterial({ 
                color: 0x333333,
                shininess: 100
            })
        );
        tvFrame.position.set(0, 1.6, 0.44);
        
        tvGroup.add(tvStand);
        tvGroup.add(tvFrame);
        tvGroup.add(tvScreen);
        tvGroup.position.set(0, 0, 14.5); // 靠後牆
        
        this.furniture.push(tvGroup);
        this.scene.add(tvGroup);
    }
    
    createCoffeeTable() {
        const tableGroup = new THREE.Group();
        
        // 桌面
        const tableTop = new THREE.Mesh(
            new THREE.CylinderGeometry(1.5, 1.5, 0.1),
            new THREE.MeshPhongMaterial({ 
                color: 0x8B4513,
                shininess: 100
            })
        );
        tableTop.position.y = 0.5;
        tableTop.castShadow = true;
        
        // 桌腳（三腳設計）
        for (let i = 0; i < 3; i++) {
            const leg = new THREE.Mesh(
                new THREE.CylinderGeometry(0.05, 0.05, 0.5),
                new THREE.MeshPhongMaterial({ 
                    color: 0x654321,
                    shininess: 80
                })
            );
            const angle = (i / 3) * Math.PI * 2;
            leg.position.set(
                Math.cos(angle) * 1,
                0.25,
                Math.sin(angle) * 1
            );
            leg.castShadow = true;
            tableGroup.add(leg);
        }
        
        tableGroup.add(tableTop);
        tableGroup.position.set(0, 0, 11.5);
        
        this.furniture.push(tableGroup);
        this.scene.add(tableGroup);
    }
    
    createMinimalDecorations() {
        // 簡化的牆上掛畫
        const painting = new THREE.Mesh(
            new THREE.PlaneGeometry(2, 1.5),
            new THREE.MeshPhongMaterial({ 
                color: 0x4169E1,
                shininess: 10
            })
        );
        painting.position.set(4, 4, -19.9);
        painting.rotation.y = Math.PI;
        this.scene.add(painting);
        
        // 畫框
        const frame = new THREE.Mesh(
            new THREE.PlaneGeometry(2.2, 1.7),
            new THREE.MeshPhongMaterial({ 
                color: 0x8B4513,
                shininess: 40
            })
        );
        frame.position.set(4, 4, -19.95);
        frame.rotation.y = Math.PI;
        this.scene.add(frame);
        
        // 一個角落植物
        const plantGroup = new THREE.Group();
        
        const pot = new THREE.Mesh(
            new THREE.CylinderGeometry(0.4, 0.3, 0.5),
            new THREE.MeshPhongMaterial({ 
                color: 0x8B4513,
                shininess: 30
            })
        );
        pot.position.y = 0.25;
        
        // 植物葉子
        for (let i = 0; i < 6; i++) {
            const leaf = new THREE.Mesh(
                new THREE.SphereGeometry(0.3, 8, 6),
                new THREE.MeshPhongMaterial({ 
                    color: 0x228B22,
                    shininess: 20
                })
            );
            leaf.scale.set(1, 0.02, 1.2);
            
            const angle = (i / 6) * Math.PI * 2;
            const height = 0.6 + Math.random() * 0.4;
            leaf.position.set(
                Math.cos(angle) * 0.2,
                height,
                Math.sin(angle) * 0.2
            );
            leaf.rotation.y = angle;
            plantGroup.add(leaf);
        }
        
        plantGroup.add(pot);
        plantGroup.position.set(7, 0, 12);
        this.scene.add(plantGroup);
    }
    
    // 擴展邊界檢查
    isInLoungeArea(position) {
        return position.x > -10 && position.x < 10 && 
               position.z > -20 && position.z < -10;
    }
    
    // 清理資源
    dispose() {
        this.furniture.forEach(item => {
            this.scene.remove(item);
        });
        
        this.decorations.forEach(item => {
            this.scene.remove(item);
        });
        
        this.furniture = [];
        this.decorations = [];
    }
}