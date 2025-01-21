/* eslint-disable @next/next/no-sync-scripts */

export default function Page() {
  return (
    <div>
        <script src="./dist/mediapipe.js" type="module"/>
        <video id="input" width="640" height="480" autoPlay playsInline></video>
        <canvas id="output" width="640" height="480"></canvas>
        <div>mediapipe moment</div>
    </div>
  );
}