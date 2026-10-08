// Thin wrapper over the Firebase JS SDK, called from C# via JS interop.
// Documents cross the boundary as JSON strings so C# controls serialization.
import { initializeApp } from "https://www.gstatic.com/firebasejs/12.18.0/firebase-app.js";
import { getAuth, GoogleAuthProvider, onAuthStateChanged, signInWithPopup, signOut as firebaseSignOut } from "https://www.gstatic.com/firebasejs/12.18.0/firebase-auth.js";
import { getFirestore, collection, doc, getDoc, getDocs, setDoc, deleteDoc } from "https://www.gstatic.com/firebasejs/12.18.0/firebase-firestore.js";

let auth;
let db;

export function init(config, dotNetRef) {
    const app = initializeApp(config);
    auth = getAuth(app);
    db = getFirestore(app);
    onAuthStateChanged(auth, user => dotNetRef.invokeMethodAsync("OnAuthChanged",
        user ? { email: user.email, name: user.displayName, photoUrl: user.photoURL } : null));
}

export async function signIn() {
    const provider = new GoogleAuthProvider();
    provider.setCustomParameters({ prompt: "select_account" });
    await signInWithPopup(auth, provider);
}

export function signOut() {
    return firebaseSignOut(auth);
}

export async function list(path) {
    const snapshot = await getDocs(collection(db, path));
    return JSON.stringify(snapshot.docs.map(d => ({ id: d.id, data: d.data() })));
}

export async function get(path) {
    const snapshot = await getDoc(doc(db, path));
    return snapshot.exists() ? JSON.stringify(snapshot.data()) : null;
}

export function set(path, json) {
    return setDoc(doc(db, path), JSON.parse(json));
}

export function remove(path) {
    return deleteDoc(doc(db, path));
}
