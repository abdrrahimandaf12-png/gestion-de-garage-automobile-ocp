(function () {
    const VIEW_META = {
        dashboard: { title: 'Tableau de bord', subtitle: "Vue d'ensemble de l'activité", load: DashboardModule.load },
        missions: { title: 'Gestion des missions', subtitle: 'Suivi des déplacements et affectations', load: MissionsModule.load },
        parc: { title: 'Parc automobile', subtitle: 'Véhicules, statuts et affectations', load: ParcModule.load },
        consommations: { title: 'Consommations', subtitle: 'Suivi carburant et lubrifiant', load: ConsoModule.load },
        reparations: { title: 'Réparations', subtitle: 'Visites techniques, entretiens et réparations', load: IntervModule.load },
        'demandes-vehicule': { title: 'Demandes véhicule', subtitle: "Demandes d'utilisation de véhicules", load: DemandesModule.load },
        users: { title: 'Gestion des utilisateurs', subtitle: 'Création et gestion des comptes', load: UsersModule.load },
        reporting: { title: 'Reporting & états', subtitle: 'États consolidés, export et impression', load: ReportingModule.load },
    };

    const ROLE_NAV = {
        admin: [
            { view: 'dashboard', label: 'Tableau de bord', icon: 'M3 12h4l3-8 4 16 3-8h4' },
            { view: 'missions', label: 'Missions', icon: 'M9 5H5a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2V9M9 5l4 4m-4-4l4-4m4 12h-4v4' },
            { view: 'parc', label: 'Parc automobile', icon: 'M5 17h14M5 17a2 2 0 104 0M5 17V9l2-4h10l2 4v8M15 17a2 2 0 104 0M3 12h18' },
            { view: 'consommations', label: 'Consommations', icon: 'M3 22h12M6 22V4a1 1 0 011-1h6a1 1 0 011 1v18M13 9h2a2 2 0 012 2v3.5a1.5 1.5 0 003 0V9l-3-3' },
            { view: 'demandes-vehicule', label: 'Demandes', icon: 'M9 5H5a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2V9M9 5l4 4m-4-4l4-4m4 12h-4v4' },
            { view: 'reparations', label: 'Réparations', icon: 'M14.7 6.3a4 4 0 01-5.4 5.4L4 17l3 3 5.3-5.3a4 4 0 015.4-5.4l-2.5 2.5-2-2 2.5-2.5z' },
            { view: 'users', label: 'Utilisateurs', icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z' },
            { view: 'reporting', label: 'Reporting & états', icon: 'M4 19h16M7 19V9m5 10V5m5 14v-7' },
        ],
        mecanicien: [
            { view: 'dashboard', label: 'Tableau de bord atelier', icon: 'M3 12h4l3-8 4 16 3-8h4' },
            { view: 'reparations', label: 'Réparations', icon: 'M14.7 6.3a4 4 0 01-5.4 5.4L4 17l3 3 5.3-5.3a4 4 0 015.4-5.4l-2.5 2.5-2-2 2.5-2.5z' },
            { view: 'parc', label: 'Parc (lecture)', icon: 'M5 17h14M5 17a2 2 0 104 0M5 17V9l2-4h10l2 4v8M15 17a2 2 0 104 0M3 12h18' },
            { view: 'users', label: 'Utilisateurs', icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z' },
            { view: 'reporting', label: 'Rapports atelier', icon: 'M4 19h16M7 19V9m5 10V5m5 14v-7' },
        ],
        user: [
            { view: 'dashboard', label: 'Tableau de bord', icon: 'M3 12h4l3-8 4 16 3-8h4' },
            { view: 'demandes-vehicule', label: 'Mes demandes', icon: 'M9 5H5a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2V9M9 5l4 4m-4-4l4-4m4 12h-4v4' },
            { view: 'parc', label: 'Parc disponible', icon: 'M5 17h14M5 17a2 2 0 104 0M5 17V9l2-4h10l2 4v8M15 17a2 2 0 104 0M3 12h18' },
            { view: 'users', label: 'Utilisateurs', icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z' },
        ],
        chauffeur: [
            { view: 'dashboard', label: 'Tableau de bord', icon: 'M3 12h4l3-8 4 16 3-8h4' },
            { view: 'missions', label: 'Mes missions', icon: 'M9 5H5a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2V9M9 5l4 4m-4-4l4-4m4 12h-4v4' },
            { view: 'demandes-vehicule', label: 'Mes demandes', icon: 'M9 5H5a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2V9M9 5l4 4m-4-4l4-4m4 12h-4v4' },
            { view: 'parc', label: 'Parc disponible', icon: 'M5 17h14M5 17a2 2 0 104 0M5 17V9l2-4h10l2 4v8M15 17a2 2 0 104 0M3 12h18' },
            { view: 'users', label: 'Utilisateurs', icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z' },
        ],
    };

    function renderSidebar(role) {
        const navItems = ROLE_NAV[role] || ROLE_NAV.admin;
        const user = AppState.getUser();
        const list = document.getElementById('navList');
        list.innerHTML = navItems.map(n => `
            <li><button class="nav-link" data-view="${n.view}">
                <svg class="ic" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="${n.icon}"/></svg>
                ${n.label}
            </button></li>`).join('');

        const footer = document.querySelector('.sidebar-footer');
        footer.innerHTML = `<div style="font-size:12px;color:var(--green);font-weight:600;">${user.nom_complet}</div>
            <div style="font-size:11px;color:var(--text-muted);text-transform:capitalize;">Rôle : ${role} · ${user.service || ''}</div>`;
    }

    function renderRoleViews(role) {
        const allViews = ['dashboard', 'missions', 'parc', 'consommations', 'demandes-vehicule', 'reparations', 'users', 'reporting', 'mon-atelier'];
        const allowedViews = new Set(ROLE_NAV[role]?.map(item => item.view) || []);
        allViews.forEach(id => {
            const el = document.getElementById(`view-${id}`);
            if (el) el.classList.toggle('hidden', !allowedViews.has(id));
        });
    }

    function updateClock() {
        const el = document.getElementById('topbarDate');
        if (el) el.textContent = new Date().toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
    }

    async function showView(name) {
        document.querySelectorAll('.nav-link').forEach(b => b.classList.toggle('active', b.dataset.view === name));
        document.querySelectorAll('.view').forEach(v => v.classList.toggle('active', v.id === `view-${name}`));
        const meta = VIEW_META[name];
        if (meta) {
            document.getElementById('viewTitle').textContent = meta.title;
            document.getElementById('viewSubtitle').textContent = meta.subtitle;
            try {
                await meta.load();
            } catch (err) {
                Toast.error('Erreur de chargement : ' + err.message);
            }
        }
        if (window.innerWidth <= 880) document.getElementById('sidebar').classList.add('closed');
    }

    function initNav() {
        document.querySelectorAll('.nav-link').forEach(btn => {
            btn.addEventListener('click', () => showView(btn.dataset.view));
        });
        const hamburger = document.getElementById('hamburgerBtn');
        if (hamburger) {
            hamburger.addEventListener('click', () => {
                const sidebar = document.getElementById('sidebar');
                sidebar.classList.toggle('closed');
                hamburger.classList.toggle('open');
            });
        }
    }

    async function initLogin() {
        const loginScreen = document.getElementById('loginScreen');
        const appShell = document.getElementById('appShell');
        const loginForm = document.getElementById('loginForm');
        const registerForm = document.getElementById('registerForm');
        const loginErrorEl = document.getElementById('loginError');
        const registerErrorEl = document.getElementById('registerError');
        const loginTab = document.getElementById('loginTab');
        const registerTab = document.getElementById('registerTab');
        const loginSubmitBtn = loginForm.querySelector('button[type="submit"]');
        const registerSubmitBtn = registerForm.querySelector('button[type="submit"]');

        function showAuthTab(tab) {
            const isLogin = tab === 'login';
            loginTab.classList.toggle('active', isLogin);
            registerTab.classList.toggle('active', !isLogin);
            loginForm.classList.toggle('hidden', !isLogin);
            registerForm.classList.toggle('hidden', isLogin);
            loginErrorEl.textContent = '';
            registerErrorEl.textContent = '';
        }

        loginTab.addEventListener('click', () => showAuthTab('login'));
        registerTab.addEventListener('click', () => showAuthTab('register'));

        loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const username = document.getElementById('loginUser').value.trim();
            const password = document.getElementById('loginPass').value;
            if (!username || !password) { loginErrorEl.textContent = 'Veuillez saisir vos identifiants.'; return; }

            loginSubmitBtn.disabled = true;
            loginSubmitBtn.textContent = 'Connexion...';
            loginErrorEl.textContent = '';

            try {
                const result = await Api.auth.login(username, password);
                AppState.setSession(result.token, result.user);
                loginScreen.classList.add('hidden');
                appShell.classList.remove('hidden');
                bootApp(result.user.role);
            } catch (err) {
                loginErrorEl.textContent = err.message || 'Échec de la connexion. Vérifiez vos identifiants.';
                loginSubmitBtn.disabled = false;
                loginSubmitBtn.textContent = 'Se connecter';
            }
        });

        registerForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const username = document.getElementById('registerUser').value.trim();
            const fullName = document.getElementById('registerFullName').value.trim();
            const password = document.getElementById('registerPass').value;
            const passwordConfirm = document.getElementById('registerPassConfirm').value;

            if (!username || !fullName || !password || !passwordConfirm) {
                registerErrorEl.textContent = 'Veuillez compléter tous les champs.';
                return;
            }
            if (password !== passwordConfirm) {
                registerErrorEl.textContent = 'Les mots de passe ne correspondent pas.';
                return;
            }
            if (password.length < 6) {
                registerErrorEl.textContent = 'Le mot de passe doit contenir au moins 6 caractères.';
                return;
            }

            registerSubmitBtn.disabled = true;
            registerSubmitBtn.textContent = 'Création...';
            registerErrorEl.textContent = '';

            try {
                await Api.auth.register(username, fullName, password);
                registerSubmitBtn.textContent = 'Compte créé';
                setTimeout(() => {
                    registerForm.reset();
                    registerSubmitBtn.disabled = false;
                    registerSubmitBtn.textContent = 'Créer le compte';
                    showAuthTab('login');
                    loginErrorEl.textContent = 'Compte créé avec succès. Vous pouvez vous connecter.';
                }, 900);
            } catch (err) {
                registerErrorEl.textContent = err.message || 'Échec de la création de compte.';
                registerSubmitBtn.disabled = false;
                registerSubmitBtn.textContent = 'Créer le compte';
            }
        });

        document.getElementById('logoutBtn').addEventListener('click', () => {
            AppState.logout();
            appShell.classList.add('hidden');
            loginScreen.classList.remove('hidden');
            document.getElementById('loginForm').reset();
            loginSubmitBtn.disabled = false;
            loginSubmitBtn.textContent = 'Se connecter';
        });

        // Session en mémoire uniquement : ne pas auto-login après rechargement
    }

    function bootApp(role) {
        updateClock();
        renderSidebar(role);
        renderRoleViews(role);
        const sidebar = document.getElementById('sidebar');
        if (window.innerWidth <= 880) sidebar.classList.add('closed');
        initNav();

        // N'initialiser que les modules accessibles
        if (role === 'admin') {
            ParcModule.init();
            MissionsModule.init();
            ConsoModule.init();
            IntervModule.init();
            DemandesModule.init();
            ReportingModule.init();
        } else if (role === 'mecanicien') {
            IntervModule.init();
            ParcModule.initReadOnly();
            ReportingModule.init();
            DashboardModule.setRole('mecanicien');
        } else if (role === 'chauffeur') {
            ParcModule.initReadOnly();
            DemandesModule.init();
            MissionsModule.initReadOnly();
            DashboardModule.setRole('chauffeur');
        } else {
            DemandesModule.init();
            ParcModule.initReadOnly();
            DashboardModule.setRole('user');
        }
        UsersModule.init();

        showView('dashboard');
    }

    document.addEventListener('DOMContentLoaded', () => {
        initLogin();
    });
})();
