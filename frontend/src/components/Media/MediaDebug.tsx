import { useState, useRef, useEffect } from "react";
import { createPoseLandmarker, predict } from "../../utils/mediapipe";

export default function MediaDebug() {
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
            createPoseLandmarker("full").then((poseLandmarker) => {
                predict(poseLandmarker, inputVideoRef, (result) => {
                    // Send result to websocket
                    if (ws.readyState === ws.OPEN)
                        ws.send(result);
                });

                return () => {
                    poseLandmarker.close();
                    if (ws)
                        ws.close();
                }
            });
        }
    }, [videoStream, ws]);

    // Render
    let content;
    if (!videoStream)
        content = (<button onClick={getVideoStream}>Get Video Stream</button>)
    else
        content = (
        <video
            id="input"
            ref={(r) => {
                inputVideoRef.current = r;
                if (inputVideoRef.current)
                    inputVideoRef.current.srcObject = videoStream;
            }}
            width="640"
            height="480"
            autoPlay
            playsInline
            />
        )

    // This thing
    return (
        <div>
            {content}
        </div>
    )
}