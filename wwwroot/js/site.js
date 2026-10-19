(() => {
    window.renderMath = (root = document) => {
        root.querySelectorAll('[data-latex]').forEach(element => {
            const source = element.dataset.latex;
            const latex = window.appI18n?.latex(source || '') || source;
            if (!latex) return;
            if (window.katex) katex.render(latex, element, { throwOnError: false, trust: false, strict: false, displayMode: element.classList.contains('math-block') });
            else element.textContent = latex;
        });
    };
    window.renderMath();
    const themeToggle = document.querySelector('[data-theme-toggle]');
    const updateThemeToggle = () => themeToggle.setAttribute('aria-pressed', String(document.documentElement.dataset.theme === 'dark'));
    themeToggle.addEventListener('click', () => appPreferences.setTheme(document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark'));
    window.addEventListener('app-theme-change', updateThemeToggle);
    updateThemeToggle();
})();
