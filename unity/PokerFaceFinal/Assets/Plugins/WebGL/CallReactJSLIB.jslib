mergeInto(LibraryManager.library, {
    TakeGameInfoFromReact: function() {
        try {
            window.dispatchReactUnityEvent("TakeGameInfoFromReact");
        }
        catch (e) {
            console.warn("Failed to dispatch event");
        }
    },
    UserDropOutToReact: function(drop) {
        try {
            window.dispatchReactUnityEvent("UserDropOutToReact");
        }
        catch (e) {
            console.warn("Failed to dispatch event");
        }
    },
    UserRoomOutToReact: function(drop) {
        try {
            window.dispatchReactUnityEvent("UserRoomOutToReact");
        }
        catch (e) {
            console.warn("Failed to dispatch event");
        }
    },
    StartFaceAPIToReact: function(over) {
        try {
            window.dispatchReactUnityEvent("StartFaceAPIToReact");
        }
        catch (e) {
            console.warn("Failed to dispatch event");
        }
    },
    GameOverToReact: function(over) {
        try {
            window.dispatchReactUnityEvent("GameOverToReact");
        }
        catch (e) {
            console.warn("Failed to dispatch event");
        }
    },
});