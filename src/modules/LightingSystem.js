import * as THREE from 'three';

export class LightingSystem {
    constructor(scene) {
        this.scene = scene;
        this.lights = [];
        this.dynamicLights = [];
        this.setupLighting();
    }
    
    setupLighting() {
        // 提升環境光亮度 - 讓整體更明亮溫馨
        const ambientLight = new THREE.AmbientLight(0x4a3c28, 5);
        this.scene.add(ambientLight);
        this.lights.push(ambientLight);
        
        // 主要暖色調照明（模擬溫暖的酒吧氛圍）
        const mainLight = new THREE.PointLight(0xffb366, 1.5, 20);
        mainLight.position.set(0, 7, 0);
        mainLight.castShadow = true;
        mainLight.shadow.mapSize.width = 2048;
        mainLight.shadow.mapSize.height = 2048;
        mainLight.shadow.camera.near = 0.1;
        mainLight.shadow.camera.far = 20;
        mainLight.shadow.bias = -0.0001;
        this.scene.add(mainLight);
        this.lights.push(mainLight);
        
        // 吧檯工作區聚光燈（更明亮的工作照明）
        const barSpotLight = new THREE.SpotLight(0xfff5dc, 1.2, 15);
        barSpotLight.position.set(0, 8, -3);
        barSpotLight.target.position.set(0, 1, -3);
        barSpotLight.castShadow = true;
        barSpotLight.angle = Math.PI / 4;
        barSpotLight.penumbra = 0.4;
        barSpotLight.decay = 2;
        barSpotLight.shadow.mapSize.width = 1024;
        barSpotLight.shadow.mapSize.height = 1024;
        this.scene.add(barSpotLight);
        this.scene.add(barSpotLight.target);
        this.lights.push(barSpotLight);
        
        // 酒架展示燈光（冷色調突出酒瓶）
        const shelfSpotLight1 = new THREE.SpotLight(0xe6f3ff, 0.8, 12);
        shelfSpotLight1.position.set(-3, 6, -6);
        shelfSpotLight1.target.position.set(-3, 2, -8);
        shelfSpotLight1.angle = Math.PI / 6;
        shelfSpotLight1.penumbra = 0.3;
        this.scene.add(shelfSpotLight1);
        this.scene.add(shelfSpotLight1.target);
        this.lights.push(shelfSpotLight1);
        
        const shelfSpotLight2 = new THREE.SpotLight(0xe6f3ff, 0.8, 12);
        shelfSpotLight2.position.set(3, 6, -6);
        shelfSpotLight2.target.position.set(3, 2, -8);
        shelfSpotLight2.angle = Math.PI / 6;
        shelfSpotLight2.penumbra = 0.3;
        this.scene.add(shelfSpotLight2);
        this.scene.add(shelfSpotLight2.target);
        this.lights.push(shelfSpotLight2);

        // 客座區域溫馨照明
        for (let i = 0; i < 4; i++) {
            const seatLight = new THREE.PointLight(0xffd4a3, 0.6, 8);
            seatLight.position.set(-3 + i * 2, 4, 2);
            this.scene.add(seatLight);
            this.lights.push(seatLight);
        }
        
        // 懸吊燈具照明
        this.createPendantLights();
        
        // 裝飾性氛圍燈光
        this.createAtmosphereLights();
        
        // 動態霓虹燈效果
        this.createNeonLights();
        
        // LED燈帶效果
        this.createLEDStrips();
    }
    
    createPendantLights() {
        // 三盞工業風懸吊燈的照明
        for (let i = 0; i < 3; i++) {
            const pendantLight = new THREE.PointLight(0xfff8dc, 0.9, 6);
            pendantLight.position.set(-4 + i * 4, 6.7, -1);
            pendantLight.castShadow = true;
            pendantLight.shadow.mapSize.width = 512;
            pendantLight.shadow.mapSize.height = 512;
            this.scene.add(pendantLight);
            this.lights.push(pendantLight);
        }
    }
    
