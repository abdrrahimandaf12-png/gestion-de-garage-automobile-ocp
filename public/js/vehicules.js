const ParcModule = (() => {
    let cache = [];

    function fields(v = {}) {
        return [
            { name: 'immatriculation', label: 'Immatriculation', required: true, value: v.immatriculation, hint: 'ex. 12345-A-28' },
            { name: 'typeVehicule', label: 'Type de véhicule', type: 'select', required: true, value: v.typeVehicule,
              options: ['Léger', 'Utilitaire', 'Poids lourd', 'Engin'] },
            { name: 'marque', label: 'Marque', required: true, value: v.marque },
            { name: 'modele', label: 'Modèle', required: true, value: v.modele },
            { name: 'serviceAffecte', label: 'Service affecté', value: v.serviceAffecte },
            { name: 'dateAcquisition', label: "Date d'acquisition", type: 'date', value: v.dateAcquisition },
            { name: 'kilometrage', label: 'Kilométrage (km)', type: 'number', value: v.kilometrage ?? 0 },
            { name: 'statut', label: 'Statut', type: 'select', value: v.statut || 'Disponible',
              options: ['Disponible', 'En mission', 'En réparation', 'Hors service'] },
        ];
    }

    function openCreate() {
        Modal.open({
            title: 'Nouveau véhicule',
            fields: fields(),
            onSubmit: async (data) => {
                await Api.vehicules.create(data);
                Toast.success('Véhicule ajouté au parc');
                await load();
            },
        });
    }

    function openEdit(v) {
        Modal.open({
            title: `Modifier — ${v.immatriculation}`,
            fields: fields(v),
            onSubmit: async (data) => {
                await Api.vehicules.update(v.id, data);
                Toast.success('Véhicule mis à jour');
                await load();
            },
        });
    }

    async function remove(v) {
        if (!confirmDelete(v.immatriculation)) return;
        try {
            await Api.vehicules.remove(v.id);
            Toast.success('Véhicule supprimé');
            await load();
        } catch (err) { Toast.error(err.message); }
    }

    function row(v) {
        const isAdmin = AppState.isAdmin();
        const isChauffeur = AppState.isChauffeur();
        const actions = isAdmin ? `
            <div class="row-actions">
                <button class="icon-btn" data-edit="${v.id}" title="Modifier">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H6a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2v-5M18.5 2.5a2.1 2.1 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                </button>
                <button class="icon-btn" data-del="${v.id}" title="Supprimer">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M8 6V4a2 2 0 012-2h4a2 2 0 012 2v2m3 0l-1 14a2 2 0 01-2 2H7a2 2 0 01-2-2L4 6h16z"/></svg>
                </button>
            </div>` : isChauffeur && (v.statut === 'Disponible' || v.statut === 'En réparation') ? `
            <div class="row-actions">
                <button class="icon-btn" data-toggle="${v.id}" title="Basculer statut">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M7 16V4m0 0L3 8m4-4l4 4M17 8v12m0 0l4-4m-4 4l-4-4"/></svg>
                </button>
            </div>` : '';
        const colspan = isAdmin ? 7 : (isChauffeur ? 7 : 6);
        return `<tr>
            <td>${Fmt.plate(v.immatriculation)}</td>
            <td>${escapeHtml(v.typeVehicule)}</td>
            <td>${escapeHtml(v.serviceAffecte || '—')}</td>
            <td class="mono">${Fmt.km(v.kilometrage)}</td>
            <td class="mono">${Fmt.date(v.dateAcquisition)}</td>
            <td>${badge(v.statut)}</td>
            ${(isAdmin || isChauffeur) ? `<td>${actions}</td>` : ''}
        </tr>`;
    }

    async function load() {
        const statut = document.getElementById('filterParcStatut').value;
        const type = document.getElementById('filterParcType').value;
        const qs = [];
        if (statut) qs.push(`statut=${encodeURIComponent(statut)}`);
        if (type) qs.push(`type=${encodeURIComponent(type)}`);
        cache = await Api.vehicules.list(qs.length ? '?' + qs.join('&') : '');
        const body = document.getElementById('parcBody');
        const isAdmin = AppState.isAdmin();
        const isChauffeur = AppState.isChauffeur();
        const colCount = (isAdmin || isChauffeur) ? 7 : 6;
        body.innerHTML = cache.length ? cache.map(row).join('') :
            `<tr><td colspan="${colCount}"><div class="empty-state">Aucun véhicule ne correspond à ces filtres.</div></td></tr>`;

        body.querySelectorAll('[data-edit]').forEach(btn =>
            btn.addEventListener('click', () => openEdit(cache.find(v => v.id == btn.dataset.edit))));
        body.querySelectorAll('[data-del]').forEach(btn =>
            btn.addEventListener('click', () => remove(cache.find(v => v.id == btn.dataset.del))));
        body.querySelectorAll('[data-toggle]').forEach(btn => {
            btn.addEventListener('click', async () => {
                const v = cache.find(x => x.id == btn.dataset.toggle);
                if (!v) return;
                const newStatut = v.statut === 'Disponible' ? 'En réparation' : 'Disponible';
                try {
                    await Api.vehicules.update(v.id, { statut: newStatut });
                    Toast.success(`Véhicule ${v.immatriculation} → ${newStatut}`);
                    await load();
                } catch (err) { Toast.error(err.message); }
            });
        });
    }

    function init() {
        const btn = document.getElementById('btnAddVehicule');
        if (btn) btn.style.display = '';
        const ths = document.querySelectorAll('#view-parc thead th');
        if (ths.length >= 7) ths[6].style.display = '';
        ths[6].textContent = '';
        document.getElementById('btnAddVehicule').addEventListener('click', openCreate);
        document.getElementById('filterParcStatut').addEventListener('change', load);
        document.getElementById('filterParcType').addEventListener('change', load);
    }

    function initReadOnly() {
        const btn = document.getElementById('btnAddVehicule');
        if (btn) btn.style.display = 'none';
        document.getElementById('filterParcStatut').addEventListener('change', load);
        document.getElementById('filterParcType').addEventListener('change', load);
        const ths = document.querySelectorAll('#view-parc thead th');
        if (ths.length >= 7) {
            if (AppState.isChauffeur()) {
                ths[6].style.display = '';
                ths[6].textContent = 'Actions';
            } else {
                ths[6].style.display = 'none';
            }
        }
    }

    return { init, initReadOnly, load, getAll: () => cache };
})();