// Game state
const gameState = {
    players: new Map(),
    stormRadius: 1000,
    finalStormRadius: 50,
    stormShrinkInterval: 60,
    nextStormShrink: 0,
    gameStarted: false,
    gameEnded: false,
    walls: []
};

// Three.js setup
const scene = new THREE.Scene();
const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 2000);
const renderer = new THREE.WebGLRenderer({ canvas: document.getElementById('gameCanvas'), antialias: true });
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.shadowMap.enabled = true;

// Lighting
const ambientLight = new THREE.AmbientLight(0xffffff, 0.5);
scene.add(ambientLight);

const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
directionalLight.position.set(100, 100, 50);
directionalLight.castShadow = true;
scene.add(directionalLight);

// Ground
const groundGeometry = new THREE.PlaneGeometry(2000, 2000);
const groundMaterial = new THREE.MeshStandardMaterial({ 
    color: 0x44aa44,
    roughness: 0.8,
    metalness: 0.2
});
const ground = new THREE.Mesh(groundGeometry, groundMaterial);
ground.rotation.x = -Math.PI / 2;
ground.receiveShadow = true;
scene.add(ground);

// Storm circle
const stormGeometry = new THREE.RingGeometry(gameState.stormRadius - 1, gameState.stormRadius, 64);
const stormMaterial = new THREE.MeshBasicMaterial({ 
    color: 0x4444ff,
    transparent: true,
    opacity: 0.3,
    side: THREE.DoubleSide
});
const stormCircle = new THREE.Mesh(stormGeometry, stormMaterial);
stormCircle.rotation.x = -Math.PI / 2;
scene.add(stormCircle);

// Wall prefab
function createWall(position, rotation) {
    const wallGeometry = new THREE.BoxGeometry(4, 3, 0.3);
    const wallMaterial = new THREE.MeshStandardMaterial({ color: 0x888888 });
    const wall = new THREE.Mesh(wallGeometry, wallMaterial);
    wall.position.copy(position);
    wall.rotation.y = rotation;
    wall.castShadow = true;
    wall.receiveShadow = true;
    scene.add(wall);
    gameState.walls.push(wall);
}

// Otter model
function createOtterModel() {
    const group = new THREE.Group();
    // Body
    const bodyGeo = new THREE.CapsuleGeometry(0.5, 1.2, 8, 16);
    const bodyMat = new THREE.MeshStandardMaterial({ color: 0x8B4513 });
    const body = new THREE.Mesh(bodyGeo, bodyMat);
    body.position.y = 1.1;
    group.add(body);
    // Head
    const headGeo = new THREE.SphereGeometry(0.45, 16, 16);
    const headMat = new THREE.MeshStandardMaterial({ color: 0x8B4513 });
    const head = new THREE.Mesh(headGeo, headMat);
    head.position.y = 2.1;
    group.add(head);
    // Nose
    const noseGeo = new THREE.SphereGeometry(0.13, 8, 8);
    const noseMat = new THREE.MeshStandardMaterial({ color: 0x222222 });
    const nose = new THREE.Mesh(noseGeo, noseMat);
    nose.position.set(0, 2.1, 0.42);
    group.add(nose);
    // Left ear
    const earGeo = new THREE.SphereGeometry(0.13, 8, 8);
    const earMat = new THREE.MeshStandardMaterial({ color: 0x5A2D0C });
    const leftEar = new THREE.Mesh(earGeo, earMat);
    leftEar.position.set(-0.25, 2.45, 0.1);
    group.add(leftEar);
    // Right ear
    const rightEar = leftEar.clone();
    rightEar.position.x = 0.25;
    group.add(rightEar);
    // Tail
    const tailGeo = new THREE.CylinderGeometry(0.12, 0.22, 1, 8);
    const tailMat = new THREE.MeshStandardMaterial({ color: 0x5A2D0C });
    const tail = new THREE.Mesh(tailGeo, tailMat);
    tail.position.set(0, 0.5, -0.6);
    tail.rotation.x = Math.PI / 3;
    group.add(tail);
    // Set shadow
    group.traverse(obj => { if (obj.isMesh) { obj.castShadow = true; obj.receiveShadow = true; } });
    return group;
}

