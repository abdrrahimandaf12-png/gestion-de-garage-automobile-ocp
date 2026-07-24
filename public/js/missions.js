const MissionsModule = (() => {
    let cache = [];

    async function vehiculeOptions() {
        const vehicules = await Api.vehicules.list();
        return vehicules.map(v => ({ value: v.id, label: `${v.immatriculation} — ${v.marque} ${v.modele}` }));
    }

    async function fields(m = {}) {
        const options = await vehiculeOptions();
        return [
            { name: 'vehiculeId', label: 'Véhicule', type: 'select', required: true, value: m.vehiculeId, options, span2: true },
            { name: 'chauffeur', label: 'Chauffeur', required: true, value: m.chauffeur },
            { name: 'destination', label: 'Destination', required: true, value: m.destination },
            { name: 'motif', label: 'Motif de la mission', value: m.motif, span2: true },
            { name: 'dateDepart', label: 'Date de départ', type: 'date', required: true, value: m.dateDepart },
            { name: 'dateRetour', label: 'Date de retour', type: 'date', value: m.dateRetour },
            { name: 'kmDepart', label: 'Km au départ', type: 'number', value: m.kmDepart },
            { name: 'kmRetour', label: 'Km au retour', type: 'number', value: m.kmRetour },
            { name: 'statut', label: 'Statut', type: 'select', value: m.statut || 'Planifiée',
              options: ['Planifiée', 'En cours', 'Terminée', 'Annulée'] },
        ];
    }

    async function openCreate() {
        Modal.open({
            title: 'Nouvelle mission',
            fields: await fields(),
            onSubmit: async (data) => {
                await Api.missions.create(data);
                Toast.success('Mission créée');
                await load();
            },
        });
    }

    async function openEdit(m) {
        Modal.open({
            title: `Modifier la mission — ${m.destination}`,
            fields: await fields(m),
            onSubmit: async (data) => {
                await Api.missions.update(m.id, data);
                Toast.success('Mission mise à jour');
                await load();
            },
        });
    }

    async function remove(m) {
        if (!confirmDelete(`la mission vers ${m.destination}`)) return;
        try {
            await Api.missions.remove(m.id);
            Toast.success('Mission supprimée');
            await load();
        } catch (err) { Toast.error(err.message); }
    }

    function row(m) {
        const isAdmin = AppState.isAdmin();
        const kmParcourus = (m.kmDepart != null && m.kmRetour != null) ? (m.kmRetour - m.kmDepart) : null;
        return `<tr>
            <td>${Fmt.plate(m.immatriculation)}</td>
            <td>${escapeHtml(m.chauffeur)}</td>
            <td>${escapeHtml(m.destination)}</td>
            <td>${escapeHtml(m.motif || '—')}</td>
            <td class="mono">${Fmt.date(m.dateDepart)}</td>
            <td class="mono">${Fmt.date(m.dateRetour)}</td>
            <td class="mono">${kmParcourus !== null ? Fmt.km(kmParcourus) : '—'}</td>
            <td>${badge(m.statut)}</td>
            ${isAdmin ? `
            <td>
                <div class="row-actions">
                    <button class="icon-btn" data-edit="${m.id}" title="Modifier">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H6a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2v-5M18.5 2.5a2.1 2.1 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                    </button>
                    <button class="icon-btn" data-del="${m.id}" title="Supprimer">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M8 6V4a2 2 0 012-2h4a2 2 0 012 2v2m3 0l-1 14a2 2 0 01-2 2H7a2 2 0 01-2-2L4 6h16z"/></svg>
                    </button>
                </div>
            </td>
            ` : ''}
        </tr>`;
    }

    async function load() {
        const statut = document.getElementById('filterMissionStatut').value;
        cache = await Api.missions.list(statut ? `?statut=${encodeURIComponent(statut)}` : '');
        const body = document.getElementById('missionsBody');
        const colCount = AppState.isAdmin() ? 9 : 8;
        body.innerHTML = cache.length ? cache.map(row).join('') :
            `<tr><td colspan="${colCount}"><div class="empty-state">Aucune mission ne correspond à ces filtres.</div></td></tr>`;

        body.querySelectorAll('[data-edit]').forEach(btn =>
            btn.addEventListener('click', () => openEdit(cache.find(m => m.id == btn.dataset.edit))));
        body.querySelectorAll('[data-del]').forEach(btn =>
            btn.addEventListener('click', () => remove(cache.find(m => m.id == btn.dataset.del))));
    }

    function init() {
        const btn = document.getElementById('btnAddMission');
        if (btn) btn.style.display = '';
        const ths = document.querySelectorAll('#view-missions thead th');
        if (ths.length >= 9) ths[8].style.display = '';
        document.getElementById('btnAddMission').addEventListener('click', openCreate);
        document.getElementById('filterMissionStatut').addEventListener('change', load);
    }

    function initReadOnly() {
        const btn = document.getElementById('btnAddMission');
        if (btn) btn.style.display = 'none';
        document.getElementById('filterMissionStatut').addEventListener('change', load);
        const ths = document.querySelectorAll('#view-missions thead th');
        if (ths.length >= 9) ths[8].style.display = 'none';
    }

    return { init, load, initReadOnly };
})();