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
            modelAssetPath: `https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_full/float16/1/pose_landmarker_full.task`,
            delegate: "GPU"
        },
        runningMode: "VIDEO",
        numPoses: 1
    });

    console.log("poseLandmarker", poseLandmarker);
}
createPoseLandmarker();

// Get canvas and context
const canvas = document.getElementById("output") as HTMLCanvasElement;
const ctx = canvas.getContext("2d");

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
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    const start = performance.now();
    if (lastTime !== video.currentTime) {
        lastTime = video.currentTime;
        poseLandmarker.detectForVideo(video, start, (result) => {
            ctx.save();
            // ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
            result.landmarks.forEach((landmark) => {
                landmark.forEach((point) => {
                    ctx.beginPath();
                    ctx.arc(
                        point.x * canvas.width,
                        point.y * canvas.height,
                        4,
                        0,
                        2 * Math.PI);
                    ctx.fill();
                });
            });
            ctx.restore();

            // Convert result.worldLandmarks to a json {landmarks: [LandmarkList]} 
            const json = {landmarks: result.worldLandmarks[0]};
            const jsonStr = JSON.stringify(json);
            console.log(jsonStr)

        })
    }

    if (video.srcObject !== null) {
        window.requestAnimationFrame(predict);
    }
}
