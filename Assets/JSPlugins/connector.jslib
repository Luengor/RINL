mergeInto(LibraryManager.library, {
    // initConnector: function() {
    //     window.addEventListener('react2unity', recvFromReact);
    //     console.log("Unity connector initialized");
    // },

    sendToReact: function(data) {
        const event = new Event('unity2react');
        event.data = UTF8ToString(data);
        window.dispatchEvent(event);
    },
})

