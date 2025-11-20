import * as THREE from 'three';

export class NPCManager {
    constructor(scene) {
        this.scene = scene;
        this.npcs = [];
        this.interactableObjects = [];
        this.gustaveInteractionCount = 0;
        this.audioContext = null;
        this.currentlyPlaying = null;
        this.musicPlaying = false; 
        this.musicGainNode = null;
        this.dialogueTimer = null;
        this.createNPCs();
        this.createMusicSystem();
    }
    
    createNPCs() {
        // Gustave - 分子調酒專家
        this.addNPC({
            name: 'Gustave',
            position: new THREE.Vector3(2, 0, -5),
            shirtColor: 0x0066cc,
            pantsColor: 0x1a1a1a,
            role: '調酒社創始社長',
            dialogues: [
                "嗨！我是 Gustave Yang，NCU 分子創意飲品研究社的創辦人！",
                "你知道嗎？ 曦樂他家裡有養一隻貓！",
                "我們用煙燻機差點把教研大樓燒了",
                "想像一下，上次曦樂把汽水提早加入雪克杯，搖到一半炸開了",
                "分子調酒不只是技術，更是科學與藝術的融合",
                "自由無價",
                "有些事情不是靠早上的一杯咖啡解決，而是晚上的一杯調酒遺忘的",
                "分子調酒的知識非常寬廣，像是...痾我忘記了",
                "說道搖盪法，你需要有點想像力，想像液體在裡面的螺旋混和",
                "如果感覺快撐不住了 記得去給加油站 因為他們會跟你說 要加油喔!",
                "什麼? 你說你有口水病 上次這樣說的人已經在溫哥華了"
            ]
        });
        
        // Seaton - 調酒技術大師
        this.addNPC({
            name: 'Seaton',
            position: new THREE.Vector3(-2, 0, -5),
            shirtColor: 0xcc0066,
            pantsColor: 0x333333,
            role: '調酒社共同創辦人',
            dialogues: [
                "哈囉！我是 Seaton 曦樂，也是社團的共同創辦人！",
                "挖勒，還得(ㄉㄟˇ)是你啊",
                "我最喜歡日本威士忌，特別是安德手裡那瓶山崎12年",
                "今天超累，回家打一場招喚峽谷",
                "我最喜歡的是經典的 Old Fashioned"
            ]
        });

        // 正安 - 女性幹部（休閒風格）
        this.addNPC({
            name: '正安',
            position: new THREE.Vector3(9, 0, 1),
            shirtColor: 0xffb6c1, // 淺粉紅色上衣
            pantsColor: 0x4169e1, // 藍色牛仔褲
            role: '公關兼副社長',
            rotation: -Math.PI / 2,
            dialogues: [
                "嗨！我是正安，負責社團的活動企劃~",
                "下次我們要辦一個分子調酒工作坊，你要來嗎？",
                "我最喜歡規劃有趣的主題派對了！",
                "你知道嗎？我們社團的IG都是我在經營的",
                "最近在策劃跟其他學校的交流活動",
                "調酒雖然不是我的強項，但活動規劃我可是專家！",
                "上次社團聚餐大家都玩得很開心呢",
                "我們下個月要辦調酒比賽，獎品超豐富的！",
                "社團的氣氛真的很棒，大家都像家人一樣",
                "有時候忙到很晚，但看到活動成功就覺得很值得",
                "你有沒有想過要加入我們的幹部團隊？",
                "最近在想要不要辦一個調酒主題的快閃活動"
            ],
            gender: 'female'
        });

        // 瑜柔 - 女性幹部（文青風格）
        this.addNPC({
            name: '瑜柔(宅魚)',
            position: new THREE.Vector3(9, 0, 3),
            shirtColor: 0x90ee90, // 淺綠色襯衫
            pantsColor: 0x2f4f4f, // 深灰色長褲
            role: '學術研究長',
            rotation: -Math.PI / 2,
            dialogues: [
                "我是瑜柔，主要負責社團的學術研究部分",
                "分子調酒其實有很深的化學原理呢",
                "我正在研究不同酒類的分子結構對味覺的影響",
                "圖書館是我最常去的地方，那裡有很多調酒相關的文獻",
                "你有興趣了解調酒背後的科學原理嗎？",
                "理論與實務並重，這是我們社團的特色"
            ],
            gender: 'female'
        });

        // 恩若 - 女性幹部（活潑風格）
        this.addNPC({
            name: '恩若',
            position: new THREE.Vector3(9, 0, -1),
            shirtColor: 0xffd700, // 金黃色T恤
            pantsColor: 0x8b4513, // 棕色短裙
            role: '美宣長',
            rotation: -Math.PI / 2,
            dialogues: [
                "嗨嗨！我是恩若，負責美宣設計",
                "你有追蹤我們的社群媒體嗎？記得按讚分享喔！",
                "下次有校外比賽我會負責報名參加",
                "我最擅長跟其他社團建立友好關係",
                "想要更了解我們社團就來找我聊天吧！",
                "高雄是個美麗的城市真的! 有機會一定要來玩"
            ],
            gender: 'female'
        });

        // 旻偉 - 男性幹部（休閒風格）
        this.addNPC({
            name: '旻偉',
            position: new THREE.Vector3(9, 0, 5),
            shirtColor: 0x708090, // 灰藍色polo衫
            pantsColor: 0x556b2f, // 軍綠色休閒褲
            role: '器材長',
            rotation: -Math.PI / 2,
            dialogues: [
                "我是旻偉，負責社團的總務和設備管理",
                "所有的調酒器材都是我在保養維護的",
                "預算規劃和採購也都是我的工作",
                "雖然我調酒技術普通，但管理能力還不錯",
                "需要什麼設備或材料都可以跟我說",
                "社團能順利運作，總務工作很重要呢",
                "陳昱嘉的線性代數好課推推"
            ],
            gender: 'male'
        });
        
        // 創建音箱互動物件
        this.createAmpInteraction();
    }
    
