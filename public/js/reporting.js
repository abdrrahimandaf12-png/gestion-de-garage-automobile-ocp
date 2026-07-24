const ReportingModule = (() => {
    let currentHeaders = [];
    let currentRows = [];
    let currentTitle = '';
    let initialized = false;

    const ETATS = {
        couts: {
            title: 'État des coûts par véhicule',
            headers: ['Véhicule', 'Marque / Modèle', 'Type', 'Consommations (DH)', 'Interventions (DH)', 'Total (DH)'],
            load: async () => {
                const rows = await Api.reporting.coutsParVehicule();
                return rows.map(r => [
                    r.immatriculation, `${r.marque} ${r.modele}`, r.typeVehicule,
                    Fmt.money(r.totalConsommations), Fmt.money(r.totalInterventions), Fmt.money(r.totalGeneral),
                ]);
            },
        },
        conso: {
            title: 'État des consommations mensuelles',
            headers: ['Mois', 'Type', 'Quantité totale (L)', 'Coût total (DH)'],
            load: async () => {
                const rows = await Api.reporting.consommationMensuelle();
                return rows.map(r => [r.mois, r.typeConso, r.quantiteTotale.toFixed(2), Fmt.money(r.coutTotal)]);
            },
        },
        missions: {
            title: 'État des missions',
            headers: ['Véhicule', 'Chauffeur', 'Destination', 'Départ', 'Retour', 'Statut'],
            load: async () => {
                const rows = await Api.missions.list();
                return rows.map(r => [r.immatriculation, r.chauffeur, r.destination, Fmt.date(r.dateDepart), Fmt.date(r.dateRetour), r.statut]);
            },
        },
        interventions: {
            title: 'État des interventions par type',
            headers: ["Type d'intervention", "Nombre d'interventions", 'Coût total (DH)'],
            load: async () => {
                const rows = await Api.reporting.interventionsParType();
                return rows.map(r => [r.typeIntervention, r.n, Fmt.money(r.coutTotal || 0)]);
            },
        },
    };

    function render(headers, rows, title) {
        document.getElementById('etatTitle').textContent = title;
        document.getElementById('etatHead').innerHTML = `<tr>${headers.map(h => `<th>${h}</th>`).join('')}</tr>`;
        document.getElementById('etatBody').innerHTML = rows.length
            ? rows.map(r => `<tr>${r.map(c => `<td>${escapeHtml(c)}</td>`).join('')}</tr>`).join('')
            : `<tr><td colspan="${headers.length}"><div class="empty-state">Aucune donnée disponible pour cet état.</div></td></tr>`;
    }

    async function loadEtat(key) {
        const etat = ETATS[key];
        currentTitle = etat.title;
        currentHeaders = etat.headers;
        currentRows = await etat.load();
        render(currentHeaders, currentRows, currentTitle);
    }

    function exportCsv() {
        if (!currentRows.length) { Toast.error('Aucune donnée à exporter'); return; }
        const lines = [currentHeaders.join(';'), ...currentRows.map(r => r.map(c => `"${String(c).replace(/"/g, '""')}"`).join(';'))];
        const blob = new Blob(['\uFEFF' + lines.join('\n')], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${currentTitle.replace(/\s+/g, '_')}.csv`;
        a.click();
        URL.revokeObjectURL(url);
        Toast.success('Export CSV généré');
    }

    function init() {
        const select = document.getElementById('etatSelect');
        const user = AppState.getUser();
        if (user && user.role === 'mecanicien') {
            select.innerHTML = '<option value="interventions">État des interventions par type</option>';
        } else {
            select.innerHTML = '<option value="couts">État des coûts par véhicule</option><option value="conso">État des consommations mensuelles</option><option value="missions">État des missions</option><option value="interventions">État des interventions par type</option>';
        }
        if (initialized) {
            return;
        }
        select.addEventListener('change', () => loadEtat(select.value));
        document.getElementById('btnExportCsv').addEventListener('click', exportCsv);
        document.getElementById('btnPrintEtat').addEventListener('click', () => window.print());
        initialized = true;
    }

    async function load() {
        const select = document.getElementById('etatSelect');
        await loadEtat(select.value);
    }

    return { init, load };
})();