    createAtmosphereLights() {
        // 角落氛圍燈
        const cornerLight1 = new THREE.PointLight(0x8b4513, 0.4, 10);
        cornerLight1.position.set(-8, 3, -8);
        this.scene.add(cornerLight1);
        this.lights.push(cornerLight1);
        
        const cornerLight2 = new THREE.PointLight(0x8b4513, 0.4, 10);
        cornerLight2.position.set(8, 3, -8);
        this.scene.add(cornerLight2);
        this.lights.push(cornerLight2);
        
        // 牆壁洗牆燈
        for (let i = -2; i <= 2; i++) {
            const wallWasher = new THREE.SpotLight(0x654321, 0.3, 8);
            wallWasher.position.set(i * 3, 7, -7);
            wallWasher.target.position.set(i * 3, 3, -9.5);
            wallWasher.angle = Math.PI / 3;
            wallWasher.penumbra = 0.5;
            this.scene.add(wallWasher);
            this.scene.add(wallWasher.target);
            this.lights.push(wallWasher);
        }
        
        // 地面裝飾燈光
        const floorAccent1 = new THREE.PointLight(0x2c1810, 0.2, 4);
        floorAccent1.position.set(-7, 0.5, -2);
        this.scene.add(floorAccent1);
        this.lights.push(floorAccent1);
        
        const floorAccent2 = new THREE.PointLight(0x2c1810, 0.2, 4);
        floorAccent2.position.set(7, 0.5, -2);
        this.scene.add(floorAccent2);
        this.lights.push(floorAccent2);
    }
    
    createNeonLights() {
        // 主霓虹招牌光源
        const neonMainLight = new THREE.PointLight(0xff1493, 0.7, 8);
        neonMainLight.position.set(0, 6, -9);
        this.scene.add(neonMainLight);
        this.dynamicLights.push({
            light: neonMainLight,
            type: 'neon',
            baseIntensity: 0.7,
            phase: 0,
            speed: 3
        });
        
        // 彩色霓虹燈環繞效果
        const neonColors = [
            { color: 0x00ffff, pos: [-6, 4, 6] },
            { color: 0xff00ff, pos: [6, 4, 6] },
            { color: 0xffff00, pos: [-6, 4, -6] },
            { color: 0x00ff00, pos: [6, 4, -6] }
        ];
        
        neonColors.forEach((neon, index) => {
            const neonLight = new THREE.PointLight(neon.color, 0.4, 12);
            neonLight.position.set(...neon.pos);
            this.scene.add(neonLight);
            this.dynamicLights.push({
                light: neonLight,
                type: 'colorNeon',
                baseIntensity: 0.4,
                phase: index * Math.PI / 2,
                speed: 2
            });
        });
        
        // 閃爍霓虹燈
        const flickerNeon = new THREE.PointLight(0x9932cc, 0.5, 6);
        flickerNeon.position.set(-2, 5, -9);
        this.scene.add(flickerNeon);
        this.dynamicLights.push({
            light: flickerNeon,
            type: 'flicker',
            baseIntensity: 0.5,
            phase: 0,
            speed: 8
        });
    }
    
    createLEDStrips() {
        // 吧檯下方LED燈帶
        const underBarLED = new THREE.RectAreaLight(0xffffff, 2, 12, 0.1);
        underBarLED.position.set(0, 0.3, -3);
        underBarLED.rotation.x = Math.PI / 2;
        this.scene.add(underBarLED);
        this.lights.push(underBarLED);
        
        // 酒架LED燈帶
        for (let i = 0; i < 3; i++) {
            const shelfLED = new THREE.RectAreaLight(0xe6f3ff, 1, 10, 0.05);
            shelfLED.position.set(0, 1.4 + i * 1.2, -7.5);
            shelfLED.rotation.x = -Math.PI / 6;
            this.scene.add(shelfLED);
            this.lights.push(shelfLED);
        }
        
        // 天花板輪廓LED
        const ceilingLED1 = new THREE.RectAreaLight(0x4a3c28, 0.8, 20, 0.1);
        ceilingLED1.position.set(0, 9.5, 0);
        ceilingLED1.rotation.x = Math.PI / 2;
        this.scene.add(ceilingLED1);
        this.lights.push(ceilingLED1);
        
        // 牆角LED燈帶
        const wallLED1 = new THREE.RectAreaLight(0x654321, 0.6, 0.1, 10);
        wallLED1.position.set(-9.9, 5, 0);
        wallLED1.rotation.y = Math.PI / 2;
        this.scene.add(wallLED1);
        this.lights.push(wallLED1);
        
        const wallLED2 = new THREE.RectAreaLight(0x654321, 0.6, 0.1, 10);
        wallLED2.position.set(9.9, 5, 0);
        wallLED2.rotation.y = -Math.PI / 2;
        this.scene.add(wallLED2);
        this.lights.push(wallLED2);
    }
    
