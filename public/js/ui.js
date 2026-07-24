// Utilitaires d'interface partagés : toasts, modal générique, formatage

const Toast = {
    show(message, type = 'info') {
        const stack = document.getElementById('toastStack');
        const el = document.createElement('div');
        el.className = `toast ${type}`;
        el.textContent = message;
        stack.appendChild(el);
        setTimeout(() => el.remove(), 3500);
    },
    success(msg) { this.show(msg, 'success'); },
    error(msg) { this.show(msg, 'error'); },
};

const Fmt = {
    date(iso) {
        if (!iso) return '—';
        const d = new Date(iso.length <= 10 ? iso + 'T00:00:00' : iso);
        if (isNaN(d)) return iso;
        return d.toLocaleDateString('fr-FR', { day: '2-digit', month: '2-digit', year: 'numeric' });
    },
    money(n) {
        if (n === null || n === undefined) return '—';
        return Number(n).toLocaleString('fr-FR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' DH';
    },
    km(n) {
        if (n === null || n === undefined) return '—';
        return Number(n).toLocaleString('fr-FR') + ' km';
    },
    plate(immat) {
        const parts = String(immat).split('-');
        const num = parts.length === 3 ? `${parts[0]} - ${parts[1]}` : immat;
        const region = parts.length === 3 ? parts[2] : '';
        return `<span class="plate"><span class="num">${num}</span><span class="ma">${region}</span></span>`;
    },
};

const STATUS_BADGE = {
    'Disponible': 'ok', 'En mission': 'blue', 'En réparation': 'warn', 'Hors service': 'danger',
    'Planifiée': 'neutral', 'En cours': 'blue', 'Terminée': 'ok', 'Annulée': 'danger',
    'En attente': 'neutral', 'Approuvée': 'ok', 'Refusée': 'danger',
    'Carburant': 'warn', 'Lubrifiant': 'blue',
    'admin': 'danger', 'mecanicien': 'warn', 'user': 'neutral', 'chauffeur': 'blue',
};

function badge(statut) {
    const cls = STATUS_BADGE[statut] || 'neutral';
    return `<span class="badge badge-${cls}">${statut}</span>`;
}

function escapeHtml(str) {
    if (str === null || str === undefined) return '';
    return String(str).replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

// ---------- Modal générique piloté par une description de champs ----------
const Modal = (() => {
    const overlay = document.getElementById('modalOverlay');
    const titleEl = document.getElementById('modalTitle');
    const fieldsEl = document.getElementById('modalFields');
    const form = document.getElementById('modalForm');
    const submitBtn = document.getElementById('modalSubmit');
    let currentFields = [];
    let onSubmitCallback = null;

    function fieldHtml(f) {
        const spanClass = f.span2 ? 'field span-2' : 'field';
        const req = f.required ? 'required' : '';
        const val = f.value !== undefined && f.value !== null ? f.value : '';

        if (f.type === 'select') {
            const opts = f.options.map(o => {
                const optVal = typeof o === 'object' ? o.value : o;
                const optLabel = typeof o === 'object' ? o.label : o;
                const sel = String(optVal) === String(val) ? 'selected' : '';
                return `<option value="${escapeHtml(optVal)}" ${sel}>${escapeHtml(optLabel)}</option>`;
            }).join('');
            return `<div class="${spanClass}"><label>${f.label}</label>
                <select class="select" name="${f.name}" ${req}>${opts}</select></div>`;
        }
        if (f.type === 'textarea') {
            return `<div class="${spanClass}"><label>${f.label}</label>
                <textarea class="input" name="${f.name}" rows="3" ${req}>${escapeHtml(val)}</textarea></div>`;
        }
        const step = f.type === 'number' ? `step="${f.step || 'any'}"` : '';
        return `<div class="${spanClass}"><label>${f.label}</label>
            <input class="input" type="${f.type || 'text'}" name="${f.name}" value="${escapeHtml(val)}" ${req} ${step}>
            ${f.hint ? `<div class="hint">${f.hint}</div>` : ''}</div>`;
    }

    function open({ title, fields, submitLabel = 'Enregistrer', onSubmit }) {
        titleEl.textContent = title;
        currentFields = fields;
        onSubmitCallback = onSubmit;
        submitBtn.textContent = submitLabel;
        fieldsEl.innerHTML = fields.map(fieldHtml).join('');
        overlay.classList.add('open');
        const first = fieldsEl.querySelector('input, select');
        if (first) setTimeout(() => first.focus(), 50);
    }

    function close() {
        overlay.classList.remove('open');
        form.reset();
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {};
        currentFields.forEach(f => {
            const input = form.elements[f.name];
            let v = input.value;
            if (f.type === 'number') v = v === '' ? null : Number(v);
            data[f.name] = v;
        });
        submitBtn.disabled = true;
        try {
            await onSubmitCallback(data);
            close();
        } catch (err) {
            Toast.error(err.message || 'Une erreur est survenue');
        } finally {
            submitBtn.disabled = false;
        }
    });

    document.getElementById('modalClose').addEventListener('click', close);
    document.getElementById('modalCancel').addEventListener('click', close);
    overlay.addEventListener('click', (e) => { if (e.target === overlay) close(); });

    return { open, close };
})();

function confirmDelete(label) {
    return window.confirm(`Confirmer la suppression : ${label} ?\nCette action est irréversible.`);
}
