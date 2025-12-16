// ====== Config / State ======
const store = {
    get baseUrl() {
        return localStorage.getItem("cp_baseUrl") || "http://localhost:5052";
    },
    set baseUrl(v) {
        localStorage.setItem("cp_baseUrl", v.replace(/\/+$/, "")); // trim trailing /
    },
    get token() { return localStorage.getItem("cp_token"); },
    set token(v) { v ? localStorage.setItem("cp_token", v) : localStorage.removeItem("cp_token"); },
    get userName() { return localStorage.getItem("cp_userName"); },
    set userName(v) { v ? localStorage.setItem("cp_userName", v) : localStorage.removeItem("cp_userName"); },
    get role() { return localStorage.getItem("cp_role"); },
    set role(v) { v ? localStorage.setItem("cp_role", v) : localStorage.removeItem("cp_role"); },
};

// ====== DOM Helpers ======
const $ = (id) => document.getElementById(id);

function toast(msg, type = "good", ms = 2400) {
    const t = $("toast");
    t.className = `toast ${type}`;
    t.textContent = msg;
    t.classList.remove("hidden");
    clearTimeout(toast._timer);
    toast._timer = setTimeout(() => t.classList.add("hidden"), ms);
}

function setActiveNav(hash) {
    const map = {
        "#/": "navWelcome",
        "#/auth": "navAuth",
        "#/dashboard": "navDash",
    };
    ["navWelcome","navAuth","navDash"].forEach(id => $(id).classList.remove("active"));
    const active = map[hash] || "navWelcome";
    $(active).classList.add("active");
}

function showPage(name) {
    ["pageWelcome","pageAuth","pageDashboard"].forEach(p => $(p).classList.add("hidden"));
    $(name).classList.remove("hidden");
}

function requireAuthOrRedirect() {
    if (!store.token) {
        toast("You must login first.", "warn");
        location.hash = "#/auth";
        return false;
    }
    return true;
}

function updateTopbar() {
    const isAuthed = !!store.token;
    const pill = $("pillUser");
    const btnLogout = $("btnLogout");

    if (isAuthed) {
        pill.textContent = `${store.userName || "User"} • ${store.role || "Role"}`;
        pill.classList.remove("hidden");
        btnLogout.classList.remove("hidden");
        $("authStatus").className = "pill tiny good";
        $("authStatus").textContent = "Logged in";
    } else {
        pill.classList.add("hidden");
        btnLogout.classList.add("hidden");
        $("authStatus").className = "pill tiny";
        $("authStatus").textContent = "Not logged in";
    }
}

// ====== API ======
async function api(path, { method="GET", body=null, auth=false } = {}) {
    const url = `${store.baseUrl}${path.startsWith("/") ? "" : "/"}${path}`;
    const headers = { "Accept": "application/json" };

    if (body !== null) headers["Content-Type"] = "application/json";
    if (auth) {
        if (!store.token) throw new Error("Not logged in");
        headers["Authorization"] = `Bearer ${store.token}`;
    }

    const res = await fetch(url, {
        method,
        headers,
        body: body !== null ? JSON.stringify(body) : null
    });

    // Try parse JSON if possible
    const text = await res.text();
    let data = null;
    try { data = text ? JSON.parse(text) : null; } catch { data = text || null; }

    if (!res.ok) {
        // show best possible error message
        const message =
            (data && data.title) ? `${data.title}` :
                (typeof data === "string" && data) ? data :
                    `Request failed (${res.status})`;

        const err = new Error(message);
        err.status = res.status;
        err.data = data;
        throw err;
    }

    return data;
}

// ====== Render: Events ======
function renderEvents(events) {
    const wrap = $("eventsTableWrap");
    const state = $("eventsState");

    if (!events || !events.length) {
        wrap.classList.add("hidden");
        state.classList.remove("hidden");
        state.textContent = "No events found.";
        return;
    }

    state.classList.add("hidden");
    wrap.classList.remove("hidden");

    const rows = events.map(e => `
    <tr>
      <td><b>${escapeHtml(e.title)}</b><div class="muted small">${escapeHtml(e.description || "")}</div></td>
      <td>${escapeHtml(e.location || "")}</td>
      <td>${fmtDate(e.startTime)}</td>
      <td>${fmtDate(e.endTime)}</td>
      <td>${e.capacity}</td>
      <td>${escapeHtml(e.categoryName || "")}</td>
      <td style="text-align:right">
        <button class="btn small primary" onclick="bookEvent(${e.id})">Book</button>
      </td>
    </tr>
  `).join("");

    wrap.innerHTML = `
    <table class="table">
      <thead>
      <tr>
        <th>Title</th><th>Location</th><th>Start</th><th>End</th><th>Cap.</th><th>Category</th><th></th>
      </tr>
      </thead>
      <tbody>${rows}</tbody>
    </table>
  `;
}

