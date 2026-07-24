const DemandesModule = (() => {
    let cache = [];

    async function vehiculeOptions() {
        const vehicules = await Api.vehicules.list();
        return vehicules.map(v => ({ value: v.id, label: `${v.immatriculation} — ${v.marque} ${v.modele}` }));
    }

    function isAdmin() { return AppState.isAdmin(); }
    function isChauffeur() { const u = AppState.getUser(); return u && u.role === 'chauffeur'; }
    function isRegularUser() { const u = AppState.getUser(); return u && (u.role === 'user' || u.role === 'chauffeur'); }

    async function fields(d = {}) {
        const options = await vehiculeOptions();
        options.unshift({ value: '', label: '— Non attribué —' });
        const user = AppState.getUser();
        return [
            { name: 'employeNom', label: "Nom de l'employé", required: true, value: d.employeNom || (isRegularUser() ? user.nomComplet : ''), span2: true },
            { name: 'service', label: 'Service', type: 'select', required: true, value: d.service || (isRegularUser() ? user.service || '' : ''),
              options: ['Exploitation', 'Logistique', 'Maintenance', 'Sécurité', 'Direction', 'Extraction'] },
            { name: 'destination', label: 'Destination', required: true, value: d.destination, span2: true },
            { name: 'motif', label: 'Motif', type: 'textarea', value: d.motif, span2: true },
            { name: 'dateDemande', label: 'Date de demande', type: 'date', value: d.dateDemande || new Date().toISOString().slice(0, 10) },
            { name: 'dateDepart', label: 'Date départ', type: 'date', required: true, value: d.dateDepart },
            { name: 'dateRetourPrevu', label: 'Retour prévu', type: 'date', value: d.dateRetourPrevu },
            { name: 'vehiculeId', label: 'Véhicule', type: 'select', value: d.vehiculeId || '', options },
            ...(isAdmin() ? [{ name: 'statut', label: 'Statut', type: 'select', value: d.statut || 'En attente',
              options: ['En attente', 'Approuvée', 'Refusée', 'Annulée'] }] : []),
        ];
    }

    function actionButtons(d) {
        const user = AppState.getUser();
        if (isAdmin()) {
            return `<div class="row-actions">
                <button class="icon-btn" data-edit="${d.id}" title="Modifier">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H6a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2v-5M18.5 2.5a2.1 2.1 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                </button>
                <button class="icon-btn" data-del="${d.id}" title="Supprimer">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M8 6V4a2 2 0 012-2h4a2 2 0 012 2v2m3 0l-1 14a2 2 0 01-2 2H7a2 2 0 01-2-2L4 6h16z"/></svg>
                </button>
            </div>`;
        }
        if (isChauffeur() && d.statut === 'En attente') {
            return `<div class="row-actions">
                <button class="icon-btn" data-accept="${d.id}" title="Accepter">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 13l4 4L19 7"/></svg>
                </button>
                <button class="icon-btn" data-reject="${d.id}" title="Refuser">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 6L6 18M6 6l12 12"/></svg>
                </button>
            </div>`;
        }
        return '';
    }

    async function openCreate() {
        Modal.open({
            title: 'Nouvelle demande de véhicule',
            fields: await fields(),
            onSubmit: async (data) => {
                await Api.demandesVehicule.create(data);
                Toast.success('Demande enregistrée');
                await load();
            },
        });
    }

    function openProcess(d, statut) {
        const fields = [
            { name: 'statut', type: 'hidden', value: statut },
            { name: 'commentaireTraitement', label: 'Commentaire', type: 'textarea', value: d.commentaireTraitement || '', span2: true },
        ];
        if (statut === 'Approuvée') {
            fields.push({ name: '_info', label: '', type: 'textarea', value: 'Une mission sera automatiquement créée avec les informations de la demande.', span2: true });
        }
        Modal.open({
            title: `${statut === 'Approuvée' ? '✅ Accepter' : '❌ Refuser'} la demande`,
            fields,
            submitLabel: statut === 'Approuvée' ? 'Accepter & créer la mission' : 'Refuser la demande',
            onSubmit: async (data) => {
                delete data._info;
                await Api.demandesVehicule.update(d.id, data);
                Toast.success(statut === 'Approuvée'
                    ? 'Demande acceptée — mission créée'
                    : 'Demande refusée');
                await load();
            },
        });
    }

    async function openEdit(d) {
        Modal.open({
            title: `Modifier la demande de ${d.employeNom}`,
            fields: await fields(d),
            onSubmit: async (data) => {
                await Api.demandesVehicule.update(d.id, data);
                Toast.success('Demande mise à jour');
                await load();
            },
        });
    }

    async function remove(d) {
        if (!confirmDelete(`cette demande (${d.employeNom}, ${d.destination})`)) return;
        try {
            await Api.demandesVehicule.remove(d.id);
            Toast.success('Demande supprimée');
            await load();
        } catch (err) { Toast.error(err.message); }
    }

    function row(d) {
        const vehicule = d.immatriculation ? `${d.immatriculation} — ${d.marque} ${d.modele}` : '—';
        const actions = actionButtons(d);
        const showActions = isAdmin() || isChauffeur();
        const missionLink = d.missionId
            ? `<a href="#" class="mission-link" data-mission="${d.missionId}" title="Voir la mission">#${d.missionId}</a>`
            : '—';
        return `<tr>
            <td>${escapeHtml(d.employeNom)}</td>
            <td>${escapeHtml(d.service)}</td>
            <td>${escapeHtml(d.destination)}</td>
            <td>${escapeHtml(d.motif || '—')}</td>
            <td class="mono">${Fmt.date(d.dateDemande)}</td>
            <td class="mono">${Fmt.date(d.dateDepart)}</td>
            <td class="mono">${Fmt.date(d.dateRetourPrevu)}</td>
            <td>${vehicule}</td>
            <td>${badge(d.statut)}</td>
            <td>${escapeHtml(d.chauffeurTraitant || '—')}</td>
            <td class="mono">${Fmt.date(d.dateTraitement)}</td>
            <td>${missionLink}</td>
            ${showActions ? `<td>${actions}</td>` : ''}
        </tr>`;
    }

    async function load() {
        const statut = document.getElementById('filterDemandeStatut').value;
        const qs = statut ? `?statut=${encodeURIComponent(statut)}` : '';
        cache = await Api.demandesVehicule.list(qs);
        const body = document.getElementById('demandeBody');
        const showActions = isAdmin() || isChauffeur();
        const colCount = showActions ? 13 : 12;
        body.innerHTML = cache.length ? cache.map(row).join('') :
            `<tr><td colspan="${colCount}"><div class="empty-state">Aucune demande ne correspond à ces filtres.</div></td></tr>`;

        body.querySelectorAll('[data-edit]').forEach(btn =>
            btn.addEventListener('click', () => openEdit(cache.find(d => d.id == btn.dataset.edit))));
        body.querySelectorAll('[data-del]').forEach(btn =>
            btn.addEventListener('click', () => remove(cache.find(d => d.id == btn.dataset.del))));
        body.querySelectorAll('[data-accept]').forEach(btn =>
            btn.addEventListener('click', () => openProcess(cache.find(d => d.id == btn.dataset.accept), 'Approuvée')));
        body.querySelectorAll('[data-reject]').forEach(btn =>
            btn.addEventListener('click', () => openProcess(cache.find(d => d.id == btn.dataset.reject), 'Refusée')));
        body.querySelectorAll('.mission-link').forEach(a =>
            a.addEventListener('click', (e) => {
                e.preventDefault();
                const view = document.querySelector('[data-view="missions"]');
                if (view) view.click();
            }));
    }

    function init() {
        const addBtn = document.getElementById('btnAddDemande');
        const filter = document.getElementById('filterDemandeStatut');
        const showActions = isAdmin() || isChauffeur();
        const ths = document.querySelectorAll('#view-demandes-vehicule thead th');

        const colVisibility = [true, true, true, true, true, true, true, true, true,
            true, true, true, showActions];
        ths.forEach((th, i) => {
            if (i < colVisibility.length) th.style.display = colVisibility[i] ? '' : 'none';
        });

        if (isChauffeur()) {
            addBtn.style.display = 'none';
        } else {
            addBtn.style.display = '';
            addBtn.addEventListener('click', openCreate);
        }
        document.getElementById('filterDemandeStatut').addEventListener('change', load);
        if (isAdmin()) {
            if (filter) filter.innerHTML = '<option value="">Tous les statuts</option><option>En attente</option><option>Approuvée</option><option>Refusée</option><option>Annulée</option>';
        } else {
            if (filter) filter.innerHTML = '<option value="">Mes demandes</option><option>En attente</option><option>Approuvée</option><option>Refusée</option><option>Annulée</option>';
        }
    }

    return { init, load };
})();