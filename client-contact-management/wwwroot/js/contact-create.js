const token = () =>
    document.querySelector('input[name="__RequestVerificationToken"]').value;

function showStatus(message, type = 'success') {
    const el = document.getElementById('statusMessage');
    el.textContent = message;
    el.className = `alert alert-${type} mb-3`;
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

    if (!email) {
        emailError.textContent = 'Email address is required.';
        emailError.classList.remove('d-none');
        valid = false;
    } else {
        emailError.classList.add('d-none');
    }

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

        if (res.status === 400) {
            const data = await res.json();
            emailError.textContent = data.error ?? 'Email address is already in use.';
            emailError.classList.remove('d-none');
            return;
        }

        if (!res.ok) throw new Error();

        const { id } = await res.json();
        window.location.href = `/Contact/Edit/${id}`;
    } catch {
        showStatus('Save failed. Please try again.', 'danger');
    }
}