    createMusicSystem() {
        // 初始化 Web Audio API
        try {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
        } catch (error) {
            console.log('Web Audio API not supported');
        }
    }
    
    createAmpInteraction() {
        // 在音箱位置創建一個互動區域
        const ampInteraction = {
            position: new THREE.Vector3(8, 0, -8), // 音箱位置
            radius: 2.5,
            name: 'Guitar Amplifier',
            type: 'music',
            action: () => this.playGuitarSound()
        };
        
        this.interactableObjects.push(ampInteraction);
    }
    
    playGuitarSound() {
        if (!this.audioContext) return;
        
        if (this.musicPlaying) {
            // 如果正在播放，則停止音樂
            this.stopMusic();
            return;
        }
        
        // 開始播放音樂
        this.startMusic();
    }

    // 新增：開始播放音樂的方法
    startMusic() {
        if (!this.audioContext) return;
        
        // 創建主增益節點
        this.musicGainNode = this.audioContext.createGain();
        this.musicGainNode.connect(this.audioContext.destination);
        this.musicGainNode.gain.setValueAtTime(0.3, this.audioContext.currentTime);
        
        // 播放背景旋律
        this.playMelody();
        
        // 播放節拍
        this.playDrums();
        
        this.musicPlaying = true;
        this.showMusicNotification('🎵 音樂播放中...', '#00ff00');
    }

    // 新增：停止音樂的方法
    stopMusic() {
        if (this.musicGainNode) {
            // 漸層降低音量後斷開
            const fadeTime = 0.5;
            this.musicGainNode.gain.exponentialRampToValueAtTime(0.001, 
                this.audioContext.currentTime + fadeTime);
            
            setTimeout(() => {
                if (this.musicGainNode) {
                    this.musicGainNode.disconnect();
                    this.musicGainNode = null;
                }
            }, fadeTime * 1000);
        }
        
        this.musicPlaying = false;
        this.showMusicNotification('🔇 音樂已停止', '#ff6600');
    }

    // 新增：播放旋律的方法
    playMelody() {
        // 8-bit 風格的簡單旋律（超級瑪利歐風格）
        const melody = [
            {note: 659.25, time: 0, duration: 0.15},      // E5
            {note: 659.25, time: 0.2, duration: 0.15},    // E5
            {note: 0, time: 0.4, duration: 0.15},         // 休止符
            {note: 659.25, time: 0.6, duration: 0.15},    // E5
            {note: 0, time: 0.8, duration: 0.15},         // 休止符
            {note: 523.25, time: 1.0, duration: 0.15},    // C5
            {note: 659.25, time: 1.2, duration: 0.15},    // E5
            {note: 0, time: 1.4, duration: 0.15},         // 休止符
            {note: 783.99, time: 1.6, duration: 0.3},     // G5
            {note: 0, time: 2.0, duration: 0.3},          // 休止符
            {note: 392.00, time: 2.4, duration: 0.3},     // G4
        ];
        
        const startTime = this.audioContext.currentTime;
        
        melody.forEach(note => {
            if (note.note === 0) return; // 跳過休止符
            
            const oscillator = this.audioContext.createOscillator();
            const noteGain = this.audioContext.createGain();
            
            oscillator.type = 'square'; // 8-bit 方波音色
            oscillator.frequency.setValueAtTime(note.note, startTime + note.time);
            
            // 8-bit 風格的音量包絡（快速衰減）
            noteGain.gain.setValueAtTime(0, startTime + note.time);
            noteGain.gain.linearRampToValueAtTime(0.3, startTime + note.time + 0.01);
            noteGain.gain.exponentialRampToValueAtTime(0.001, 
                startTime + note.time + note.duration);
            
            oscillator.connect(noteGain);
            noteGain.connect(this.musicGainNode);
            
            oscillator.start(startTime + note.time);
            oscillator.stop(startTime + note.time + note.duration);
        });
        
        // 循環播放
        if (this.musicPlaying) {
            setTimeout(() => {
                if (this.musicPlaying) {
                    this.playMelody();
                }
            }, 2800); // 2.8秒後重複
        }
    }

