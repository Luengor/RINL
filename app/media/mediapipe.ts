import {
    PoseLandmarker,
    FilesetResolver,
} from '@mediapipe/tasks-vision';

type ModelType = "lite" | "full" | "heavy"

export async function createPoseLandmarker(
        modelType: ModelType,
        minDetectionConfidence:number = 0.5,
        minPresenceConfidence:number = 0.5,
        minTrackingConfidence:number = 0.5): Promise<PoseLandmarker> {
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
