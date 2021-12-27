import * as THREE from 'https://cdn.skypack.dev/three@0.133.0/build/three.module.js'
import { OrbitControls } from 'https://cdn.skypack.dev/three@0.133.0/examples/jsm/controls/OrbitControls.js';
import { GLTFLoader } from 'https://cdn.skypack.dev/three@0.133.0/examples/jsm/loaders/GLTFLoader.js';
import * as Flow from './flow.module.js';

window.model = {
    initialize3D: () => { initialize3D(); },
    initializeEditor: () => { initializeEditor(); },
    loadModel: (glb) => { loadModel(glb) },
    initializeGraph: () => { initializeGraph(); },
    addNode: (nodeData) => { addNode(nodeData); }
};

const scene = new THREE.Scene();
var gltfScene = null;
var editor = null;
var canvas = null;
var graphNodes = [];
var currentIndex = 0;

function getNextIndex() {
    currentIndex++;
    return currentIndex;
}

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
    constructor(title, posx, posy, width) {
        this.id = `${title}_${getNextIndex()}`;
        this.node = new Flow.Node();
        this.node.setPosition(posx, posy);
        this.node.setWidth(width);
        this.node.wrapper = this;

        const output = new Flow.TitleElement(title).setStyle('gray').setOutput(1);
        this.node.add(output);
    }

    compile() {
        return '';
    }

    getData() {
        return {};
    }
}

class NumberNode extends Node {

    #vId = `number_${getNextIndex()}`;
    #vSlider
    constructor(posx, posy, onConnect, onChange) {
        super('Number', posx, posy, 300);
        this.#vSlider = new Flow.SliderInput(5);
        this.#vSlider.onChange(() => {
            onChange();
        });

        const v = new Flow.LabelElement('value').add(this.#vSlider);
        this.node.add(v);
    }

    compile() {
        return `var ${this.id} = Inputs["${this.#vId}"];`;
    }

    getData() {
        var data = {};
        data[this.#vId] = this.#vSlider.getValue();
        return data;
    }
}

class NodeDataNode extends Node {
    #typeName
    #parameters = {}
    #nodeData
    constructor(nodeData, posx, posy, width, onConnect, onChange) {
        super(nodeData.isConstructor ? nodeData.typeName : `${nodeData.typeName}_${nodeData.methodName}`, posx, posy, width);
        this.#nodeData = nodeData
        this.#typeName = nodeData.typeName;

        if (!nodeData.isConstructor) {
            this.#parameters["this"] = new Flow.LabelElement(nodeData.typeName).setInput(1);
            this.#parameters["this"].onConnect(() => {
                onConnect();
            });
            this.node.add(this.#parameters["this"]);
        }

        if (nodeData.parameterData) {
            Object.entries(nodeData.parameterData).forEach(([key, value]) => {
                const parameterName = `${key}`; // `${pd.name}_${getNextIndex()}`;
                this.#parameters[parameterName] = new Flow.LabelElement(`${key}`).setInput(1);
                this.#parameters[parameterName].onConnect(() => {
                    onConnect();
                });
                this.node.add(this.#parameters[parameterName]);
            })
        }
    }

    compile() {
        const params = Object.entries(this.#parameters).reduce((filtered, [key, value]) => {
            if (key !== "this") {
                if (!value.linkedElement) {
                    if (this.#nodeData.parameterData[key].defaultValue) {
                        filtered.push(`${key}: ${this.#nodeData.parameterData[key].defaultValue}`);
                    } else {
                        filtered.push(`${key}: null`);
                    }
                } else {
                    filtered.push(`${key}: ${value.linkedElement ? value.linkedElement.node.wrapper.id : 'null'}`);
                }
            }
            return filtered;
        }, []);

        let code = '';
        if (this.#nodeData.isConstructor) {
            code += `var ${this.id} = new ${this.#typeName}(${params.join(',')});\n`
        } else {
            code += `var ${this.id} = ${this.#parameters["this"].linkedElement ? this.#parameters["this"].linkedElement.node.wrapper.id : 'null'}.${this.#nodeData.methodName}(${params.join(',')});\n`
        }

        // Draw curves
        const curveTypes = ['Line', 'Arc', 'Bezier', 'Circle', 'Polygon', 'Polyline'];
        if (curveTypes.includes(this.#nodeData.returnType)) {
            code += `model.AddElement(new ModelCurve(${this.id}));`;
        }

        // Draw transforms
        if (this.#nodeData.returnType === 'Transform') {
            code += `model.AddElements(${this.id}.ToModelCurves());`;
        }

        return code;
    }
}

