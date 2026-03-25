function filterClients(query) {
    const q = query.toLowerCase().trim();
    document.querySelectorAll('#clientList .client-item').forEach(item => {
        item.style.display = item.dataset.search.includes(q) ? '' : 'none';
    });
}

document.getElementById('linkClientModal').addEventListener('hidden.bs.modal', () => {
    const input = document.getElementById('clientSearch');
    if (input) { input.value = ''; filterClients(''); }
});

const params = new URLSearchParams(window.location.search);
if (params.get('tab') === 'clients') {
    const tabEl = document.querySelector('[data-bs-target="#tab-clients"]');
    if (tabEl) bootstrap.Tab.getOrCreateInstance(tabEl).show();
}