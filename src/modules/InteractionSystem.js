import * as THREE from 'three';

/**
 * 互動系統 - 處理物品拿取、放置、使用
 */
export default class InteractionSystem {
    constructor(camera, physicsSystem) {
        this.camera = camera;
        this.physics = physicsSystem;

        // 射線檢測器（用於檢測玩家看向的物品）
        this.raycaster = new THREE.Raycaster();
        this.raycaster.far = 3; // 互動距離 3 米

        // 可互動物品列表
        this.interactableObjects = [];

        // 當前瞄準的物品
        this.targetedObject = null;

        // 當前手持的物品
        this.heldObject = null;

        // 手持物品的偏移位置（相對於相機）
        this.holdOffset = new THREE.Vector3(0.3, -0.3, -0.8);

        // 物品原始位置（用於放回酒牆）
        this.originalPositions = new Map();

        // 物品類型標記
        this.objectTypes = new Map();

        // 互動狀態
        this.isHolding = false;

        // 調酒系統引用（用於獲取酒類名稱）
        this.cocktailSystem = null;
    }

    /**
     * 設定調酒系統引用
     * @param {CocktailSystem} cocktailSystem - 調酒系統
     */
    setCocktailSystem(cocktailSystem) {
        this.cocktailSystem = cocktailSystem;
    }

    /**
     * 註冊可互動物品
     * @param {THREE.Object3D} object - Three.js 物品
     * @param {string} type - 物品類型 ('bottle', 'glass', 'shaker', 'jigger')
     * @param {THREE.Vector3} originalPosition - 原始位置（可選）
     */
    registerInteractable(object, type, originalPosition = null) {
        this.interactableObjects.push(object);
        this.objectTypes.set(object, type);

        // 儲存原始位置（用於放回酒牆）
        if (originalPosition) {
            this.originalPositions.set(object, originalPosition.clone());
        } else {
            this.originalPositions.set(object, object.position.clone());
        }

        // 標記為可互動
        object.userData.interactable = true;
        object.userData.type = type;
    }

    /**
     * 取消註冊互動物品
     * @param {THREE.Object3D} object - Three.js 物品
     */
    unregisterInteractable(object) {
        const index = this.interactableObjects.indexOf(object);
        if (index > -1) {
            this.interactableObjects.splice(index, 1);
        }
        this.objectTypes.delete(object);
        this.originalPositions.delete(object);
    }

    /**
     * 檢測玩家瞄準的物品
     * @returns {Object|null} { object, type, distance }
     */
    checkTargeted() {
        // 從相機中心發射射線（準心位置）
        this.raycaster.setFromCamera(new THREE.Vector2(0, 0), this.camera);

        // 檢測碰撞
        const intersects = this.raycaster.intersectObjects(
            this.interactableObjects,
            true // 遞迴檢查子物件
        );

        if (intersects.length > 0) {
            // 優先選擇準心直接指向的物品（第一個碰撞點）
            // 這樣即使玩家在兩個物品中間，也會選擇準心指向的那個
            let targetObject = intersects[0].object;

            // 如果碰撞的是子物件，找到父級 Group
            while (targetObject.parent && !targetObject.userData.interactable) {
                targetObject = targetObject.parent;
            }

            if (targetObject.userData.interactable) {
                this.targetedObject = {
                    object: targetObject,
                    type: this.objectTypes.get(targetObject),
                    distance: intersects[0].distance
                };
                return this.targetedObject;
            }
        }

        this.targetedObject = null;
        return null;
    }

    /**
     * 拾取物品
     * @param {THREE.Object3D} object - 要拾取的物品（可選，默認拾取瞄準的物品）
     * @returns {boolean} 是否成功拾取
     */
    pickupObject(object = null) {
        // 如果已經拿著物品，不能再拿
        if (this.isHolding) {
            return false;
        }

        const targetObj = object || (this.targetedObject ? this.targetedObject.object : null);

        if (!targetObj) {
            return false;
        }

        // 拿起物品
        this.heldObject = targetObj;
        this.isHolding = true;

        // 禁用物理模擬
        this.physics.setBodyEnabled(targetObj, false);

        // 從可互動列表中暫時移除（避免重複拾取）
        const index = this.interactableObjects.indexOf(targetObj);
        if (index > -1) {
            this.interactableObjects.splice(index, 1);
        }

        return true;
    }

