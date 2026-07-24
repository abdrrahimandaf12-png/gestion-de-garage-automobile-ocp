const AppState = (() => {
    let currentUser = null;
    let currentToken = null;

    function setSession(token, user) {
        currentToken = token;
        currentUser = user;
    }

    function clearSession() {
        currentToken = null;
        currentUser = null;
    }

    function getUser() { return currentUser; }
    function getToken() { return currentToken; }
    function isAdmin() { const u = getUser(); return u && u.role === 'admin'; }
    function isMecanicien() { const u = getUser(); return u && u.role === 'mecanicien'; }
    function isUser() { const u = getUser(); return u && u.role === 'user'; }
    function isChauffeur() { const u = getUser(); return u && u.role === 'chauffeur'; }
    function logout() { clearSession(); }

    return { setSession, getUser, getToken, isAdmin, isMecanicien, isUser, isChauffeur, logout };
})();
