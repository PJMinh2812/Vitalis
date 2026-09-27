
    document.addEventListener("DOMContentLoaded", () => {
      const activeNav = document.querySelector('a[data-path="quan-ly-kho-duoc"]');
      if (activeNav) {
        activeNav.classList.remove("text-on-surface-variant", "hover:bg-surface-container");
        activeNav.classList.add("bg-primary-container", "text-on-primary", "font-bold");
      }
    });
  