    update(time) {
        // 更新動態燈光效果
        this.dynamicLights.forEach(lightObj => {
            const { light, type, baseIntensity, phase, speed } = lightObj;
            
            switch (type) {
                case 'neon':
                    // 柔和的霓虹燈脈動
                    light.intensity = baseIntensity + Math.sin(time * speed + phase) * 0.2;
                    break;
                    
                case 'colorNeon':
                    // 彩色霓虹燈漸變
                    light.intensity = baseIntensity + Math.sin(time * speed + phase) * 0.15;
                    break;
                    
                case 'flicker':
                    // 隨機閃爍效果
                    if (Math.random() < 0.02) {
                        light.intensity = Math.random() * baseIntensity;
                    } else {
                        light.intensity = baseIntensity + Math.sin(time * speed) * 0.1;
                    }
                    break;
            }
        });
        
        // 主照明的微妙變化（模擬真實環境）
        if (this.lights[1]) { // 主燈光
            this.lights[1].intensity = 1.5 + Math.sin(time * 0.5) * 0.1;
        }
        
        // 懸吊燈的輕微搖擺效果
        for (let i = 0; i < 3; i++) {
            const pendantIndex = 4 + i; // 懸吊燈在燈光陣列中的索引
            if (this.lights[pendantIndex]) {
                const swayX = Math.sin(time * 1.2 + i) * 0.05;
                const swayZ = Math.cos(time * 0.8 + i) * 0.03;
                this.lights[pendantIndex].position.x = -4 + i * 4 + swayX;
                this.lights[pendantIndex].position.z = -1 + swayZ;
            }
        }
    }
    
    // 場景時間控制（模擬不同時段的燈光變化）
    setTimeOfDay(hour) {
        // hour: 0-23
        const isNight = hour < 6 || hour > 20;
        const intensity = isNight ? 1.2 : 0.8;
        
        // 調整環境光
        if (this.lights[0]) {
            this.lights[0].intensity = isNight ? 0.6 : 1.0;
        }
        
        // 調整主燈光
        if (this.lights[1]) {
            this.lights[1].intensity = intensity;
        }
        
        // 調整霓虹燈
        this.dynamicLights.forEach(lightObj => {
            if (lightObj.type.includes('neon')) {
                lightObj.baseIntensity = isNight ? 0.8 : 0.4;
            }
        });
    }
    
    // 營業狀態燈光控制
    setOperatingMode(mode) {
        // mode: 'opening', 'busy', 'closing', 'closed'
        switch(mode) {
            case 'opening':
                this.setOpeningLights();
                break;
            case 'busy':
                this.setBusyLights();
                break;
            case 'closing':
                this.setClosingLights();
                break;
            case 'closed':
                this.setClosedLights();
                break;
        }
    }
    
    setOpeningLights() {
        // 開店時溫和明亮的燈光
        if (this.lights[0]) this.lights[0].intensity = 0.9; // 環境光
        if (this.lights[1]) this.lights[1].intensity = 1.3; // 主燈光
        if (this.lights[2]) this.lights[2].intensity = 1.0; // 吧檯聚光燈
        
        this.dynamicLights.forEach(lightObj => {
            lightObj.baseIntensity *= 0.7; // 減少霓虹燈強度
        });
    }
    
