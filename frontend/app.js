// Calls go through nginx reverse-proxy → backend (no CORS config needed)
const API = "/api/products";

const grid = document.getElementById("grid");
const statusEl = document.getElementById("status");
const searchInput = document.getElementById("search");
const categorySel = document.getElementById("category");
const prevBtn = document.getElementById("prev");
const nextBtn = document.getElementById("next");
const pageInfo = document.getElementById("pageInfo");
const dialog = document.getElementById("detail");
const detailBody = document.getElementById("detailBody");

const state = { page: 1, size: 12, query: "", category: "" };

// --- Data fetchers ------------------------------------------------
async function fetchCategories() {
  const res = await fetch(`${API}/categories`);
  if (!res.ok) return [];
  return res.json();
}

async function fetchProducts() {
  const params = new URLSearchParams({ page: state.page, size: state.size });
  let url;
  if (state.query) {
    params.set("q", state.query);
    url = `${API}/search?${params}`;
  } else {
    if (state.category) params.set("category", state.category);
    url = `${API}?${params}`;
  }

  statusEl.textContent = "Loading...";
  try {
    const res = await fetch(url);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();
    // Paged list endpoint returns {items,total,page,size}; search returns array directly
    const items = Array.isArray(data) ? data : data.items ?? [];
    statusEl.textContent = `${items.length} product(s) shown`;
    return items;
  } catch (err) {
    statusEl.textContent = `Error: ${err.message}`;
    return [];
  }
}

async function fetchDetail(id) {
  const res = await fetch(`${API}/${id}`);
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

// --- Rendering ----------------------------------------------------
function renderGrid(items) {
  grid.innerHTML = "";
  if (!items.length) {
    grid.innerHTML = `<p style="grid-column:1/-1;color:#64748b;">No products found.</p>`;
    return;
  }
  for (const p of items) {
    const card = document.createElement("div");
    card.className = "card";
    card.innerHTML = `
      <img src="${p.thumbnail ?? ""}" alt="${escapeHtml(p.title)}" loading="lazy" />
      <div class="body">
        <div class="title">${escapeHtml(p.title)}</div>
        <div class="brand">${escapeHtml(p.brand ?? "")} · ${escapeHtml(p.category ?? "")}</div>
        <div class="rating">★ ${(p.rating ?? 0).toFixed(1)}</div>
        <div class="price">$${p.price?.toFixed(2) ?? "—"}</div>
      </div>`;
    card.onclick = () => openDetail(p.id);
    grid.appendChild(card);
  }
}

async function openDetail(id) {
  detailBody.innerHTML = `<p>Loading...</p>`;
  dialog.showModal();
  try {
    const p = await fetchDetail(id);
    detailBody.innerHTML = `
      <div class="detail-header">
        <img src="${p.thumbnail ?? ""}" alt="" />
        <div class="detail-meta">
          <h2>${escapeHtml(p.title)}</h2>
          <p><b>${escapeHtml(p.brand ?? "")}</b> · ${escapeHtml(p.category ?? "")}</p>
          <p>★ ${(p.rating ?? 0).toFixed(1)} · Stock: ${p.stock} · ${escapeHtml(p.availabilityStatus ?? "")}</p>
          <p style="font-size:20px;font-weight:700;">$${p.price?.toFixed(2)}
            <small style="color:#64748b;font-weight:400;">(−${p.discountPercentage?.toFixed(0)}%)</small></p>
          <div class="tag-list">${(p.tags ?? []).map(t => `<span class="tag">${escapeHtml(t)}</span>`).join("")}</div>
        </div>
      </div>
      <p>${escapeHtml(p.description ?? "")}</p>
      <h3>Reviews (${p.reviews?.length ?? 0})</h3>
      ${(p.reviews ?? []).map(r => `
        <div class="review">
          <div class="r-head"><b>${escapeHtml(r.reviewerName)}</b><span>★ ${r.rating}</span></div>
          <div>${escapeHtml(r.comment)}</div>
        </div>`).join("")}
    `;
  } catch (err) {
    detailBody.innerHTML = `<p>Error: ${err.message}</p>`;
  }
}

function escapeHtml(s) {
  return String(s ?? "").replace(/[&<>"']/g, c => ({
    "&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"
  }[c]));
}

// --- Event wiring -------------------------------------------------
let debounce;
searchInput.addEventListener("input", (e) => {
  clearTimeout(debounce);
  debounce = setTimeout(() => {
    state.query = e.target.value.trim();
    state.page = 1;
    refresh();
  }, 300);
});

categorySel.addEventListener("change", (e) => {
  state.category = e.target.value;
  state.page = 1;
  refresh();
});

prevBtn.addEventListener("click", () => {
  if (state.page > 1) { state.page--; refresh(); }
});
nextBtn.addEventListener("click", () => { state.page++; refresh(); });

dialog.addEventListener("click", (e) => {
  if (e.target === dialog || e.target.hasAttribute("data-close")) dialog.close();
});

async function refresh() {
  pageInfo.textContent = `Page ${state.page}`;
  prevBtn.disabled = state.page <= 1;
  const items = await fetchProducts();
  renderGrid(items);
}

// --- Init ---------------------------------------------------------
(async function init() {
  const cats = await fetchCategories();
  for (const c of cats) {
    const opt = document.createElement("option");
    opt.value = c; opt.textContent = c;
    categorySel.appendChild(opt);
  }
  refresh();
})();
