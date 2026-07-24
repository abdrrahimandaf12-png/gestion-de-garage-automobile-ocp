const UsersModule = (() => {
    let cache = [];

    function fields(u = {}) {
        const ROLES = ['admin', 'mecanicien', 'user', 'chauffeur'];
        return [
            { name: 'username', label: 'Identifiant', required: true, value: u.username, span2: true },
            { name: 'password', label: 'Mot de passe', type: 'password', required: !u.id, value: '',
              hint: u.id ? 'Laissez vide pour conserver le mot de passe actuel' : 'Minimum 6 caractères' },
            { name: 'nomComplet', label: 'Nom complet', required: true, value: u.nomComplet, span2: true },
            { name: 'role', label: 'Rôle', type: 'select', required: true, value: u.role || 'user', options: ROLES },
            { name: 'service', label: 'Service', type: 'select', value: u.service || '',
              options: ['', 'Direction', 'Exploitation', 'Logistique', 'Maintenance', 'Sécurité', 'Extraction', 'Atelier central', 'Transport'] },
            { name: 'actif', label: 'Actif', type: 'select', value: u.actif ?? 1,
              options: [{ value: 1, label: 'Oui' }, { value: 0, label: 'Non' }] },
        ];
    }

    function openCreate() {
        Modal.open({
            title: 'Nouvel utilisateur',
            fields: fields(),
            submitLabel: 'Créer le compte',
            onSubmit: async (data) => {
                await Api.users.create(data);
                Toast.success('Utilisateur créé');
                await load();
            },
        });
    }

    function openEdit(u) {
        Modal.open({
            title: `Modifier — ${u.username}`,
            fields: fields(u),
            onSubmit: async (data) => {
                if (!data.password) delete data.password;
                await Api.users.update(u.id, data);
                Toast.success('Utilisateur mis à jour');
                await load();
            },
        });
    }

    async function remove(u) {
        if (!confirmDelete(`l'utilisateur ${u.username}`)) return;
        try {
            await Api.users.remove(u.id);
            Toast.success('Utilisateur désactivé');
            await load();
        } catch (err) { Toast.error(err.message); }
    }

    function row(u) {
        return `<tr>
            <td><strong>${escapeHtml(u.username)}</strong></td>
            <td>${escapeHtml(u.nomComplet)}</td>
            <td>${badge(u.role)}</td>
            <td>${escapeHtml(u.service || '—')}</td>
            <td>${u.actif ? '<span style="color:#2f9e6b">●</span> Actif' : '<span style="color:#c94a3d">●</span> Inactif'}</td>
            <td class="mono">${Fmt.date(u.dateCreation)}</td>
            <td>
                <div class="row-actions">
                    <button class="icon-btn" data-edit="${u.id}" title="Modifier">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H6a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2v-5M18.5 2.5a2.1 2.1 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                    </button>
                    <button class="icon-btn" data-del="${u.id}" title="Désactiver">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"/></svg>
                    </button>
                </div>
            </td>
        </tr>`;
    }

    async function load() {
        cache = await Api.users.list();
        const body = document.getElementById('usersBody');
        body.innerHTML = cache.length ? cache.map(row).join('') :
            `<tr><td colspan="7"><div class="empty-state">Aucun utilisateur</div></td></tr>`;

        body.querySelectorAll('[data-edit]').forEach(btn =>
            btn.addEventListener('click', () => openEdit(cache.find(u => u.id == btn.dataset.edit))));
        body.querySelectorAll('[data-del]').forEach(btn =>
            btn.addEventListener('click', () => remove(cache.find(u => u.id == btn.dataset.del))));
    }

    function init() {
        const btn = document.getElementById('btnAddUser');
        if (btn) btn.addEventListener('click', openCreate);
    }

    return { init, load };
})();