    setBusyLights() {
        // 營業高峰期的活躍燈光
        if (this.lights[0]) this.lights[0].intensity = 0.7;
        if (this.lights[1]) this.lights[1].intensity = 1.6;
        if (this.lights[2]) this.lights[2].intensity = 1.3;
        
        this.dynamicLights.forEach(lightObj => {
            lightObj.baseIntensity *= 1.2; // 增強霓虹燈效果
            lightObj.speed *= 1.3; // 加快動態效果
        });
    }
    
    setClosingLights() {
        // 打烊前的溫馨燈光
        if (this.lights[0]) this.lights[0].intensity = 0.5;
        if (this.lights[1]) this.lights[1].intensity = 1.0;
        if (this.lights[2]) this.lights[2].intensity = 0.8;
        
        this.dynamicLights.forEach(lightObj => {
            lightObj.baseIntensity *= 0.6;
            lightObj.speed *= 0.7; // 放慢動態效果
        });
    }
    
    setClosedLights() {
        // 關店後的安全照明
        if (this.lights[0]) this.lights[0].intensity = 0.2;
        if (this.lights[1]) this.lights[1].intensity = 0.4;
        if (this.lights[2]) this.lights[2].intensity = 0.3;
        
        // 只保留基本的安全燈光
        this.dynamicLights.forEach(lightObj => {
            if (lightObj.type === 'flicker') {
                lightObj.light.intensity = 0; // 關閉閃爍燈
            } else {
                lightObj.baseIntensity *= 0.3;
            }
        });
    }
    
    // 特殊氛圍效果
    createSpecialEffects() {
        this.createCandelabra();
        this.createFireplace();
        this.createAquariumLighting();
        this.createMirrorBallEffect();
    }
    
    createCandelabra() {
        // 燭台裝飾燈光（模擬蠟燭效果）
        for (let i = 0; i < 3; i++) {
            const candleLight = new THREE.PointLight(0xffa500, 0.3, 3);
            candleLight.position.set(-4 + i * 4, 1.5, -7.5);
            this.scene.add(candleLight);
            this.dynamicLights.push({
                light: candleLight,
                type: 'candle',
                baseIntensity: 0.3,
                phase: Math.random() * Math.PI * 2,
                speed: 4 + Math.random() * 2
            });
        }
    }
    
    createFireplace() {
        // 壁爐效果燈光（溫暖的橘紅色）
        const fireplaceLight = new THREE.PointLight(0xff4500, 0.8, 8);
        fireplaceLight.position.set(-8, 2, -8);
        this.scene.add(fireplaceLight);
        this.dynamicLights.push({
            light: fireplaceLight,
            type: 'fireplace',
            baseIntensity: 0.8,
            phase: 0,
            speed: 6
        });
    }
    
    createAquariumLighting() {
        // 水族箱藍光效果
        const aquariumLight = new THREE.PointLight(0x00bfff, 0.4, 6);
        aquariumLight.position.set(8, 3, -8);
        this.scene.add(aquariumLight);
        this.dynamicLights.push({
            light: aquariumLight,
            type: 'aquarium',
            baseIntensity: 0.4,
            phase: 0,
            speed: 1.5
        });
    }
    
    createMirrorBallEffect() {
        // 鏡球反射效果
        const mirrorSpots = [];
        for (let i = 0; i < 8; i++) {
            const spotLight = new THREE.SpotLight(0xffffff, 0.2, 10);
            spotLight.angle = Math.PI / 12;
            spotLight.penumbra = 0.5;
            
            const angle = (i / 8) * Math.PI * 2;
            spotLight.position.set(
                Math.cos(angle) * 6,
                5,
                Math.sin(angle) * 6
            );
            spotLight.target.position.set(
                Math.cos(angle + Math.PI) * 3,
                0,
                Math.sin(angle + Math.PI) * 3
            );
            
            this.scene.add(spotLight);
            this.scene.add(spotLight.target);
            mirrorSpots.push({
                light: spotLight,
                target: spotLight.target,
                baseAngle: angle,
                radius: 6
            });
        }
        
        this.mirrorBallSpots = mirrorSpots;
    }
    
