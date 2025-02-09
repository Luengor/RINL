mergeInto(LibraryManager.library, {
    sendToReact: function(data) {
        const event = new Event('unity2react');
        event.data = UTF8ToString(data);
        window.dispatchEvent(event);
    },
})

