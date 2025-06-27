import { useEffect } from "react";

interface RINLBackProps {
  canvasRef: React.RefObject<HTMLCanvasElement>;
  text?: string;
  fontSize?: number;
  fontFamily?: string;
  fontWeight?: string;
  textColor?: string;
  scrollSpeed?: number;
  columnSep?: number;
  repetitions?: number;
  randomSpeedMagnitude?: number;
}

export default function useBackground(props: RINLBackProps) {
  useEffect(() => {
    console.log("useBackground effect triggered");
    const canvas = props.canvasRef.current;
    if (!canvas)
    {
      console.warn("Canvas reference is null");
      return;
    }

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
    const repetitions = props.repetitions || 2;
    const randomSpeedMagnitude = props.randomSpeedMagnitude || 1;

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
      const xOffset = Math.random() * fontSize;
      for (let i = 0; i < numColumns; i++) {
        columns.push({
          x: i * columnWidth - xOffset,
          y: Math.random() * canvasHeight,
          speed: Math.max(scrollSpeed + (Math.random() - 0.5) * randomSpeedMagnitude, 0),
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
        const ySep = (canvasHeight + fontSize) / repetitions;
        for (let i = 0; i < repetitions; i++) {
          ctx.fillText(
            text,
            col.x,
            (col.y + i * ySep) % (canvasHeight + fontSize)
          );
        }

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

  }, [props.canvasRef]);
}
