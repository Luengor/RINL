import { useState, useRef, useEffect } from "react";
import { Unity, useUnityContext } from "react-unity-webgl";
import { createPoseLandmarker, predict } from "../../utils/mediapipe";
import { Center, Loader } from "@mantine/core";
import { ActivityBase, createActivityActivityPost } from "../../client";
import { useUser } from "../../hooks/useUser";
import { useNavigate } from "react-router-dom";

const isOnMobile = navigator.userAgent.toLowerCase().includes("mobile");

export default function Media() {
  // Exit if we don't have a shape or verified user
  const { user, verified, hasShape, latestShape } = useUser();
  const navigate = useNavigate();
  if (!verified || !hasShape) {
    navigate("/my/data");
  }

  // Prepare video
  const [videoStream, setVideoStream] = useState<MediaStream>(null);
  const inputVideoRef = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    if (videoStream) return;

    navigator.mediaDevices.getUserMedia({ video: true }).then((stream) => {
      setVideoStream(stream);
    });
  }, [videoStream]);

  // Prepare unity
  const { unityProvider, sendMessage: sendUnityMessage, isLoaded: isUnityLoaded, requestFullscreen } = useUnityContext({
    loaderUrl: "/unity/Build/unity.loader.js",
    dataUrl: "/unity/Build/unity.data",
    frameworkUrl: "/unity/Build/unity.framework.js",
    codeUrl: "/unity/Build/unity.wasm",
    webglContextAttributes: {
      powerPreference: isOnMobile ? 1 : 2,
    }
  });

  // Custom event type expanding Event
  interface UnityEvent extends Event {
    data: {
      type: "activity";
      payload: object;
    };
  }

  // Unity messages
  useEffect(() => {
    // Subscribe to unity events
    const callback = (e: Event) => {
      // Do smth with the event 
      const {type: t, payload: p} = (e as UnityEvent).data;

      switch (t) {
        // Handle activity event
        case "activity":
          createActivityActivityPost({
            body: p as ActivityBase
          })
          break;

        default:
          console.log("Unknown event", t, p);
      }
    };

    window.addEventListener("unity2react", callback);

    return () => {
      // Remove event listener
      window.removeEventListener("unity2react", callback);
    }
  });

  // Send video size to unity
  useEffect(() => {
    if (!isUnityLoaded) return;

    let timeout_id: NodeJS.Timeout = null;

    const sendUserData = () => {
      if (inputVideoRef.current) {
        const msg = {
          width: inputVideoRef.current.videoWidth,
          height: inputVideoRef.current.videoHeight
        };

        if (msg.width === 0 || msg.height === 0) {
          timeout_id = setTimeout(sendUserData, 5000);
          return;
        }

        console.log("Sending video and shape to unity", msg);
        sendUnityMessage("JSConnector", "SetVideoSize", JSON.stringify(msg));
        sendUnityMessage("JSConnector", "SetCurrentShape", JSON.stringify(latestShape));
        sendUnityMessage("JSConnector", "SetCurrentUser", JSON.stringify(user));
        timeout_id = setTimeout(sendUserData, 5000);
      }
    }

    sendUserData();

    return () => {
      // Cancel the interval
      if (timeout_id !== null) {
        clearTimeout(timeout_id);
      }
    }
  }, [isUnityLoaded, sendUnityMessage]);

  // Create pose landmarker and start detecting
  useEffect(() => {
    if (videoStream && !!unityProvider && isUnityLoaded) {
      requestFullscreen(true);

      createPoseLandmarker(isOnMobile ? "lite" : "heavy").then((poseLandmarker) => {
        predict(poseLandmarker, inputVideoRef, (result) => {
          sendUnityMessage("GameController", "SetBodyPosition", result);
        });

        return () => {
          poseLandmarker.close();
        }
      });
    }
  }, [isUnityLoaded]);

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
