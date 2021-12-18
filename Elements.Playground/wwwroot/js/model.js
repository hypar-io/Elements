import * as THREE from 'https://cdn.skypack.dev/three@0.133.0/build/three.module.js'
import { OrbitControls } from 'https://cdn.skypack.dev/three@0.133.0/examples/jsm/controls/OrbitControls.js';
import { GLTFLoader } from 'https://cdn.skypack.dev/three@0.133.0/examples/jsm/loaders/GLTFLoader.js';
import * as Flow from './flow.module.js';

window.model = {
    initialize3D: () => { initialize3D(); },
    initializeEditor: () => { initializeEditor(); },
    loadModel: (glb) => { loadModel(glb) },
    initializeGraph: () => { initializeGraph(); },
};

const scene = new THREE.Scene();
var gltfScene = null;
var editor = null;

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

class Vector3Node {
    constructor(posx, posy) {
        const vectorNode = new Flow.Node();
        vectorNode.setWidth(300)
        vectorNode.setPosition(posx, posy);
        const vectorOutput = new Flow.TitleElement('Vector3').setStyle('gray').setOutput(1);
        vectorNode.add(vectorOutput);
        const x = new Flow.LabelElement('x').setStyle('red').add(new Flow.SliderInput(5));
        const y = new Flow.LabelElement('y').setStyle('green').add(new Flow.SliderInput(5));
        const z = new Flow.LabelElement('z').setStyle('blue').add(new Flow.SliderInput(5));
        vectorNode.add(x);
        vectorNode.add(y);
        vectorNode.add(z);
        this.node = vectorNode;
    }

    compile() {
        return 'var v = new Vector3();';
    }
}

class LineNode {
    constructor(posx, posy) {
        const line = new Flow.Node();
        line.setWidth(200)
        line.setPosition(posx, posy);
        line.add(new Flow.TitleElement('Line').setStyle('gray'));
        const start = new Flow.LabelElement('start');
        const end = new Flow.LabelElement('end');
        line.add(start);
        line.add(end);

        start.setInput(1);
        end.setInput(1);

        this.node = line;
        this.start = start;
        this.end = end;
    }

    compile() {
        return 'var l = new Line();';
    }
}

function compileGraph(graphNodes) {
    let code = '';
    graphNodes.forEach((node) => {
        code += node.compile() + '\n';
    });
    return code;
}

function initializeGraph() {

    const graphDiv = document.getElementById('graph');
    const w = graphDiv.clientWidth;
    const h = graphDiv.clientHeight;

    const canvas = new Flow.Canvas();

    const vector1 = new Vector3Node(canvas.relativeX + 10, canvas.relativeY);
    const vector2 = new Vector3Node(canvas.relativeX + 10, canvas.relativeY + 100);
    const line = new LineNode(canvas.relativeX + 10, canvas.relativeY + 200);

    canvas.add(vector1.node);
    canvas.add(vector2.node);
    canvas.add(line.node);

    const graphNodes = [vector1, vector2, line];

    line.start.onConnect(() => {

        //console.log( slider.link.inputElement === slider );
        // console.log( slider.link ? slider.link.outputElement === vector : null );
        console.log(compileGraph(graphNodes));
    });

    graphDiv.appendChild(canvas.dom);
}

function initializeEditor() {
    ace.config.set("packaged", true)
    ace.config.set("basePath", "https://pagecdn.io/lib/ace/1.4.12/")
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

    let timer;
    let runTimer = () => {
        timer = setTimeout(() => {
            invokeRun();
        }, 2000);
    }

    editor.getSession().on('change', function () {
        var code = editor.getValue();
        DotNet.invokeMethod('Elements.Playground', 'SetCodeValue', code)
        clearTimeout(timer);
        runTimer();
    });
}

function invokeRun() {
    DotNet.invokeMethodAsync('Elements.Playground', 'Run');
}

function initialize3D() {
    const div = document.getElementById("model");
    const camera = new THREE.PerspectiveCamera(75, div.clientWidth / div.clientHeight, 0.1, 1000);

    const renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });

    renderer.setSize(div.clientWidth, div.clientHeight);
    div.appendChild(renderer.domElement);

    const controls = new OrbitControls(camera, renderer.domElement);

    const directionalLight = new THREE.DirectionalLight(0xffffff, 1.0);
    directionalLight.position.set(0.5, 0.5, 0);
    scene.add(directionalLight);

    const size = 100;
    const divisions = 20;

    const gridHelper = new THREE.GridHelper(size, divisions, "darkgray", "lightgray");
    scene.add(gridHelper);

    const light = new THREE.HemisphereLight(0xffffbb, 0x080820, 1.0);
    scene.add(light);

    camera.position.z = 5;
    camera.position.y = 10;

    controls.update();

    const animate = function () {
        requestAnimationFrame(animate);
        controls.update();
        renderer.render(scene, camera);
    };

    window.addEventListener('resize', () => {
        camera.aspect = div.clientWidth / div.clientHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(div.clientWidth, div.clientHeight);
    }, false);

    animate();
}