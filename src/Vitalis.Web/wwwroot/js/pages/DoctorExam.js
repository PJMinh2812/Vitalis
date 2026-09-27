
  // Keyboard Shortcut listener for clinical workflow (F10 - Complete Exam)
  document.addEventListener('keydown', function(event) {
    if (event.key === 'F10') {
      event.preventDefault();
      alert('Đang hoàn thành ca khám của BN Phạm Quốc Bảo, tạo mã đơn thuốc điện tử và chuyển tiếp sang Quầy Dược & Thu ngân.');
    }
  });
