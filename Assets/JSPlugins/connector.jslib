mergeInto(LibraryManager.library, {
    SendToReact: function(type, payload) {
        const event = new Event('unity2react');
        event.data = {
            type: UTF8ToString(type),
            payload: UTF8ToString(payload)
        }
        window.dispatchEvent(event);
    },
})

