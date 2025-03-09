import { useState, useRef, useEffect } from "react";
import { Unity, useUnityContext } from "react-unity-webgl";
import { createPoseLandmarker, predict } from "../../utils/mediapipe";
import { Center, Loader } from "@mantine/core";

export default function Media() {
  // Prepare video
  const [videoStream, setVideoStream] = useState<MediaStream>(null);
  const inputVideoRef = useRef<HTMLVideoElement>(null);

  // Prepare unity if not on debug
  const { unityProvider, sendMessage, isLoaded } = useUnityContext({
    loaderUrl: "/unity/Build/unity.loader.js",
    dataUrl: "/unity/Build/unity.data",
    frameworkUrl: "/unity/Build/unity.framework.js",
    codeUrl: "/unity/Build/unity.wasm",
  });

  const getVideoStream = async () => {
    if (videoStream) return;

    const stream = await navigator.mediaDevices.getUserMedia({
      video: true
    });

    setVideoStream(stream);
  }

  useEffect(() => {
    getVideoStream();
  })

  // Create pose landmarker and start detecting
  useEffect(() => {
    if (videoStream && !!unityProvider && isLoaded) {
      const isOnMobile = navigator.userAgent.toLowerCase().includes("mobile");
      createPoseLandmarker(isOnMobile ? "lite" : "full").then((poseLandmarker) => {
        predict(poseLandmarker, inputVideoRef, (result) => {
          sendMessage("JSConnector", "SetBodyPosition", result);
        });

        return () => {
          poseLandmarker.close();
        }
      });
    }
  }, [videoStream, unityProvider, sendMessage, isLoaded]);

  // Custom event type expanding Event
  interface UnityEvent extends Event {
    data: {
      type: string;
      payload: object;
    };
  }

  // Unity messages
  useEffect(() => {
    // Subscribe to unity events
    const callback = (e: Event) => {
      // Do smth with the event 
      const {type: t, payload: p} = (e as UnityEvent).data;
      console.log(t, p);
    };

    window.addEventListener("unity2react", callback);

    return () => {
      // Remove event listener
      window.removeEventListener("unity2react", callback);
    }
  });

  useEffect(() => {
    if (!isLoaded) return;

    let timeout_id: NodeJS.Timeout = null;

    const sendVideoSize = () => {
      if (inputVideoRef.current) {
        const msg = {
          width: inputVideoRef.current.videoWidth,
          height: inputVideoRef.current.videoHeight
        };

        if (msg.width === 0 || msg.height === 0) {
          timeout_id = setTimeout(sendVideoSize, 5000);
          return;
        }

        console.log("Sending video size", msg);
        sendMessage("JSConnector", "SetVideoSize", JSON.stringify(msg));
        timeout_id = setTimeout(sendVideoSize, 5000);
      }
    }

    sendVideoSize();

    return () => {
      // Cancel the interval
      if (timeout_id !== null) {
        clearTimeout(timeout_id);
      }
    }
  });

  // Render
  let content = <Loader type="dots" size="xl"/>;
  if (videoStream) {
    content = (
    <>
    <Unity
      unityProvider={unityProvider}
      style={{ width: "100%", height: "100%" }}
      matchWebGLToCanvasSize={true}
    />
    <video
      ref={(r) => {
        inputVideoRef.current = r;
        if (inputVideoRef.current)
          inputVideoRef.current.srcObject = videoStream;
      }}
      hidden
      autoPlay
      playsInline />
    </>
    );

  }

  return (
    <Center h="100%">
      {content}
    </Center>
  )
}