    // 新增：播放鼓點的方法（8-bit 風格）
    playDrums() {
        const drumPattern = [0, 0.4, 0.8, 1.2, 1.6, 2.0, 2.4]; // 8-bit 節奏
        const startTime = this.audioContext.currentTime;
        
        drumPattern.forEach(time => {
            const oscillator = this.audioContext.createOscillator();
            const drumGain = this.audioContext.createGain();
            
            oscillator.type = 'triangle'; // 使用三角波
            oscillator.frequency.setValueAtTime(80, startTime + time); // 更低的鼓聲頻率
            
            drumGain.gain.setValueAtTime(0, startTime + time);
            drumGain.gain.linearRampToValueAtTime(0.25, startTime + time + 0.005);
            drumGain.gain.exponentialRampToValueAtTime(0.001, startTime + time + 0.1);
            
            oscillator.connect(drumGain);
            drumGain.connect(this.musicGainNode);
            
            oscillator.start(startTime + time);
            oscillator.stop(startTime + time + 0.1);
        });
        
        // 循環播放鼓點
        if (this.musicPlaying) {
            setTimeout(() => {
                if (this.musicPlaying) {
                    this.playDrums();
                }
            }, 2800); // 與旋律同步
        }
    }
    
    showMusicNotification(message = '🎸 Electric Guitar Playing...', color = '#00ff00') {
        // 創建音樂播放通知
        const notification = document.createElement('div');
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: rgba(0, 0, 0, 0.8);
            color: ${color};
            padding: 15px 20px;
            border-radius: 8px;
            font-family: Arial, sans-serif;
            font-size: 16px;
            z-index: 1000;
            border: 2px solid ${color};
            animation: fadeInOut 3s ease-in-out;
        `;
        
        notification.textContent = message;
        document.body.appendChild(notification);
        
        // 添加 CSS 動畫（保持原樣）
        if (!document.getElementById('music-notification-style')) {
            const style = document.createElement('style');
            style.id = 'music-notification-style';
            style.textContent = `
                @keyframes fadeInOut {
                    0% { opacity: 0; transform: translateX(100px); }
                    20% { opacity: 1; transform: translateX(0); }
                    80% { opacity: 1; transform: translateX(0); }
                    100% { opacity: 0; transform: translateX(100px); }
                }
            `;
            document.head.appendChild(style);
        }
        
        setTimeout(() => {
            if (document.body.contains(notification)) {
                document.body.removeChild(notification);
            }
        }, 3000);
    }
    
    addNPC(config) {
        const npc = new THREE.Group();
        const isFemale = config.gender === 'female';
        
        // 根據性別調整體型參數
        const shoulderWidth = isFemale ? 0.35 : 0.4;
        
        // === 軀幹設計根據性別和角色身份調整 ===
        let shirtGeometry, vestGeometry = null;
        
        if (isFemale) {
            // 女性上衣（比較修身）
            shirtGeometry = new THREE.CylinderGeometry(0.35, 0.4, 0.9, 16);
        } else if (config.role.includes('部長') && !config.role.includes('創')) {
            // 男性幹部休閒裝
            shirtGeometry = new THREE.CylinderGeometry(0.38, 0.42, 0.95, 16);
        } else {
            // 原本的調酒師裝扮
            shirtGeometry = new THREE.CylinderGeometry(0.4, 0.45, 1.0, 16);
            vestGeometry = new THREE.CylinderGeometry(0.42, 0.47, 1.0, 16, 1, true);
        }
        
        const shirt = new THREE.Mesh(
            shirtGeometry,
            new THREE.MeshStandardMaterial({ color: config.shirtColor, roughness: 0.8 })
        );
        shirt.position.y = 1.2;
        shirt.castShadow = true;

        // 只有調酒師才穿背心
        let vest = null;
        if (vestGeometry) {
            vest = new THREE.Mesh(
                vestGeometry,
                new THREE.MeshStandardMaterial({ color: 0x222222, roughness: 0.6 })
            );
            vest.position.y = 1.2;
            vest.castShadow = true;
        }
        
        // === 褲子/裙子 ===
        let bottomGeometry;
        if (isFemale && config.pantsColor === 0x8b4513) {
            // 短裙設計（恩若）
            bottomGeometry = new THREE.CylinderGeometry(0.45, 0.35, 0.6, 16);
        } else {
            // 一般褲子
            bottomGeometry = new THREE.CylinderGeometry(0.35, 0.35, 1.0, 16);
        }
        
        const pants = new THREE.Mesh(
            bottomGeometry,
            new THREE.MeshStandardMaterial({ color: config.pantsColor, roughness: 0.9 })
        );
        pants.position.y = (isFemale && config.pantsColor === 0x8b4513) ? 0.4 : 0.2;
        pants.castShadow = true;

        // === 腿 ===
        const leftLeg = new THREE.Mesh(
            new THREE.CylinderGeometry(0.15, 0.15, 1.0, 16),
            new THREE.MeshStandardMaterial({ color: config.pantsColor })
        );
        leftLeg.position.set(-0.18, -0.3, 0);
        leftLeg.castShadow = true;

        const rightLeg = new THREE.Mesh(
            new THREE.CylinderGeometry(0.15, 0.15, 1.0, 16),
            new THREE.MeshStandardMaterial({ color: config.pantsColor })
        );
        rightLeg.position.set(0.18, -0.3, 0);
        rightLeg.castShadow = true;

        // === 手臂 ===
        const armDistance = shoulderWidth + 0.15;
        const leftArm = new THREE.Mesh(
            new THREE.CylinderGeometry(0.12, 0.12, 0.8, 12),
            new THREE.MeshStandardMaterial({ color: 0xfdbcb4 })
        );
        leftArm.position.set(-armDistance, 1.2, 0);
        leftArm.castShadow = true;

        const rightArm = new THREE.Mesh(
            new THREE.CylinderGeometry(0.12, 0.12, 0.8, 12),
            new THREE.MeshStandardMaterial({ color: 0xfdbcb4 })
        );
        rightArm.position.set(armDistance, 1.2, 0);
        rightArm.castShadow = true;

        // === 頭部 ===
        const head = new THREE.Mesh(
            new THREE.SphereGeometry(0.35, 16, 16),
            new THREE.MeshStandardMaterial({ color: 0xfdbcb4 })
        );
        head.position.y = 2.1;
        head.castShadow = true;

        // === 頭髮（根據性別調整）===
        let hairGeometry;
        if (isFemale) {
            // 女性長髮
            hairGeometry = new THREE.SphereGeometry(0.38, 16, 16, 0, Math.PI * 2, 0, Math.PI * 0.4);
        } else {
            // 男性短髮
            hairGeometry = new THREE.SphereGeometry(0.36, 16, 16, 0, Math.PI * 2, 0, Math.PI * 0.35);
        }
        
        const hair = new THREE.Mesh(
            hairGeometry,
            new THREE.MeshStandardMaterial({ color: 0x333333, roughness: 0.7 })
        );
        hair.position.y = 2.1;
        hair.castShadow = true;

        // === 領結/領帶（只有正式場合才戴）===
        let accessory = null;
        if (!isFemale && vestGeometry) {
            accessory = new THREE.Mesh(
                new THREE.BoxGeometry(0.3, 0.1, 0.05),
                new THREE.MeshStandardMaterial({ color: 0x990000, metalness: 0.2 })
            );
            accessory.position.set(0, 1.7, 0.35);
        }

        // === 手持物品（只有調酒師拿酒杯）===
        let handItem = null;
        if (vestGeometry) {
            handItem = new THREE.Mesh(
                new THREE.CylinderGeometry(0.1, 0.12, 0.3, 16),
                new THREE.MeshPhysicalMaterial({ 
                    color: 0xffffff, 
                    transparent: true, 
                    opacity: 0.4, 
                    roughness: 0, 
                    metalness: 0,
                    transmission: 1.0,
                    ior: 1.5
                })
            );
            handItem.position.set(0.75, 1.2, 0);
            handItem.rotation.x = Math.PI / 8;
        }

        // === 臉部（眼睛 + 嘴巴）===
        const faceGroup = new THREE.Group();
        faceGroup.position.y = 2.1;

        const leftEye = new THREE.Mesh(
            new THREE.SphereGeometry(0.05, 8, 8),
            new THREE.MeshBasicMaterial({ color: 0x000000 })
        );
        leftEye.position.set(-0.12, 0.05, 0.33);

        const rightEye = new THREE.Mesh(
            new THREE.SphereGeometry(0.05, 8, 8),
            new THREE.MeshBasicMaterial({ color: 0x000000 })
        );
        rightEye.position.set(0.12, 0.05, 0.33);

        const mouth = new THREE.Mesh(
            new THREE.TorusGeometry(0.12, 0.02, 8, 16, Math.PI),
            new THREE.MeshBasicMaterial({ color: 0x000000 })
        );
        mouth.rotation.x = Math.PI;
        mouth.position.set(0, -0.1, 0.32);

        faceGroup.add(leftEye);
        faceGroup.add(rightEye);
        faceGroup.add(mouth);

        // === 名字標籤 ===
        const nameTag = this.createNameTag(config.name, config.role);
        nameTag.position.y = 2.8;

        // === 組裝 ===
        npc.add(shirt);
        if (vest) npc.add(vest);
        npc.add(pants);
        npc.add(leftLeg);
        npc.add(rightLeg);
        npc.add(leftArm);
        npc.add(rightArm);
        npc.add(head);
        npc.add(hair);
        if (accessory) npc.add(accessory);
        if (handItem) npc.add(handItem);
        npc.add(faceGroup);
        npc.add(nameTag);

        npc.position.copy(config.position);
        // 如果有設定旋轉角度，就應用旋轉
        if (config.rotation !== undefined) {
            npc.rotation.y = config.rotation;
        }

        // 儲存資料
        npc.userData = {
            name: config.name,
            role: config.role,
            dialogues: config.dialogues,
            currentDialogue: 0,
            originalY: config.position.y,
            baseRotation: config.rotation || 0, // 新增：儲存基礎旋轉角度
            nameTagSprite: nameTag.children[0]
        };

        this.npcs.push(npc);
        this.scene.add(npc);
    }
    
    createNameTag(name, role) {
        const tagGroup = new THREE.Group();
        
        // 創建 Canvas 來繪製文字
        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');
        canvas.width = 512;
        canvas.height = 256;
        
        // 清除畫布並設定透明背景
        context.clearRect(0, 0, canvas.width, canvas.height);
        
        // 繪製半透明背景
        context.fillStyle = 'rgba(0, 0, 0, 0.7)';
        context.roundRect = context.roundRect || function(x, y, w, h, r) {
            context.beginPath();
            context.moveTo(x + r, y);
            context.lineTo(x + w - r, y);
            context.arc(x + w - r, y + r, r, -Math.PI/2, 0);
            context.lineTo(x + w, y + h - r);
            context.arc(x + w - r, y + h - r, r, 0, Math.PI/2);
            context.lineTo(x + r, y + h);
            context.arc(x + r, y + h - r, r, Math.PI/2, Math.PI);
            context.lineTo(x, y + r);
            context.arc(x + r, y + r, r, Math.PI, -Math.PI/2);
            context.closePath();
            context.fill();
        };
        
        // 繪製圓角矩形背景
        if (context.roundRect) {
            context.roundRect(50, 70, 412, 116, 20);
        } else {
            context.fillRect(50, 70, 412, 116);
        }
        
        // 繪製名字
        context.fillStyle = 'white';
        context.font = 'bold 64px Arial';
        context.textAlign = 'center';
        context.textBaseline = 'middle';
        context.fillText(name, 256, 110);
        
        // 繪製角色職稱
        context.font = '40px Arial';
        context.fillStyle = '#FFD700';
        context.fillText(role, 256, 160);
        
        // 創建材質和精靈
        const texture = new THREE.CanvasTexture(canvas);
        texture.needsUpdate = true;
        
        const spriteMaterial = new THREE.SpriteMaterial({ 
            map: texture,
            transparent: true,
            depthWrite: false // 避免深度問題
        });
        
        const sprite = new THREE.Sprite(spriteMaterial);
        sprite.scale.set(2, 1, 1);
        
        tagGroup.add(sprite);
        
        return tagGroup;
    }
    
    createSpecialDialogue(text) {
        // 創建特殊的對話框
        const dialogueBox = document.createElement('div');
        dialogueBox.id = 'special-dialogue-box';
        dialogueBox.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: linear-gradient(135deg, #faca2cff, #ec692bff);
            color: white;
            padding: 30px 40px;
            border-radius: 15px;
            font-family: Arial, sans-serif;
            font-size: 24px;
            font-weight: bold;
            text-align: center;
            z-index: 1000;
            border: 3px solid #ffffff;
            box-shadow: 0 0 30px rgba(255, 0, 102, 0.5);
            max-width: 600px;
            animation: specialPulse 2s infinite;
        `;
        
        const characterName = document.createElement('div');
        characterName.style.cssText = `
            font-size: 20px;
            color: #ffff00;
            margin-bottom: 15px;
        `;
        characterName.textContent = 'Gustave 智宇';
        
        const dialogueText = document.createElement('div');
        dialogueText.style.cssText = `
            margin-bottom: 20px;
            line-height: 1.4;
        `;
        dialogueText.textContent = text;
        
        const closeButton = document.createElement('div');
        closeButton.style.cssText = `
            background: rgba(255, 255, 255, 0.2);
            border: 2px solid white;
            border-radius: 25px;
            padding: 10px 20px;
            cursor: pointer;
            display: inline-block;
            font-size: 16px;
            transition: all 0.3s ease;
        `;
        closeButton.textContent = '按下 X 關閉';
        closeButton.addEventListener('mouseenter', () => {
            closeButton.style.background = 'rgba(255, 255, 255, 0.4)';
        });
        closeButton.addEventListener('mouseleave', () => {
            closeButton.style.background = 'rgba(255, 255, 255, 0.2)';
        });
        
        dialogueBox.appendChild(characterName);
        dialogueBox.appendChild(dialogueText);
        dialogueBox.appendChild(closeButton);
        
        // 添加特殊動畫 CSS
        if (!document.getElementById('special-dialogue-style')) {
            const style = document.createElement('style');
            style.id = 'special-dialogue-style';
            style.textContent = `
                @keyframes specialPulse {
                    0% { box-shadow: 0 0 30px rgba(255, 0, 102, 0.5); }
                    50% { box-shadow: 0 0 50px rgba(255, 102, 0, 0.8); }
                    100% { box-shadow: 0 0 30px rgba(255, 0, 102, 0.5); }
                }
            `;
            document.head.appendChild(style);
        }
        
        document.body.appendChild(dialogueBox);
        
        // 監聽 X 鍵關閉
        const closeHandler = (e) => {
            if (e.key.toLowerCase() === 'x') {
                document.body.removeChild(dialogueBox);
                document.removeEventListener('keydown', closeHandler);
            }
        };
        document.addEventListener('keydown', closeHandler);
        
        // 點擊關閉按鈕也能關閉
        closeButton.addEventListener('click', () => {
            document.body.removeChild(dialogueBox);
            document.removeEventListener('keydown', closeHandler);
        });
    }
    
