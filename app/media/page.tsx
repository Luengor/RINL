import Script from "next/script";
import React from "react";

export default function Page() {
    return (
        <>
        <div id="unity-container" className="unity-desktop">
            <canvas id="unity-canvas" width={960} height={600} tabIndex={-1}></canvas>
        </div>
        <Script src="/dist/mediapipe.js" type="module" />
        <video id="input" width="640" height="480" style={{display: "none"}} autoPlay playsInline></video>
        </>
    )
}