// Player class
class Player {
    constructor(id, isLocal = false) {
        this.id = id;
        this.isLocal = isLocal;
        this.health = 100;
        this.shield = 100;
        this.position = new THREE.Vector3(
            (Math.random() - 0.5) * gameState.stormRadius * 0.8,
            1,
            (Math.random() - 0.5) * gameState.stormRadius * 0.8
        );
        this.direction = 0; // Yaw in radians
        this.model = createOtterModel();
        this.model.position.copy(this.position);
        scene.add(this.model);
        if (isLocal) {
            camera.position.set(this.position.x, this.position.y + 2, this.position.z + 5);
            camera.lookAt(this.position);
        }
        this.alive = true;
    }
    update(deltaTime) {
        if (!this.alive) return;
        if (this.isLocal) {
            // Handle local player movement
            const speed = 5;
            let moved = false;
            if (keys.w) { this.position.z -= speed * deltaTime * Math.cos(this.direction); this.position.x -= speed * deltaTime * Math.sin(this.direction); moved = true; }
            if (keys.s) { this.position.z += speed * deltaTime * Math.cos(this.direction); this.position.x += speed * deltaTime * Math.sin(this.direction); moved = true; }
            if (keys.a) { this.direction += 2 * deltaTime; }
            if (keys.d) { this.direction -= 2 * deltaTime; }
            // Update camera position
            camera.position.set(this.position.x - 5 * Math.sin(this.direction), this.position.y + 2, this.position.z - 5 * Math.cos(this.direction));
            camera.lookAt(this.position);
        }
        // Update model position and rotation
        this.model.position.copy(this.position);
        this.model.rotation.y = this.direction;
        // Check if player is outside storm
        const distanceToCenter = this.position.length();
        if (distanceToCenter > gameState.stormRadius) {
            this.takeDamage(1 * deltaTime);
        }
        // Check death
        if (this.health <= 0 && this.alive) {
            this.alive = false;
            scene.remove(this.model);
            if (this.isLocal) {
                document.querySelector('#gameState').textContent = 'You have been eliminated!';
            }
        }
    }
    takeDamage(amount) {
        if (!this.alive) return;
        if (this.shield > 0) {
            const remainingDamage = Math.max(0, amount - this.shield);
            this.shield = Math.max(0, this.shield - amount);
            amount = remainingDamage;
        }
        if (amount > 0) {
            this.health = Math.max(0, this.health - amount);
            if (this.isLocal) {
                updateUI();
            }
        }
    }
}

// Input handling
const keys = {
    w: false,
    a: false,
    s: false,
    d: false
};

window.addEventListener('keydown', (e) => {
    switch(e.key.toLowerCase()) {
        case 'w': keys.w = true; break;
        case 'a': keys.a = true; break;
        case 's': keys.s = true; break;
        case 'd': keys.d = true; break;
    }
});
window.addEventListener('keyup', (e) => {
    switch(e.key.toLowerCase()) {
        case 'w': keys.w = false; break;
        case 'a': keys.a = false; break;
        case 's': keys.s = false; break;
        case 'd': keys.d = false; break;
    }
});

// Mouse controls for shooting and building
window.addEventListener('mousedown', (e) => {
    const localPlayer = Array.from(gameState.players.values()).find(p => p.isLocal && p.alive);
    if (!localPlayer) return;
    if (e.button === 0) {
        // Left click: shoot
        shoot(localPlayer);
    } else if (e.button === 2) {
        // Right click: build wall
        buildWall(localPlayer);
    }
});
window.addEventListener('contextmenu', e => e.preventDefault());

