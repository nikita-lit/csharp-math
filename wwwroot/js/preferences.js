(() => {
    const read = key => { try { return localStorage.getItem(key); } catch { return null; } };
    const write = (key, value) => { try { localStorage.setItem(key, value); } catch { /* Preferences still work without browser storage. */ } };
    const savedTheme = read('function-explorer-theme');
    const savedLanguage = read('function-explorer-language');
    const root = document.documentElement;
    root.dataset.theme = savedTheme === 'dark' || savedTheme === 'light' ? savedTheme
        : window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    root.dataset.bsTheme = root.dataset.theme;
    root.lang = savedLanguage === 'ru' ? 'ru' : 'et';
    window.appPreferences = {
        setTheme(theme) {
            root.dataset.theme = theme === 'dark' ? 'dark' : 'light';
            root.dataset.bsTheme = root.dataset.theme;
            write('function-explorer-theme', root.dataset.theme);
            window.dispatchEvent(new Event('app-theme-change'));
        },
        setLanguage(language) {
            root.lang = language === 'ru' ? 'ru' : 'et';
            write('function-explorer-language', root.lang);
            window.dispatchEvent(new Event('app-language-change'));
        }
    };
})();
