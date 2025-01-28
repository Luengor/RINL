"use client";
import { useState, useRef, useEffect } from "react";
import { Unity, useUnityContext } from "react-unity-webgl";
import { createPoseLandmarker } from "./mediapipe";

export default function Page() {
    // Prepare video
    const [videoStream, setVideoStream] = useState<MediaStream>(null);
    const inputVideoRef = useRef<HTMLVideoElement>(null);

    // Prepare unity
    const unityCanvasRef = useRef<HTMLCanvasElement>(null);
    const { unityProvider, sendMessage  } = useUnityContext({
        loaderUrl: "unity/Build/unity.loader.js",
        dataUrl: "unity/Build/unity.data",
        frameworkUrl: "unity/Build/unity.framework.js",
        codeUrl: "unity/Build/unity.wasm",
    });

    const getVideoStream = async () => {
        if (videoStream) return;

        const stream = await navigator.mediaDevices.getUserMedia({
            video: true
        });

        setVideoStream(stream);
    }

    // Change the size of the unity player when it its ready
    if (!!unityCanvasRef.current && !!videoStream) {
        unityCanvasRef.current.style.width = videoStream.getVideoTracks()[0].getSettings().width + "px";
        unityCanvasRef.current.style.height = videoStream.getVideoTracks()[0].getSettings().height + "px";
    }

    // Create pose landmarker and start detecting
    useEffect(() => {
        if (videoStream && !!unityProvider) {
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

                                sendMessage("RINLBody", "SetBodyPosition", jsonStr);
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
    }, [videoStream, unityProvider, sendMessage]);

    // Render
    let content;
    if (!videoStream)
        content = (<button onClick={getVideoStream}>Get Video Stream</button>)
    else
        content = (
        <>
        <Unity
            id="unity-canvas"
            unityProvider={unityProvider}
            ref={unityCanvasRef}
            matchWebGLToCanvasSize={true}/>
        <br />
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