const token = () =>
    document.querySelector('input[name="__RequestVerificationToken"]').value;

function showStatus(message, type = 'success') {
    const el = document.getElementById('statusMessage');
    el.textContent = message;
    el.className = `alert alert-${type} mb-3`;
    setTimeout(() => el.className = 'alert d-none mb-3', 3000);
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