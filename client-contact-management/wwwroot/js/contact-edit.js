const token = () =>
    document.querySelector('input[name="__RequestVerificationToken"]').value;

function showStatus(message, type = 'success') {
    const el = document.getElementById('statusMessage');
    el.textContent = message;
    el.className = `alert alert-${type} mb-3`;
    setTimeout(() => el.className = 'alert d-none mb-3', 3000);
}

function updateClientCount() {
    const count = document.querySelectorAll('#linkedClientsBody tr').length;
    document.getElementById('clientCount').textContent = count;
}

async function linkClient(clientId, clientName, clientCode) {
    try {
        const form = new FormData();
        form.append('contactId', contactId);
        form.append('clientId', clientId);

        const res = await fetch('/Contact/LinkClient', {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token()
            },
            body: form
        });

        if (!res.ok) throw new Error();

        const tbody = document.getElementById('linkedClientsBody');
        const row = document.createElement('tr');
        row.id = `client-row-${clientId}`;
        row.innerHTML = `
            <td>${clientName}</td>
            <td>${clientCode}</td>
            <td>
                <button class="btn btn-sm btn-outline-danger"
                        onclick="unlinkClient(${clientId}, '${clientName}')">
                    Unlink
                </button>
            </td>`;
        tbody.appendChild(row);

        document.getElementById('linkedClientsTable').classList.remove('d-none');
        const empty = document.getElementById('emptyClients');
        if (empty) empty.classList.add('d-none');

        document.querySelectorAll('#clientList .client-item').forEach(item => {
            if (parseInt(item.dataset.id) === clientId) item.remove();
        });

        if (document.querySelectorAll('#clientList .client-item').length === 0) {
            document.getElementById('clientList').innerHTML =
                '<li class="list-group-item text-muted fst-italic">All clients are already linked.</li>';
            document.getElementById('linkClientBtn').disabled = true;
        }

        updateClientCount();
        showStatus(`"${clientName}" linked successfully.`);
    } catch {
        showStatus('Link failed. Please try again.', 'danger');
    }
}

async function unlinkClient(clientId, clientName) {
    if (!confirm(`Unlink "${clientName}"?`)) return;

    try {
        const form = new FormData();
        form.append('contactId', contactId);
        form.append('clientId', clientId);

        const res = await fetch('/Contact/UnlinkClient', {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token()
            },
            body: form
        });

        if (!res.ok) throw new Error();

        const row = document.getElementById(`client-row-${clientId}`);
        row.style.transition = 'opacity 0.3s';
        row.style.opacity = '0';
        setTimeout(() => {
            row.remove();
            updateClientCount();

            if (document.querySelectorAll('#linkedClientsBody tr').length === 0) {
                document.getElementById('linkedClientsTable').classList.add('d-none');
                const empty = document.getElementById('emptyClients');
                if (empty) empty.classList.remove('d-none');
            }
        }, 300);

        showStatus(`"${clientName}" unlinked successfully.`);
    } catch {
        showStatus('Unlink failed. Please try again.', 'danger');
    }
}

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