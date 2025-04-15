export async function SendAndAck(
  sendMessageFunction: () => void,
  eventName: string
) {
  // Create a promise that resolves when the event is received
  const promise = new Promise<void>((resolve) => {
    const listener = () => {
      window.removeEventListener(`unityAck${eventName}`, listener);
      console.log("Unity ack received for", eventName);
      resolve();
    };
    window.addEventListener(`unityAck${eventName}`, listener);
  });

  // Send the message to Unity
  console.log("Waiting Unity ack for", eventName);
  sendMessageFunction();

  // Wait for the promise to resolve
  await promise;
}
