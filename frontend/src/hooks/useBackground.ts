import { useEffect, useState } from "react";

interface RINLBackProps {
  canvasRef: React.RefObject<HTMLCanvasElement>;
  text?: string;
  fontSize?: number;
  fontFamily?: string;
  fontWeight?: string;
  textColor?: string;
  scrollSpeed?: number;
  columnSep?: number;
}

export default function useBackground(props: RINLBackProps) {
  const [shouldAnimate, setShouldAnimate] = useState(true);
  const { canvasRef } = props;

  useEffect(() => {
    console.log("useBackground effect triggered");
    if (!shouldAnimate) {
      console.log("Animation is disabled");
      return;
    }
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    // Configuración de cosas
    const text = props.text || "RINL";
    const fontSize = props.fontSize || 120;
    const fontFamily = props.fontFamily || "Helvetica, Arial, sans-serif";
    const fontWeight = props.fontWeight || "900";
    const textColor = props.textColor || "rgba(229, 231, 235, 0.5)";
    const scrollSpeed = props.scrollSpeed || 0.8;
    const columnSep = props.columnSep || 100;

    interface Column {
      x: number;
      y: number;
      speed: number;
    }
    let columns: Column[] = [];
    let canvasWidth = window.innerWidth;
    let canvasHeight = window.innerHeight;

    // Setup hay que hacerlo si se redimensiona la ventana así que lo metemos en una función
    function setup() {
      canvasWidth = window.innerWidth;
      canvasHeight = window.innerHeight;
      canvas.width = canvasWidth;
      canvas.height = canvasHeight;

      ctx.font = `${fontWeight} ${fontSize}px ${fontFamily}`;
      const textMetrics = ctx.measureText(text);
      const textWidth = textMetrics.width;

      const columnWidth = textWidth + columnSep;
      const numColumns = Math.ceil(canvasWidth / columnWidth) + 1;

      columns = [];
      for (let i = 0; i < numColumns; i++) {
        columns.push({
          x: i * columnWidth - 100,
          y: Math.random() * canvasHeight,
          speed: Math.max(scrollSpeed + (Math.random() - 0.5) * 1, 0),
        });
      }
    }

    let animationFrameId: number | null = null;

    // la animación
    function animate() {
      ctx.clearRect(0, 0, canvasWidth, canvasHeight);

      ctx.fillStyle = textColor;
      ctx.font = `${fontWeight} ${fontSize}px ${fontFamily}`;
      ctx.textAlign = "left";

      columns.forEach((col) => {
        ctx.fillText(text, col.x, col.y % (canvasHeight + fontSize));
        ctx.fillText(text, col.x, (col.y + (canvasHeight + fontSize) / 2) % (canvasHeight + fontSize));

        col.y += col.speed;
      });

      animationFrameId = requestAnimationFrame(animate);
    }

    // Redraw canvas if window is resized
    window.addEventListener("resize", setup);

    // Start the animation
    setup();
    animate();

    return () => {
      window.removeEventListener("resize", setup);
      if (animationFrameId) {
        cancelAnimationFrame(animationFrameId);
      }
    };

  }, [canvasRef, shouldAnimate]);

  return {
      shouldAnimate,
      setShouldAnimate,
  }
}