    // 更新函數中添加新的動態效果
    updateAdvanced(time) {
        // 基本動態燈光更新
        this.update(time);
        
        // 蠟燭效果
        this.dynamicLights.forEach(lightObj => {
            const { light, type, baseIntensity, phase, speed } = lightObj;
            
            if (type === 'candle') {
                // 蠟燭的不規則閃爍
                const flicker = Math.sin(time * speed + phase) * 0.1 + 
                              Math.sin(time * speed * 1.5 + phase) * 0.05 +
                              Math.random() * 0.03;
                light.intensity = Math.max(0.1, baseIntensity + flicker);
            }
            
            if (type === 'fireplace') {
                // 壁爐的火焰效果
                const flame = Math.sin(time * speed + phase) * 0.2 + 
                             Math.sin(time * speed * 2.3 + phase * 1.5) * 0.1 +
                             Math.sin(time * speed * 0.7 + phase * 2) * 0.15;
                light.intensity = Math.max(0.3, baseIntensity + flame);
                
                // 火焰位置輕微搖擺
                light.position.x = -8 + Math.sin(time * 3) * 0.1;
                light.position.y = 2 + Math.sin(time * 4) * 0.05;
            }
            
            if (type === 'aquarium') {
                // 水族箱的水波光效
                const wave = Math.sin(time * speed + phase) * 0.15 + 
                            Math.sin(time * speed * 1.7 + phase) * 0.1;
                light.intensity = baseIntensity + wave;
                
                // 光線顏色變化（模擬水波）
                const hue = 0.55 + Math.sin(time * 0.5) * 0.1; // 藍綠色變化
                light.color.setHSL(hue, 1, 0.7);
            }
        });
        
        // 鏡球效果更新
        if (this.mirrorBallSpots) {
            this.mirrorBallSpots.forEach((spot, index) => {
                const rotationSpeed = 0.5;
                const currentAngle = spot.baseAngle + time * rotationSpeed;
                
                // 鏡球反射點旋轉
                spot.light.position.x = Math.cos(currentAngle) * spot.radius;
                spot.light.position.z = Math.sin(currentAngle) * spot.radius;
                
                spot.target.position.x = Math.cos(currentAngle + Math.PI) * 3;
                spot.target.position.z = Math.sin(currentAngle + Math.PI) * 3;
                
                // 強度變化
                spot.light.intensity = 0.2 + Math.sin(time * 2 + index) * 0.1;
            });
        }
    }
    
    // 環境音響配合燈光
    syncWithAudio(audioData) {
        if (!audioData) return;
        
        const bassLevel = audioData.bass || 0;
        const midLevel = audioData.mid || 0;
        const highLevel = audioData.high || 0;
        
        // 根據音樂節拍調整燈光
        this.dynamicLights.forEach(lightObj => {
            if (lightObj.type.includes('neon')) {
                lightObj.light.intensity = lightObj.baseIntensity + bassLevel * 0.3;
            }
            
            if (lightObj.type === 'flicker') {
                lightObj.speed = 8 + midLevel * 5;
            }
        });
        
        // 主燈光隨音樂脈動
        if (this.lights[1]) {
            this.lights[1].intensity = 1.5 + (bassLevel + midLevel) * 0.2;
        }
        
        // 鏡球效果隨高頻變化
        if (this.mirrorBallSpots) {
            this.mirrorBallSpots.forEach(spot => {
                spot.light.intensity = 0.2 + highLevel * 0.4;
            });
        }
    }
    
    // 清理資源
    dispose() {
        this.lights.forEach(light => {
            if (light.dispose) light.dispose();
            this.scene.remove(light);
        });
        
        this.dynamicLights.forEach(lightObj => {
            if (lightObj.light.dispose) lightObj.light.dispose();
            this.scene.remove(lightObj.light);
        });
        
        if (this.mirrorBallSpots) {
            this.mirrorBallSpots.forEach(spot => {
                this.scene.remove(spot.light);
                this.scene.remove(spot.target);
                if (spot.light.dispose) spot.light.dispose();
            });
        }
        
        this.lights = [];
        this.dynamicLights = [];
        this.mirrorBallSpots = null;
    }
}