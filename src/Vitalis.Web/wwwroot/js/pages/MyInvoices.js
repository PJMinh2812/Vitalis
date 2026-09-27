
  // Simple interactive mock drawer update logic
  function openInvoiceDrawer(invoiceId) {
    const drawer = document.getElementById('invoiceDrawer');
    if (!drawer) return;

    // Remove active styles on all rows
    document.querySelectorAll('tbody tr').forEach(tr => {
      tr.classList.remove('bg-surface-container-high/40');
    });

    // Add active styling to target row
    const targetRow = document.getElementById('row-' + invoiceId);
    if (targetRow) {
      targetRow.classList.add('bg-surface-container-high/40');
    }

    // Scroll slightly to view drawer cleanly on mobile
    if (window.innerWidth < 1280) {
      drawer.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
  }

  function closeDrawer() {
    const drawer = document.getElementById('invoiceDrawer');
    if (drawer) {
      drawer.classList.add('hidden');
    }
  }

  // Filter interaction
  document.getElementById('invoiceSearchInput')?.addEventListener('input', function(e) {
    const query = e.target.value.toLowerCase();
    const rows = document.querySelectorAll('tbody tr');
    rows.forEach(row => {
      const text = row.innerText.toLowerCase();
      row.style.display = text.includes(query) ? '' : 'none';
    });
  });
