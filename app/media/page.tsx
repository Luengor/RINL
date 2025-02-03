"use client";
import { useState, useRef, useEffect } from "react";
import { Unity, useUnityContext } from "react-unity-webgl";
import { createPoseLandmarker, predict } from "./mediapipe";

export default function Page() {
    // Prepare video
    const [videoStream, setVideoStream] = useState<MediaStream>(null);
    const inputVideoRef = useRef<HTMLVideoElement>(null);

    // Prepare unity if not on debug
    const unityCanvasRef = useRef<HTMLCanvasElement>(null);
    const { unityProvider, sendMessage, isLoaded } = useUnityContext({
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

    // Create pose landmarker and start detecting
    useEffect(() => {
        if (videoStream && !!unityProvider && isLoaded) {
            const isOnMobile = navigator.userAgent.toLowerCase().includes("mobile");
            createPoseLandmarker(isOnMobile ? "lite" : "full").then((poseLandmarker) => {;
                predict(poseLandmarker, inputVideoRef, (result) => {
                    sendMessage("RINLBody", "SetBodyPosition", result);
                });

                return () => {
                    poseLandmarker.close();
                }
            });
        }
    }, [videoStream, unityProvider, sendMessage, isLoaded]);

    // Unity messages
    useEffect(() => {
        // Subscribe to unity events
        const callback = (e: Event) => {
            // Do smth with the event 
            // console.log(e);
        };

        window.addEventListener("unity2react", callback);

        return () => {
            // Remove event listener
            window.removeEventListener("unity2react", callback);
        }
    });

    // Render
    let content;
    if (!videoStream)
        content = (<button onClick={getVideoStream}>Get Video Stream</button>)
    else
        content = (
        <>
        <Unity
            id="unity-canvas"
            className="flex-auto"
            unityProvider={unityProvider}
            ref={unityCanvasRef}
            matchWebGLToCanvasSize={true}/>
        <br />
        <video
            id="input"
            className="hidden"
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
        <div className="flex items-center">
            {content}
        </div>
    )
}