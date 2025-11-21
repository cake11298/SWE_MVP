import * as THREE from 'three';
import { BarEnvironment } from './modules/BarEnvironment';
import { NPCManager } from './modules/NPCManager';
import { PlayerController } from './modules/PlayerController';
import { LightingSystem } from './modules/LightingSystem';
import { RetirementLounge } from './modules/RetirementLounge';
import PhysicsSystem from './modules/PhysicsSystem';
import InteractionSystem from './modules/InteractionSystem';
import CocktailSystem from './modules/CocktailSystem';
import './styles/main.css';

class BarSimulator {
    constructor() {
        this.clock = new THREE.Clock();
        this.lastInteraction = false;
        this.lastPickup = false;
        this.lastDrop = false;
        this.lastReturn = false;
        this.lastRightMouse = false;
        this.lastGiveToNPC = false;
        this.isPaused = false;
        this.targetOutline = null;

        // 遊戲狀態
        this.gameState = 'menu'; // menu, playing, gameOver
        this.score = {
            satisfiedDrinks: 0,
            totalScore: 0,
            totalDrinks: 0,
            targetSatisfied: 5
        };

        // 音效系統
        this.audioContext = null;
        this.sfxVolume = 0.7;
        this.mouseSensitivity = 5;

        this.initMenu();
    }

