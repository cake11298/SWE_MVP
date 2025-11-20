import * as THREE from 'three';

/**
 * 吧檯家具模組
 * 負責創建吧台椅等家具
 */
export class BarFurniture {
    constructor(scene) {
        this.scene = scene;
        this.guitar = null; // 吉他物件引用
    }

    /**
     * 創建所有家具
     */
    createAll() {
        this.createBarStools();
        this.createMusicCorner();
    }

    /**
     * 獲取吉他物件
     */
    getGuitar() {
        return this.guitar;
    }

    /**
     * 創建吧台椅
     */
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

    /**
     * 創建音樂角落(吉他和音箱)
     */
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

        // 弦鈕(調音鈕)
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

        // 副音箱(監聽音箱)
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
    }
}