    checkInteractions(playerPosition) {
        const interactionDistance = 2.5;
        let nearNPC = false;
        let closestNPC = null;
        let minDistance = Infinity;
        
        // 檢查 NPC 互動
        for (const npc of this.npcs) {
            const distance = playerPosition.distanceTo(npc.position);
            
            if (distance < interactionDistance) {
                nearNPC = true;
                if (distance < minDistance) {
                    minDistance = distance;
                    closestNPC = npc;
                }
            }
        }
        
        // 檢查其他互動物件（如音箱）
        let nearInteractable = false;
        let closestInteractable = null;
        
        for (const obj of this.interactableObjects) {
            const distance = playerPosition.distanceTo(obj.position);
            
            if (distance < obj.radius) {
                nearInteractable = true;
                if (!closestNPC || distance < minDistance) {
                    closestInteractable = obj;
                    minDistance = distance; // 更新最小距離
                }
            }
        }
        
        // 更新互動提示 - 修改這裡
        const hint = document.getElementById('interaction-hint');
        if (hint) {
            // 優先顯示最近的互動對象
            if (closestInteractable && (!closestNPC || minDistance === playerPosition.distanceTo(closestInteractable.position))) {
                hint.classList.add('visible'); // 改用 visible 而不是 active
                if (closestInteractable.type === 'music') {
                    const action = this.musicPlaying ? '關閉音樂' : '播放音樂';
                    hint.textContent = `按 E ${action}`;
                } else {
                    hint.textContent = `按 E 與 ${closestInteractable.name} 互動`;
                }
            } else if (closestNPC) {
                hint.classList.add('visible');
                hint.textContent = `按 E 與 ${closestNPC.userData.name} 交談`;
            } else {
                hint.classList.remove('visible');
            }
        }
        
        return { npc: closestNPC, interactable: closestInteractable };
    }
    
