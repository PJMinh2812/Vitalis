
    // Audio Announcement Simulation
    function callAudible(ticketNo, patientName, roomNo) {
      const toast = document.getElementById('toastNotification');
      const title = document.getElementById('toastTitle');
      const desc = document.getElementById('toastDesc');

      title.textContent = `Phát loa: Mời số #${ticketNo}`;
      desc.textContent = `Bệnh nhân ${patientName} vui lòng tới Phòng ${roomNo}`;

      toast.classList.remove('translate-y-[-100%]', 'opacity-0', 'pointer-events-none');
      toast.classList.add('translate-y-0', 'opacity-100');

      setTimeout(() => {
        toast.classList.remove('translate-y-0', 'opacity-100');
        toast.classList.add('translate-y-[-100%]', 'opacity-0', 'pointer-events-none');
      }, 4000);
    }

    // Modal logic for moving patient to end of queue
    let currentPatientToMove = '';
    function openModalMove(patientIdentifier) {
      currentPatientToMove = patientIdentifier;
      document.getElementById('modalPatientName').textContent = patientIdentifier;
      const modal = document.getElementById('modalConfirmMove');
      modal.classList.remove('hidden');
    }

    function closeModalMove() {
      const modal = document.getElementById('modalConfirmMove');
      modal.classList.add('hidden');
    }

    function confirmMoveEndAction() {
      closeModalMove();
      const toast = document.getElementById('toastNotification');
      const title = document.getElementById('toastTitle');
      const desc = document.getElementById('toastDesc');

      title.textContent = "Đã chuyển về cuối hàng chờ";
      desc.textContent = `${currentPatientToMove} đã được cập nhật số thứ tự mới`;

      toast.classList.remove('translate-y-[-100%]', 'opacity-0', 'pointer-events-none');
      toast.classList.add('translate-y-0', 'opacity-100');

      setTimeout(() => {
        toast.classList.remove('translate-y-0', 'opacity-100');
        toast.classList.add('translate-y-[-100%]', 'opacity-0', 'pointer-events-none');
      }, 3000);
    }

    // Quick issue ticket microinteraction
    function quickIssueTicket() {
      const promptName = prompt("Nhập họ tên bệnh nhân cấp số vãng lai:", "Nguyễn Hoàng Nam");
      if (promptName) {
        alert(`Đã cấp thành công số khám mới: #12 cho ${promptName} tại Phòng 102 - Nội Tổng quát. Vui lòng in phiếu.`);
      }
    }

    // Manual Refresh Trigger
    function triggerImmediateRefresh() {
      const d = new Date();
      const timeStr = d.toTimeString().split(' ')[0];
      document.getElementById('lastUpdatedTimer').textContent = timeStr;
      
      const toast = document.getElementById('toastNotification');
      const title = document.getElementById('toastTitle');
      const desc = document.getElementById('toastDesc');
      title.textContent = "Đã đồng bộ thời gian thực";
      desc.textContent = `Dữ liệu hàng chờ cập nhật lúc ${timeStr}`;

      toast.classList.remove('translate-y-[-100%]', 'opacity-0', 'pointer-events-none');
      toast.classList.add('translate-y-0', 'opacity-100');

      setTimeout(() => {
        toast.classList.remove('translate-y-0', 'opacity-100');
        toast.classList.add('translate-y-[-100%]', 'opacity-0', 'pointer-events-none');
      }, 2500);
    }

    // Smooth scroll down to Public TV display preview
    function scrollToTVPreview() {
      const tvSection = document.getElementById('tvPreviewContainer');
      if (tvSection) {
        tvSection.scrollIntoView({ behavior: 'smooth' });
      }
    }

    // Launch Fullscreen TV Mode
    function launchFullscreenTV() {
      const tvEl = document.getElementById('tvPreviewContainer');
      if (tvEl.requestFullscreen) {
        tvEl.requestFullscreen();
      } else if (tvEl.webkitRequestFullscreen) {
        tvEl.webkitRequestFullscreen();
      }
    }

    // Table quick search filter
    function filterQueueTable() {
      const input = document.getElementById("queueSearchInput");
      const filter = input.value.toLowerCase();
      const table = document.getElementById("mainQueueTable");
      const tr = table.getElementsByTagName("tr");

      for (let i = 1; i < tr.length; i++) {
        const textContent = tr[i].textContent || tr[i].innerText;
        if (textContent.toLowerCase().indexOf(filter) > -1) {
          tr[i].style.display = "";
        } else {
          tr[i].style.display = "none";
        }
      }
    }

    // Simple context menu toggle mock
    function toggleActionMenu(btn) {
      alert("Tùy chọn bổ sung:\n1. Bỏ lượt khám (Vắng mặt 3 lần)\n2. Hủy số tiếp nhận\n3. Chuyển phòng khám khác");
    }

    // TV Live Clock updater
    setInterval(() => {
      const now = new Date();
      const hours = String(now.getHours()).padStart(2, '0');
      const mins = String(now.getMinutes()).padStart(2, '0');
      const tvClock = document.getElementById('tvClock');
      if (tvClock) {
        tvClock.textContent = `${hours}:${mins}`;
      }
    }, 1000);
  