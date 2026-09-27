
    function toggleProfileBanner() {
      const foundBox = document.getElementById('state-found-profile');
      const notFoundBox = document.getElementById('state-not-found-profile');
      if (foundBox && notFoundBox) {
        foundBox.classList.toggle('hidden');
        notFoundBox.classList.toggle('hidden');
      }
    }

    function togglePasswordVisibility(fieldId, btn) {
      const input = document.getElementById(fieldId);
      const icon = btn.querySelector('.material-symbols-outlined');
      if (input && icon) {
        if (input.type === 'password') {
          input.type = 'text';
          icon.textContent = 'visibility_off';
        } else {
          input.type = 'password';
          icon.textContent = 'visibility';
        }
      }
    }

    const checkBtn = document.getElementById('btn-check-profile');
    if (checkBtn) {
      checkBtn.addEventListener('click', () => {
        const phoneInput = document.getElementById('phone');
        const phoneError = document.getElementById('phone-validation-error');
        if (phoneInput && phoneInput.value.replace(/\s+/g, '').length < 10) {
          if (phoneError) phoneError.classList.remove('hidden');
        } else {
          if (phoneError) phoneError.classList.add('hidden');
          toggleProfileBanner();
        }
      });
    }
  