    /**
     * 放下物品
     * @param {boolean} returnToOriginal - 是否放回原位
     */
    dropObject(returnToOriginal = false) {
        if (!this.heldObject) {
            return;
        }

        const object = this.heldObject;
        const objectType = this.objectTypes.get(object);

        if (returnToOriginal) {
            // 放回原位（酒牆）
            const originalPos = this.originalPositions.get(object);
            if (originalPos) {
                // 重置旋轉為垂直站立
                const upright = new THREE.Quaternion(0, 0, 0, 1);
                object.quaternion.copy(upright);
                object.position.copy(originalPos);

                // 凍結在原位置（不受物理影響），並重置旋轉
                this.physics.freezeBodyAt(object, originalPos, upright);
            }
        } else {
            // 所有物品都只能放回原位，不能原地放下
            // 這個 else 分支已經被停用
            console.log('請按 R 放回原位');
            return;

            // 以下程式碼已停用
            // 使用射線檢測找到下方最近的表面
            const surfacePosition = this.findNearestSurface(object.position);

            if (surfacePosition) {
                // 將物品放置在檢測到的表面上
                object.position.copy(surfacePosition);
                object.position.y += 0.35; // 稍微抬高避免穿模

                // 重置旋轉為垂直站立
                const upright = new THREE.Quaternion(0, 0, 0, 1);
                object.quaternion.copy(upright);

                // 凍結在該位置
                this.physics.freezeBodyAt(object, object.position, upright);
            } else {
                // 如果沒有檢測到表面，使用物理掉落
                this.physics.setBodyEnabled(object, true);
                this.physics.setBodyVelocity(
                    object,
                    new THREE.Vector3(0, -3.0, 0)
                );
            }
        }

        // 重新加入可互動列表
        this.interactableObjects.push(object);

        // 清空手持狀態
        this.heldObject = null;
        this.isHolding = false;
    }

    /**
     * 尋找物品下方最近的表面
     * @param {THREE.Vector3} position - 物品位置
     * @returns {THREE.Vector3|null} 表面位置
     */
    findNearestSurface(position) {
        // 使用物理系統的射線檢測向下發射射線
        const from = position.clone();
        const to = position.clone();
        to.y -= 10; // 向下檢測 10 米

        const result = this.physics.raycast(from, to);

        if (result && result.point) {
            return result.point;
        }

        // 如果沒有檢測到，返回地面位置
        return new THREE.Vector3(position.x, 0, position.z);
    }

    /**
     * 投擲物品
     * @param {number} force - 投擲力度
     */
    throwObject(force = 5) {
        if (!this.heldObject) {
            return;
        }

        const object = this.heldObject;

        // 啟用物理
        this.physics.setBodyEnabled(object, true);

        // 計算投擲方向（相機朝向）
        const direction = new THREE.Vector3();
        this.camera.getWorldDirection(direction);
        direction.multiplyScalar(force);

        // 設置速度
        this.physics.setBodyVelocity(object, direction);

        // 重新加入可互動列表
        this.interactableObjects.push(object);

        // 清空手持狀態
        this.heldObject = null;
        this.isHolding = false;
    }

    /**
     * 更新手持物品位置（每幀調用）
     */
    update() {
        if (this.heldObject) {
            // 計算手持位置（相機前方偏右下）
            const holdPosition = new THREE.Vector3();
            holdPosition.copy(this.holdOffset);

            // 轉換到世界坐標
            holdPosition.applyMatrix4(this.camera.matrixWorld);

            // 更新物品位置
            this.heldObject.position.copy(holdPosition);

            // 使物品朝向相機方向
            const cameraDirection = new THREE.Vector3();
            this.camera.getWorldDirection(cameraDirection);

            // 計算旋轉
            const targetQuaternion = new THREE.Quaternion();
            const lookAtMatrix = new THREE.Matrix4();
            lookAtMatrix.lookAt(
                holdPosition,
                holdPosition.clone().add(cameraDirection),
                new THREE.Vector3(0, 1, 0)
            );
            targetQuaternion.setFromRotationMatrix(lookAtMatrix);

            // 平滑旋轉
            this.heldObject.quaternion.slerp(targetQuaternion, 0.2);
        }
    }

    /**
     * 獲取當前手持物品
     * @returns {THREE.Object3D|null}
     */
    getHeldObject() {
        return this.heldObject;
    }

