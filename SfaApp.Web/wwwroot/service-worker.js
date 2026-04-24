const CACHE_NAME = "sfa-mobile-shell-v1";
const SHELL_ASSETS = [
    "/mobile",
    "/css/site.css",
    "/lib/bootstrap/dist/css/bootstrap.min.css",
    "/lib/bootstrap/dist/js/bootstrap.bundle.min.js",
    "/js/mobile-agent.js",
    "/manifest.webmanifest",
    "/offline.html",
    "/favicon.ico"
];

self.addEventListener("install", (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => cache.addAll(SHELL_ASSETS)).catch(() => Promise.resolve())
    );
    self.skipWaiting();
});

self.addEventListener("activate", (event) => {
    event.waitUntil(
        caches.keys().then((keys) =>
            Promise.all(
                keys
                    .filter((key) => key !== CACHE_NAME)
                    .map((key) => caches.delete(key))
            )
        )
    );
    self.clients.claim();
});

self.addEventListener("fetch", (event) => {
    const request = event.request;
    if (request.method !== "GET") {
        return;
    }

    if (request.mode === "navigate") {
        event.respondWith(
            fetch(request)
                .then((response) => {
                    if (response && response.ok) {
                        const responseClone = response.clone();
                        caches.open(CACHE_NAME).then((cache) => cache.put(request, responseClone)).catch(() => Promise.resolve());
                    }

                    return response;
                })
                .catch(async () => {
                    const cache = await caches.open(CACHE_NAME);
                    const cachedNavigation = await cache.match(request);
                    if (cachedNavigation) {
                        return cachedNavigation;
                    }

                    const cachedMobile = await cache.match("/mobile");
                    if (cachedMobile) {
                        return cachedMobile;
                    }

                    return cache.match("/offline.html");
                })
        );

        return;
    }

    event.respondWith(
        caches.match(request).then((cachedResponse) => {
            if (cachedResponse) {
                return cachedResponse;
            }

            return fetch(request)
                .then((networkResponse) => {
                    if (networkResponse && networkResponse.ok) {
                        const responseClone = networkResponse.clone();
                        caches.open(CACHE_NAME).then((cache) => cache.put(request, responseClone)).catch(() => Promise.resolve());
                    }

                    return networkResponse;
                })
                .catch(() => {
                    if (request.destination === "document") {
                        return caches.match("/offline.html");
                    }

                    return new Response("", { status: 503, statusText: "Offline" });
                });
        })
    );
});