    interact(target) {
        if (target.npc) {
            this.interactWithNPC(target.npc);
        } else if (target.interactable) {
            this.interactWithObject(target.interactable);
        }
    }
    
    interactWithNPC(npc) {
        if (!npc) return;
        
        const userData = npc.userData;
        
        // 特別處理 Gustave 的互動計數
        if (userData.name === 'Gustave') {
            this.gustaveInteractionCount++;

            // 1% 機率出現特殊對話
            if (Math.random() < 0.01) {
                this.createSpecialDialogue('其實我挺喜歡黃正安的！');
                return;
            }

            if (this.gustaveInteractionCount == 100) {
                const hiddenName = "\u9EC3\u6B63\u5B89";
                this.createSpecialDialogue(`${hiddenName}？ 別再按了！！！`);
                return;
            }
            if (this.gustaveInteractionCount == 200) {
                const hiddenName = "\u9EC3\u6B63\u5B89";
                this.createSpecialDialogue(`${hiddenName}？ 雖然這是我留的小心思 但別再按了！！！ \u6211\u4ec0\u9ebc\u90fd\u4e0d\u6703\u8aaa\u7684`);
                return;
            }
            if (this.gustaveInteractionCount == 300) {
                this.createSpecialDialogue(`你到底是有多無聊啊？ \u4e0d\u8981\u5728\u9019\u88e1\u57f7\u8457\u4e86!`);
                return;
            }
        }
        
        const dialogueBox = document.getElementById('dialogue-box');
        const characterName = document.getElementById('character-name');
        const dialogueText = document.getElementById('dialogue-text');
        
        if (!dialogueBox || !characterName || !dialogueText) return;
        
        // 立即設置新的對話內容
        characterName.textContent = `${userData.name} - ${userData.role}`;
        dialogueText.textContent = userData.dialogues[userData.currentDialogue];
        
        // 強制顯示對話框（移除後立即添加，觸發重新動畫）
        dialogueBox.classList.remove('active');
        // 強制瀏覽器重繪，確保 remove 生效
        dialogueBox.offsetHeight;
        dialogueBox.classList.add('active');
        
        // 循環對話
        userData.currentDialogue = (userData.currentDialogue + 1) % userData.dialogues.length;
        
        // 創建這次對話專屬的計時器，不管理任何全局計時器
        setTimeout(() => {
            // 只有在對話框仍然顯示相同內容時才隱藏
            // 通過檢查內容來確保這是同一次對話
            const currentName = characterName.textContent;
            const currentText = dialogueText.textContent;
            
            if (currentName === `${userData.name} - ${userData.role}` && 
                currentText === userData.dialogues[(userData.currentDialogue - 1 + userData.dialogues.length) % userData.dialogues.length]) {
                dialogueBox.classList.remove('active');
            }
        }, 4000);
    }
    
