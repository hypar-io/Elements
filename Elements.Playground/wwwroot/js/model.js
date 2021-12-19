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

class Node {
    constructor(prefix) {
        this.id = `${prefix}_${uuidv4()}`.replace(/-/g, '_');
    }

    getData() {
        return {};
    }
}

class Vector3Node extends Node {

    #xSlider;
    #ySlider;
    #zSlider;
    #xId = `x_${uuidv4()}`.replace(/-/g, '_');
    #yId = `y_${uuidv4()}`.replace(/-/g, '_');
    #zId = `z_${uuidv4()}`.replace(/-/g, '_');
    constructor(posx, posy, onConnect, onChange) {
        super('vector3');
        const vectorNode = new Flow.Node();
        vectorNode.setWidth(300)
        vectorNode.setPosition(posx, posy);
        const vectorOutput = new Flow.TitleElement('Vector3').setStyle('gray').setOutput(1);
        vectorNode.add(vectorOutput);
        this.#xSlider = new Flow.SliderInput(5);
        this.#xSlider.onChange(() => {
            onChange();
        });
        this.#ySlider = new Flow.SliderInput(5);
        this.#ySlider.onChange(() => {
            onChange();
        });
        this.#zSlider = new Flow.SliderInput(5);
        this.#zSlider.onChange(() => {
            onChange();
        });
        const x = new Flow.LabelElement('x').add(this.#xSlider);
        const y = new Flow.LabelElement('y').add(this.#ySlider);
        const z = new Flow.LabelElement('z').add(this.#zSlider);
        vectorNode.add(x);
        vectorNode.add(y);
        vectorNode.add(z);
        this.node = vectorNode;
        this.node.wrapper = this;
    }

    compile() {
        return `var ${this.id} = new Vector3(Inputs["${this.#xId}"],Inputs["${this.#yId}"],Inputs["${this.#zId}"]);\n`;
    }

    getData() {
        var data = {};
        data[this.#xId] = this.#xSlider.getValue();
        data[this.#yId] = this.#ySlider.getValue();
        data[this.#zId] = this.#zSlider.getValue();
        return data;
    }
}

class LineNode extends Node {
    constructor(posx, posy, onConnect) {
        super('line');
        const line = new Flow.Node();
        line.setWidth(200)
        line.setPosition(posx, posy);
        line.add(new Flow.TitleElement('Line').setStyle('gray').setOutput(1));
        const start = new Flow.LabelElement('start');
        start.onConnect(() => {
            onConnect();
        });
        const end = new Flow.LabelElement('end');
        end.onConnect(() => {
            onConnect();
        });
        line.add(start);
        line.add(end);

        start.setInput(1);
        end.setInput(1);

        this.node = line;
        this.start = start;
        this.end = end;
        this.node.wrapper = this;
    }

    compile() {
        var startId = this.start.linkedElement ? this.start.linkedElement.node.wrapper.id : 'null';
        var endId = this.end.linkedElement ? this.end.linkedElement.node.wrapper.id : null;
        var code = `var ${this.id} = new Line(${startId}, ${endId});\n`;
        code += `model.AddElement(new ModelCurve(${this.id}));\n`;
        return code;
    }
}

function compileGraph(graphNodes) {
    let code = 'var model = new Model();\n';
    graphNodes.forEach((node) => {
        code += node.compile() + '\n';
    });
    code += 'return model;';
    return code;
}

function getData(graphNodes) {
    let data = {};
    graphNodes.forEach((node) => {
        for (const [key, value] of Object.entries(node.getData())) {
            data[key] = value;
        };
    });

    return data;
}

function initializeGraph() {

    const canvas = new Flow.Canvas();

    const vector1 = new Vector3Node(canvas.relativeX + 10, canvas.relativeY, () => {
        DotNet.invokeMethod('Elements.Playground', 'SetCodeValue', compileGraph(graphNodes));
        DotNet.invokeMethod('Elements.Playground', 'Compile');
    }, () => {
        DotNet.invokeMethod('Elements.Playground', 'SetCodeContext', getData(graphNodes));
        DotNet.invokeMethodAsync('Elements.Playground', 'Run');
    });
    const vector2 = new Vector3Node(canvas.relativeX + 10, canvas.relativeY + 100, () => {
        DotNet.invokeMethod('Elements.Playground', 'SetCodeValue', compileGraph(graphNodes));
        DotNet.invokeMethod('Elements.Playground', 'Compile');
    }, () => {
        DotNet.invokeMethod('Elements.Playground', 'SetCodeContext', getData(graphNodes));
        DotNet.invokeMethodAsync('Elements.Playground', 'Run');
    });
    const line = new LineNode(canvas.relativeX + 10, canvas.relativeY + 200, () => {
        DotNet.invokeMethod('Elements.Playground', 'SetCodeValue', compileGraph(graphNodes));
        DotNet.invokeMethod('Elements.Playground', 'Compile');
    }, () => {
        DotNet.invokeMethod('Elements.Playground', 'SetCodeContext', getData(graphNodes));
        DotNet.invokeMethodAsync('Elements.Playground', 'Run');
    });

    canvas.add(vector1.node);
    canvas.add(vector2.node);
    canvas.add(line.node);

    const graphNodes = [vector1, vector2, line];

    const graphDiv = document.getElementById('graph');
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