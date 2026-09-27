document.querySelectorAll('a[data-nav-on]').forEach(function (a) {
    var active = a.getAttribute('href') === location.pathname;
    a.className = active ? a.dataset.navOn : a.dataset.navOff;
    if (active) a.setAttribute('aria-current', 'page');
});