function shoot(player) {
    // Raycast from player forward
    const origin = player.position.clone();
    const dir = new THREE.Vector3(-Math.sin(player.direction), 0, -Math.cos(player.direction));
    const raycaster = new THREE.Raycaster(origin, dir, 0, 30);
    // Check for hit on AI players
    for (const p of gameState.players.values()) {
        if (!p.isLocal && p.alive) {
            const box = new THREE.Box3().setFromObject(p.model);
            if (raycaster.ray.intersectsBox(box)) {
                p.takeDamage(50);
                if (!p.alive) {
                    scene.remove(p.model);
                    gameState.players.delete(p.id);
                }
                break;
            }
        }
    }
}

function buildWall(player) {
    // Place wall 3 units in front of player
    const pos = player.position.clone().add(new THREE.Vector3(-Math.sin(player.direction), 0, -Math.cos(player.direction)).multiplyScalar(3));
    pos.y = 1.5;
    createWall(pos, player.direction);
}

// UI updates
function updateUI() {
    const localPlayer = Array.from(gameState.players.values()).find(p => p.isLocal);
    if (localPlayer) {
        document.querySelector('#healthBar div').style.width = `${localPlayer.health}%`;
        document.querySelector('#shieldBar div').style.width = `${localPlayer.shield}%`;
    }
    document.querySelector('#playerCount').textContent = 
        `Players Alive: ${Array.from(gameState.players.values()).filter(p => p.alive).length}`;
    const timeUntilShrink = Math.max(0, gameState.nextStormShrink - Date.now() / 1000);
    document.querySelector('#stormTimer').textContent = 
        `Next Storm Shrink: ${Math.ceil(timeUntilShrink)}s`;
}

// Game loop
let lastTime = 0;
function gameLoop(time) {
    const deltaTime = (time - lastTime) / 1000;
    lastTime = time;
    // Update game state
    if (gameState.gameStarted && !gameState.gameEnded) {
        // Update storm
        if (Date.now() / 1000 >= gameState.nextStormShrink) {
            const shrinkAmount = (gameState.stormRadius - gameState.finalStormRadius) / 
                (gameState.stormShrinkInterval * (gameState.players.size / 10));
            gameState.stormRadius = Math.max(gameState.finalStormRadius, 
                gameState.stormRadius - shrinkAmount);
            gameState.nextStormShrink = Date.now() / 1000 + gameState.stormShrinkInterval;
            // Update storm circle
            scene.remove(stormCircle);
            const newStormGeometry = new THREE.RingGeometry(
                gameState.stormRadius - 1, 
                gameState.stormRadius, 
                64
            );
            stormCircle.geometry = newStormGeometry;
            scene.add(stormCircle);
        }
        // Update players
        for (const player of gameState.players.values()) {
            player.update(deltaTime);
        }
        // Check for game end
        const aliveCount = Array.from(gameState.players.values()).filter(p => p.alive).length;
        if (aliveCount <= 1) {
            gameState.gameEnded = true;
            document.querySelector('#gameState').textContent = 
                aliveCount === 1 ? 'Victory Royale!' : 'Game Over - Draw!';
        }
    }
    // Render scene
    renderer.render(scene, camera);
    requestAnimationFrame(gameLoop);
}

// Start game
function startGame() {
    // Create local player
    const localPlayer = new Player('local', true);
    gameState.players.set('local', localPlayer);
    // Create some AI players
    for (let i = 0; i < 99; i++) {
        const aiPlayer = new Player(`ai_${i}`);
        gameState.players.set(`ai_${i}`, aiPlayer);
    }
    gameState.gameStarted = true;
    gameState.nextStormShrink = Date.now() / 1000 + gameState.stormShrinkInterval;
    document.querySelector('#gameState').textContent = 'Game Started!';
    // Start game loop
    requestAnimationFrame(gameLoop);
}

// Handle window resize
window.addEventListener('resize', () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
});

// Start the game after a short delay
setTimeout(startGame, 3000); 