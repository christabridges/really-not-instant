const isLocal = ["localhost", "127.0.0.1"].includes(location.hostname);

export const API_URL = isLocal ? "http://localhost:8080" : "https://really-not-instant.onrender.com";