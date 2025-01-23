import Script from "next/script";
import React from "react";

export default function Page() {
    return (
        <>
        <Script src="/dist/mediadebug.js" type="module" />
        <video id="input" width="640" height="480" autoPlay playsInline></video>
        </>
    )
}