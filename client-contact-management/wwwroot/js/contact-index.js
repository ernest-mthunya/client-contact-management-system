const token = () =>
    document.querySelector('input[name="__RequestVerificationToken"]').value;

function showStatus(message, type = 'success') {
    const el = document.getElementById('statusMessage');
    el.textContent = message;
    el.className = `alert alert-${type} mb-3`;
    setTimeout(() => el.className = 'alert d-none mb-3', 3000);
}

async function createContact() {
    const name = document.getElementById('contactName').value.trim();
    const surname = document.getElementById('contactSurname').value.trim();
    const email = document.getElementById('contactEmail').value.trim();

    const nameError = document.getElementById('nameError');
    const surnameError = document.getElementById('surnameError');
    const emailError = document.getElementById('emailError');

    let valid = true;

    if (!name) { nameError.classList.remove('d-none'); valid = false; }
    else nameError.classList.add('d-none');

    if (!surname) { surnameError.classList.remove('d-none'); valid = false; }
    else surnameError.classList.add('d-none');

    if (!email) { emailError.classList.remove('d-none'); valid = false; }
    else emailError.classList.add('d-none');

    if (!valid) return;

    try {
        const form = new FormData();
        form.append('name', name);
        form.append('surname', surname);
        form.append('email', email);

        const res = await fetch('/Contact/Create', {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': token()
            },
            body: form
        });

        if (!res.ok) {
            const err = await res.json();
            showStatus(err.error ?? 'Save failed. Please try again.', 'danger');
            return;
        }

        const { id } = await res.json();
        window.location.href = `/Contact/Edit/${id}`;
    } catch {
        showStatus('Save failed. Please try again.', 'danger');
    }
}

async function deleteContact(id, name) {
    if (!confirm(`Delete "${name}"? This cannot be undone.`)) return;

    try {
        const res = await fetch(`/Contact/Delete/${id}`, {
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

            if (document.querySelectorAll('#contactTableBody tr').length === 0) {
                document.querySelector('table').remove();
                document.querySelector('.container').innerHTML +=
                    '<p class="text-muted fst-italic">No contact(s) found.</p>';
            }
        }, 300);

        showStatus(`"${name}" deleted successfully.`);
    } catch {
        showStatus('Delete failed. Please try again.', 'danger');
    }
}