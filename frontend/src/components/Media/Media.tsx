import { useState, useRef, useEffect } from "react";
import { Unity, useUnityContext } from "react-unity-webgl";
import { createPoseLandmarker, predict } from "../../utils/mediapipe";
import {
  Button,
  Card,
  Center,
  Loader,
  Stack,
  Text,
  Title,
} from "@mantine/core";
import { ActivityBase, createActivityActivityPost } from "../../client";
import { useUser } from "../../hooks/useUser";
import { useNavigate } from "react-router-dom";
import { SendAndAck } from "../../utils/unity";
import { useClient } from "../../hooks/useClient";
import { useQueryClient } from "@tanstack/react-query";

const isOnMobile = navigator.userAgent.toLowerCase().includes("mobile");

export default function Media() {
  // Exit if we don't have a shape or verified user
  const { user, verified, hasShape, latestShape } = useUser();
  const navigate = useNavigate();
  if (!verified || !hasShape) {
    navigate("/my/data");
  }

  // Prepare video
  const [videoError, setVideoError] = useState<string>(null);
  const [videoStream, setVideoStream] = useState<MediaStream>(null);
  const inputVideoRef = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    if (videoStream || videoError) return;

    navigator.mediaDevices
      .getUserMedia({ video: true })
      .then((stream) => {
        // Check if the stream is not 0x0
        if (
          stream.getVideoTracks().length === 0 ||
          stream.getVideoTracks()[0].getSettings().width === 0
        ) {
          setVideoError(
            "La cámara no está disponible o no tiene resolución válida."
          );

          // Stop the stream
          stream.getTracks().forEach((track) => {
            track.stop();
          });

          return;
        }

        setVideoStream(stream);
      })
      .catch((err) => {
        setVideoError(
          "No se pudo acceder a la cámara. Por favor, asegúrate de que tienes una cámara conectada y que has concedido los permisos necesarios."
        );
      });
  }, [videoStream, videoError]);

  const projectName = "com.luengor.rinl";

  // Prepare unity
  const {
    unityProvider,
    sendMessage: sendUnityMessage,
    isLoaded: isUnityLoaded,
    requestFullscreen,
    unload: unloadUnity,
  } = useUnityContext({
    loaderUrl: `/${projectName}/Build/${projectName}.loader.js`,
    dataUrl: `/${projectName}/Build/${projectName}.data`,
    frameworkUrl: `/${projectName}/Build/${projectName}.framework.js`,
    codeUrl: `/${projectName}/Build/${projectName}.wasm`,
    webglContextAttributes: {
      powerPreference: isOnMobile ? 1 : 2,
    },
  });

  // Custom event type expanding Event
  interface UnityEvent extends Event {
    data: {
      type: "activity";
      payload: object;
    };
  }

  // Unity messages
  const { client } = useClient();
  const queryClient = useQueryClient();
  useEffect(() => {
    // Subscribe to unity events
    const callback = (e: Event) => {
      // Do smth with the event
      console.log("Unity event", e);
      const { type: t, payload: p } = (e as UnityEvent).data;
      const activity = p as ActivityBase;
      activity.date = new Date().toISOString();

      switch (t) {
        // Handle activity event
        case "activity":
          // Create an activity and invalidate the cache
          createActivityActivityPost({
            client: client,
            body: activity,
          });
          queryClient.invalidateQueries({ queryKey: ["activity-shape-data"] });
          break;

        default:
          console.log("Unknown event", t, p);
      }
    };

    window.addEventListener("unity2react", callback);

    return () => {
      // Remove event listener
      window.removeEventListener("unity2react", callback);
    };
  });

  // Send video size to unity
  useEffect(() => {
    if (!isUnityLoaded) return;

    let timeout_id: NodeJS.Timeout = null;

    const sendUserData = async () => {
      if (inputVideoRef.current) {
        const msg = {
          width: inputVideoRef.current.videoWidth,
          height: inputVideoRef.current.videoHeight,
        };

        if (msg.width === 0 || msg.height === 0) {
          timeout_id = setTimeout(sendUserData, 1000);
          return;
        }

        console.log("Sending video and shape to unity", msg);
        await SendAndAck(() => {
          sendUnityMessage(
            "GameController",
            "SetVideoSize",
            JSON.stringify(msg)
          );
        }, "SetVideoSize");
        await SendAndAck(() => {
          sendUnityMessage(
            "GameController",
            "SetCurrentShape",
            JSON.stringify(latestShape)
          );
        }, "SetCurrentShape");
        await SendAndAck(() => {
          sendUnityMessage(
            "GameController",
            "SetCurrentUser",
            JSON.stringify(user)
          );
        }, "SetCurrentUser");
      }
    };

    sendUserData();

    return () => {
      // Cancel the interval
      if (timeout_id !== null) {
        clearTimeout(timeout_id);
      }
    };
  }, [isUnityLoaded, sendUnityMessage]);

  // Create pose landmarker and start detecting
  useEffect(() => {
    if (videoStream && !!unityProvider && isUnityLoaded) {
      // requestFullscreen(true);

      createPoseLandmarker(isOnMobile ? "lite" : "full").then(
        (poseLandmarker) => {
          predict(poseLandmarker, inputVideoRef, (result) => {
            sendUnityMessage("GameController", "SetBodyPosition", result);
          });

          return () => {
            poseLandmarker.close();
          };
        }
      );
    }
  }, [isUnityLoaded]);

  // Unload unity when unmounting
  useEffect(() => {
    return () => {
      if (isUnityLoaded) {
        console.log("Unloading unity");
        unloadUnity();
      }
    };
  }, [isUnityLoaded, unloadUnity]);

  // Stop video stream when unmounting
  useEffect(() => {
    return () => {
      if (videoStream) {
        videoStream.getTracks().forEach((track) => {
          track.stop();
        });
      }
    };
  }, [videoStream]);

  // Render
  let content: JSX.Element;
  if (videoError) {
    content = (
      <Stack align="center">
        <Card
          shadow="sm"
          p="xl"
          radius="md"
          withBorder
          w={{ base: "100%", sm: 500 }}
        >
          <Text size="xl" ta="justify">
            {videoError}
          </Text>
        </Card>
      </Stack>
    );
  } else if (videoStream === null) {
    content = (
      <Card
        shadow="sm"
        p="xl"
        radius="md"
        withBorder
        w={{ base: "100%", sm: 500 }}
      >
        <Stack align="center">
          <Title order={2}>Esperando a la cámara...</Title>
          <Loader type="dots" size="xl" />
          <Text size="md" ta="justify">
            Por favor, asegúrate de que tienes una cámara conectada y que has
            concedido los permisos necesarios.
          </Text>
        </Stack>
      </Card>
    );
  } else if (videoStream !== null) {
    content = (
      <>
        <Stack w="100%" h="100%" justify="center" align="center">
          <Button
            mih={30}
            fullWidth
            onClick={() => {
              requestFullscreen(true);
            }}
          >
            Jugar en pantalla completa
          </Button>
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
            style={{
              position: "absolute",
              right: "var(--mantine-spacing-xl)",
              bottom: "var(--mantine-spacing-xl)",
              width: "20%",
              objectFit: "cover",
              opacity: 0.8,
            }}
            autoPlay
            playsInline
          />
        </Stack>
      </>
    );
  }

  return <Center h="100%">{content}</Center>;
}
