async function encryptToken(rawToken, secretKey) {
    const key = await window.crypto.subtle.importKey(
        "raw",
        new TextEncoder().encode(secretKey),
        {name: "AES-GCM", length: 256},
        false,
        ["encrypt"]
    );

    const iv = window.crypto.getRandomValues(new Uint8Array(12));
    const encryptedData = await window.crypto.subtle.encrypt(
        {name: "AES-GCM", iv: iv},
        key,
        new TextEncoder().encode(rawToken)
    );

    const buffer = new Uint8Array(encryptedData);
    const array = Array.from(buffer);
    const encryptedToken = array.join(",");

    return {encryptedToken, iv: Array.from(iv).join(",")};
}

async function decryptToken(encryptedToken, iv, secretKey) {
    const key = await window.crypto.subtle.importKey(
        "raw",
        new TextEncoder().encode(secretKey),
        {name: "AES-GCM", length: 256},
        false,
        ["decrypt"]
    );

    const encryptedArray = new Uint8Array(encryptedToken.split(",").map(Number));
    const ivArray = new Uint8Array(iv.split(",").map(Number));

    const decryptedData = await window.crypto.subtle.decrypt(
        {name: "AES-GCM", iv: ivArray},
        key,
        encryptedArray
    );

    const decryptedToken = new TextDecoder().decode(decryptedData);
    return decryptedToken;
}