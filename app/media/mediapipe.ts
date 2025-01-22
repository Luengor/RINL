import {
    PoseLandmarker,
    FilesetResolver,
} from '@mediapipe/tasks-vision';

let poseLandmarker: PoseLandmarker = undefined;

export const createPoseLandmarker = async () => {
    const vision = await FilesetResolver.forVisionTasks(
        "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.0/wasm"
    );

    poseLandmarker = await PoseLandmarker.createFromOptions(vision, {
        baseOptions: {
            modelAssetPath: `https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_heavy/float16/1/pose_landmarker_heavy.task`,
            delegate: "GPU"
        },
        runningMode: "VIDEO",
        numPoses: 1
    });

    console.log("poseLandmarker", poseLandmarker);
}
createPoseLandmarker();

// Get unity things
const canvas = document.querySelector("#unity-canvas") as HTMLCanvasElement;

const buildUrl = "unity/Build";
const loaderUrl = buildUrl + "/unity.loader.js";
const config = {
  dataUrl: buildUrl + "/unity.data",
  frameworkUrl: buildUrl + "/unity.framework.js",
  codeUrl: buildUrl + "/unity.wasm",
  streamingAssetsUrl: "StreamingAssets",
  companyName: "DefaultCompany",
  productName: "plswork",
  productVersion: "0.1",
};

canvas.style.width = "960px";
canvas.style.height = "600px";

const script = document.createElement("script");

type UnityInstance = {
    SendMessage: (gameObject: string, methodName: string, message: string) => void;
};

let unityInstance: UnityInstance = null;
script.src = loaderUrl;
script.onload = () => {
  createUnityInstance(canvas, config, (progress: number) => {
    console.log("progress", progress);
        }).then((ui: UnityInstance) => {
            unityInstance = ui;
            console.log("unityInstance", unityInstance);
        });
      };

document.body.appendChild(script);

// Get video stream
const video = document.getElementById("input") as HTMLVideoElement;
const constraints = {
    video: true
};
navigator.mediaDevices.getUserMedia(constraints).then((stream) => {
    video.srcObject = stream;
    video.addEventListener("loadeddata", predict)
})

// Predict
let lastTime = -1;
async function predict() {
    const start = performance.now();
    if (lastTime !== video.currentTime && unityInstance !== null) {
        lastTime = video.currentTime;
        poseLandmarker.detectForVideo(video, start, (result) => {
            // Convert result.worldLandmarks to a json {landmarks: [LandmarkList]} 
            const json = {landmarks: result.worldLandmarks[0]};
            const jsonStr = JSON.stringify(json);
            unityInstance.SendMessage("RINLBody", "SetBodyPosition", jsonStr);
        })
    }

    if (video.srcObject !== null) {
        window.requestAnimationFrame(predict);
    }
}
