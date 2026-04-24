(function () {
    "use strict";

    const OFFLINE_QUEUE_KEY = "sfa_mobile_offline_queue";
    const IDB_DB_NAME = "sfa_mobile_offline_db";
    const IDB_STORE_NAME = "submission_queue";
    const IDB_VERSION = 1;

    const LAST_AUTH_AT_KEY = "sfa_mobile_last_auth_at";
    const LAST_AUTH_PATH_KEY = "sfa_mobile_last_auth_path";
    const OFFLINE_LOGIN_REDIRECT_ATTEMPTED_KEY = "sfa_mobile_offline_login_redirect_attempted";

    let deferredInstallPrompt = null;

    function supportsIndexedDb() {
        return typeof window.indexedDB !== "undefined";
    }

    function isLoginPath(pathname) {
        const normalized = (pathname || "").toLowerCase();
        return normalized.includes("/mobile/agent/login") || normalized.includes("/identity/account/login");
    }

    function isAuthenticatedPage() {
        return document.body.dataset.isAuthenticated === "true";
    }

    function toRequestPromise(request) {
        return new Promise(function (resolve, reject) {
            request.onsuccess = function () {
                resolve(request.result);
            };

            request.onerror = function () {
                reject(request.error || new Error("IndexedDB request failed."));
            };
        });
    }

    function toTransactionPromise(transaction) {
        return new Promise(function (resolve, reject) {
            transaction.oncomplete = function () {
                resolve();
            };

            transaction.onabort = function () {
                reject(transaction.error || new Error("IndexedDB transaction aborted."));
            };

            transaction.onerror = function () {
                reject(transaction.error || new Error("IndexedDB transaction failed."));
            };
        });
    }

    function openQueueDb() {
        return new Promise(function (resolve, reject) {
            const request = window.indexedDB.open(IDB_DB_NAME, IDB_VERSION);

            request.onupgradeneeded = function () {
                const database = request.result;
                if (!database.objectStoreNames.contains(IDB_STORE_NAME)) {
                    const store = database.createObjectStore(IDB_STORE_NAME, { keyPath: "id", autoIncrement: true });
                    store.createIndex("createdAtUtc", "createdAtUtc", { unique: false });
                }
            };

            request.onsuccess = function () {
                resolve(request.result);
            };

            request.onerror = function () {
                reject(request.error || new Error("Failed to open IndexedDB."));
            };
        });
    }

    const queueStore = {
        async getAll() {
            if (!supportsIndexedDb()) {
                return this.getAllFromLocalStorage();
            }

            const db = await openQueueDb();
            try {
                const transaction = db.transaction(IDB_STORE_NAME, "readonly");
                const store = transaction.objectStore(IDB_STORE_NAME);
                const request = store.getAll();
                const result = await toRequestPromise(request);
                await toTransactionPromise(transaction);
                return result || [];
            } finally {
                db.close();
            }
        },

        async add(item) {
            if (!supportsIndexedDb()) {
                return this.addToLocalStorage(item);
            }

            const db = await openQueueDb();
            try {
                const transaction = db.transaction(IDB_STORE_NAME, "readwrite");
                const store = transaction.objectStore(IDB_STORE_NAME);
                const request = store.add(item);
                const id = await toRequestPromise(request);
                await toTransactionPromise(transaction);
                return id;
            } finally {
                db.close();
            }
        },

        async update(item) {
            if (!supportsIndexedDb()) {
                return this.updateInLocalStorage(item);
            }

            const db = await openQueueDb();
            try {
                const transaction = db.transaction(IDB_STORE_NAME, "readwrite");
                const store = transaction.objectStore(IDB_STORE_NAME);
                store.put(item);
                await toTransactionPromise(transaction);
            } finally {
                db.close();
            }
        },

        async remove(id) {
            if (!supportsIndexedDb()) {
                return this.removeFromLocalStorage(id);
            }

            const db = await openQueueDb();
            try {
                const transaction = db.transaction(IDB_STORE_NAME, "readwrite");
                const store = transaction.objectStore(IDB_STORE_NAME);
                store.delete(id);
                await toTransactionPromise(transaction);
            } finally {
                db.close();
            }
        },

        getAllFromLocalStorage() {
            const raw = window.localStorage.getItem(OFFLINE_QUEUE_KEY);
            if (!raw) {
                return [];
            }

            try {
                const parsed = JSON.parse(raw);
                return Array.isArray(parsed) ? parsed : [];
            } catch {
                return [];
            }
        },

        writeLocalStorage(items) {
            window.localStorage.setItem(OFFLINE_QUEUE_KEY, JSON.stringify(items));
        },

        addToLocalStorage(item) {
            const items = this.getAllFromLocalStorage();
            const newItem = Object.assign({}, item, { id: Date.now() + Math.floor(Math.random() * 10000) });
            items.push(newItem);
            this.writeLocalStorage(items);
            return newItem.id;
        },

        updateInLocalStorage(item) {
            const items = this.getAllFromLocalStorage();
            const index = items.findIndex(function (entry) {
                return entry.id === item.id;
            });

            if (index >= 0) {
                items[index] = item;
                this.writeLocalStorage(items);
            }
        },

        removeFromLocalStorage(id) {
            const items = this.getAllFromLocalStorage().filter(function (entry) {
                return entry.id !== id;
            });
            this.writeLocalStorage(items);
        }
    };

    function createClientUuid() {
        if (window.crypto && typeof window.crypto.randomUUID === "function") {
            return window.crypto.randomUUID();
        }

        return "local-" + Date.now() + "-" + Math.floor(Math.random() * 1000000);
    }

    function getCurrentAntiforgeryToken() {
        const tokenInput = document.querySelector("input[name='__RequestVerificationToken']");
        return tokenInput ? tokenInput.value : "";
    }

    function withCurrentAntiforgeryToken(entries) {
        const token = getCurrentAntiforgeryToken();
        if (!token) {
            return entries;
        }

        const filtered = entries.filter(function (entry) {
            return entry[0] !== "__RequestVerificationToken";
        });
        filtered.unshift(["__RequestVerificationToken", token]);
        return filtered;
    }

    function setStatusMessage(element, message, isError) {
        if (!element) {
            return;
        }

        element.textContent = message;
        element.classList.toggle("text-danger", !!isError);
        element.classList.toggle("d-none", !message);
    }

    function setGeoStatus(form, message, isError) {
        setStatusMessage(form.querySelector("[data-geo-status]"), message, isError);
    }

    function setOfflineStatus(form, message, isError) {
        const statusElement = form.querySelector("[data-offline-status]") || form.querySelector("[data-geo-status]");
        setStatusMessage(statusElement, message, isError);
    }

    function setSyncStatus(message, isError) {
        setStatusMessage(document.querySelector("[data-mobile-sync-status]"), message, isError);
    }

    function updateNetworkStatusBanner(customMessage) {
        const banner = document.querySelector("[data-network-status]");
        if (!banner) {
            return;
        }

        if (navigator.onLine && !customMessage) {
            banner.classList.add("d-none");
            banner.textContent = "";
            return;
        }

        banner.classList.remove("d-none");
        banner.textContent = customMessage || "Offline mode active. You can continue working and records will be stored locally in IndexedDB.";
    }

    function rememberAuthenticatedLocation() {
        if (!isAuthenticatedPage() || isLoginPath(window.location.pathname)) {
            return;
        }

        const currentPath = window.location.pathname + window.location.search;
        window.localStorage.setItem(LAST_AUTH_PATH_KEY, currentPath);
        window.localStorage.setItem(LAST_AUTH_AT_KEY, Date.now().toString());
        window.sessionStorage.removeItem(OFFLINE_LOGIN_REDIRECT_ATTEMPTED_KEY);
    }

    async function primeOfflinePages() {
        if (!navigator.onLine || !isAuthenticatedPage()) {
            return;
        }

        const staticUrls = [
            "/mobile/Agent/Dashboard",
            "/mobile/Agent/RouteSelection",
            "/mobile/Agent/CustomerList",
            "/mobile/Agent/PendingSyncQueue"
        ];

        const linkedUrls = Array.from(document.querySelectorAll("a[href^='/mobile/Agent/']"))
            .map(function (anchor) {
                return anchor.getAttribute("href");
            })
            .filter(function (href) {
                return !!href;
            });

        const urlsToPrime = Array.from(new Set(staticUrls.concat(linkedUrls))).slice(0, 25);

        await Promise.all(
            urlsToPrime.map(async function (url) {
                try {
                    await fetch(url, {
                        method: "GET",
                        credentials: "same-origin",
                        cache: "no-store"
                    });
                } catch {
                    // Ignore network/cache priming failures; runtime fallback still applies.
                }
            })
        );
    }

    function hasOfflineAuthHint() {
        const timestampRaw = window.localStorage.getItem(LAST_AUTH_AT_KEY);
        if (!timestampRaw) {
            return false;
        }

        const timestamp = Number.parseInt(timestampRaw, 10);
        if (Number.isNaN(timestamp)) {
            return false;
        }

        const maxAgeMs = 1000 * 60 * 60 * 24 * 7;
        return Date.now() - timestamp <= maxAgeMs;
    }

    function tryRecoverFromOfflineLoginRedirect() {
        if (navigator.onLine || !isLoginPath(window.location.pathname) || !hasOfflineAuthHint()) {
            return false;
        }

        if (window.sessionStorage.getItem(OFFLINE_LOGIN_REDIRECT_ATTEMPTED_KEY) === "true") {
            return false;
        }

        const lastPath = window.localStorage.getItem(LAST_AUTH_PATH_KEY);
        if (!lastPath || isLoginPath(lastPath)) {
            return false;
        }

        window.sessionStorage.setItem(OFFLINE_LOGIN_REDIRECT_ATTEMPTED_KEY, "true");
        updateNetworkStatusBanner("Internet is unavailable. Reopening your last working page in offline mode.");
        window.location.replace(lastPath);
        return true;
    }

    function initializeInstallPrompt() {
        const installButton = document.querySelector("[data-pwa-install]");
        const installStatus = document.querySelector("[data-pwa-install-status]");
        if (!installButton) {
            return;
        }

        const isStandalone = window.matchMedia("(display-mode: standalone)").matches || window.navigator.standalone === true;
        if (isStandalone) {
            setStatusMessage(installStatus, "PWA installed on this device.", false);
            return;
        }

        window.addEventListener("beforeinstallprompt", function (event) {
            event.preventDefault();
            deferredInstallPrompt = event;
            installButton.classList.remove("d-none");
            setStatusMessage(installStatus, "Install this app for a full-screen mobile experience and better offline handling.", false);
        });

        window.addEventListener("appinstalled", function () {
            deferredInstallPrompt = null;
            installButton.classList.add("d-none");
            setStatusMessage(installStatus, "App installed successfully.", false);
        });

        installButton.addEventListener("click", async function () {
            if (!deferredInstallPrompt) {
                setStatusMessage(installStatus, "Install prompt is not available yet on this browser.", true);
                return;
            }

            deferredInstallPrompt.prompt();
            const choice = await deferredInstallPrompt.userChoice;
            if (choice && choice.outcome === "accepted") {
                setStatusMessage(installStatus, "Installing app...", false);
            } else {
                setStatusMessage(installStatus, "Install dismissed. You can continue with browser mode.", false);
            }

            deferredInstallPrompt = null;
            installButton.classList.add("d-none");
        });
    }

    function getSerializableFormEntries(form) {
        const formData = new FormData(form);
        const entries = [];

        for (const pair of formData.entries()) {
            if (typeof pair[1] === "string") {
                entries.push([pair[0], pair[1]]);
            }
        }

        return entries;
    }

    async function queueFormSubmission(form) {
        const entries = getSerializableFormEntries(form);
        const queueItem = {
            actionUrl: form.action,
            method: (form.method || "POST").toUpperCase(),
            entityType: form.dataset.offlineEntity || new URL(form.action).pathname,
            entityClientUuid: createClientUuid(),
            payloadEntries: entries,
            createdAtUtc: new Date().toISOString(),
            retryCount: 0,
            lastErrorMessage: ""
        };

        await queueStore.add(queueItem);
    }

    function isLoginResponse(response) {
        if (!response || !response.redirected || !response.url) {
            return false;
        }

        const responsePath = new URL(response.url, window.location.origin).pathname;
        return isLoginPath(responsePath);
    }

    async function syncQueuedSubmissions() {
        if (!navigator.onLine) {
            return { total: 0, synced: 0, failed: 0 };
        }

        const allItems = await queueStore.getAll();
        const items = allItems.slice().sort(function (a, b) {
            return (a.id || 0) - (b.id || 0);
        });

        let synced = 0;
        let failed = 0;

        for (const item of items) {
            try {
                const entries = withCurrentAntiforgeryToken(item.payloadEntries || []);
                const body = new URLSearchParams();
                entries.forEach(function (entry) {
                    body.append(entry[0], entry[1]);
                });

                const response = await fetch(item.actionUrl, {
                    method: item.method || "POST",
                    body: body.toString(),
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
                    },
                    credentials: "same-origin",
                    redirect: "follow"
                });

                if (isLoginResponse(response)) {
                    throw new Error("Session expired. Login once online to continue sync.");
                }

                if (!response.ok) {
                    throw new Error("HTTP " + response.status);
                }

                await queueStore.remove(item.id);
                synced += 1;
            } catch (error) {
                failed += 1;
                item.retryCount = (item.retryCount || 0) + 1;
                item.lastRetryAtUtc = new Date().toISOString();
                item.lastErrorMessage = error && error.message ? error.message : "Sync failed.";
                await queueStore.update(item);
                break;
            }
        }

        return {
            total: items.length,
            synced: synced,
            failed: failed
        };
    }

    function formatDate(value) {
        if (!value) {
            return "";
        }

        const parsed = new Date(value);
        if (Number.isNaN(parsed.getTime())) {
            return value;
        }

        return parsed.toLocaleString();
    }

    async function renderLocalQueue() {
        const countElement = document.querySelector("[data-mobile-local-count]");
        const container = document.querySelector("[data-mobile-local-queue]");
        if (!countElement && !container) {
            return;
        }

        const items = await queueStore.getAll();
        const sortedItems = items.slice().sort(function (a, b) {
            return (a.id || 0) - (b.id || 0);
        });

        if (countElement) {
            countElement.textContent = sortedItems.length + " pending";
        }

        if (!container) {
            return;
        }

        container.innerHTML = "";

        if (!sortedItems.length) {
            const empty = document.createElement("div");
            empty.className = "sfa-alert sfa-alert-success";
            empty.textContent = "No offline records are pending on this device.";
            container.appendChild(empty);
            return;
        }

        sortedItems.forEach(function (item) {
            const block = document.createElement("div");
            block.className = "sfa-soft-block mb-2";

            const title = document.createElement("div");
            title.className = "fw-semibold";
            title.textContent = (item.entityType || "OfflineSubmission") + " (" + (item.entityClientUuid || "-") + ")";

            const meta = document.createElement("div");
            meta.className = "small text-muted";
            meta.textContent = "Saved: " + formatDate(item.createdAtUtc) + " | Retry: " + (item.retryCount || 0);

            block.appendChild(title);
            block.appendChild(meta);

            if (item.lastErrorMessage) {
                const error = document.createElement("div");
                error.className = "small mt-1 text-danger";
                error.textContent = item.lastErrorMessage;
                block.appendChild(error);
            }

            container.appendChild(block);
        });
    }

    async function refreshQueueWidgets() {
        await renderLocalQueue();
    }

    function setSubmitButtonState(form, disabled, label) {
        const submitButton = form.querySelector("button[type='submit']");
        if (!submitButton) {
            return function () { };
        }

        const originalText = submitButton.textContent;
        submitButton.disabled = disabled;
        if (label) {
            submitButton.textContent = label;
        }

        return function () {
            submitButton.disabled = false;
            submitButton.textContent = originalText;
        };
    }

    function captureGeolocation(form) {
        return new Promise(function (resolve) {
            const latFieldName = form.dataset.latField;
            const lngFieldName = form.dataset.lngField;
            if (!latFieldName || !lngFieldName) {
                resolve(true);
                return;
            }

            const latInput = form.querySelector("[name='" + latFieldName + "']");
            const lngInput = form.querySelector("[name='" + lngFieldName + "']");
            if (!latInput || !lngInput) {
                resolve(true);
                return;
            }

            if (!navigator.geolocation) {
                setGeoStatus(form, "Geolocation is not supported by this browser.", true);
                resolve(false);
                return;
            }

            const restoreButton = setSubmitButtonState(form, true, "Capturing location...");
            setGeoStatus(form, "Fetching current location...", false);

            navigator.geolocation.getCurrentPosition(
                function (position) {
                    latInput.value = position.coords.latitude.toFixed(7);
                    lngInput.value = position.coords.longitude.toFixed(7);
                    form.dataset.geoReady = "true";
                    setGeoStatus(form, "Location captured.", false);
                    restoreButton();
                    resolve(true);
                },
                function (error) {
                    let message = "Unable to capture location. Please allow location permission and retry.";
                    if (error && error.code === error.PERMISSION_DENIED) {
                        message = "Location permission denied. Enable location access and retry.";
                    }

                    setGeoStatus(form, message, true);
                    restoreButton();
                    resolve(false);
                },
                {
                    enableHighAccuracy: true,
                    timeout: 12000,
                    maximumAge: 0
                }
            );
        });
    }

    function wireForm(form) {
        form.addEventListener("submit", async function (event) {
            const requiresGeo = form.dataset.autoGeolocation === "true";
            if (requiresGeo && form.dataset.geoReady !== "true") {
                event.preventDefault();
                const captured = await captureGeolocation(form);
                if (captured) {
                    form.requestSubmit();
                }
                return;
            }

            const offlineEnabled = form.dataset.offlineEnabled === "true";
            if (offlineEnabled && !navigator.onLine) {
                event.preventDefault();
                try {
                    await queueFormSubmission(form);
                    form.dataset.geoReady = "false";
                    setOfflineStatus(form, "Saved offline in IndexedDB. It will sync automatically when internet is available.", false);
                    await refreshQueueWidgets();
                } catch {
                    setOfflineStatus(form, "Unable to save locally. Reconnect internet and submit again.", true);
                }
            }
        });
    }

    function initializeForms() {
        const forms = document.querySelectorAll("form[data-auto-geolocation='true'], form[data-offline-enabled='true']");
        forms.forEach(function (form) {
            wireForm(form);
        });
    }

    function initializeSyncButton() {
        const syncButton = document.querySelector("[data-mobile-sync-now]");
        if (!syncButton) {
            return;
        }

        syncButton.addEventListener("click", async function () {
            if (!navigator.onLine) {
                setSyncStatus("You are offline. Connect internet and retry sync.", true);
                return;
            }

            syncButton.disabled = true;
            const originalText = syncButton.textContent;
            syncButton.textContent = "Syncing...";

            try {
                const result = await syncQueuedSubmissions();
                await refreshQueueWidgets();
                if (result.synced > 0) {
                    setSyncStatus("Synced " + result.synced + " record(s) from this device queue.", false);
                } else if (result.total === 0) {
                    setSyncStatus("No device records are pending sync.", false);
                } else {
                    setSyncStatus("Sync attempted, but some records are still pending.", true);
                }
            } catch {
                setSyncStatus("Sync failed. Retry when internet is stable.", true);
            } finally {
                syncButton.disabled = false;
                syncButton.textContent = originalText;
            }
        });
    }

    function initializeNetworkHandlers() {
        window.addEventListener("online", async function () {
            updateNetworkStatusBanner();
            const result = await syncQueuedSubmissions();
            await refreshQueueWidgets();
            if (result.synced > 0) {
                setSyncStatus("Auto-synced " + result.synced + " record(s) after reconnect.", false);
            }
        });

        window.addEventListener("offline", function () {
            updateNetworkStatusBanner();
        });
    }

    document.addEventListener("DOMContentLoaded", async function () {
        initializeInstallPrompt();
        rememberAuthenticatedLocation();
        if (tryRecoverFromOfflineLoginRedirect()) {
            return;
        }

        await primeOfflinePages();

        initializeForms();
        initializeSyncButton();
        initializeNetworkHandlers();
        updateNetworkStatusBanner();

        await refreshQueueWidgets();

        if (navigator.onLine) {
            const result = await syncQueuedSubmissions();
            if (result.synced > 0) {
                setSyncStatus("Auto-synced " + result.synced + " record(s).", false);
            }
            await refreshQueueWidgets();
        }
    });
})();
