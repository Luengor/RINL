import {
    PoseLandmarker,
    FilesetResolver,
} from '@mediapipe/tasks-vision';

type ModelType = "lite" | "full" | "heavy"

export async function createPoseLandmarker(
        modelType: ModelType,
        minDetectionConfidence = 0.5,
        minPresenceConfidence = 0.5,
        minTrackingConfidence = 0.5): Promise<PoseLandmarker> {
    const vision = await FilesetResolver.forVisionTasks(
        "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.20/wasm"
    );

    const modelPath = `https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_${modelType}/float16/1/pose_landmarker_${modelType}.task`

    return PoseLandmarker.createFromOptions(vision, {
        baseOptions: {
            modelAssetPath: modelPath,
            delegate: "GPU"
        },
        runningMode: "VIDEO",
        numPoses: 1,
        minPoseDetectionConfidence: minDetectionConfidence,
        minPosePresenceConfidence: minPresenceConfidence,
        minTrackingConfidence: minTrackingConfidence
    });
}

let lastTime = 0;
export async function predict(
    poseLandmarker: PoseLandmarker,
    inputVideoRef: React.RefObject<HTMLVideoElement>,
    resultFunc: (result: string) => void,
) {
    if (!inputVideoRef.current) return;

    const start = performance.now();
    if (lastTime !== inputVideoRef.current.currentTime) {
        lastTime = inputVideoRef.current.currentTime;
        poseLandmarker.detectForVideo(
            inputVideoRef.current,
            start,
            (result) => {
                if (!result.worldLandmarks || !result.worldLandmarks.length) return;

                // Convert to a json
                const worldLandmarks = result.worldLandmarks[0].map((landmark) => {
                    return {
                        x: landmark.x.toFixed(3),
                        y: landmark.y.toFixed(3),
                        z: landmark.z.toFixed(3),
                    }
                });
                const imageLandmarks = result.landmarks[0].map((landmark) => {
                    return {
                        x: landmark.x.toFixed(3),
                        y: landmark.y.toFixed(3),
                        z: landmark.z.toFixed(3),
                    }
                });

                const json = {
                    world: worldLandmarks,
                    image: imageLandmarks
                };

                const jsonStr = JSON.stringify(json);

                resultFunc(jsonStr);
            }
        )
    }

    if (inputVideoRef.current.srcObject) {
        requestAnimationFrame(() => predict(poseLandmarker, inputVideoRef, resultFunc));
    }
}
