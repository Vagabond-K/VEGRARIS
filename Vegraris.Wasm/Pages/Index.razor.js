export function initialize(updater) {
    window.updater = updater;
    window.lineClearAudio = new Audio("./sounds/lineClear.wav");
    window.lockDownAudio = new Audio("./sounds/lockDown.wav");
    window.readyMessageAudio = new Audio("./sounds/readyMessage.wav");
    window.requestAnimationFrame(updateFrame);
}

function updateFrame(timeStamp) {
    window.updater.invokeMethodAsync('UpdateFrame', timeStamp)
        .then(() => {
            window.requestAnimationFrame(updateFrame);
        });
}

export function lineClear() {
    playSound(window.lineClearAudio);
}

export function lockDown() {
    playSound(window.lockDownAudio);
}

export function readyMessage() {
    playSound(window.readyMessageAudio);
}

function playSound(audio) {
    audio.pause();
    audio.currentTime = 0;
    audio.play();
}

function serializeEventArgs(e) {
    return {
        key: e.key,
        code: e.code,
        location: e.location,
        repeat: e.repeat,
        ctrlKey: e.ctrlKey,
        shiftKey: e.shiftKey,
        altKey: e.altKey,
        metaKey: e.metaKey,
        type: e.type
    };
}

window.addEventListener('keydown', function (e) { if (e) updater.invokeMethodAsync('OnKeyDown', serializeEventArgs(e)) });
window.addEventListener('keyup', function (e) { if (e) updater.invokeMethodAsync('OnKeyUp', serializeEventArgs(e)) });
window.addEventListener('blur', function (e) { if (e) updater.invokeMethodAsync('OnBlur') });