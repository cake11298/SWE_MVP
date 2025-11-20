import * as CANNON from 'cannon-es';
import * as THREE from 'three';

/**
 * 物理系統 - 整合 Cannon.js 物理引擎
 * 負責管理所有物理物體的模擬和同步
 */
export default class PhysicsSystem {
    constructor() {
        // 創建物理世界
        this.world = new CANNON.World();
        this.world.gravity.set(0, -9.82, 0); // 重力加速度

        // 設置碰撞檢測
        this.world.broadphase = new CANNON.NaiveBroadphase();
        this.world.solver.iterations = 10;

        // 允許物體休眠以提高性能
        this.world.allowSleep = true;

        // 物理物體映射表 [Three.js Object -> Cannon.js Body]
        this.physicsObjects = new Map();

        // 碰撞群組定義（使用位元遮罩）
        this.COLLISION_GROUP_DEFAULT = 1;  // 預設群組（玩家、地板等）
        this.COLLISION_GROUP_OBJECT = 2;   // 可互動物品（酒瓶、杯子等）
        this.COLLISION_GROUP_SHELF = 4;    // 架子（只與物品碰撞）

        // 材質設定
        this.setupMaterials();

        // 創建地板碰撞體
        this.createFloor();
    }

    /**
     * 設置物理材質和碰撞屬性
     */
    setupMaterials() {
        // 默認材質
        this.defaultMaterial = new CANNON.Material('default');

        // 地板材質
        this.floorMaterial = new CANNON.Material('floor');

        // 玻璃材質（酒瓶）
        this.glassMaterial = new CANNON.Material('glass');

        // 金屬材質（Shaker）
        this.metalMaterial = new CANNON.Material('metal');

        // 碰撞參數設置
        const defaultContact = new CANNON.ContactMaterial(
            this.defaultMaterial,
            this.defaultMaterial,
            {
                friction: 0.3,
                restitution: 0.3 // 彈性係數
            }
        );

        const glassFloorContact = new CANNON.ContactMaterial(
            this.glassMaterial,
            this.floorMaterial,
            {
                friction: 0.4,
                restitution: 0.4 // 玻璃掉落會彈跳
            }
        );

        this.world.addContactMaterial(defaultContact);
        this.world.addContactMaterial(glassFloorContact);
    }

    /**
     * 創建地板碰撞體
     */
    createFloor() {
        const floorShape = new CANNON.Plane();
        const floorBody = new CANNON.Body({
            mass: 0, // 靜態物體
            material: this.floorMaterial
        });
        floorBody.addShape(floorShape);

        // 旋轉平面使其水平（默認是垂直的）
        floorBody.quaternion.setFromAxisAngle(
            new CANNON.Vec3(1, 0, 0),
            -Math.PI / 2
        );

        this.world.addBody(floorBody);
    }

    /**
     * 添加物理物體（圓柱體 - 用於酒瓶）
     * @param {THREE.Object3D} mesh - Three.js 網格
     * @param {number} radiusTop - 頂部半徑
     * @param {number} radiusBottom - 底部半徑
     * @param {number} height - 高度
     * @param {number} mass - 質量（0 = 靜態）
     * @param {string} materialType - 材質類型
     */
    addCylinderBody(mesh, radiusTop, radiusBottom, height, mass = 1, materialType = 'glass') {
        // Cannon.js 使用圓柱體近似（簡化計算）
        const radius = (radiusTop + radiusBottom) / 2;
        const shape = new CANNON.Cylinder(radiusTop, radiusBottom, height, 8);

        const body = new CANNON.Body({
            mass: mass,
            material: this[`${materialType}Material`] || this.defaultMaterial,
            collisionFilterGroup: this.COLLISION_GROUP_OBJECT,  // 屬於物品群組
            collisionFilterMask: this.COLLISION_GROUP_DEFAULT | this.COLLISION_GROUP_OBJECT | this.COLLISION_GROUP_SHELF  // 與所有群組碰撞
        });

        body.addShape(shape);

        // 不旋轉形狀 - 讓圓柱體保持垂直站立
        // （移除了之前的 Math.PI / 2 旋轉）

        // 設置初始位置和旋轉（從 mesh 複製）
        body.position.copy(mesh.position);
        body.quaternion.copy(mesh.quaternion);

        this.world.addBody(body);
        this.physicsObjects.set(mesh, body);

        return body;
    }