    interactWithObject(obj) {
        if (obj.type === 'music') {
            obj.action();
        }
    }
    
    /**
     * 計算調酒的酒精濃度
     * @param {Object} drinkInfo - 飲品資訊
     * @returns {number} 酒精濃度（百分比）
     */
    calculateAlcoholContent(drinkInfo) {
        if (!drinkInfo || drinkInfo.volume === 0) return 0;

        // 酒類的酒精濃度資料庫
        const alcoholData = {
            vodka: 40, gin: 40, rum: 40, whiskey: 40, tequila: 40, brandy: 40,
            vermouth_dry: 18, vermouth_sweet: 16, campari: 25, triple_sec: 40,
            angostura_bitters: 44.7
        };

        let totalAlcohol = 0; // 總酒精量（ml）

        // 計算每種材料的酒精含量
        drinkInfo.ingredients.forEach(ing => {
            const alcoholContent = alcoholData[ing.type] || 0;
            if (alcoholContent) {
                // 酒精含量 = 材料量 * 酒精濃度
                totalAlcohol += ing.amount * (alcoholContent / 100);
            }
        });

        // 平均酒精濃度 = 總酒精量 / 總容量
        return (totalAlcohol / drinkInfo.volume) * 100;
    }

    /**
     * NPC 喝酒互動系統
     * @param {Object} npc - NPC 物件
     * @param {Object} drinkInfo - 飲品資訊（從 cocktailSystem.drink() 返回）
     * @returns {Object} { reaction: string, rating: number }
     */
    npcDrinkCocktail(npc, drinkInfo) {
        if (!npc || !drinkInfo) return null;

        const userData = npc.userData;
        const npcName = userData.name;

        // 計算評分（基於成分和酒精含量）
        let rating = 5; // 基礎分
        let reaction = '';

        // 根據成分調整評分
        const ingredientTypes = drinkInfo.ingredients.map(ing => ing.type);
        const volume = drinkInfo.volume;

        // 計算酒精濃度
        const alcoholContent = this.calculateAlcoholContent(drinkInfo);

        // 評分邏輯
        if (volume < 30) {
            rating -= 2;
            reaction = '這...也太少了吧？';
        } else if (volume > 200) {
            rating += 1;
            reaction = '哇！份量真足！';
        }

        // 成分多樣性
        const uniqueIngredients = new Set(ingredientTypes);
        if (uniqueIngredients.size >= 3) {
            rating += 2;
        }

        // 黃正安的特殊反應
        if (npcName === '正安') {
            // 檢查是否是馬丁尼
            const hasGin = ingredientTypes.includes('gin');
            const hasDryVermouth = ingredientTypes.includes('vermouth_dry');
            const isMartini = hasGin && hasDryVermouth && ingredientTypes.length <= 3; // 馬丁尼通常只有2-3種材料

            if (isMartini) {
                // 馬丁尼特殊反應
                reaction = '楊智宇是你嗎？這馬丁尼調得太經典了！';
                rating = Math.min(10, rating + 3);
            } else if (alcoholContent > 30) {
                // 酒精濃度高於30度時的反應
                const strongDrinkReactions = [
                    '好烈！這是曦樂調的嗎？',
                    '哇...感覺很難喝，曦樂也是這樣調的嗎？',
                    '這個酒精濃度...讓我想起曦樂的風格呢',
                    '這麼烈！曦樂是不是教過你這個配方？'
                ];
                reaction = strongDrinkReactions[Math.floor(Math.random() * strongDrinkReactions.length)];
                rating = Math.max(1, rating - 1); // 減少評分
            } else {
                // 一般反應（酒精濃度適中或較低）
                const normalReactions = [
                    '嗯...味道還不錯呢！',
                    '這個調得挺順口的',
                    '喝起來很舒服，我喜歡',
                    '這個配方很不錯！',
                    '哇，比曦樂調的好喝多了！',
                    '這杯調得很平衡呢'
                ];
                reaction = normalReactions[Math.floor(Math.random() * normalReactions.length)];
                rating = Math.min(10, rating + Math.floor(Math.random() * 2) + 1);
            }
        }
        // 其他幹部的隨機反應
        else {
            const positiveReactions = [
                '哇！這調得不錯！',
                '味道很棒呢！',
                '嗯...有專業的感覺',
                '這個配方很有創意！',
                '我喜歡這個味道',
                '調得真好！',
                '完美的平衡！',
                '真是令人驚艷的作品！'
            ];

            const neutralReactions = [
                '還不錯啦',
                '嗯...可以接受',
                '味道普通',
                '還有進步空間',
                '中規中矩'
            ];

            const negativeReactions = [
                '呃...這味道有點奇怪',
                '可能需要再調整一下',
                '嗯...不是我的菜',
                '這個組合有點怪',
                '下次可以試試別的配方'
            ];

            // 根據評分選擇反應
            if (rating >= 8) {
                reaction = positiveReactions[Math.floor(Math.random() * positiveReactions.length)];
                rating += Math.floor(Math.random() * 2);
            } else if (rating >= 5) {
                reaction = neutralReactions[Math.floor(Math.random() * neutralReactions.length)];
            } else {
                reaction = negativeReactions[Math.floor(Math.random() * negativeReactions.length)];
            }
        }

        // 特定NPC的特殊評論
        if (npcName === 'Gustave') {
            if (uniqueIngredients.size >= 4) {
                reaction += ' 分子調酒的精髓就是創新！';
                rating += 1;
            }
        } else if (npcName === 'Seaton') {
            if (ingredientTypes.includes('whiskey')) {
                reaction = '嘿！有威士忌，我喜歡！' + reaction;
                rating += 2;
            }
        } else if (npcName === '瑜柔(宅魚)') {
            if (rating >= 7) {
                // 如果評分不錯，特殊反應
                reaction = '黃曦樂是你嗎？這個調得太好了！從化學角度來看，這個配方很有趣。';
            } else {
                reaction += ' 從化學角度來看，這個配方很有趣。';
            }
        } else if (npcName === '恩若') {
            reaction += ' 顏色也很漂亮呢！';
        } else if (npcName === '旻偉') {
            reaction += ' 器材運用得不錯。';
        }

        // 限制評分範圍 1-10
        rating = Math.max(1, Math.min(10, rating));

        // 顯示反應對話
        this.showDrinkReaction(npc, reaction, rating, drinkInfo.name);

        return { reaction, rating };
    }

