mergeInto(LibraryManager.library, {
    SendToReact: function(type, payload) {
        const event = new Event('unity2react');
        event.data = {
            type: UTF8ToString(type),
            payload: JSON.parse(UTF8ToString(payload))
        };
        window.dispatchEvent(event);
    },

    SendAck: function(functionName) {
        const parsedName = UTF8ToString(functionName);
        window.dispatchEvent(new Event(`unityAck${parsedName}`));
    }
})

