"use client";
import { useState, useRef, useEffect } from "react";
import { createPoseLandmarker } from "../media/mediapipe";

export default function Page() {
    // Get a websocket
    const [ws, setWs] = useState<WebSocket>(null);

    // Prepare video
    const [videoStream, setVideoStream] = useState<MediaStream>(null);
    const inputVideoRef = useRef<HTMLVideoElement>(null);

    const getVideoStream = async () => {
        if (videoStream) return;

        const stream = await navigator.mediaDevices.getUserMedia({
            video: true
        });

        setVideoStream(stream);
        setWs(new WebSocket("ws://localhost:8778"));
    }

    // Create pose landmarker and start detecting
    useEffect(() => {
        if (videoStream) {
            const isOnMobile = navigator.userAgent.toLowerCase().includes("mobile");
            createPoseLandmarker(isOnMobile ? "lite" : "full").then((poseLandmarker) => {;
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

                                ws.send(jsonStr);
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
                    // if (ws)
                    //     ws.close();
                }
            });
        }
    }, [videoStream]);

    // Render
    let content;
    if (!videoStream)
        content = (<button onClick={getVideoStream}>Get Video Stream</button>)
    else
        content = (
        <>
        <video
            id="input"
            ref={(r) => {
                inputVideoRef.current = r;
                if (!!inputVideoRef.current)
                    inputVideoRef.current.srcObject = videoStream;
            }}
            width="640"
            height="480"
            autoPlay
            playsInline
            />
        </>)

    // This thing
    return (
        <>
        {content}
        </>
    )
}