class Grid2dNode extends Node {
    #uSlider;
    #vSlider;
    #uId = `u${getNextIndex()} `;
    #vId = `v${getNextIndex()} `;
    #rectangle
    constructor(posx, posy, onConnect, onChange) {
        super('grid2d', posx, posy, 300);
        this.node.add(new Flow.TitleElement('Grid2d').setStyle('gray').setOutput(1));
        this.#uSlider = new Flow.SliderInput(5);
        this.#uSlider.onChange(() => {
            onChange();
        });
        this.#vSlider = new Flow.SliderInput(5);
        this.#vSlider.onChange(() => {
            onChange();
        });
        this.#rectangle = new Flow.LabelElement('rectangle');
        this.#rectangle.onConnect(() => {
            onConnect();
        });
        this.#rectangle.setInput(1);

        const u = new Flow.LabelElement('u').add(this.#uSlider);
        const v = new Flow.LabelElement('v').add(this.#vSlider);
        this.node.add(u);
        this.node.add(v);
        this.node.add(this.#rectangle)
    }

    compile() {
        var rectangleId = this.#rectangle.linkedElement ? this.#rectangle.linkedElement.node.wrapper.id : 'null';
        var code = `
            var ${this.id} = new Grid2d(${rectangleId});
${this.id}.U.DivideByCount((int)Inputs["${this.#uId}"]);
${this.id}.V.DivideByCount((int)Inputs["${this.#vId}"]);
            foreach(var s in ${this.id}.GetCellSeparators(GridDirection.U))
            {
                model.AddElement(new ModelCurve((Line)s));
            }
            foreach(var s in ${this.id}.GetCellSeparators(GridDirection.V))
            {
                model.AddElement(new ModelCurve((Line)s));
            }
            `;
        return code;
    }

    getData() {
        var data = {};
        data[this.#uId] = this.#uSlider.getValue();
        data[this.#vId] = this.#vSlider.getValue();
        return data;
    }
}

function addNode(nodeData) {

    const onConnect = () => {
        DotNet.invokeMethod('Elements.Playground', 'SetCodeValue', compileGraph(canvas));
        DotNet.invokeMethod('Elements.Playground', 'Compile');
    };
    const onChange = () => {
        DotNet.invokeMethod('Elements.Playground', 'SetCodeContext', getData(graphNodes));
        DotNet.invokeMethodAsync('Elements.Playground', 'Run');
    };

    const startX = canvas.relativeX + 10;
    const startY = canvas.relativeY + 50;

    var graphNode = null;
    if (nodeData == "Number") {
        graphNode = new NumberNode(startX, startY, onConnect, onChange)
    }
    else {
        graphNode = new NodeDataNode(nodeData, startX, startY, 200, onConnect, onChange);
    }

    canvas.add(graphNode.node);
    graphNodes.push(graphNode);
}

function compileGraph(canvas) {
    let code = `
var model = new Model();
Validator.DisableValidationOnConstruction = true;
`;

    // TODO: Verify that links gives you a list of nodes in ordered fashion
    // Links have inputElement.node and outputElement.node

    let processedNodes = [];
    const links = canvas.getLinks();

    // Process all upstream nodes on links first.
    // The remainder nodes after this operation will be the leaves.
    links.forEach(link => {
        if (!processedNodes.includes(link.outputElement.node.wrapper.id)) {
            console.debug(`Compiling ${link.outputElement.node.wrapper.constructor.name} `);
            code += link.outputElement.node.wrapper.compile() + '\n';
            processedNodes.push(link.outputElement.node.wrapper.id);
        }
    });

    // Process all the leaves.
    links.forEach(link => {
        if (!processedNodes.includes(link.inputElement.node.wrapper.id)) {
            console.log(`Compiling ${link.inputElement.node.wrapper.constructor.name} `);
            code += link.inputElement.node.wrapper.compile() + '\n';
            processedNodes.push(link.inputElement.node.wrapper.id);
        }
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
    canvas = new Flow.Canvas();
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