// Shares text through the phone's share sheet where there is one, otherwise copies it.
export async function shareText(title, text) {
    if (navigator.share) {
        try {
            await navigator.share({ title, text });
            return "shared";
        } catch (e) {
            if (e.name === "AbortError") return "cancelled";
        }
    }
    await navigator.clipboard.writeText(text);
    return "copied";
}
