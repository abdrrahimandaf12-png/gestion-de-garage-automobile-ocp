const Api = (() => {
    const BASE = '/api';

    function getHeaders() {
        const h = {};
        const token = AppState.getToken();
        if (token) {
            h['Authorization'] = `Bearer ${token}`;
        }
        return h;
    }

    async function request(method, path, body) {
        const opts = { method, headers: getHeaders() };
        if (body !== undefined) {
            opts.headers['Content-Type'] = 'application/json';
            opts.body = JSON.stringify(body);
        }
        const res = await fetch(BASE + path, opts);
        if (res.status === 204) return null;
        const data = await res.json().catch(() => ({}));
        if (!res.ok) throw new Error(data.error || `Erreur ${res.status}`);
        return data;
    }

    const get = (path) => request('GET', path);
    const post = (path, body) => request('POST', path, body);
    const put = (path, body) => request('PUT', path, body);
    const del = (path) => request('DELETE', path);

    return {
        auth: {
            login: (username, password) => post('/auth/login', { username, password }),
            register: (username, fullName, password) => post('/auth/register', { username, nomComplet: fullName, password, service: 'Service' }),
        },
        vehicules: {
            list: (q = '') => get('/vehicules' + q),
            create: (data) => post('/vehicules', data),
            update: (id, data) => put(`/vehicules/${id}`, data),
            remove: (id) => del(`/vehicules/${id}`),
        },
        missions: {
            list: (q = '') => get('/missions' + q),
            create: (data) => post('/missions', data),
            update: (id, data) => put(`/missions/${id}`, data),
            remove: (id) => del(`/missions/${id}`),
        },
        consommations: {
            list: (q = '') => get('/consommations' + q),
            create: (data) => post('/consommations', data),
            update: (id, data) => put(`/consommations/${id}`, data),
            remove: (id) => del(`/consommations/${id}`),
        },
        interventions: {
            list: (q = '') => get('/interventions' + q),
            echeances: () => get('/interventions/echeances/proches'),
            create: (data) => post('/interventions', data),
            update: (id, data) => put(`/interventions/${id}`, data),
            remove: (id) => del(`/interventions/${id}`),
        },
        demandesVehicule: {
            list: (q = '') => get('/demandes-vehicule' + q),
            create: (data) => post('/demandes-vehicule', data),
            update: (id, data) => put(`/demandes-vehicule/${id}`, data),
            remove: (id) => del(`/demandes-vehicule/${id}`),
        },
        reporting: {
            kpis: () => get('/reporting/kpis'),
            coutsParVehicule: () => get('/reporting/couts-par-vehicule'),
            consommationMensuelle: () => get('/reporting/consommation-mensuelle'),
            missionsParStatut: () => get('/reporting/missions-par-statut'),
            interventionsParType: () => get('/reporting/interventions-par-type'),
            villesStats: () => get('/reporting/villes-stats'),
        },
        users: {
            list: () => get('/users'),
            get: (id) => get(`/users/${id}`),
            create: (data) => post('/users', data),
            update: (id, data) => put(`/users/${id}`, data),
            remove: (id) => del(`/users/${id}`),
        },
    };
})();
