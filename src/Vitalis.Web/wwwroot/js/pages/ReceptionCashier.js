
    function selectPaymentTab(method) {
      // Elements
      const tabVietQR = document.getElementById('tabContentVietQR');
      const tabCash = document.getElementById('tabContentCash');
      const tabPOS = document.getElementById('tabContentPOS');

      const btnVietQR = document.getElementById('tabBtnVietQR');
      const btnCash = document.getElementById('tabBtnCash');
      const btnPOS = document.getElementById('tabBtnPOS');

      // Reset tabs display
      tabVietQR.classList.add('hidden');
      tabCash.classList.add('hidden');
      tabPOS.classList.add('hidden');

      // Reset button styles
      [btnVietQR, btnCash, btnPOS].forEach(btn => {
        btn.classList.remove('bg-surface-container-lowest', 'text-primary', 'shadow-sm');
        btn.classList.add('text-on-surface-variant');
      });

      if (method === 'vietqr') {
        tabVietQR.classList.remove('hidden');
        btnVietQR.classList.add('bg-surface-container-lowest', 'text-primary', 'shadow-sm');
        btnVietQR.classList.remove('text-on-surface-variant');
      } else if (method === 'cash') {
        tabCash.classList.remove('hidden');
        btnCash.classList.add('bg-surface-container-lowest', 'text-primary', 'shadow-sm');
        btnCash.classList.remove('text-on-surface-variant');
      } else if (method === 'pos') {
        tabPOS.classList.remove('hidden');
        btnPOS.classList.add('bg-surface-container-lowest', 'text-primary', 'shadow-sm');
        btnPOS.classList.remove('text-on-surface-variant');
      }
    }

    function calculateChange() {
      const due = 870000;
      const given = parseInt(document.getElementById('cashGivenInput').value) || 0;
      const change = given - due;
      const display = document.getElementById('cashChangeDisplay');
      if (change >= 0) {
        display.innerText = change.toLocaleString('vi-VN') + 'đ';
        display.className = 'font-headline-sm text-headline-sm font-bold text-emerald-700';
      } else {
        display.innerText = 'Thiếu ' + Math.abs(change).toLocaleString('vi-VN') + 'đ';
        display.className = 'font-headline-sm text-headline-sm font-bold text-error';
      }
    }

    function setCash(amount) {
      document.getElementById('cashGivenInput').value = amount;
      calculateChange();
    }

    function confirmPaymentSuccess() {
      document.getElementById('receiptModal').classList.remove('hidden');
    }

    function closeReceiptModal() {
      document.getElementById('receiptModal').classList.add('hidden');
    }
  