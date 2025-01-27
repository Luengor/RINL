"use client";
import { useState, useRef, useEffect } from "react";

import {
    PoseLandmarker,
    FilesetResolver,
} from '@mediapipe/tasks-vision';

export default function Page() {
    // Prepare video
    const [videoStream, setVideoStream] = useState<MediaStream>(null);
    const inputVideoRef = useRef<HTMLVideoElement>(null);

    const getVideoStream = async () => {
        if (videoStream) return;

        const stream = await navigator.mediaDevices.getUserMedia({
            video: true
        });

        setVideoStream(stream);

        if (inputVideoRef.current) {
            inputVideoRef.current.srcObject = stream;
        }
    }

    const createPoseLandmarker = async () => {
        const vision = await FilesetResolver.forVisionTasks(
            "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.0/wasm"
        );

        return PoseLandmarker.createFromOptions(vision, {
            baseOptions: {
                modelAssetPath: `https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_heavy/float16/1/pose_landmarker_heavy.task`,
                delegate: "GPU"
            },
            runningMode: "VIDEO",
            numPoses: 1
        });
    }

    useEffect(() => {
        if (videoStream) {
            console.log("video stream ready");
            // Create pose landmarker
            createPoseLandmarker().then((poseLandmarker) => {;
                let lastTime = 0;

                const predict = async () => {
                    const start = performance.now();
                    if (lastTime !== inputVideoRef.current.currentTime) {
                        lastTime = inputVideoRef.current.currentTime;
                        poseLandmarker.detectForVideo(
                            inputVideoRef.current,
                            start,
                            (result) => {
                                if (!result.worldLandmarks || !result.worldLandmarks.length) return;

                                // Convert result.worldLandmarks to a json {landmarks: [LandmarkList]} 
                                const worldLandmarks = result.worldLandmarks[0].map((landmark) => {
                                    return {
                                        x: landmark.x.toFixed(4),
                                        y: landmark.y.toFixed(4),
                                        z: landmark.z.toFixed(4),
                                        v: landmark.visibility.toFixed(4)
                                    }
                                });
                                const json = {landmarks: worldLandmarks};
                                const jsonStr = JSON.stringify(json);
                            }
                        )
                    }

                    if (inputVideoRef.current.srcObject) {
                        requestAnimationFrame(predict);
                    }
                }

                predict();

                return () => {
                    poseLandmarker.close();
                }
            });
        }
    }, [videoStream]);


    // This thing
    return (
        <>
        <video
            id="input"
            ref={(r) => {
                inputVideoRef.current = r;
                getVideoStream();
            }}
            width="640"
            height="480"
            autoPlay
            playsInline
            />
        </>
    )
}