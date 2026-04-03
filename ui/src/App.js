import React, { useEffect, useState } from "react";
import "./App.css";

const API_URL = process.env.REACT_APP_API_URL || "http://localhost:5152";

const BRAND_STYLE = {
  apple:   { bg: "#1d1d1f", color: "#fff" },
  samsung: { bg: "#1428a0", color: "#fff" },
  google:  { bg: "#4285f4", color: "#fff" },
};
function brandPlaceholder(brand) {
  const key = brand?.toLowerCase();
  const s   = BRAND_STYLE[key] || { bg: "#0f3460", color: "#fff" };
  return (
    <div className="card-img-placeholder" style={{ background: s.bg }}>
      <span style={{ fontSize: "3rem" }}>📱</span>
      <span className="brand-label" style={{ color: s.color }}>{brand}</span>
    </div>
  );
}

const EMPTY_FORM = { name: "", brand: "", price: "", stock: "", imageUrl: "" };

function stockBadge(stock) {
  if (stock === 0) return <span className="stock-badge out">Out of stock</span>;
  if (stock <= 5)  return <span className="stock-badge low">Low stock: {stock}</span>;
  return                  <span className="stock-badge in">In stock: {stock}</span>;
}

export default function App() {
  const [products, setProducts]   = useState([]);
  const [loading, setLoading]     = useState(true);
  const [error, setError]         = useState(null);
  const [showForm, setShowForm]   = useState(false);   // add/edit modal
  const [editTarget, setEditTarget] = useState(null);  // product being edited
  const [form, setForm]           = useState(EMPTY_FORM);
  const [deleteTarget, setDeleteTarget] = useState(null); // confirm-delete modal
  const [saving, setSaving]       = useState(false);

  /* ── Fetch all ── */
  function loadProducts() {
    setLoading(true);
    fetch(`${API_URL}/api/Products`)
      .then(r => { if (!r.ok) throw new Error(`HTTP ${r.status}`); return r.json(); })
      .then(data => { setProducts(data); setLoading(false); })
      .catch(e  => { setError(e.message); setLoading(false); });
  }
  useEffect(loadProducts, []);

  /* ── Open add modal ── */
  function openAdd() {
    setEditTarget(null);
    setForm(EMPTY_FORM);
    setShowForm(true);
  }

  /* ── Open edit modal ── */
  function openEdit(p) {
    setEditTarget(p);
    setForm({ name: p.name, brand: p.brand, price: p.price, stock: p.stock, imageUrl: p.imageUrl || "" });
    setShowForm(true);
  }

  /* ── Save (add or update) ── */
  function handleSave(e) {
    e.preventDefault();
    setSaving(true);
    const isEdit = editTarget !== null;
    const url    = isEdit ? `${API_URL}/api/Products/${editTarget.id}` : `${API_URL}/api/Products`;
    const method = isEdit ? "PUT" : "POST";
    fetch(url, {
      method,
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        ...form,
        price: parseFloat(form.price),
        stock: parseInt(form.stock, 10),
      }),
    })
      .then(r => { if (!r.ok) throw new Error(`HTTP ${r.status}`); return r.json(); })
      .then(() => { setShowForm(false); setSaving(false); loadProducts(); })
      .catch(e => { alert("Error: " + e.message); setSaving(false); });
  }

  /* ── Delete ── */
  function handleDelete() {
    fetch(`${API_URL}/api/Products/${deleteTarget.id}`, { method: "DELETE" })
      .then(r => { if (!r.ok) throw new Error(`HTTP ${r.status}`); return r.json(); })
      .then(() => { setDeleteTarget(null); loadProducts(); })
      .catch(e => alert("Error: " + e.message));
  }

  return (
    <>
      {/* ── Header ── */}
      <header className="header">
        <div>
          <h1><span className="logo-mark">M</span> MOBIlinq</h1>
          <div className="header-subtitle">{products.length} phones in inventory</div>
        </div>
        <button className="btn-add" onClick={openAdd}>+ Add Phone</button>
      </header>

      {/* ── Main grid ── */}
      <main className="container">
        <div className="section-label">All Products — {products.length} phones</div>
        {loading ? (
          <div className="status-bar">Loading products…</div>
        ) : error ? (
          <div className="status-bar error">⚠ {error}</div>
        ) : products.length === 0 ? (
          <div className="status-bar">No products yet. Click <strong>+ Add Phone</strong> to get started.</div>
        ) : (
          <div className="grid">
            {products.map(p => (
              <div className="card" key={p.id}>
                <div className="card-img-wrap">
                  {p.imageUrl
                    ? <img src={p.imageUrl} alt={p.name}
                        onError={e => { e.target.style.display="none"; e.target.nextSibling.style.display="flex"; }}
                      />
                    : null}
                  <div style={{ display: p.imageUrl ? "none" : "flex", width:"100%", height:"100%" }}>
                    {brandPlaceholder(p.brand)}
                  </div>
                </div>
                <div className="card-body">
                  <div className="card-brand">{p.brand}</div>
                  <div className="card-name">{p.name}</div>
                  <div className="card-price">${Number(p.price).toLocaleString()}</div>
                  <div className="card-stock">{stockBadge(p.stock)}</div>
                </div>
                <div className="card-actions">
                  <button className="btn-edit"   onClick={() => openEdit(p)}>✏ Edit</button>
                  <button className="btn-delete" onClick={() => setDeleteTarget(p)}>🗑 Delete</button>
                </div>
              </div>
            ))}
          </div>
        )}
      </main>

      {/* ── Add / Edit modal ── */}
      {showForm && (
        <div className="modal-overlay" onClick={() => setShowForm(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h2>{editTarget ? "✏ Edit Product" : "➕ Add New Phone"}</h2>
            <form onSubmit={handleSave}>
              {[
                { label: "Name",      key: "name",     type: "text",   placeholder: "e.g. iPhone 16" },
                { label: "Brand",     key: "brand",    type: "text",   placeholder: "e.g. Apple" },
                { label: "Price ($)", key: "price",    type: "number", placeholder: "e.g. 999.99" },
                { label: "Stock",     key: "stock",    type: "number", placeholder: "e.g. 25" },
                { label: "Image URL", key: "imageUrl", type: "url",    placeholder: "https://…" },
              ].map(f => (
                <div className="form-group" key={f.key}>
                  <label>{f.label}</label>
                  <input
                    type={f.type}
                    placeholder={f.placeholder}
                    value={form[f.key]}
                    onChange={e => setForm({ ...form, [f.key]: e.target.value })}
                    required={f.key !== "imageUrl"}
                    min={f.type === "number" ? "0" : undefined}
                    step={f.key === "price" ? "0.01" : undefined}
                  />
                </div>
              ))}
              <div className="form-actions">
                <button type="button" className="btn-cancel" onClick={() => setShowForm(false)}>Cancel</button>
                <button type="submit" className="btn-save" disabled={saving}>{saving ? "Saving…" : "Save"}</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ── Confirm delete modal ── */}
      {deleteTarget && (
        <div className="modal-overlay" onClick={() => setDeleteTarget(null)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h2>🗑 Delete Product</h2>
            <p className="confirm-text">
              Are you sure you want to delete <strong>{deleteTarget.name}</strong>?
              This action cannot be undone.
            </p>
            <div className="form-actions">
              <button className="btn-cancel"         onClick={() => setDeleteTarget(null)}>Cancel</button>
              <button className="btn-confirm-delete" onClick={handleDelete}>Delete</button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
