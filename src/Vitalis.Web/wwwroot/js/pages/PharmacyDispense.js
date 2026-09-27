
    function toggleScanRow(rowId) {
      const el = document.getElementById(rowId);
      if (!el) return;
      const badge = el.querySelector('.scan-status-badge');
      const icon = el.querySelector('.scan-status-icon');
      const text = el.querySelector('.scan-status-text');
      const inputVal = el.querySelector('.qty-picked');
      
      if (badge.dataset.status === 'pending') {
        badge.dataset.status = 'verified';
        badge.className = 'scan-status-badge inline-flex items-center gap-1 px-space-xs py-0.5 rounded-full bg-surface-container-high text-primary font-label-xs text-[11px]';
        icon.innerText = 'check_circle';
        text.innerText = 'Đã khớp vạch';
        if (inputVal) inputVal.value = inputVal.dataset.max;
      } else {
        badge.dataset.status = 'pending';
        badge.className = 'scan-status-badge inline-flex items-center gap-1 px-space-xs py-0.5 rounded-full bg-surface-container-highest text-tertiary font-label-xs text-[11px]';
        icon.innerText = 'hourglass_empty';
        text.innerText = 'Chờ quét mã';
      }
      updateCheckCount();
    }

    function scanAllItems() {
      ['item-row-3', 'item-row-4'].forEach(id => {
        const el = document.getElementById(id);
        if (el) {
          const badge = el.querySelector('.scan-status-badge');
          if (badge && badge.dataset.status === 'pending') {
            toggleScanRow(id);
          }
        }
      });
    }

    function updateCheckCount() {
      const allVerified = document.querySelectorAll('[data-status="verified"]').length;
      const countEl = document.getElementById('verified-count-num');
      if (countEl) countEl.innerText = allVerified;
    }
  