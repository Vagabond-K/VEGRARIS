export function setState(element, pointerId, state) {
    if (state == true) {
        element.setPointerCapture(pointerId);
    }
    else {
        element.releasePointerCapture(pointerId);
    }
}