
    (function initDoctorQueueLive() {
      // Live clock ticker
      const timerEl = document.getElementById('live-timer');
      if (timerEl) {
        setInterval(() => {
          const now = new Date();
          const h = String(now.getHours()).padStart(2, '0');
          const m = String(now.getMinutes()).padStart(2, '0');
          const s = String(now.getSeconds()).padStart(2, '0');
          timerEl.textContent = `${h}:${m}:${s}`;
        }, 1000);
      }

      // Hotkey F2 listener
      window.addEventListener('keydown', (e) => {
        if (e.key === 'F2') {
          e.preventDefault();
          const callBtn = document.getElementById('btn-call-next');
          if (callBtn) callBtn.click();
        }
      });

      // Call next button trigger animation & notification
      const callBtn = document.getElementById('btn-call-next');
      if (callBtn) {
        callBtn.addEventListener('click', () => {
          const originalContent = callBtn.innerHTML;
          callBtn.innerHTML = `
            <span class="material-symbols-outlined text-[20px] animate-bounce">campaign</span>
            <span>Đang phát loa gọi: STT #05...</span>
          `;
          callBtn.classList.remove('bg-primary-container');
          callBtn.classList.add('bg-secondary-container');

          setTimeout(() => {
            callBtn.innerHTML = originalContent;
            callBtn.classList.remove('bg-secondary-container');
            callBtn.classList.add('bg-primary-container');
          }, 2400);
        });
      }
    })();
  