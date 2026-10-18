(() => {
    window.renderMath = (root = document) => {
        root.querySelectorAll('[data-latex]').forEach(element => {
            const latex = element.dataset.latex;
            if (!latex) return;
            if (window.katex) katex.render(latex, element, { throwOnError: false, trust: false, strict: false, displayMode: element.classList.contains('math-block') });
            else element.textContent = latex;
        });
    };
    window.renderMath();
    function updateThemeLabel() {
        const dark = document.documentElement.dataset.theme === 'dark';
        document.querySelectorAll('.theme-label').forEach(label => label.textContent = dark ? 'Hele teema' : 'Tume teema');
        document.querySelectorAll('.theme-toggle').forEach(button => button.setAttribute('aria-pressed', String(dark)));
    }
    document.querySelectorAll('.theme-toggle').forEach(button => button.addEventListener('click', () => {
        const theme = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark';
        document.documentElement.dataset.theme = theme;
        try { localStorage.setItem('lab-theme', theme); } catch { }
        updateThemeLabel();
        window.dispatchEvent(new Event('themechange'));
    }));
    updateThemeLabel();
})();
