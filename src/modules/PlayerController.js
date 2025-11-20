import * as THREE from 'three';  

export class PlayerController {
    constructor(camera, domElement = document.body) {
        this.camera = camera;
        this.domElement = domElement;
        this.position = new THREE.Vector3(0, 1.6, 5);
        this.rotation = 0;
        this.pitch = 0; // 上下視角
        this.speed = 5;
        this.mouseSensitivity = 0.002;
        
        this.keys = {};
        this.keyPressed = {}; // 追蹤按鍵狀態，避免重複觸發
        this.isLocked = false;

        // 滑鼠按鍵狀態
        this.mouseButtons = {
            left: false,
            right: false
        };
        this.mousePressed = {
            left: false,
            right: false
        };

        this.setupEventListeners();
    }
    
    setupEventListeners() {
        // 鍵盤事件
        window.addEventListener('keydown', (e) => {
            const key = e.key.toLowerCase();
            
            // 避免重複觸發
            if (!this.keyPressed[key]) {
                this.keyPressed[key] = true;
                
                // 處理特殊按鍵
                if (key === 'x') {
                    this.handleCloseSpecialDialogue();
                }
            }
            
            this.keys[key] = true;
        });
        
        window.addEventListener('keyup', (e) => {
            const key = e.key.toLowerCase();
            this.keys[key] = false;
            this.keyPressed[key] = false;
        });
        
        // 滑鼠鎖定事件
        this.domElement.addEventListener('click', () => {
            this.domElement.requestPointerLock();
        });
        
        // 指針鎖定狀態改變
        document.addEventListener('pointerlockchange', () => {
            this.isLocked = document.pointerLockElement === this.domElement;
        });
        
        // 滑鼠移動事件
        document.addEventListener('mousemove', (e) => {
            if (!this.isLocked) return;
            
            // 水平旋轉（左右）
            this.rotation -= e.movementX * this.mouseSensitivity;
            
            // 垂直旋轉（上下）
            this.pitch -= e.movementY * this.mouseSensitivity;
            
            // 限制上下視角範圍
            this.pitch = Math.max(-Math.PI / 2, Math.min(Math.PI / 2, this.pitch));
        });
        
        // ESC 鍵退出滑鼠鎖定
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                document.exitPointerLock();
            }
        });

        // 滑鼠按鍵事件
        document.addEventListener('mousedown', (e) => {
            if (!this.isLocked) return;

            if (e.button === 0) { // 左鍵
                this.mouseButtons.left = true;
                if (!this.mousePressed.left) {
                    this.mousePressed.left = true;
                }
            } else if (e.button === 2) { // 右鍵
                this.mouseButtons.right = true;
                if (!this.mousePressed.right) {
                    this.mousePressed.right = true;
                }
            }
        });

        document.addEventListener('mouseup', (e) => {
            if (e.button === 0) {
                this.mouseButtons.left = false;
                this.mousePressed.left = false;
            } else if (e.button === 2) {
                this.mouseButtons.right = false;
                this.mousePressed.right = false;
            }
        });

        // 禁用右鍵菜單
        document.addEventListener('contextmenu', (e) => {
            if (this.isLocked) {
                e.preventDefault();
            }
        });
    }
    
    handleCloseSpecialDialogue() {
        // 尋找並關閉特殊對話框
        const specialDialogue = document.getElementById('special-dialogue-box');
        if (specialDialogue) {
            specialDialogue.remove();
        }
    }
    
    update(deltaTime = 0.016) {
        // 計算移動方向
        const moveVector = new THREE.Vector3();
        
        if (this.keys['w']) {
            moveVector.z -= 1;
        }
        if (this.keys['s']) {
            moveVector.z += 1;
        }
        if (this.keys['a']) {
            moveVector.x -= 1;
        }
        if (this.keys['d']) {
            moveVector.x += 1;
        }
        
        // 應用旋轉到移動向量
        if (moveVector.length() > 0) {
            moveVector.normalize();
            moveVector.multiplyScalar(this.speed * deltaTime);
            moveVector.applyAxisAngle(new THREE.Vector3(0, 1, 0), this.rotation);
            
            const newPosition = this.position.clone().add(moveVector);
            const inExpandedRoom = Math.abs(newPosition.x) < 10 && 
                                newPosition.z > -9.8 && newPosition.z < 15;

            if (inExpandedRoom) {
                this.position.add(moveVector);
            }
        }
        
        // 更新相機位置
        this.camera.position.copy(this.position);
        
        // 正確的相機旋轉方式 - 使用歐拉角並設定正確的旋轉順序
        this.camera.rotation.order = 'YXZ';
        this.camera.rotation.y = this.rotation;
        this.camera.rotation.x = this.pitch;
        this.camera.rotation.z = 0;
    }
    
    isInteracting() {
        return this.keys['e'] && this.keyPressed['e'];
    }
    
    // 重置互動按鍵狀態，避免重複觸發
    resetInteractionKey() {
        this.keyPressed['e'] = false;
    }
    
    // 取得滑鼠鎖定狀態
    isMouseLocked() {
        return this.isLocked;
    }
    
    // 手動設定滑鼠敏感度
    setMouseSensitivity(sensitivity) {
        this.mouseSensitivity = sensitivity;
    }

    // 檢查是否按下拾取鍵 (E)
    isPickupPressed() {
        return this.keys['e'] && this.keyPressed['e'];
    }

    // 檢查是否按下放回原位鍵 (R)
    isReturnPressed() {
        return this.keys['r'] && this.keyPressed['r'];
    }

    // 檢查是否按住滑鼠左鍵（倒酒/搖酒）
    isLeftMouseHeld() {
        return this.mouseButtons.left;
    }

    // 檢查是否按下滑鼠右鍵（喝酒）
    isRightMousePressed() {
        return this.mouseButtons.right && this.mousePressed.right;
    }

    // 重置按鍵狀態
    resetKey(key) {
        this.keyPressed[key] = false;
    }

    // 重置滑鼠按鍵狀態
    resetMouseButton(button) {
        if (button === 'left') {
            this.mousePressed.left = false;
        } else if (button === 'right') {
            this.mousePressed.right = false;
        }
    }

    // 通用按鍵檢測方法
    isKeyPressed(key) {
        key = key.toLowerCase();
        return this.keys[key] && this.keyPressed[key];
    }
}