function fmtDate(iso) {
    if (!iso) return "";
    const d = new Date(iso);
    return d.toLocaleString();
}

// ====== Render: Bookings ======
function bookingCard(b) {
    const statusClass = (b.status || "").toLowerCase() === "cancelled" ? "bad" : "good";
    return `
  <div class="booking-card" id="booking-${b.id}">
    <div class="booking-top">
      <div>
        <div class="booking-title">${escapeHtml(b.eventTitle || "Event")}</div>
        <div class="muted small">Start: ${fmtDate(b.eventStartTime)}</div>
      </div>
      <div class="pill ${statusClass}">${escapeHtml(b.status || "")}</div>
    </div>

    <div class="booking-meta">
      <div><b>Booking ID:</b> ${b.id}</div>
      <div><b>Event ID:</b> ${b.eventId}</div>
      <div><b>Booked At:</b> ${fmtDate(b.bookedAt)}</div>
      <div><b>User:</b> ${escapeHtml(b.userName || store.userName || "")}</div>
    </div>

    <div class="booking-actions">
      <button class="btn small" onclick="cancelBooking(${b.id})">Cancel</button>
    </div>
  </div>`;
}

function renderBookings(list) {
    const box = $("bookingsList");
    const state = $("bookingsState");

    if (!list || !list.length) {
        box.classList.add("hidden");
        state.classList.remove("hidden");
        state.textContent = "No bookings yet.";
        return;
    }

    state.classList.add("hidden");
    box.classList.remove("hidden");
    box.innerHTML = list.map(bookingCard).join("");
}
document.addEventListener("click", (e) => {
    const btn = e.target.closest(".quick");
    if (!btn) return;
    const url = btn.dataset.url;
    const input = document.getElementById("baseUrl");
    if (input && url) input.value = url;
});


// ====== Actions ======
async function loadEvents() {
    $("eventsState").classList.remove("hidden");
    $("eventsState").textContent = "Loading events...";
    $("eventsTableWrap").classList.add("hidden");

    try {
        const events = await api("/api/Events");
        // search filter client-side
        const q = ($("eventSearch").value || "").trim().toLowerCase();
        const filtered = q
            ? events.filter(e =>
                (e.title || "").toLowerCase().includes(q) ||
                (e.location || "").toLowerCase().includes(q) ||
                (e.categoryName || "").toLowerCase().includes(q)
            )
            : events;

        renderEvents(filtered);
        toast("Events loaded.", "good");
    } catch (e) {
        $("eventsState").textContent = e.message;
        toast(e.message, "bad", 3200);
    }
}

async function loadBookings() {
    if (!requireAuthOrRedirect()) return;

    $("bookingsState").classList.remove("hidden");
    $("bookingsState").textContent = "Loading bookings...";
    $("bookingsList").classList.add("hidden");

    try {
        const list = await api("/api/Bookings", { auth: true });

        const activeBookings = (list || []).filter(
            b => (b.status || "").toLowerCase() !== "cancelled"
        );

        renderBookings(activeBookings);

        if (activeBookings.length === 0) {
            $("bookingsState").classList.remove("hidden");
            $("bookingsState").textContent = "No active bookings.";
        } else {
            $("bookingsState").classList.add("hidden");
            $("bookingsList").classList.remove("hidden");
        }

        toast("Bookings loaded.", "good");
    } catch (e) {
        $("bookingsState").textContent = e.message;
        toast(e.message, "bad", 3200);
    }
}


window.bookEvent = async function(eventId) {
    if (!requireAuthOrRedirect()) return;

    try {
        await api("/api/Bookings", {
            method:"POST",
            auth:true,
            body: { eventId }
        });
        toast("Booking created!", "good");
        await loadBookings();
    } catch (e) {
        toast(e.message, "bad", 3400);
    }
}