    /**
     * 添加物理物體（盒子 - 用於杯子、Jigger）
     * @param {THREE.Object3D} mesh - Three.js 網格
     * @param {THREE.Vector3} size - 尺寸
     * @param {number} mass - 質量
     */
    addBoxBody(mesh, size, mass = 1) {
        const shape = new CANNON.Box(
            new CANNON.Vec3(size.x / 2, size.y / 2, size.z / 2)
        );

        const body = new CANNON.Body({
            mass: mass,
            collisionFilterGroup: this.COLLISION_GROUP_OBJECT,  // 屬於物品群組
            collisionFilterMask: this.COLLISION_GROUP_DEFAULT | this.COLLISION_GROUP_OBJECT | this.COLLISION_GROUP_SHELF  // 與所有群組碰撞
        });
        body.addShape(shape);
        body.position.copy(mesh.position);

        this.world.addBody(body);
        this.physicsObjects.set(mesh, body);

        return body;
    }

    /**
     * 添加物理物體（球體 - 用於裝飾或特殊物品）
     * @param {THREE.Object3D} mesh - Three.js 網格
     * @param {number} radius - 半徑
     * @param {number} mass - 質量
     */
    addSphereBody(mesh, radius, mass = 1) {
        const shape = new CANNON.Sphere(radius);
        const body = new CANNON.Body({ mass });
        body.addShape(shape);
        body.position.copy(mesh.position);

        this.world.addBody(body);
        this.physicsObjects.set(mesh, body);

        return body;
    }

    /**
     * 移除物理物體
     * @param {THREE.Object3D} mesh - Three.js 網格
     */
    removeBody(mesh) {
        const body = this.physicsObjects.get(mesh);
        if (body) {
            this.world.removeBody(body);
            this.physicsObjects.delete(mesh);
        }
    }

    /**
     * 啟用/禁用物理模擬（當物品被拿起時禁用）
     * @param {THREE.Object3D} mesh - Three.js 網格
     * @param {boolean} enabled - 是否啟用
     */
    setBodyEnabled(mesh, enabled) {
        const body = this.physicsObjects.get(mesh);
        if (body) {
            if (enabled) {
                body.type = CANNON.Body.DYNAMIC;
                body.wakeUp();
            } else {
                body.type = CANNON.Body.KINEMATIC;
                body.velocity.setZero();
                body.angularVelocity.setZero();
            }
        }
    }

    /**
     * 設置物體速度（用於投擲）
     * @param {THREE.Object3D} mesh - Three.js 網格
     * @param {THREE.Vector3} velocity - 速度向量
     */
    setBodyVelocity(mesh, velocity) {
        const body = this.physicsObjects.get(mesh);
        if (body) {
            body.velocity.set(velocity.x, velocity.y, velocity.z);
        }
    }

    /**
     * 更新物理世界（每幀調用）
     * @param {number} deltaTime - 時間增量
     */
    update(deltaTime) {
        // 固定時間步長以確保穩定性
        const fixedTimeStep = 1.0 / 60.0;
        this.world.step(fixedTimeStep, deltaTime, 3);

        // 同步 Three.js 網格與 Cannon.js 物理體
        this.physicsObjects.forEach((body, mesh) => {
            // 只同步動態物體
            if (body.type === CANNON.Body.DYNAMIC) {
                mesh.position.copy(body.position);
                mesh.quaternion.copy(body.quaternion);
            }
        });
    }

