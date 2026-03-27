const token = () =>
    document.querySelector('input[name="__RequestVerificationToken"]').value;

function showStatus(message, type = 'success') {
    const el = document.getElementById('statusMessage');
    el.textContent = message;
    el.className = `alert alert-${type} mb-3`;
}

async function createClient() {
    const name = document.getElementById('clientName').value.trim();
    const nameError = document.getElementById('nameError');

    if (!name) {
        nameError.classList.remove('d-none');
        return;
    }
    nameError.classList.add('d-none');

    try {
        const form = new FormData();
        form.append('name', name);

        const res = await fetch('/Client/Create', {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token()
            },
            body: form
        });

        if (!res.ok) throw new Error();

        const { id } = await res.json();
        window.location.href = `/Client/Edit/${id}`;
    } catch {
        showStatus('Save failed. Please try again.', 'danger');
    }
}

async function deleteClient(id, name) {
    if (!confirm(`Delete "${name}"? This cannot be undone.`)) return;

    try {
        const res = await fetch(`/Client/Delete/${id}`, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token()
            }
        });

        if (!res.ok) throw new Error();

        const row = document.getElementById(`row-${id}`);
        row.style.transition = 'opacity 0.3s';
        row.style.opacity = '0';
        setTimeout(() => {
            row.remove();

            if (document.querySelectorAll('#clientTableBody tr').length === 0) {
                document.querySelector('table').remove();
                document.querySelector('.container').innerHTML +=
                    '<p class="text-muted fst-italic">No client(s) found.</p>';
            }
        }, 300);

        showStatus(`"${name}" deleted successfully.`);
    } catch {
        showStatus('Delete failed. Please try again.', 'danger');
    }
}