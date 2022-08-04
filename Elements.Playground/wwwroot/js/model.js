import * as THREE from 'https://cdn.skypack.dev/three@0.133.0/build/three.module.js'
import { OrbitControls } from 'https://cdn.skypack.dev/three@0.133.0/examples/jsm/controls/OrbitControls.js';
import { TransformControls } from 'https://cdn.skypack.dev/three@0.133.0/examples/jsm/controls/TransformControls.js';
import { GLTFLoader } from 'https://cdn.skypack.dev/three@0.133.0/examples/jsm/loaders/GLTFLoader.js';
import { RGBELoader } from 'https://cdn.skypack.dev/three@0.133.0/examples/jsm/loaders/RGBELoader.js'

window.model = {
    initialize3D: () => { initialize3D(); },
    initializeEditor: () => { initializeEditor(); },
    loadModel: (glb) => { loadModel(glb) },
    createTransformablePoint: (name) => { createTransformablePoint(name) }
};

const scene = new THREE.Scene();
var gltfScene = null;
var editor = null;
var directionalLight = null;
var renderer = null;
var camera = null;
var pointMaterial = new THREE.MeshBasicMaterial({ color: 0xff0000 });
var orbitControl = null;

function loadModel(glb) {
    const contentArray = Blazor.platform.toUint8Array(glb);
    const blob = new Blob([new Uint8Array(contentArray)], { type: "application/octet-stream" });
    const blobUrl = URL.createObjectURL(blob);

    const loader = new GLTFLoader();

    loader.load(
        blobUrl,
        function (gltf) {

            if (gltfScene != null) {
                scene.remove(gltfScene);
            }

            gltf.scene.traverse((o) => {
                if (o instanceof THREE.Mesh) {
                    o.castShadow = true;
                    o.receiveShadow = true;
                }
            })

            scene.add(gltf.scene);
            gltfScene = gltf.scene;

            URL.revokeObjectURL(blobUrl);

            return true;
        },
        function (xhr) {
            console.log((xhr.loaded / xhr.total * 100) + '% loaded');
        },
        function (error) {
            console.log('An error happened');
            console.log(error)
        }
    );
}

function initializeEditor() {
    ace.config.set("packaged", true)
    ace.config.set("basePath", "https://cdnjs.cloudflare.com/ajax/libs/ace/1.8.1/")
    ace.require("ace/ext/language_tools");
    editor = ace.edit("editor");
    editor.setTheme("ace/theme/tomorrow");
    editor.session.setMode("ace/mode/csharp");
    editor.setOptions({
        fontSize: "10pt",
        enableBasicAutocompletion: true,
        enableSnippets: true,
        enableLiveAutocompletion: true
    });

    const code = localStorage.getItem('code');
    if (code) {
        editor.setValue(code);
        DotNet.invokeMethod('Elements.Playground', 'SetCodeValue', code)
    }

    editor.getSession().on('change', function () {
        var code = editor.getValue();
        DotNet.invokeMethod('Elements.Playground', 'SetCodeValue', code)
        localStorage.setItem('code', code);
    });
}

function initialize3D() {
    const div = document.getElementById("model");
    camera = new THREE.PerspectiveCamera(75, div.clientWidth / div.clientHeight, 0.1, 300);

    // Renderer
    renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap; // default THREE.PCFShadowMap
    renderer.physicallyCorrectLights = true;
    renderer.outputEncoding = THREE.sRGBEncoding
    renderer.toneMappingExposure = 0.9
    renderer.setSize(div.clientWidth, div.clientHeight);
    div.appendChild(renderer.domElement);

    orbitControl = new OrbitControls(camera, renderer.domElement);

    // Main light
    directionalLight = new THREE.DirectionalLight(0xffffff, 0.8 * Math.PI);
    directionalLight.position.set(-2, 10, 0);
    directionalLight.castShadow = true;
    scene.add(directionalLight);
    var side = 30
    directionalLight.shadow.mapSize.width = 4096;
    directionalLight.shadow.mapSize.height = 4096;
    // directionalLight.shadow.camera.near = 1;
    // directionalLight.shadow.camera.far = 20;
    directionalLight.shadow.camera.left = -side
    directionalLight.shadow.camera.right = side
    directionalLight.shadow.camera.top = side
    directionalLight.shadow.camera.bottom = -side
    directionalLight.shadow.bias = -0.00001 // -r / 2000000.0

    // const size = 100;
    // const divisions = 20;
    // const gridHelper = new THREE.GridHelper(size, divisions, "darkgray", "lightgray");
    // scene.add(gridHelper);

    // const helper = new THREE.CameraHelper(directionalLight.shadow.camera);
    // scene.add(helper);

    // Environment
    new RGBELoader()
        .setPath('textures/equirectangular/')
        .load('royal_esplanade_1k.hdr', function (texture) {

            texture.mapping = THREE.EquirectangularReflectionMapping;
            // scene.background = texture;
            scene.environment = texture;
        });

    // Shadow plane
    var shadowMaterial = new THREE.ShadowMaterial();
    shadowMaterial.opacity = 0.5;
    const planeGeometry = new THREE.PlaneGeometry(50, 50, 32, 32);
    const plane = new THREE.Mesh(planeGeometry, shadowMaterial);
    plane.rotateX(-Math.PI / 2);
    plane.receiveShadow = true;
    scene.add(plane);

    // Axes
    const axesHelper = new THREE.AxesHelper(5);
    scene.add(axesHelper);

    camera.position.z = 5;
    camera.position.y = 10;

    orbitControl.update();

    const animate = function () {
        requestAnimationFrame(animate);
        orbitControl.update();
        renderer.render(scene, camera);
    };

    window.addEventListener('resize', () => {
        camera.aspect = div.clientWidth / div.clientHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(div.clientWidth, div.clientHeight);
    }, false);

    animate();
}

function createTransformablePoint(name) {
    const geometry = new THREE.SphereGeometry(0.2, 16, 16);
    const mesh = new THREE.Mesh(geometry, pointMaterial);
    mesh.name = `${name}_mesh`;

    scene.add(mesh);
    var transformControl = new TransformControls(camera, renderer.domElement);
    transformControl.addEventListener('mouseUp', () => {
        mesh.updateMatrixWorld();
        var position = new THREE.Vector3();
        position.setFromMatrixPosition(mesh.matrixWorld);
        DotNet.invokeMethodAsync('Elements.Playground', 'UpdatePointInput', name, position.x, position.z, position.y);
        render();
    });
    transformControl.addEventListener('dragging-changed', function (event) {
        orbitControl.enabled = !event.value;
    });
    transformControl.name = `${name}_control`;
    transformControl.attach(mesh);
    scene.add(transformControl);
}

function render() {
    renderer.render(scene, camera)
}