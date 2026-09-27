
    let currentSelectedDate = '30/09/2026';
    let currentSelectedTime = '09:30 - 10:00';

    function showNotification(msg) {
      const toast = document.getElementById('toastNotification');
      const toastMsg = document.getElementById('toastMsg');
      if (toast && toastMsg) {
        toastMsg.innerText = msg;
        toast.classList.remove('translate-y-[-20px]', 'opacity-0', 'pointer-events-none');
        toast.classList.add('translate-y-0', 'opacity-100');
        setTimeout(() => {
          toast.classList.remove('translate-y-0', 'opacity-100');
          toast.classList.add('translate-y-[-20px]', 'opacity-0', 'pointer-events-none');
        }, 3200);
      }
    }

    function switchTab(tabKey) {
      const upWrapper = document.getElementById('upcomingTableWrapper');
      const compWrapper = document.getElementById('completedTableWrapper');
      const canWrapper = document.getElementById('cancelledTableWrapper');
      const emptyState = document.getElementById('emptyStateBox');

      const tabUp = document.getElementById('tabUpcoming');
      const tabComp = document.getElementById('tabCompleted');
      const tabCan = document.getElementById('tabCancelled');

      const indUp = document.getElementById('tabUpcomingIndicator');
      const indComp = document.getElementById('tabCompletedIndicator');
      const indCan = document.getElementById('tabCancelledIndicator');

      [upWrapper, compWrapper, canWrapper, emptyState].forEach(el => el && el.classList.add('hidden'));

      // Reset Tab Styles
      [tabUp, tabComp, tabCan].forEach(t => {
        if(t) {
          t.classList.remove('text-primary', 'font-semibold');
          t.classList.add('text-on-surface-variant');
        }
      });
      [indUp, indComp, indCan].forEach(i => {
        if(i) i.classList.replace('bg-primary', 'bg-transparent');
      });

      if (tabKey === 'upcoming') {
        upWrapper.classList.remove('hidden');
        tabUp.classList.add('text-primary', 'font-semibold');
        tabUp.classList.remove('text-on-surface-variant');
        indUp.classList.replace('bg-transparent', 'bg-primary');
      } else if (tabKey === 'completed') {
        compWrapper.classList.remove('hidden');
        tabComp.classList.add('text-primary', 'font-semibold');
        tabComp.classList.remove('text-on-surface-variant');
        indComp.classList.replace('bg-transparent', 'bg-primary');
      } else if (tabKey === 'cancelled') {
        canWrapper.classList.remove('hidden');
        tabCan.classList.add('text-primary', 'font-semibold');
        tabCan.classList.remove('text-on-surface-variant');
        indCan.classList.replace('bg-transparent', 'bg-primary');
      }
    }

    function filterAppointments() {
      const searchVal = document.getElementById('searchInput').value.toLowerCase().trim();
      const specVal = document.getElementById('specialtyFilter').value.toLowerCase();
      const rows = document.querySelectorAll('#upcomingTableWrapper tbody tr');
      let visibleCount = 0;

      rows.forEach(row => {
        const text = row.innerText.toLowerCase();
        const matchesSearch = !searchVal || text.includes(searchVal);
        const matchesSpec = !specVal || text.includes(specVal);

        if (matchesSearch && matchesSpec) {
          row.style.display = '';
          visibleCount++;
        } else {
          row.style.display = 'none';
        }
      });

      const emptyBox = document.getElementById('emptyStateBox');
      const upWrapper = document.getElementById('upcomingTableWrapper');
      if (visibleCount === 0) {
        upWrapper.classList.add('hidden');
        emptyBox.classList.remove('hidden');
      } else {
        upWrapper.classList.remove('hidden');
        emptyBox.classList.add('hidden');
      }
    }

    function resetFilters() {
      document.getElementById('searchInput').value = '';
      document.getElementById('specialtyFilter').value = '';
      filterAppointments();
      showNotification('Đã đặt lại bộ lọc danh sách');
    }

    // Modal 1 Cancel handlers
    function openCancelModal(code, doctor, time) {
      document.getElementById('modalCancelCode').innerText = code;
      document.getElementById('modalCancelDoctor').innerText = doctor;
      document.getElementById('modalCancelTime').innerText = time;
      document.getElementById('cancelReasonInput').value = '';
      document.getElementById('cancelReasonError').classList.add('hidden');

      const modal = document.getElementById('cancelAppointmentModal');
      modal.classList.remove('hidden');
      setTimeout(() => {
        modal.classList.remove('opacity-0');
        modal.children[0].classList.remove('scale-95');
      }, 10);
    }

    function closeCancelModal() {
      const modal = document.getElementById('cancelAppointmentModal');
      modal.classList.add('opacity-0');
      modal.children[0].classList.add('scale-95');
      setTimeout(() => modal.classList.add('hidden'), 250);
    }

    function confirmCancelAppointment() {
      const reason = document.getElementById('cancelReasonInput').value.trim();
      if (!reason) {
        document.getElementById('cancelReasonError').classList.remove('hidden');
        return;
      }
      closeCancelModal();
      showNotification('Lịch khám đã được hủy thành công theo yêu cầu');
    }

    // Modal 2 Reschedule handlers
    function openRescheduleModal(code, doctor, time) {
      document.getElementById('modalRescheduleSubtitle').innerText = `${code} · ${doctor}`;
      const modal = document.getElementById('rescheduleAppointmentModal');
      modal.classList.remove('hidden');
      setTimeout(() => {
        modal.classList.remove('opacity-0');
        modal.children[0].classList.remove('scale-95');
      }, 10);
    }

    function closeRescheduleModal() {
      const modal = document.getElementById('rescheduleAppointmentModal');
      modal.classList.add('opacity-0');
      modal.children[0].classList.add('scale-95');
      setTimeout(() => modal.classList.add('hidden'), 250);
    }

    function selectDate(btn, dateStr) {
      document.querySelectorAll('.reschedule-date-btn').forEach(b => {
        b.className = 'reschedule-date-btn p-2.5 rounded-xl flex flex-col items-center justify-center text-center transition-all bg-surface-container-lowest shadow-sm hover:bg-surface-container';
        b.children[0].className = 'font-label-xs text-label-xs text-on-surface-variant';
        b.children[1].className = 'font-headline-sm text-headline-sm font-bold text-on-surface my-0.5';
        b.children[2].className = 'font-label-xs text-label-xs text-tertiary';
      });

      btn.className = 'reschedule-date-btn p-2.5 rounded-xl flex flex-col items-center justify-center text-center transition-all bg-primary-container text-on-primary shadow-md';
      btn.children[0].className = 'font-label-xs text-label-xs text-on-primary/80';
      btn.children[1].className = 'font-headline-sm text-headline-sm font-bold text-on-primary my-0.5';
      btn.children[2].className = 'font-label-xs text-label-xs text-on-primary font-semibold';

      currentSelectedDate = dateStr;
      updateRescheduleSummary();
    }

    function selectTimeSlot(btn) {
      document.querySelectorAll('.reschedule-time-btn').forEach(b => {
        b.className = 'reschedule-time-btn py-2 px-3 rounded-lg text-center font-label-sm text-label-sm bg-surface-container hover:bg-primary-fixed hover:text-on-primary-fixed transition-colors';
      });
      btn.className = 'reschedule-time-btn py-2 px-3 rounded-lg text-center font-label-sm text-label-sm bg-primary-container text-on-primary shadow-sm font-bold';
      currentSelectedTime = btn.innerText.trim();
      updateRescheduleSummary();
    }

    function updateRescheduleSummary() {
      const el = document.getElementById('selectedSlotSummary');
      if (el) {
        el.innerText = `${currentSelectedDate} (${currentSelectedTime})`;
      }
    }

    function confirmRescheduleAppointment() {
      closeRescheduleModal();
      showNotification(`Lịch khám đã được đổi sang: ${currentSelectedDate} (${currentSelectedTime})`);
    }
  