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