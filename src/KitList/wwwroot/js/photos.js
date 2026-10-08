// Shrinks a chosen photo to a JPEG data URL small enough to store in a Firestore document (1 MB limit).
export async function readResizedImage(input, maxSize, quality) {
    const file = input.files && input.files[0];
    if (!file) return null;
    try {
        const bitmap = await createImageBitmap(file);
        const scale = Math.min(1, maxSize / Math.max(bitmap.width, bitmap.height));
        const canvas = document.createElement("canvas");
        canvas.width = Math.round(bitmap.width * scale);
        canvas.height = Math.round(bitmap.height * scale);
        canvas.getContext("2d").drawImage(bitmap, 0, 0, canvas.width, canvas.height);
        bitmap.close();
        return canvas.toDataURL("image/jpeg", quality);
    } finally {
        input.value = "";
    }
}
