const ConsoModule = (() => {
    let cache = [];

    async function vehiculeOptions() {
        const vehicules = await Api.vehicules.list();
        return vehicules.map(v => ({ value: v.id, label: `${v.immatriculation} — ${v.marque} ${v.modele}` }));
    }

    async function fields(c = {}) {
        const options = await vehiculeOptions();
        return [
            { name: 'vehiculeId', label: 'Véhicule', type: 'select', required: true, value: c.vehiculeId, options, span2: true },
            { name: 'typeConso', label: 'Type', type: 'select', required: true, value: c.typeConso || 'Carburant',
              options: ['Carburant', 'Lubrifiant'] },
            { name: 'dateConso', label: 'Date', type: 'date', required: true, value: c.dateConso },
            { name: 'quantite', label: 'Quantité (L)', type: 'number', step: '0.01', required: true, value: c.quantite },
            { name: 'coutUnitaire', label: 'Coût unitaire (DH/L)', type: 'number', step: '0.01', value: c.coutUnitaire },
            { name: 'kilometrage', label: 'Kilométrage au relevé', type: 'number', value: c.kilometrage },
            { name: 'fournisseur', label: 'Fournisseur / station', value: c.fournisseur, span2: true },
        ];
    }

    async function openCreate() {
        Modal.open({
            title: 'Nouvel enregistrement de consommation',
            fields: await fields(),
            onSubmit: async (data) => {
                await Api.consommations.create(data);
                Toast.success('Consommation enregistrée');
                await load();
            },
        });
    }

    async function openEdit(c) {
        Modal.open({
            title: `Modifier l'enregistrement — ${c.immatriculation}`,
            fields: await fields(c),
            onSubmit: async (data) => {
                await Api.consommations.update(c.id, data);
                Toast.success('Enregistrement mis à jour');
                await load();
            },
        });
    }

    async function remove(c) {
        if (!confirmDelete(`cet enregistrement (${c.typeConso}, ${c.immatriculation})`)) return;
        try {
            await Api.consommations.remove(c.id);
            Toast.success('Enregistrement supprimé');
            await load();
        } catch (err) { Toast.error(err.message); }
    }

    function row(c) {
        return `<tr>
            <td>${Fmt.plate(c.immatriculation)}</td>
            <td>${badge(c.typeConso)}</td>
            <td class="mono">${Fmt.date(c.dateConso)}</td>
            <td class="mono">${c.quantite} ${c.unite}</td>
            <td class="mono">${Fmt.money(c.coutUnitaire)}</td>
            <td class="mono">${Fmt.money(c.coutTotal)}</td>
            <td>${escapeHtml(c.fournisseur || '—')}</td>
            <td>
                <div class="row-actions">
                    <button class="icon-btn" data-edit="${c.id}" title="Modifier">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H6a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2v-5M18.5 2.5a2.1 2.1 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                    </button>
                    <button class="icon-btn" data-del="${c.id}" title="Supprimer">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M8 6V4a2 2 0 012-2h4a2 2 0 012 2v2m3 0l-1 14a2 2 0 01-2 2H7a2 2 0 01-2-2L4 6h16z"/></svg>
                    </button>
                </div>
            </td>
        </tr>`;
    }

    async function load() {
        const type = document.getElementById('filterConsoType').value;
        cache = await Api.consommations.list(type ? `?type_conso=${encodeURIComponent(type)}` : '');
        const body = document.getElementById('consoBody');
        body.innerHTML = cache.length ? cache.map(row).join('') :
            `<tr><td colspan="8"><div class="empty-state">Aucun enregistrement pour ce filtre.</div></td></tr>`;

        body.querySelectorAll('[data-edit]').forEach(btn =>
            btn.addEventListener('click', () => openEdit(cache.find(c => c.id == btn.dataset.edit))));
        body.querySelectorAll('[data-del]').forEach(btn =>
            btn.addEventListener('click', () => remove(cache.find(c => c.id == btn.dataset.del))));
    }

    function init() {
        document.getElementById('btnAddConso').addEventListener('click', openCreate);
        document.getElementById('filterConsoType').addEventListener('change', load);
    }

    return { init, load };
})();