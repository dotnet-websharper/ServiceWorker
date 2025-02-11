// Cache name
const CACHE_NAME = "simple-cache";

// Files to cache
const CACHE_ASSETS = [
    "/",
    "/index.html"
];

// Install event (Cache the files)
self.addEventListener("install", (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            return cache.addAll(CACHE_ASSETS);
        })
    );
    console.log("Service Worker: Installed");
});

// Fetch event (Serve files from cache)
self.addEventListener("fetch", (event) => {
    event.respondWith(
        caches.match(event.request).then((response) => {
            return response || fetch(event.request);
        })
    );
});
