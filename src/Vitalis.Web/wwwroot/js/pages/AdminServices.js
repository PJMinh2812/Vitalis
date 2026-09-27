
  // Simple micro-interaction for Digital Signature approval action
  const signBtn = document.getElementById('btnSignApprove');
  if (signBtn) {
    signBtn.addEventListener('click', () => {
      const originalText = signBtn.innerHTML;
      signBtn.disabled = true;
      signBtn.innerHTML = `
        <span class="material-symbols-outlined animate-spin text-[18px]">sync</span>
        Đang xác thực Token Chữ ký số Viện Trưởng...
      `;
      setTimeout(() => {
        signBtn.innerHTML = `
          <span class="material-symbols-outlined text-[18px]">done_all</span>
          Đã Ký Số Ban Hành Thành Công (HS-2026.4)
        `;
        signBtn.classList.remove('bg-primary-container');
        signBtn.classList.add('bg-primary');
      }, 1200);
    });
  }

  // Quick filter interactive search simulation
  const searchInput = document.getElementById('serviceSearch');
  if (searchInput) {
    searchInput.addEventListener('input', (e) => {
      const val = e.target.value.toLowerCase();
      const rows = document.querySelectorAll('tbody tr');
      rows.forEach(row => {
        const text = row.innerText.toLowerCase();
        row.style.display = text.includes(val) ? '' : 'none';
      });
    });
  }
