const IntervModule = (() => {
    let cache = [];

    async function vehiculeOptions() {
        const vehicules = await Api.vehicules.list();
        return vehicules.map(v => ({ value: v.id, label: `${v.immatriculation} — ${v.marque} ${v.modele}` }));
    }

    async function fields(i = {}) {
        const options = await vehiculeOptions();
        return [
            { name: 'vehiculeId', label: 'Véhicule', type: 'select', required: true, value: i.vehiculeId, options, span2: true },
            { name: 'typeIntervention', label: "Type d'intervention", type: 'select', required: true, value: i.typeIntervention,
              options: ['Visite technique', 'Réparation', 'Entretien préventif', 'Vidange', 'Pneumatiques'] },
            { name: 'dateIntervention', label: 'Date', type: 'date', required: true, value: i.dateIntervention },
            { name: 'description', label: 'Description', type: 'textarea', value: i.description, span2: true },
            { name: 'prestataire', label: 'Prestataire / atelier', value: i.prestataire },
            { name: 'cout', label: 'Coût (DH)', type: 'number', step: '0.01', value: i.cout },
            { name: 'dateProchaineEcheance', label: 'Prochaine échéance', type: 'date', value: i.dateProchaineEcheance,
              hint: 'Ex. date de renouvellement de la visite technique' },
            { name: 'statut', label: 'Statut', type: 'select', value: i.statut || 'Planifiée',
              options: ['Planifiée', 'En cours', 'Terminée'] },
        ];
    }

    async function openCreate() {
        Modal.open({
            title: 'Nouvelle intervention',
            fields: await fields(),
            onSubmit: async (data) => {
                await Api.interventions.create(data);
                Toast.success('Intervention enregistrée');
                await load();
            },
        });
    }

    async function openEdit(i) {
        Modal.open({
            title: `Modifier l'intervention — ${i.immatriculation}`,
            fields: await fields(i),
            onSubmit: async (data) => {
                await Api.interventions.update(i.id, data);
                Toast.success('Intervention mise à jour');
                await load();
            },
        });
    }

    async function remove(i) {
        if (!confirmDelete(`cette intervention (${i.typeIntervention}, ${i.immatriculation})`)) return;
        try {
            await Api.interventions.remove(i.id);
            Toast.success('Intervention supprimée');
            await load();
        } catch (err) { Toast.error(err.message); }
    }

    function row(i) {
        return `<tr>
            <td>${Fmt.plate(i.immatriculation)}</td>
            <td>${escapeHtml(i.typeIntervention)}</td>
            <td class="mono">${Fmt.date(i.dateIntervention)}</td>
            <td>${escapeHtml(i.description || '—')}</td>
            <td>${escapeHtml(i.prestataire || '—')}</td>
            <td class="mono">${Fmt.money(i.cout)}</td>
            <td class="mono">${Fmt.date(i.dateProchaineEcheance)}</td>
            <td>${badge(i.statut)}</td>
            <td>
                <div class="row-actions">
                    <button class="icon-btn" data-edit="${i.id}" title="Modifier">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H6a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2v-5M18.5 2.5a2.1 2.1 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                    </button>
                    <button class="icon-btn" data-del="${i.id}" title="Supprimer">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M8 6V4a2 2 0 012-2h4a2 2 0 012 2v2m3 0l-1 14a2 2 0 01-2 2H7a2 2 0 01-2-2L4 6h16z"/></svg>
                    </button>
                </div>
            </td>
        </tr>`;
    }

    async function load() {
        const type = document.getElementById('filterIntervType').value;
        const statut = document.getElementById('filterIntervStatut').value;
        const qs = [];
        if (type) qs.push(`type_intervention=${encodeURIComponent(type)}`);
        if (statut) qs.push(`statut=${encodeURIComponent(statut)}`);
        cache = await Api.interventions.list(qs.length ? '?' + qs.join('&') : '');
        const body = document.getElementById('intervBody');
        body.innerHTML = cache.length ? cache.map(row).join('') :
            `<tr><td colspan="9"><div class="empty-state">Aucune intervention ne correspond à ces filtres.</div></td></tr>`;

        body.querySelectorAll('[data-edit]').forEach(btn =>
            btn.addEventListener('click', () => openEdit(cache.find(i => i.id == btn.dataset.edit))));
        body.querySelectorAll('[data-del]').forEach(btn =>
            btn.addEventListener('click', () => remove(cache.find(i => i.id == btn.dataset.del))));
    }

    function init() {
        document.getElementById('btnAddInterv').addEventListener('click', openCreate);
        document.getElementById('filterIntervType').addEventListener('change', load);
        document.getElementById('filterIntervStatut').addEventListener('change', load);
    }

    return { init, load };
})();