    initMenu() {
        // 設定主選單按鈕事件
        document.getElementById('start-game-btn').addEventListener('click', () => this.startGame());
        document.getElementById('tutorial-btn').addEventListener('click', () => this.showTutorial());
        document.getElementById('settings-btn').addEventListener('click', () => this.showSettings());
        document.getElementById('credits-menu-btn').addEventListener('click', () => this.showCreditsFromMenu());

        // 設定面板
        document.getElementById('settings-back-btn').addEventListener('click', () => this.hideSettings());
        document.getElementById('sfx-volume').addEventListener('input', (e) => {
            this.sfxVolume = e.target.value / 100;
            document.getElementById('sfx-volume-value').textContent = `${e.target.value}%`;
        });
        document.getElementById('mouse-sensitivity').addEventListener('input', (e) => {
            this.mouseSensitivity = parseInt(e.target.value);
            document.getElementById('sensitivity-value').textContent = e.target.value;
            if (this.playerController) {
                this.playerController.setMouseSensitivity(this.mouseSensitivity / 2500);
            }
        });

        // 教學面板
        document.getElementById('tutorial-back-btn').addEventListener('click', () => this.hideTutorial());
        document.getElementById('tutorial-start-btn').addEventListener('click', () => {
            this.hideTutorial();
            this.startGame();
        });

        // 遊戲結束面板
        document.getElementById('play-again-btn').addEventListener('click', () => this.restartGame());
        document.getElementById('back-to-menu-btn').addEventListener('click', () => this.backToMenu());

        // 暫停選單
        document.getElementById('resume-btn').addEventListener('click', () => this.resumeGame());
        document.getElementById('pause-restart-btn').addEventListener('click', () => {
            this.hidePauseMenu();
            this.restartGame();
        });
        document.getElementById('pause-menu-btn').addEventListener('click', () => {
            this.hidePauseMenu();
            this.backToMenu();
        });

        // ESC 鍵暫停遊戲
        window.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.gameState === 'playing') {
                if (this.isPaused) {
                    this.resumeGame();
                } else {
                    this.pauseGame();
                }
            }
        });

        // 隱藏載入畫面
        document.getElementById('loading').classList.add('hidden');
    }

    pauseGame() {
        if (this.gameState !== 'playing') return;

        this.isPaused = true;
        document.exitPointerLock();
        document.getElementById('pause-menu').style.display = 'flex';
    }

    resumeGame() {
        this.isPaused = false;
        document.getElementById('pause-menu').style.display = 'none';
        document.body.requestPointerLock();
    }

    hidePauseMenu() {
        document.getElementById('pause-menu').style.display = 'none';
        this.isPaused = false;
    }

    startGame() {
        document.getElementById('main-menu').classList.add('hidden');
        this.gameState = 'playing';

        // 重置分數
        this.score = {
            satisfiedDrinks: 0,
            totalScore: 0,
            totalDrinks: 0,
            targetSatisfied: 5
        };
        this.updateScoreUI();

        // 顯示分數面板和控制提示
        document.getElementById('score-panel').classList.add('visible');
        document.getElementById('controls').style.display = 'block';

        // 初始化遊戲（只在第一次）
        if (!this.scene) {
            this.init();
        } else {
            // 重置遊戲狀態
            this.resetGameState();
        }

        // 請求滑鼠鎖定
        document.body.requestPointerLock();

        // 播放開始音效
        this.playSound('start');
    }

    resetGameState() {
        // 重置所有容器內容
        this.cocktailSystem.resetAllContainers();

        // 放回所有物品
        this.interactionSystem.returnAllObjects();

        // 重置玩家位置
        if (this.playerController) {
            this.playerController.position.set(0, 1.6, 5);
            this.playerController.rotation = 0;
            this.playerController.pitch = 0;
        }
    }

    showTutorial() {
        document.getElementById('tutorial-panel').style.display = 'flex';
    }

    hideTutorial() {
        document.getElementById('tutorial-panel').style.display = 'none';
    }

    showSettings() {
        document.getElementById('settings-panel').style.display = 'flex';
    }

    hideSettings() {
        document.getElementById('settings-panel').style.display = 'none';
    }

    showCreditsFromMenu() {
        document.getElementById('main-menu').classList.add('hidden');
        document.getElementById('credits-panel').style.display = 'block';

        // 修改關閉行為
        const closeHandler = (e) => {
            if (e.key.toLowerCase() === 'p') {
                document.getElementById('credits-panel').style.display = 'none';
                document.getElementById('main-menu').classList.remove('hidden');
                window.removeEventListener('keydown', closeHandler);
            }
        };
        window.addEventListener('keydown', closeHandler);
    }

    gameOver(won) {
        this.gameState = 'gameOver';
        this.isPaused = true;

        // 解除滑鼠鎖定
        document.exitPointerLock();

        // 更新結束畫面
        document.getElementById('game-over-title').textContent = won ? '恭喜過關！' : '遊戲結束';
        document.getElementById('final-satisfied').textContent = this.score.satisfiedDrinks;
        document.getElementById('final-score').textContent = this.score.totalScore;
        document.getElementById('final-drinks').textContent = this.score.totalDrinks;

        // 顯示結束畫面
        document.getElementById('game-over-panel').style.display = 'flex';

        // 隱藏遊戲 UI
        document.getElementById('score-panel').classList.remove('visible');
        document.getElementById('controls').style.display = 'none';

        // 播放音效
        this.playSound(won ? 'victory' : 'gameOver');
    }

    restartGame() {
        document.getElementById('game-over-panel').style.display = 'none';
        this.isPaused = false;
        this.startGame();
    }

    backToMenu() {
        document.getElementById('game-over-panel').style.display = 'none';
        document.getElementById('main-menu').classList.remove('hidden');
        this.gameState = 'menu';
        this.isPaused = false;
    }

    updateScoreUI() {
        document.getElementById('satisfied-count').textContent = this.score.satisfiedDrinks;
        document.getElementById('total-score').textContent = this.score.totalScore;
    }

    addScore(rating, npcName) {
        this.score.totalDrinks++;
        this.score.totalScore += rating;

        // 7分以上算滿意
        if (rating >= 7) {
            this.score.satisfiedDrinks++;
            this.playSound('success');
        } else {
            this.playSound('fail');
        }

        this.updateScoreUI();
        this.showRatingPopup(rating, npcName);

        // 檢查勝利條件
        if (this.score.satisfiedDrinks >= this.score.targetSatisfied) {
            setTimeout(() => this.gameOver(true), 2000);
        }
    }

    showRatingPopup(rating, npcName) {
        const ratingPanel = document.getElementById('recent-rating');
        const starsElement = document.getElementById('rating-stars');
        const textElement = document.getElementById('rating-text');

        // 生成星星
        let stars = '';
        const fullStars = Math.floor(rating / 2);
        const halfStar = rating % 2 >= 1;

        for (let i = 0; i < fullStars; i++) {
            stars += '⭐';
        }
        if (halfStar) {
            stars += '✨';
        }

        starsElement.textContent = stars || '💫';

        // 評價文字
        let comment = '';
        if (rating >= 9) {
            comment = '完美！';
        } else if (rating >= 7) {
            comment = '很棒！';
        } else if (rating >= 5) {
            comment = '還可以';
        } else {
            comment = '需要改進';
        }

        textElement.textContent = `${npcName}: ${rating}/10 - ${comment}`;

        // 顯示彈窗
        ratingPanel.style.display = 'block';

        // 3秒後隱藏
        setTimeout(() => {
            ratingPanel.style.display = 'none';
        }, 3000);
    }

    // 音效系統
    initAudio() {
        if (!this.audioContext) {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
        }
    }

    playSound(type) {
        this.initAudio();
        if (!this.audioContext) return;

        const oscillator = this.audioContext.createOscillator();
        const gainNode = this.audioContext.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(this.audioContext.destination);

        gainNode.gain.value = this.sfxVolume * 0.3;

        switch(type) {
            case 'pour':
                oscillator.frequency.value = 200;
                oscillator.type = 'sine';
                gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioContext.currentTime + 0.3);
                oscillator.start();
                oscillator.stop(this.audioContext.currentTime + 0.3);
                break;

            case 'shake':
                oscillator.frequency.value = 150;
                oscillator.type = 'triangle';
                // 搖晃效果
                const lfo = this.audioContext.createOscillator();
                lfo.frequency.value = 10;
                const lfoGain = this.audioContext.createGain();
                lfoGain.gain.value = 50;
                lfo.connect(lfoGain);
                lfoGain.connect(oscillator.frequency);
                lfo.start();
                oscillator.start();
                oscillator.stop(this.audioContext.currentTime + 0.2);
                lfo.stop(this.audioContext.currentTime + 0.2);
                break;

            case 'pickup':
                oscillator.frequency.value = 440;
                oscillator.type = 'sine';
                gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioContext.currentTime + 0.1);
                oscillator.start();
                oscillator.stop(this.audioContext.currentTime + 0.1);
                break;

            case 'drop':
                oscillator.frequency.value = 220;
                oscillator.type = 'sine';
                gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioContext.currentTime + 0.15);
                oscillator.start();
                oscillator.stop(this.audioContext.currentTime + 0.15);
                break;

            case 'success':
                // 成功音效 - 上升音階
                oscillator.frequency.value = 523;
                oscillator.type = 'sine';
                oscillator.frequency.exponentialRampToValueAtTime(784, this.audioContext.currentTime + 0.2);
                gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioContext.currentTime + 0.3);
                oscillator.start();
                oscillator.stop(this.audioContext.currentTime + 0.3);
                break;

            case 'fail':
                // 失敗音效 - 下降音階
                oscillator.frequency.value = 440;
                oscillator.type = 'sawtooth';
                oscillator.frequency.exponentialRampToValueAtTime(220, this.audioContext.currentTime + 0.3);
                gainNode.gain.value = this.sfxVolume * 0.15;
                gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioContext.currentTime + 0.3);
                oscillator.start();
                oscillator.stop(this.audioContext.currentTime + 0.3);
                break;

            case 'start':
                // 遊戲開始音效
                oscillator.frequency.value = 262;
                oscillator.type = 'sine';
                oscillator.frequency.setValueAtTime(330, this.audioContext.currentTime + 0.1);
                oscillator.frequency.setValueAtTime(392, this.audioContext.currentTime + 0.2);
                oscillator.frequency.setValueAtTime(523, this.audioContext.currentTime + 0.3);
                gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioContext.currentTime + 0.5);
                oscillator.start();
                oscillator.stop(this.audioContext.currentTime + 0.5);
                break;

            case 'victory':
                // 勝利音效 - 歡快旋律
                const notes = [523, 659, 784, 1047];
                notes.forEach((freq, i) => {
                    const osc = this.audioContext.createOscillator();
                    const gain = this.audioContext.createGain();
                    osc.connect(gain);
                    gain.connect(this.audioContext.destination);
                    osc.frequency.value = freq;
                    osc.type = 'sine';
                    gain.gain.value = this.sfxVolume * 0.2;
                    gain.gain.exponentialRampToValueAtTime(0.01, this.audioContext.currentTime + 0.3 + i * 0.15);
                    osc.start(this.audioContext.currentTime + i * 0.15);
                    osc.stop(this.audioContext.currentTime + 0.3 + i * 0.15);
                });
                break;

            case 'drink':
                // 喝酒音效
                oscillator.frequency.value = 100;
                oscillator.type = 'sine';
                gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioContext.currentTime + 0.5);
                oscillator.start();
                oscillator.stop(this.audioContext.currentTime + 0.5);
                break;
        }
    }
    
    init() {
        // 初始化 Three.js
        this.scene = new THREE.Scene();
        this.scene.fog = new THREE.Fog(0x1a0033, 10, 30);
        
        this.camera = new THREE.PerspectiveCamera(
            75,
            window.innerWidth / window.innerHeight,
            0.1,
            1000
        );
        
        this.renderer = new THREE.WebGLRenderer({ antialias: true });
        this.renderer.setSize(window.innerWidth, window.innerHeight);
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        document.getElementById('app').appendChild(this.renderer.domElement);
        
        // 載入模組
        this.retirementLounge = new RetirementLounge(this.scene);

        // 初始化新系統
        this.physicsSystem = new PhysicsSystem();
        this.interactionSystem = new InteractionSystem(this.camera, this.physicsSystem);
        this.cocktailSystem = new CocktailSystem(this.scene, this.interactionSystem);

        // 設定調酒系統引用到互動系統（用於顯示酒類名稱）
        this.interactionSystem.setCocktailSystem(this.cocktailSystem);

        // 載入環境（傳入系統引用）
        this.environment = new BarEnvironment(
            this.scene,
            this.interactionSystem,
            this.physicsSystem,
            this.cocktailSystem
        );

        this.npcManager = new NPCManager(this.scene);
        this.playerController = new PlayerController(this.camera);
        this.lightingSystem = new LightingSystem(this.scene);
        
        // 設定視窗調整
        window.addEventListener('resize', () => this.onWindowResize());

        // 初始化食譜系統
        this.initRecipeSystem();

        // 設定按鍵監聽（M鍵顯示食譜，P鍵顯示製作人資訊）
        this.setupUIControls();

        // 隱藏載入畫面
        document.getElementById('loading').classList.add('hidden');

        // 開始動畫
        this.animate();
    }
    
    onWindowResize() {
        this.camera.aspect = window.innerWidth / window.innerHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(window.innerWidth, window.innerHeight);
    }
    
    animate() {
        requestAnimationFrame(() => this.animate());

        const deltaTime = this.clock.getDelta();
        const elapsedTime = this.clock.getElapsedTime();

        // 如果遊戲暫停（顯示食譜或製作人資訊），不更新遊戲邏輯
        if (this.isPaused) {
            this.renderer.render(this.scene, this.camera);
            return;
        }

        // 更新各個系統
        this.playerController.update(deltaTime);
        this.npcManager.update(deltaTime);
        this.lightingSystem.update(elapsedTime);

        // 更新新系統
        this.physicsSystem.update(deltaTime);
        this.interactionSystem.update();
        this.cocktailSystem.update(deltaTime);

        // === 物品互動系統 ===
        // 檢測玩家瞄準的物品
        const targetedItem = this.interactionSystem.checkTargeted();

        // 處理拾取/放下物品
        const isPickupPressed = this.playerController.isPickupPressed();
        const isReturnPressed = this.playerController.isReturnPressed();

        // E 鍵：拾取物品 或 與 NPC 互動 或 彈吉他
        if (isPickupPressed && !this.lastPickup) {
            if (!this.interactionSystem.isHoldingObject() && targetedItem) {
                // 特殊處理：吉他不拾取，而是播放音樂
                if (targetedItem.type === 'guitar') {
                    this.npcManager.playGuitarSound();
                } else {
                    // 拾取物品
                    this.interactionSystem.pickupObject();
                    this.updateInteractionHint();
                    this.playSound('pickup');
                }
            } else {
                // 與 NPC 互動
                const npcTarget = this.npcManager.checkInteractions(this.playerController.position);
                this.npcManager.interact(npcTarget);
            }
            this.playerController.resetKey('e');
        }
        this.lastPickup = isPickupPressed;

        // R 鍵：放回原位
        if (isReturnPressed && !this.lastReturn) {
            if (this.interactionSystem.isHoldingObject()) {
                this.interactionSystem.dropObject(true);
                this.updateInteractionHint();
                this.playSound('drop');
            }
            this.playerController.resetKey('r');
        }
        this.lastReturn = isReturnPressed;

        // === 調酒互動 ===
        const heldObject = this.interactionSystem.getHeldObject();
        const heldType = this.interactionSystem.getHeldObjectType();

        if (heldObject && (heldType === 'bottle' || heldType === 'shaker')) {
            const nearbyTarget = this.findNearbyContainer(heldObject);
            this.highlightTarget(nearbyTarget);
        } else {
            this.highlightTarget(null);
        }


        // 滑鼠左鍵：倒酒 或 搖酒
        if (this.playerController.isLeftMouseHeld() && heldObject) {
            if (heldType === 'bottle') {
                // 倒酒：找尋附近的容器
                const nearbyGlass = this.findNearbyContainer(heldObject);
                if (nearbyGlass) {
                    const liquorType = heldObject.userData.liquorType;
                    // 傳入相機以進行視角和距離檢測
                    const wasPouring = this.cocktailSystem.isPouringActive;
                    this.cocktailSystem.pour(heldObject, nearbyGlass, liquorType, deltaTime, this.camera);
                    // 播放倒酒音效（每秒一次）
                    if (!wasPouring && this.cocktailSystem.isPouringActive) {
                        this.playSound('pour');
                    }
                }
            } else if (heldType === 'shaker') {
                // Shaker 邏輯：如果有內容且附近有容器，優先倒酒；否則搖酒
                const shakerContents = this.cocktailSystem.containerContents.get(heldObject);
                const nearbyContainer = this.findNearbyContainer(heldObject);

                if (shakerContents && shakerContents.volume > 0 && nearbyContainer) {
                    // 倒酒：從 shaker 倒入其他容器
                    const wasPouring = this.cocktailSystem.isPouringActive;
                    this.cocktailSystem.pourFromShaker(heldObject, nearbyContainer, deltaTime);
                    if (!wasPouring && this.cocktailSystem.isPouringActive) {
                        this.playSound('pour');
                    }
                } else {
                    // 搖酒
                    const wasShaking = this.cocktailSystem.isShakingActive;
                    this.cocktailSystem.shake(heldObject, deltaTime);
                    // 播放搖酒音效（開始時）
                    if (!wasShaking && this.cocktailSystem.isShakingActive) {
                        this.playSound('shake');
                    }
                }
            }
        } else {
            // 停止倒酒/搖酒
            if (this.cocktailSystem.isPouringActive) {
                this.cocktailSystem.stopPouring();
            }
            if (heldType === 'shaker' && this.cocktailSystem.isShakingActive) {
                this.cocktailSystem.stopShaking(heldObject);
            }
        }

        // 滑鼠右鍵：喝酒（開始動畫）
        const isRightPressed = this.playerController.isRightMousePressed();
        if (isRightPressed && !this.lastRightMouse && heldObject) {
            if (heldType === 'glass') {
                // 開始喝酒動畫
                this.cocktailSystem.drink(heldObject, true);
                this.playSound('drink');
            }
            this.playerController.resetMouseButton('right');
        }
        this.lastRightMouse = isRightPressed;

        // 檢查喝酒動畫是否完成並顯示訊息
        const lastDrinkInfo = this.cocktailSystem.getLastDrinkInfo();
        if (lastDrinkInfo) {
            this.showDrinkMessage(lastDrinkInfo);
        }

        // F 鍵：給附近的 NPC 喝酒
        const isGiveToNPCPressed = this.playerController.isKeyPressed('f');
        if (isGiveToNPCPressed && !this.lastGiveToNPC && heldObject && heldType === 'glass') {
            const nearbyNPC = this.npcManager.getNearbyNPC(this.playerController.position);
            if (nearbyNPC) {
                const contents = this.cocktailSystem.containerContents.get(heldObject);
                if (contents && contents.volume > 0) {
                    // NPC喝酒並給予評分（不使用動畫）
                    const drinkInfo = this.cocktailSystem.drink(heldObject, false);
                    if (drinkInfo) {
                        const evaluation = this.npcManager.npcDrinkCocktail(nearbyNPC, drinkInfo);
                        // 加入分數系統
                        if (evaluation && evaluation.rating) {
                            const npcName = nearbyNPC.userData.name || 'NPC';
                            this.addScore(evaluation.rating, npcName);
                        }
                    }
                }
            }
            this.playerController.resetKey('f');
        }
        this.lastGiveToNPC = isGiveToNPCPressed;

        // 顯示容器成分信息（當拿著杯子或搖酒器時）
        if (heldObject && (heldType === 'glass' || heldType === 'shaker')) {
            this.cocktailSystem.showContainerInfo(heldObject);
        } else {
            this.cocktailSystem.hideContainerInfo();
        }

        // 更新 UI 提示
        this.updateInteractionHint();

        // 渲染場景
        this.renderer.render(this.scene, this.camera);
    }

    /**
     * 尋找附近的容器（用於倒酒）
     * 需要同時滿足：1) 準星指向容器 2) 距離夠近（約2.5m）
     */
    findNearbyContainer(sourceObject) {
        const allContainers = [
            ...this.environment.glasses,
            this.environment.barTools.shaker
        ].filter(c => c !== null && c !== undefined);

        for (const container of allContainers) {
            if (container === sourceObject) continue;

            const distance = sourceObject.position.distanceTo(container.position);
            // 距離必須小於 2.5 米
            if (distance > 2.5) {
                continue;
            }

            // 計算相機到容器的方向
            const cameraToContainer = new THREE.Vector3();
            cameraToContainer.subVectors(container.position, this.camera.position).normalize();

            // 計算相機朝向（準星方向）
            const cameraDirection = new THREE.Vector3();
            this.camera.getWorldDirection(cameraDirection);

            // 計算角度（點積）
            const dotProduct = cameraDirection.dot(cameraToContainer);

            // 準星必須對準容器（角度小於30度，cos(30°) ≈ 0.866）
            if (dotProduct >= 0.85) {
                return container;
            }
        }

        return null;
    }

    /**
     * 更新互動提示
     */
    updateInteractionHint() {
        const hintElement = document.getElementById('interaction-hint');
        if (!hintElement) return;

        // 優先顯示物品互動提示
        let hint = this.interactionSystem.getInteractionHint();

        // 如果沒有物品互動提示，檢查是否靠近 NPC
        if (!hint) {
            const npcTarget = this.npcManager.checkInteractions(this.playerController.position);
            if (npcTarget.npc) {
                hint = `按 E 與 ${npcTarget.npc.userData.name} 交談`;
            } else if (npcTarget.interactable) {
                if (npcTarget.interactable.type === 'music') {
                    const action = this.npcManager.musicPlaying ? '關閉音樂' : '播放音樂';
                    hint = `按 E ${action}`;
                }
            }
        }

        hintElement.textContent = hint;

        if (hint) {
            hintElement.classList.add('visible');
        } else {
            hintElement.classList.remove('visible');
        }
    }

    /**
     * 顯示飲品訊息
     */
    showDrinkMessage(drinkInfo) {
        const messageElement = document.getElementById('drink-message');
        if (!messageElement) return;

        messageElement.textContent = `你喝了 ${drinkInfo.name}！容量: ${drinkInfo.volume.toFixed(0)}ml`;
        messageElement.classList.add('visible');

        setTimeout(() => {
            messageElement.classList.remove('visible');
        }, 3000);
    }

    /**
     * 初始化食譜系統
     */
    initRecipeSystem() {
        // 獲取可用的材料
        const availableIngredients = Array.from(this.cocktailSystem.liquorDatabase.keys());

        // 定義經典IBA調酒食譜（至少25個）
        this.cocktailRecipes = this.cocktailSystem.getCocktailRecipes();

        // 載入食譜到UI
        this.loadRecipesToUI();
    }

    /**
     * 載入食譜到UI
     */
    loadRecipesToUI() {
        const recipeContent = document.getElementById('recipe-content');
        if (!recipeContent) return;

        recipeContent.innerHTML = '';

        this.cocktailRecipes.forEach(recipe => {
            const card = document.createElement('div');
            card.className = 'recipe-card';

            const ingredientsList = recipe.ingredients.map(ing => {
                return `<li>${ing.amount} - ${ing.name}</li>`;
            }).join('');

            card.innerHTML = `
                <h3>${recipe.name}</h3>
                <ul class="recipe-ingredients">
                    ${ingredientsList}
                </ul>
                <p class="recipe-method">${recipe.method}</p>
            `;

            recipeContent.appendChild(card);
        });
    }

    /**
     * 設定UI控制（M鍵和P鍵）
     */
    setupUIControls() {
        let lastMKey = false;
        let lastPKey = false;

        window.addEventListener('keydown', (e) => {
            const key = e.key.toLowerCase();

            // M鍵：顯示/隱藏食譜面板
            if (key === 'm' && !lastMKey) {
                this.toggleRecipePanel();
                lastMKey = true;
            }

            // P鍵：顯示/隱藏製作人資訊面板
            if (key === 'p' && !lastPKey) {
                this.toggleCreditsPanel();
                lastPKey = true;
            }
        });

        window.addEventListener('keyup', (e) => {
            const key = e.key.toLowerCase();

            if (key === 'm') {
                lastMKey = false;
            }

            if (key === 'p') {
                lastPKey = false;
            }
        });
    }

    /**
     * 切換食譜面板
     */
    toggleRecipePanel() {
        const panel = document.getElementById('recipe-panel');
        if (!panel) return;

        const isVisible = panel.style.display === 'block';

        if (isVisible) {
            panel.style.display = 'none';
            this.isPaused = false;
            // 恢復滑鼠鎖定
            if (document.pointerLockElement === null) {
                document.body.requestPointerLock();
            }
        } else {
            panel.style.display = 'block';
            this.isPaused = true;
            // 解除滑鼠鎖定
            document.exitPointerLock();
        }
    }

    highlightTarget(target) {
        // 移除舊的外框
        if (this.targetOutline) {
            this.scene.remove(this.targetOutline);
            this.targetOutline = null;
        }

        // 如果有新目標，創建黃色外框
        if (target) {
            const outlineGeometry = new THREE.CylinderGeometry(0.25, 0.25, 0.7, 16, 1, true);
            const outlineMaterial = new THREE.MeshBasicMaterial({
                color: 0xFFD700,
                transparent: true,
                opacity: 0.6,
                side: THREE.DoubleSide
            });
            this.targetOutline = new THREE.Mesh(outlineGeometry, outlineMaterial);
            this.targetOutline.position.copy(target.position);
            this.targetOutline.position.y += 0.35;
            this.scene.add(this.targetOutline);
        }
    }

    /**
     * 切換製作人資訊面板
     */
    toggleCreditsPanel() {
        const panel = document.getElementById('credits-panel');
        if (!panel) return;

        const isVisible = panel.style.display === 'block';

        if (isVisible) {
            panel.style.display = 'none';
            this.isPaused = false;
            // 恢復滑鼠鎖定
            if (document.pointerLockElement === null) {
                document.body.requestPointerLock();
            }
        } else {
            panel.style.display = 'block';
            this.isPaused = true;
            // 解除滑鼠鎖定
            document.exitPointerLock();
        }
    }
}

// 啟動應用
window.addEventListener('DOMContentLoaded', () => {
    new BarSimulator();
});