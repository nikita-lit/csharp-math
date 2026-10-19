(() => {
    const calculator = document.querySelector('.calculator');
    if (!calculator) return;
    const input = document.getElementById('Formula');
    const positionLabel = calculator.querySelector('.calculator-position');
    const errorLabel = calculator.querySelector('.calculator-error');
    let start = input.value.length;
    let end = start;
    const limit = 200;

    function updatePosition() {
        positionLabel.textContent = `${start} / ${input.value.length}`;
    }

    function captureSelection() {
        start = input.selectionStart ?? input.value.length;
        end = input.selectionEnd ?? start;
        updatePosition();
    }

    function select(position, selectionEnd = position) {
        start = Math.max(0, Math.min(input.value.length, position));
        end = Math.max(start, Math.min(input.value.length, selectionEnd));
        input.focus({ preventScroll: true });
        input.setSelectionRange(start, end);
        updatePosition();
    }

    function replaceSelection(text, offset = text.length) {
        const next = input.value.slice(0, start) + text + input.value.slice(end);
        if (next.length > limit) {
            errorLabel.textContent = 'Valem võib olla kuni 200 märki pikk.';
            errorLabel.hidden = false;
            return;
        }
        errorLabel.hidden = true;
        const cursor = start + offset;
        input.value = next;
        select(cursor);
        input.dispatchEvent(new Event('input', { bubbles: true }));
    }

    calculator.addEventListener('pointerdown', event => {
        // Keep the display's selection when a keypad button receives focus.
        if (event.target.closest('button')) event.preventDefault();
    });
    calculator.addEventListener('click', event => {
        const button = event.target.closest('button');
        if (!button) return;
        if (button.dataset.insert != null) {
            const selected = input.value.slice(start, end);
            const text = button.dataset.insert;
            const needsBase = !input.value.slice(0, start).trim() || /[+\-*/(]\s*$/.test(input.value.slice(0, start));
            replaceSelection(text.startsWith('^') ? (selected ? `(${selected})${text}` : needsBase ? `x${text}` : text) : text);
        } else if (button.dataset.function) {
            const selected = input.value.slice(start, end);
            const name = button.dataset.function;
            const text = `${name}(${selected})`;
            replaceSelection(text, selected ? text.length : name.length + 1);
        } else if (button.dataset.template) {
            const selected = input.value.slice(start, end);
            const needsBase = !input.value.slice(0, start).trim() || /[+\-*/(]\s*$/.test(input.value.slice(0, start));
            const prefix = selected ? `(${selected})` : needsBase ? 'x' : '';
            replaceSelection(prefix + button.dataset.template, prefix.length + Number(button.dataset.offset));
        } else {
            switch (button.dataset.action) {
                case 'clear': start = 0; end = input.value.length; replaceSelection(''); break;
                case 'backspace':
                    if (start === end && start > 0) start--;
                    replaceSelection('');
                    break;
                case 'left': select(start > 0 ? start - 1 : 0); break;
                case 'right': select(end < input.value.length ? end + 1 : end); break;
                case 'analyze': input.dispatchEvent(new Event('calculate', { bubbles: true })); break;
            }
        }
    });
    input.addEventListener('select', captureSelection);
    input.addEventListener('click', captureSelection);
    input.addEventListener('keyup', captureSelection);
    input.addEventListener('input', () => {
        start = input.selectionStart ?? input.value.length;
        end = input.selectionEnd ?? start;
        updatePosition();
    });
    calculator.addEventListener('keydown', event => {
        if (event.target !== input || event.ctrlKey || event.metaKey || event.altKey) return;
        if (event.key === 'Enter' || event.key === '=') {
            event.preventDefault();
            input.dispatchEvent(new Event('calculate', { bubbles: true }));
        }

    });
    document.getElementById('calculatorExample').addEventListener('change', event => {
        const formula = event.target.value;
        if (!formula) return;
        input.value = formula;
        errorLabel.hidden = true;
        document.getElementById('RestrictDomain').checked = false;
        document.getElementById('RangeFrom').value = -5;
        document.getElementById('RangeTo').value = 5;
        select(input.value.length);
        input.dispatchEvent(new Event('input', { bubbles: true }));
        event.target.value = '';
    });
    const restriction = document.getElementById('RestrictDomain');
    const scopeSettings = calculator.querySelector('.scope-settings');
    const rangeFrom = document.getElementById('RangeFrom');
    const rangeTo = document.getElementById('RangeTo');
    function updateScopeHint() {
        document.getElementById('scopeHint').textContent = restriction.checked
            ? 'Funktsiooni uuritakse sellel kinnisel lõigul, sealhulgas otspunktides.'
            : 'Funktsiooni uuritakse kogu määramispiirkonnas. x-vahemik määrab graafiku akna.';
        const boundary = value => Math.abs(Number(value) - Math.PI) < 1e-12 ? 'π' : value;
        document.getElementById('scopeSummary').textContent = restriction.checked
            ? `Uurimine lõigul [${boundary(rangeFrom.value)}; ${boundary(rangeTo.value)}]`
            : 'Vahemiku seaded';
        scopeSettings.classList.toggle('is-restricted', restriction.checked);
    }
    restriction.addEventListener('input', updateScopeHint);
    [rangeFrom, rangeTo].forEach(input => {
        input.addEventListener('input', updateScopeHint);
        input.addEventListener('invalid', () => { scopeSettings.open = true; });
    });
    document.getElementById('calculatorExample').addEventListener('change', updateScopeHint);
    updateScopeHint();
    updatePosition();
})();
