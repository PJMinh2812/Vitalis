
  function togglePasswordVisibility() {
    const passInput = document.getElementById('password');
    const eyeIcon = document.getElementById('eyeIcon');
    if (!passInput || !eyeIcon) return;

    if (passInput.type === 'password') {
      passInput.type = 'text';
      eyeIcon.textContent = 'visibility_off';
    } else {
      passInput.type = 'password';
      eyeIcon.textContent = 'visibility';
    }
  }

  function setRole(role) {
    const userInput = document.getElementById('username');
    const passInput = document.getElementById('password');
    if (!userInput || !passInput) return;

    const roleMap = {
      receptionist: { user: 'letan.hoangmai@vitalis.vn', pass: 'Reception2025#' },
      doctor: { user: 'bs.minhtri@vitalis.vn', pass: 'DoctorPass2025#' },
      pharmacist: { user: 'duoc.ngananh@vitalis.vn', pass: 'PharmaVitalis@88' },
      admin: { user: 'admin.hethong@vitalis.vn', pass: 'SystemRoot99#@' },
      patient: { user: 'nguyenvana.bn@gmail.com', pass: 'PatientSecure@12' }
    };

    if (roleMap[role]) {
      userInput.value = roleMap[role].user;
      passInput.value = roleMap[role].pass;
      toggleAlert('clear');
    }
  }

  function toggleAlert(type) {
    const alertPass = document.getElementById('alert-pass');
    const alertLocked = document.getElementById('alert-locked');
    if (!alertPass || !alertLocked) return;

    alertPass.classList.add('hidden');
    alertLocked.classList.add('hidden');

    if (type === 'error-pass') {
      alertPass.classList.remove('hidden');
    } else if (type === 'error-locked') {
      alertLocked.classList.remove('hidden');
    }
  }

  function handleLogin(e) {
    e.preventDefault();
    const btn = document.getElementById('submitBtn');
    const userInput = document.getElementById('username');
    if (userInput && userInput.value.includes('error')) {
      toggleAlert('error-pass');
      return;
    }
    if (btn) {
      const originalText = btn.innerHTML;
      btn.innerHTML = `<span class="material-symbols-outlined animate-spin text-lg">progress_activity</span><span>Đang xác thực...</span>`;
      btn.disabled = true;
      setTimeout(() => {
        btn.innerHTML = `<span class="material-symbols-outlined text-lg">check</span><span>Thành công!</span>`;
        setTimeout(() => {
          btn.innerHTML = originalText;
          btn.disabled = false;
        }, 1200);
      }, 900);
    }
  }

  function openForgotPasswordModal() {
    const modal = document.getElementById('forgot-modal');
    const msg = document.getElementById('reset-success-msg');
    if (msg) msg.classList.add('hidden');
    if (modal) modal.classList.remove('hidden');
  }

  function closeForgotPasswordModal() {
    const modal = document.getElementById('forgot-modal');
    if (modal) modal.classList.add('hidden');
  }

  function handleSendReset(e) {
    e.preventDefault();
    const msg = document.getElementById('reset-success-msg');
    const resetBtn = document.getElementById('resetSubmitBtn');
    if (resetBtn) {
      resetBtn.disabled = true;
      resetBtn.textContent = 'Đang gửi...';
    }
    setTimeout(() => {
      if (msg) msg.classList.remove('hidden');
      if (resetBtn) {
        resetBtn.disabled = false;
        resetBtn.textContent = 'Đã gửi';
      }
      setTimeout(() => {
        closeForgotPasswordModal();
      }, 1800);
    }, 800);
  }