    /**
     * 獲取手持物品類型
     * @returns {string|null}
     */
    getHeldObjectType() {
        if (!this.heldObject) {
            return null;
        }
        return this.objectTypes.get(this.heldObject);
    }

    /**
     * 是否正在拿著物品
     * @returns {boolean}
     */
    isHoldingObject() {
        return this.isHolding;
    }

    /**
     * 獲取當前瞄準的物品
     * @returns {Object|null}
     */
    getTargetedObject() {
        return this.targetedObject;
    }

    /**
     * 獲取互動提示文本
     * @returns {string}
     */
    getInteractionHint() {
        if (this.isHolding) {
            const type = this.getHeldObjectType();
            let itemName = this.getTypeName(type);

            // 如果是酒瓶，顯示酒類名稱
            if (type === 'bottle' && this.heldObject.userData.liquorType && this.cocktailSystem) {
                const liquorType = this.heldObject.userData.liquorType;
                const liquorInfo = this.cocktailSystem.liquorDatabase.get(liquorType);
                if (liquorInfo) {
                    itemName = `酒瓶(${liquorInfo.name})`;
                }
            }

            switch(type) {
                case 'bottle':
                    return `${itemName} | 按住滑鼠左鍵倒酒 | 按 R 放回酒牆`;
                case 'glass':
                    return `${itemName} | 按滑鼠右鍵喝掉 | 按 R 放回原位`;
                case 'shaker':
                    return `${itemName} | 按住滑鼠左鍵搖晃 | 按 R 放回原位`;
                case 'jigger':
                    return `${itemName} | 用於精確量酒 | 按 R 放回原位`;
                default:
                    return `${itemName} | 按 R 放回原位`;
            }
        } else if (this.targetedObject) {
            const type = this.targetedObject.type;
            const distance = this.targetedObject.distance.toFixed(1);
            const targetObj = this.targetedObject.object;
            let itemName = this.getTypeName(type);
            let additionalInfo = '';

            // 特殊處理：吉他顯示「互動」而非「拾取」
            if (type === 'guitar') {
                return `按 E 互動 ${itemName} (${distance}m)`;
            }

            // 如果是酒瓶，顯示酒類名稱
            if (type === 'bottle' && targetObj.userData.liquorType && this.cocktailSystem) {
                const liquorType = targetObj.userData.liquorType;
                const liquorInfo = this.cocktailSystem.liquorDatabase.get(liquorType);
                if (liquorInfo) {
                    itemName = liquorInfo.name;
                    additionalInfo = ` (${liquorInfo.displayName})`;
                }
                // 如果有自定義顯示名稱（材料瓶）
                if (targetObj.userData.displayName) {
                    itemName = targetObj.userData.displayName;
                }
            }
            // 如果是杯子或搖酒器，顯示內容物
            else if ((type === 'glass' || type === 'shaker') && this.cocktailSystem) {
                const contents = this.cocktailSystem.containerContents.get(targetObj);
                if (contents && contents.volume > 0) {
                    const ingredientNames = contents.ingredients
                        .map(ing => {
                            const info = this.cocktailSystem.liquorDatabase.get(ing.type);
                            return info ? info.name : ing.type;
                        })
                        .join('+');
                    additionalInfo = ` [${ingredientNames}, ${contents.volume.toFixed(0)}ml]`;
                } else {
                    additionalInfo = ' [空的]';
                }
            }

            return `按 E 拾取 ${itemName}${additionalInfo} (${distance}m)`;
        }

        return '';
    }

    /**
     * 獲取物品類型名稱（中文）
     * @param {string} type - 物品類型
     * @returns {string}
     */
    getTypeName(type) {
        const names = {
            'bottle': '酒瓶',
            'glass': '杯子',
            'shaker': '搖酒器',
            'jigger': '量酒器',
            'guitar': '吉他'
        };
        return names[type] || '物品';
    }

    /**
     * 檢測兩個物品是否接近（用於倒酒等互動）
     * @param {THREE.Object3D} obj1 - 物品1
     * @param {THREE.Object3D} obj2 - 物品2
     * @param {number} maxDistance - 最大距離
     * @returns {boolean}
     */
    areObjectsNear(obj1, obj2, maxDistance = 0.5) {
        if (!obj1 || !obj2) {
            return false;
        }
        return obj1.position.distanceTo(obj2.position) < maxDistance;
    }
}