    /**
     * 檢測射線碰撞（用於互動檢測）
     * @param {THREE.Vector3} from - 起點
     * @param {THREE.Vector3} to - 終點
     * @returns {Object|null} 碰撞結果
     */
    raycast(from, to) {
        const rayFrom = new CANNON.Vec3(from.x, from.y, from.z);
        const rayTo = new CANNON.Vec3(to.x, to.y, to.z);
        const result = new CANNON.RaycastResult();

        this.world.raycast(rayFrom, rayTo, {}, result);

        if (result.hasHit) {
            // 找到對應的 Three.js 網格
            for (const [mesh, body] of this.physicsObjects) {
                if (body === result.body) {
                    return {
                        body: result.body,
                        mesh: mesh,
                        point: new THREE.Vector3(
                            result.hitPointWorld.x,
                            result.hitPointWorld.y,
                            result.hitPointWorld.z
                        ),
                        distance: result.distance
                    };
                }
            }
        }

        return null;
    }

    /**
     * 凍結物體位置（放回酒牆時使用）
     * @param {THREE.Object3D} mesh - Three.js 網格
     * @param {THREE.Vector3} position - 目標位置
     * @param {THREE.Quaternion} rotation - 目標旋轉（可選，默認為單位旋轉）
     */
    freezeBodyAt(mesh, position, rotation = null) {
        const body = this.physicsObjects.get(mesh);
        if (body) {
            body.type = CANNON.Body.STATIC;
            body.position.set(position.x, position.y, position.z);

            // 重置旋轉為垂直站立（單位四元數）
            if (rotation) {
                body.quaternion.set(rotation.x, rotation.y, rotation.z, rotation.w);
            } else {
                body.quaternion.set(0, 0, 0, 1); // 單位四元數（無旋轉）
            }

            body.velocity.setZero();
            body.angularVelocity.setZero();

            // 同步 Three.js 網格
            mesh.position.copy(body.position);
            mesh.quaternion.copy(body.quaternion);
        }
    }

    /**
     * 添加靜態碰撞體（用於環境物體如吧台、牆壁）
     * @param {THREE.Vector3} position - 位置
     * @param {THREE.Vector3} size - 尺寸
     * @param {THREE.Quaternion} rotation - 旋轉（可選）
     * @returns {CANNON.Body}
     */
    addStaticBox(position, size, rotation = null) {
        const shape = new CANNON.Box(
            new CANNON.Vec3(size.x / 2, size.y / 2, size.z / 2)
        );

        const body = new CANNON.Body({
            mass: 0, // 靜態物體
            material: this.floorMaterial
        });

        body.addShape(shape);
        body.position.set(position.x, position.y, position.z);

        if (rotation) {
            body.quaternion.copy(rotation);
        }

        this.world.addBody(body);
        return body;
    }

    /**
     * 添加架子碰撞體（只與物品碰撞，不與玩家碰撞）
     * @param {THREE.Vector3} position - 位置
     * @param {THREE.Vector3} size - 尺寸
     * @param {THREE.Quaternion} rotation - 旋轉（可選）
     * @returns {CANNON.Body}
     */
    addShelfCollision(position, size, rotation = null) {
        const shape = new CANNON.Box(
            new CANNON.Vec3(size.x / 2, size.y / 2, size.z / 2)
        );

        const body = new CANNON.Body({
            mass: 0, // 靜態物體
            material: this.floorMaterial,
            collisionFilterGroup: this.COLLISION_GROUP_SHELF,  // 屬於架子群組
            collisionFilterMask: this.COLLISION_GROUP_OBJECT   // 只與物品碰撞
        });

        body.addShape(shape);
        body.position.set(position.x, position.y, position.z);

        if (rotation) {
            body.quaternion.copy(rotation);
        }

        this.world.addBody(body);
        return body;
    }

    /**
     * 獲取物體的物理 body
     * @param {THREE.Object3D} mesh - Three.js 網格
     * @returns {CANNON.Body|null}
     */
    getBody(mesh) {
        return this.physicsObjects.get(mesh) || null;
    }
}
