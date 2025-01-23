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

// Get video stream
const video = document.getElementById("input") as HTMLVideoElement;
const constraints = {
    video: true
};
navigator.mediaDevices.getUserMedia(constraints).then((stream) => {
    video.srcObject = stream;
    video.addEventListener("loadeddata", predict)
})

// Open websocket to localhost:8765
const ws = new WebSocket("ws://localhost:8765");

// Predict
let lastTime = -1;
async function predict() {
    const start = performance.now();
    if (lastTime !== video.currentTime) {
        lastTime = video.currentTime;
        poseLandmarker.detectForVideo(video, start, (result) => {
            // Convert result.worldLandmarks to a json {landmarks: [LandmarkList]} 
            const json = {landmarks: result.worldLandmarks[0]};
            const jsonStr = JSON.stringify(json);

            // Send through websocket to localhost:8765
            ws.send(jsonStr);
        })
    }

    if (video.srcObject !== null) {
        window.requestAnimationFrame(predict);
    }
}
