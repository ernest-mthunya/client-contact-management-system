const token = () =>
    document.querySelector('input[name="__RequestVerificationToken"]').value;

function showStatus(message, type = 'success') {
    const el = document.getElementById('statusMessage');
    el.textContent = message;
    el.className = `alert alert-${type} mb-3`;
    setTimeout(() => el.className = 'alert d-none mb-3', 3000);
}

function updateContactCount() {
    const count = document.querySelectorAll('#linkedContactsBody tr').length;
    document.getElementById('contactCount').textContent = count;
}

async function linkContact(contactId, contactName, contactEmail) {
    try {
        const form = new FormData();
        form.append('clientId', clientId);
        form.append('contactId', contactId);

        const res = await fetch('/Client/LinkContact', {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token()
            },
            body: form
        });

        if (!res.ok) throw new Error();

        const tbody = document.getElementById('linkedContactsBody');
        const row = document.createElement('tr');
        row.id = `contact-row-${contactId}`;
        row.innerHTML = `
            <td>${contactName}</td>
            <td>${contactEmail}</td>
            <td>
                <button class="btn btn-sm btn-outline-danger"
                        onclick="unlinkContact(${contactId}, '${contactName}')">
                    Unlink
                </button>
            </td>`;
        tbody.appendChild(row);

        document.getElementById('linkedContactsTable').classList.remove('d-none');
        const empty = document.getElementById('emptyContacts');
        if (empty) empty.classList.add('d-none');

        document.querySelectorAll('#contactList .contact-item').forEach(item => {
            if (parseInt(item.dataset.id) === contactId) item.remove();
        });

        if (document.querySelectorAll('#contactList .contact-item').length === 0) {
            document.getElementById('contactList').innerHTML =
                '<li class="list-group-item text-muted fst-italic">All contacts are already linked.</li>';
            document.getElementById('linkContactBtn').disabled = true;
        }

        updateContactCount();
        showStatus(`"${contactName}" linked successfully.`);
    } catch {
        showStatus('Link failed. Please try again.', 'danger');
    }
}

async function unlinkContact(contactId, contactName) {
    if (!confirm(`Unlink "${contactName}"?`)) return;

    try {
        const form = new FormData();
        form.append('clientId', clientId);
        form.append('contactId', contactId);

        const res = await fetch('/Client/UnlinkContact', {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token()
            },
            body: form
        });

        if (!res.ok) throw new Error();

        const row = document.getElementById(`contact-row-${contactId}`);
        row.style.transition = 'opacity 0.3s';
        row.style.opacity = '0';
        setTimeout(() => {
            row.remove();
            updateContactCount();

            if (document.querySelectorAll('#linkedContactsBody tr').length === 0) {
                document.getElementById('linkedContactsTable').classList.add('d-none');
                const empty = document.getElementById('emptyContacts');
                if (empty) empty.classList.remove('d-none');
            }
        }, 300);

        showStatus(`"${contactName}" unlinked successfully.`);
    } catch {
        showStatus('Unlink failed. Please try again.', 'danger');
    }
}

function filterContacts(query) {
    const q = query.toLowerCase().trim();
    document.querySelectorAll('#contactList .contact-item').forEach(item => {
        item.style.display = item.dataset.search.includes(q) ? '' : 'none';
    });
}

document.getElementById('linkContactModal').addEventListener('hidden.bs.modal', () => {
    const input = document.getElementById('contactSearch');
    if (input) { input.value = ''; filterContacts(''); }
});

const params = new URLSearchParams(window.location.search);
if (params.get('tab') === 'contacts') {
    const tabEl = document.querySelector('[data-bs-target="#tab-contacts"]');
    if (tabEl) bootstrap.Tab.getOrCreateInstance(tabEl).show();
}