    /**
     * 顯示NPC喝酒後的反應
     * @param {Object} npc - NPC 物件
     * @param {string} reaction - 反應文字
     * @param {number} rating - 評分
     * @param {string} drinkName - 飲品名稱
     */
    showDrinkReaction(npc, reaction, rating, drinkName) {
        const userData = npc.userData;
        const dialogueBox = document.getElementById('dialogue-box');
        const characterName = document.getElementById('character-name');
        const dialogueText = document.getElementById('dialogue-text');

        if (!dialogueBox || !characterName || !dialogueText) return;

        // 設置對話內容
        characterName.textContent = `${userData.name} - ${userData.role}`;

        // 評分星星
        const stars = '⭐'.repeat(Math.round(rating / 2));

        dialogueText.innerHTML = `
            <div style="margin-bottom: 10px; color: #FFD700; font-size: 18px;">
                品嚐了「${drinkName}」
            </div>
            <div style="margin-bottom: 8px;">
                ${reaction}
            </div>
            <div style="margin-top: 12px; font-size: 20px;">
                評分: ${stars} (${rating}/10)
            </div>
        `;

        // 顯示對話框
        dialogueBox.classList.remove('active');
        dialogueBox.offsetHeight; // 強制重繪
        dialogueBox.classList.add('active');

        // 5秒後隱藏
        setTimeout(() => {
            dialogueBox.classList.remove('active');
        }, 5000);
    }

    /**
     * 檢查是否可以給NPC喝酒
     * @param {THREE.Vector3} playerPos - 玩家位置
     * @returns {Object|null} 最近的NPC
     */
    getNearbyNPC(playerPos) {
        const interactionDistance = 2.5; // 互動距離

        for (const npc of this.npcs) {
            const distance = playerPos.distanceTo(npc.position);
            if (distance < interactionDistance) {
                return npc;
            }
        }

        return null;
    }

    update(deltaTime) {
        // 讓 NPC 有簡單的動畫
        this.npcs.forEach((npc, index) => {
        // 輕微的上下浮動
        const floatY = Math.sin(Date.now() * 0.001 + index) * 0.02;
        npc.position.y = npc.userData.originalY + floatY;

        // 輕微的左右搖擺（保留原始旋轉角度）
        const baseRotation = npc.userData.baseRotation || 0; // 獲取基礎旋轉角度
        const swayAmount = Math.sin(Date.now() * 0.0008 + index * 2) * 0.05;
        npc.rotation.y = baseRotation + swayAmount;
        // 讓名字標籤始終面向相機（Sprite 會自動面向相機）
        });
    }
}