window.cancelBooking = async function(bookingId) {
    if (!requireAuthOrRedirect()) return;

    try {
        await api(`/api/Bookings/${bookingId}/cancel`, { method:"PUT", auth:true });

        const el = document.getElementById(`booking-${bookingId}`);
        if (el) el.remove();

        toast("Booking cancelled.", "good");

        const list = document.getElementById("bookingsList");
        if (list && list.children.length === 0) {
            list.classList.add("hidden");
            const state = document.getElementById("bookingsState");
            state.classList.remove("hidden");
            state.textContent = "No bookings yet.";
        }
    } catch (e) {
        toast(e.message, "bad", 3400);
    }
}

// ====== Auth ======
async function register() {
    const name = $("regName").value.trim();
    const email = $("regEmail").value.trim();
    const password = $("regPassword").value;
    const role = $("regRole").value;

    if (!name || !email || !password) {
        toast("Please fill in name, email, password.", "warn");
        return;
    }

    try {
        const user = await api("/api/Auth/register", {
            method:"POST",
            body: { name, email, password, role }
        });

        store.token = user.token;
        store.userName = user.name;
        store.role = user.role;
        updateTopbar();

        toast("Registered & logged in!", "good");
        location.hash = "#/dashboard";
    } catch (e) {
        toast(e.message, "bad", 3500);
    }
}

async function login() {
    const email = $("loginEmail").value.trim();
    const password = $("loginPassword").value;

    if (!email || !password) {
        toast("Enter email and password.", "warn");
        return;
    }

    try {
        const user = await api("/api/Auth/login", {
            method:"POST",
            body: { email, password }
        });

        store.token = user.token;
        store.userName = user.name;
        store.role = user.role;
        updateTopbar();

        toast("Logged in!", "good");
        location.hash = "#/dashboard";
    } catch (e) {
        toast(e.message, "bad", 3500);
    }
}

// ====== Router ======
function onRoute() {
    const hash = location.hash || "#/";
    setActiveNav(hash);

    if (hash === "#/auth") {
        showPage("pageAuth");
    } else if (hash === "#/dashboard") {
        if (!requireAuthOrRedirect()) return;
        showPage("pageDashboard");
        $("helloTitle").textContent = `Hello, ${store.userName || "User"} 👋`;
        $("helloSub").textContent = `Role: ${store.role || "Unknown"} • Manage events & bookings`;
    } else {
        showPage("pageWelcome");
    }

    updateTopbar();
}

// ====== Utilities ======
function escapeHtml(s) {
    return String(s ?? "")
        .replaceAll("&","&amp;")
        .replaceAll("<","&lt;")
        .replaceAll(">","&gt;")
        .replaceAll('"',"&quot;")
        .replaceAll("'","&#039;");
}

// ====== Init ======
function init() {
    // base url setup
    $("baseUrl").value = store.baseUrl;

    $("btnSaveBaseUrl").addEventListener("click", () => {
        const v = $("baseUrl").value.trim();
        if (!v) return toast("Enter a base URL.", "warn");
        store.baseUrl = v;
        toast(`Saved base URL: ${store.baseUrl}`, "good");
    });

    $("btnPing").addEventListener("click", async () => {
        try {
            // simplest endpoint that always exists in template: /WeatherForecast
            await api("/WeatherForecast");
            toast("API is reachable ✅", "good");
        } catch (e) {
            toast(`API not reachable: ${e.message}`, "bad", 3500);
        }
    });

    // auth buttons
    $("btnRegister").addEventListener("click", register);
    $("btnLogin").addEventListener("click", login);

    // dash buttons
    $("btnLoadEvents").addEventListener("click", loadEvents);
    $("btnLoadBookings").addEventListener("click", loadBookings);
    $("btnRefreshAll").addEventListener("click", async () => {
        await loadEvents();
        await loadBookings();
    });

    $("eventSearch").addEventListener("input", () => {
        // re-load events filter only if table already loaded
        const wrapVisible = !$("eventsTableWrap").classList.contains("hidden");
        if (wrapVisible) loadEvents();
    });

    $("btnClearToken").addEventListener("click", () => {
        store.token = null;
        store.userName = null;
        store.role = null;
        updateTopbar();
        toast("Session cleared.", "warn");
        location.hash = "#/auth";
    });

    $("btnLogout").addEventListener("click", () => {
        store.token = null;
        store.userName = null;
        store.role = null;
        updateTopbar();
        toast("Logged out.", "warn");
        location.hash = "#/";
    });

    window.addEventListener("hashchange", onRoute);
    onRoute();
}

init();
