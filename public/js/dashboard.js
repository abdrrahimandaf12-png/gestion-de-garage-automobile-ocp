// Module : Tableau de bord (vue d'ensemble)
const DashboardModule = (() => {
    let charts = {};
    let currentRole = 'admin';

    function setRole(role) { currentRole = role; }

    function kpiCard({ label, value, sub, accent }) {
        return `<div class="kpi-card ${accent ? 'accent-' + accent : ''}">
            <div class="kpi-label">${label}</div>
            <div class="kpi-value">${value}</div>
            ${sub ? `<div class="kpi-sub">${sub}</div>` : ''}
        </div>`;
    }

    function renderKpis(k) {
        let cards;
        if (currentRole === 'mecanicien') {
            cards = [
                kpiCard({ label: 'Parc total', value: k.totalVehicules, sub: `${k.enReparation} en réparation`, accent: 'blue' }),
                kpiCard({ label: 'Interventions en cours', value: k.interventionsEnCours, sub: `${k.interventionsMois} ce mois-ci` }),
                kpiCard({ label: 'Coût interventions (mois)', value: Fmt.money(k.coutInterventionsMois), accent: 'ok' }),
                kpiCard({ label: 'Échéances (30 j)', value: k.echeancesProches, sub: 'Visites & entretiens', accent: k.echeancesProches > 0 ? 'danger' : 'ok' }),
            ];
        } else if (currentRole === 'chauffeur') {
            cards = [
                kpiCard({ label: 'Missions en cours', value: k.missionsEnCours, sub: `${k.missionsMois} ce mois-ci`, accent: 'blue' }),
                kpiCard({ label: 'Véhicules disponibles', value: k.vehiculesDisponibles, sub: `sur ${k.totalVehicules} au total` }),
            ];
        } else if (currentRole === 'user') {
            cards = [
                kpiCard({ label: 'Véhicules disponibles', value: k.vehiculesDisponibles, sub: `sur ${k.totalVehicules} au total` }),
                kpiCard({ label: 'Mes demandes', value: k.mesDemandes, sub: `${k.demandesApprouvees} approuvée(s)` }),
            ];
        } else {
            const disponibles = (k.vehiculesParStatut.find(s => s.statut === 'Disponible') || {}).n || 0;
            cards = [
                kpiCard({ label: 'Parc automobile', value: k.totalVehicules, sub: `${disponibles} disponible(s)`, accent: 'blue' }),
                kpiCard({ label: 'Missions en cours', value: k.missionsEnCours, sub: `${k.missionsMois} ce mois-ci` }),
                kpiCard({ label: 'Coûts du mois', value: Fmt.money(k.coutTotalMois), sub: 'Carburant + lubrifiant + interventions', accent: 'ok' }),
                kpiCard({ label: 'Échéances (30 j)', value: k.echeancesProches, sub: 'Visites techniques & entretiens', accent: k.echeancesProches > 0 ? 'danger' : 'ok' }),
            ];
        }
        document.getElementById('kpiRow').innerHTML = cards.join('');
    }

    function destroyChart(key) { if (charts[key]) { charts[key].destroy(); delete charts[key]; } }

    function renderConsoMensuelle(rows) {
        const mois = [...new Set(rows.map(r => r.mois))].sort();
        const carburant = mois.map(m => (rows.find(r => r.mois === m && r.typeConso === 'Carburant') || {}).coutTotal || 0);
        const lubrifiant = mois.map(m => (rows.find(r => r.mois === m && r.typeConso === 'Lubrifiant') || {}).coutTotal || 0);
        destroyChart('conso');
        charts.conso = new Chart(document.getElementById('chartConsoMensuelle'), {
            type: 'bar',
            data: {
                labels: mois,
                datasets: [
                    { label: 'Carburant (DH)', data: carburant, backgroundColor: '#f2a93b', borderRadius: 4 },
                    { label: 'Lubrifiant (DH)', data: lubrifiant, backgroundColor: '#2f6f8f', borderRadius: 4 },
                ],
            },
            options: {
                responsive: true,
                plugins: { legend: { position: 'bottom', labels: { boxWidth: 10, font: { size: 11 } } } },
                scales: { y: { beginAtZero: true, grid: { color: '#eef0f1' } }, x: { grid: { display: false } } },
            },
        });
    }

    function renderParcStatut(rows) {
        const labels = rows.map(r => r.statut);
        const colors = { 'Disponible': '#2f9e6b', 'En mission': '#2f6f8f', 'En réparation': '#d6871a', 'Hors service': '#c94a3d' };
        destroyChart('parc');
        charts.parc = new Chart(document.getElementById('chartParcStatut'), {
            type: 'doughnut',
            data: { labels, datasets: [{ data: rows.map(r => r.n), backgroundColor: labels.map(l => colors[l] || '#6c7a89') }] },
            options: { responsive: true, cutout: '65%', plugins: { legend: { position: 'bottom', labels: { boxWidth: 10, font: { size: 11 } } } } },
        });
    }

    function renderMissionsStatut(rows) {
        const labels = rows.map(r => r.statut);
        const colors = { 'Planifiée': '#6c7a89', 'En cours': '#2f6f8f', 'Terminée': '#2f9e6b', 'Annulée': '#c94a3d' };
        destroyChart('missions');
        charts.missions = new Chart(document.getElementById('chartMissionsStatut'), {
            type: 'doughnut',
            data: { labels, datasets: [{ data: rows.map(r => r.n), backgroundColor: labels.map(l => colors[l] || '#6c7a89') }] },
            options: { responsive: true, cutout: '65%', plugins: { legend: { position: 'bottom', labels: { boxWidth: 10, font: { size: 11 } } } } },
        });
    }

    function renderEcheances(rows) {
        const el = document.getElementById('echeancesList');
        if (!rows.length) {
            el.innerHTML = `<div class="empty-state">Aucune échéance dans les 30 prochains jours.</div>`;
            return;
        }
        el.innerHTML = rows.map(r => `
            <div class="alert-item">
                <div><span class="veh">${escapeHtml(r.immatriculation)}</span> — ${escapeHtml(r.typeIntervention)}</div>
                <div class="date">${Fmt.date(r.dateProchaineEcheance)}</div>
            </div>`).join('');
    }

    function renderVillesStats(data) {
        const tbody = document.getElementById('villesStatsBody');
        if (!data.villes || !data.villes.length) {
            tbody.innerHTML = '<tr><td colspan="4" class="empty-state">Aucune donnée par ville.</td></tr>';
            return;
        }
        tbody.innerHTML = data.villes.map(v => `
            <tr>
                <td><strong>${escapeHtml(v.nom)}</strong></td>
                <td>${v.missions}</td>
                <td>${badge(v.missionsEnCours > 0 ? 'En cours' : 'Terminée')}</td>
                <td>${v.demandes}</td>
            </tr>`).join('');
    }

    async function load() {
        const kpis = await Api.reporting.kpis();
        renderKpis(kpis);
        renderParcStatut(kpis.vehiculesParStatut);

        if (currentRole === 'admin') {
            try {
                const [conso, missionsStatut, echeances, villesStats] = await Promise.all([
                    Api.reporting.consommationMensuelle(),
                    Api.reporting.missionsParStatut(),
                    Api.interventions.echeances(),
                    Api.reporting.villesStats(),
                ]);
                renderConsoMensuelle(conso);
                renderMissionsStatut(missionsStatut);
                renderEcheances(echeances);
                renderVillesStats(villesStats);
            } catch (_) { /* certaines routes peuvent être interdites */ }
        } else if (currentRole === 'mecanicien') {
            try {
                const echeances = await Api.interventions.echeances();
                renderEcheances(echeances);
            } catch (_) {}
            document.getElementById('chartConsoMensuelle') && (document.getElementById('chartConsoMensuelle').closest('.panel').style.display = 'none');
            document.getElementById('chartMissionsStatut') && (document.getElementById('chartMissionsStatut').closest('.panel').style.display = 'none');
        } else if (currentRole === 'chauffeur') {
            document.querySelectorAll('.charts-grid').forEach(el => el.style.display = 'none');
            document.getElementById('echeancesList') && (document.getElementById('echeancesList').closest('.panel').style.display = 'none');
        } else {
            document.querySelectorAll('.charts-grid').forEach(el => el.style.display = 'none');
            document.getElementById('echeancesList') && (document.getElementById('echeancesList').closest('.panel').style.display = 'none');
        }
    }

    return { load, setRole };
})();
