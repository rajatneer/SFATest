const CACHE_NAME = "sfa-mobile-shell-v2";
const LAST_MOBILE_PAGE_KEY = "/mobile-offline-last-page";
const SHELL_ASSETS = [
    "/css/site.css",
    "/lib/bootstrap/dist/css/bootstrap.min.css",
    "/lib/bootstrap/dist/js/bootstrap.bundle.min.js",
    "/js/mobile-agent.js",
    "/manifest.webmanifest",
    "/offline.html",
    "/favicon.ico"
];

function isMobilePath(pathname) {
    return pathname.toLowerCase().startsWith("/mobile");
}

function isLoginPath(pathname) {
    const normalized = pathname.toLowerCase();
    return normalized.includes("/mobile/agent/login") || normalized.includes("/identity/account/login");
}

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
                .then(async (response) => {
                    if (response && response.ok) {
                        const requestUrl = new URL(request.url);
                        const responseUrl = new URL(response.url);
                        const responseClone = response.clone();
                        const cache = await caches.open(CACHE_NAME);

                        await cache.put(request, responseClone);

                        if (isMobilePath(requestUrl.pathname) && !isLoginPath(responseUrl.pathname)) {
                            await cache.put(LAST_MOBILE_PAGE_KEY, response.clone());
                        }
                    }

                    return response;
                })
                .catch(async () => {
                    const requestUrl = new URL(request.url);
                    const cache = await caches.open(CACHE_NAME);
                    const cachedNavigation = await cache.match(request);
                    if (cachedNavigation) {
                        return cachedNavigation;
                    }

                    if (isMobilePath(requestUrl.pathname)) {
                        const cachedLastMobilePage = await cache.match(LAST_MOBILE_PAGE_KEY);
                        if (cachedLastMobilePage) {
                            return cachedLastMobilePage;
                        }
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
