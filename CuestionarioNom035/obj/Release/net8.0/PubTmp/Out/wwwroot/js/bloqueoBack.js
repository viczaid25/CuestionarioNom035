// Evita volver con history
history.pushState(null, document.title, location.href);
window.addEventListener('popstate', function () {
    history.pushState(null, document.title, location.href);
});
document.getElementById('formPregunta')?.addEventListener('submit', function () {
    const btn = document.getElementById('btnContinuar');
    if (btn) { btn.disabled = true; btn.innerText = 'Enviando...'; }
});
