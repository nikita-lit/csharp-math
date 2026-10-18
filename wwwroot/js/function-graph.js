(() => {
    const workspace = document.querySelector('.workspace');
    if (!workspace) return;
    const preview = workspace.dataset.preview === 'true';
    const formulaInput = document.getElementById('Formula');
    const inputFrom = document.getElementById('RangeFrom');
    const inputTo = document.getElementById('RangeTo');
    const graph = workspace.querySelector('.function-graph');
    const canvas = graph.querySelector('canvas');
    const ctx = canvas.getContext('2d');
    const graphFrom = document.getElementById('graphFrom');
    const graphTo = document.getElementById('graphTo');
    const graphStep = document.getElementById('graphStep');
    const status = workspace.querySelector('.workspace-status');
    const graphError = graph.querySelector('.graph-error');
    const empty = graph.querySelector('.graph-empty');
    const tooltip = graph.querySelector('.graph-tooltip');
    const solutionPanel = workspace.querySelector('.solution-panel');
    const solutionContent = workspace.querySelector('.solution-content');
    const solutionToggle = workspace.querySelector('.solution-toggle');
    let data = null, view = null, geometry = null;
    let graphAbort, previewAbort, solutionAbort, previewTimer, zoomTimer;
    let solutionKey = '', chartVersion = 0, previewVersion = 0, solutionVersion = 0;
    let width = 0, height = 0, dragging = null;
    const current = () => ({ formula: preview ? formulaInput.value.trim() : workspace.dataset.formula,
        from: preview ? Number(inputFrom.value) : Number(workspace.dataset.from),
        to: preview ? Number(inputTo.value) : Number(workspace.dataset.to) });
    const params = values => new URLSearchParams(values);
    const number = x => Number.isFinite(x) ? Number(x.toFixed(4)).toString() : '∄';
    const colors = () => {
        const css = getComputedStyle(document.documentElement);
        return ['--accent', '--orange', '--blue', '--line', '--muted', '--plot', '--text', '--surface'].map(key => css.getPropertyValue(key).trim());
    };
    function setStatus(message, error = false) { status.textContent = message; status.classList.toggle('is-error', error); }
    function showGraphError(message = '') { graphError.textContent = message; graphError.hidden = !message; }
    async function read(url, signal) {
        const response = await fetch(url, { signal });
        const json = await response.json();
        if (!response.ok) throw new Error(json.error || 'Arvutamine ei õnnestunud. Kontrolli sisendit.');
        return json;
    }
    function clearGraph() { data = null; view = null; tooltip.hidden = true; render(); }
    async function loadGraph() {
        graphAbort?.abort();
        const version = ++chartVersion;
        const formula = current().formula;
        if (!formula) { clearGraph(); showGraphError(); return; }
        graphAbort = new AbortController();
        graph.setAttribute('aria-busy', 'true');
        try {
            const result = await read(`/Functions/Points?${params({ formula, from: graphFrom.value, to: graphTo.value, step: graphStep.value })}`, graphAbort.signal);
            if (version !== chartVersion) return;
            data = result;
            view = { min: Number(graphFrom.value), max: Number(graphTo.value) };
            empty.hidden = true;
            showGraphError();
            render();
        } catch (error) {
            if (error.name !== 'AbortError' && version === chartVersion) { clearGraph(); showGraphError(error.message); }
        } finally { if (version === chartVersion) graph.removeAttribute('aria-busy'); }
    }
    function updateResults(result) {
        workspace.querySelectorAll('[data-property]').forEach(element => {
            const key = element.dataset.property;
            element.textContent = result[key] || '—';
            const latex = result[`${key}Latex`];
            if (latex) element.dataset.latex = latex;
            else delete element.dataset.latex;
        });
        renderMath(workspace.querySelector('.analysis-table'));
        workspace.querySelector('.analysis-note').textContent = result.isNumerical
            ? `Osa tulemusi on leitud numbriliselt vahemikus [${result.rangeFrom}; ${result.rangeTo}].`
            : 'Algebralised tulemused kehtivad kogu määramispiirkonnas.';
    }
    function clearResults() {
        workspace.querySelectorAll('[data-property]').forEach(element => { element.textContent = '—'; delete element.dataset.latex; });
        workspace.querySelector('.analysis-note').textContent = 'Sisesta valem ja uuri selle omadusi.';
    }
    function invalidateSolution() {
        solutionKey = '';
        solutionAbort?.abort();
        solutionVersion++;
        solutionContent.replaceChildren();
    }
    async function loadPreview() {
        previewAbort?.abort(); graphAbort?.abort(); invalidateSolution();
        const version = ++previewVersion;
        const values = current();
        graphFrom.value = values.from; graphTo.value = values.to;
        if (!values.formula) { clearResults(); clearGraph(); setStatus(''); return; }
        previewAbort = new AbortController();
        setStatus('Uurin funktsiooni…');
        try {
            const result = await read(`/Functions/Preview?${params(values)}`, previewAbort.signal);
            if (version !== previewVersion) return;
            updateResults(result);
            setStatus('Arvutus valmis.');
            await loadGraph();
            if (!solutionPanel.hidden && version === previewVersion) loadSolution();
        } catch (error) {
            if (error.name !== 'AbortError' && version === previewVersion) {
                clearResults(); clearGraph(); setStatus(error.message, true);
                solutionContent.textContent = error.message;
            }
        }
    }
    function schedulePreview() {
        previewAbort?.abort(); graphAbort?.abort(); solutionAbort?.abort();
        previewVersion++; chartVersion++; solutionVersion++;
        graph.removeAttribute('aria-busy');
        clearTimeout(previewTimer);
        clearResults(); clearGraph(); invalidateSolution();
        setStatus(current().formula ? 'Uurin funktsiooni…' : '');
        previewTimer = setTimeout(loadPreview, 400);
    }
    function syncRange() {
        if (view) { graphFrom.value = Number(view.min.toFixed(4)); graphTo.value = Number(view.max.toFixed(4)); }
    }
    function axes(values) {
        const finite = values.filter(v => v != null && Number.isFinite(v)).sort((a,b) => a-b);
        if (!finite.length) return { min: -1, max: 1 };
        let low = finite[Math.floor(finite.length * .03)], high = finite[Math.floor((finite.length - 1) * .97)];
        low = Math.min(low, 0); high = Math.max(high, 0);
        const extra = Math.max((high - low) * .16, .5);
        return { min: low - extra, max: high + extra };
    }
    function ticks(min, max, count) {
        const rough = (max-min)/count;
        if (!(rough > 0)) return [];
        const power = 10 ** Math.floor(Math.log10(rough));
        const factor = rough/power;
        const step = (factor > 5 ? 10 : factor > 2 ? 5 : factor > 1 ? 2 : 1) * power;
        const out = [];
        for (let v = Math.ceil(min/step)*step; v <= max + step*.0001 && out.length < 30; v += step) out.push(Math.abs(v) < step*.0001 ? 0 : v);
        return out;
    }
    function render() {
        const rect = canvas.getBoundingClientRect();
        width = Math.max(1,rect.width); height = Math.max(1,rect.height);
        const dpr = window.devicePixelRatio || 1;
        canvas.width = Math.round(width*dpr); canvas.height = Math.round(height*dpr);
        ctx.setTransform(dpr,0,0,dpr,0,0);
        const palette = colors();
        ctx.fillStyle = palette[5]; ctx.fillRect(0,0,width,height);
        empty.hidden = !!data;
        if (!data || !view) return;
        const series = [data.y, data.derivativeY, data.secondDerivativeY];
        const enabled = [...graph.querySelectorAll('[data-series]')].map(input => input.checked);
        const visible = [];
        data.x.forEach((x,i) => { if (x >= view.min && x <= view.max) series.forEach((s,j) => { if (enabled[j]) visible.push(s[i]); }); });
        const yr = axes(visible);
        const plot = { left: 53, right: width-17, top: 25, bottom: height-38 };
        const X = x => plot.left + (x-view.min)/(view.max-view.min)*(plot.right-plot.left);
        const Y = y => plot.bottom - (y-yr.min)/(yr.max-yr.min)*(plot.bottom-plot.top);
        geometry = { plot, X, Y, yr };
        ctx.font = '10px Arial'; ctx.lineWidth = 1; ctx.textBaseline = 'middle';
        ticks(view.min,view.max,7).forEach(x => {
            const px = X(x); ctx.strokeStyle = Math.abs(x) < 1e-10 ? palette[4] : palette[3];
            ctx.beginPath();ctx.moveTo(px,plot.top);ctx.lineTo(px,plot.bottom);ctx.stroke();
            ctx.fillStyle = palette[4];ctx.textAlign='center';ctx.fillText(number(x),px,plot.bottom+18);
        });
        ticks(yr.min,yr.max,7).forEach(y => {
            const py = Y(y); ctx.strokeStyle = Math.abs(y) < 1e-10 ? palette[4] : palette[3];
            ctx.beginPath();ctx.moveTo(plot.left,py);ctx.lineTo(plot.right,py);ctx.stroke();
            ctx.fillStyle=palette[4];ctx.textAlign='right';ctx.fillText(number(y),plot.left-9,py);
        });
        ctx.fillStyle=palette[4];ctx.textAlign='right';ctx.fillText('x',plot.right,height-10);ctx.textAlign='left';ctx.fillText('y',12,16);
        ctx.save();ctx.beginPath();ctx.rect(plot.left,plot.top,plot.right-plot.left,plot.bottom-plot.top);ctx.clip();
        data.asymptotes.forEach(x => {
            ctx.strokeStyle=palette[4];ctx.lineWidth=1;ctx.setLineDash([3,5]);ctx.beginPath();ctx.moveTo(X(x),plot.top);ctx.lineTo(X(x),plot.bottom);ctx.stroke();
        });
        series.forEach((values,j) => {
            if (!enabled[j]) return;
            ctx.strokeStyle=palette[j];ctx.lineWidth=j===0?2.3:1.6;ctx.setLineDash(j===0?[]:j===1?[6,4]:[2,4]);ctx.beginPath();
            let started=false;
            data.x.forEach((x,i) => {
                const value=values[i];
                if (value == null || !Number.isFinite(value)) {started=false;return;}
                const px=X(x),py=Y(Math.max(yr.min-(yr.max-yr.min)*4,Math.min(yr.max+(yr.max-yr.min)*4,value)));
                if (!started) ctx.moveTo(px,py); else ctx.lineTo(px,py);
                started=true;
            });
            ctx.stroke();
        });
        if (graph.querySelector('.show-points').checked) {
            ctx.setLineDash([]);
            [data.zeros,data.extrema,data.inflections].forEach((points,j) => points.forEach(p => {
                if (p.x<view.min || p.x>view.max) return;
                ctx.beginPath();ctx.fillStyle=palette[7];ctx.strokeStyle=palette[j];ctx.lineWidth=2;
                ctx.arc(X(p.x),Y(p.y),j===1?4.5:3.5,0,Math.PI*2);ctx.fill();ctx.stroke();
                if (j===1) {ctx.fillStyle=palette[j];ctx.font='10px Arial';ctx.fillText(p.kind==='max'?'max':'min',X(p.x)+8,Y(p.y)-10);}
            }));
        }
        ctx.restore();
    }
    function node(tag, className, text) {
        const element=document.createElement(tag);
        if(className)element.className=className;
        if(text != null)element.textContent=text;
        return element;
    }
    function math(latex, block=false) { const element=node('span',block?'math-block':'math');element.dataset.latex=latex;return element; }
    function rich(text) {
        const p=node('p');
        text.split(/(\$[^$]+\$|\*\*.+?\*\*)/g).forEach(part => {
            if(part.startsWith('$')&&part.endsWith('$'))p.append(math(part.slice(1,-1)));
            else if(part.startsWith('**')&&part.endsWith('**'))p.append(node('strong','',part.slice(2,-2)));
            else p.append(document.createTextNode(part));
        });
        return p;
    }
    function signTable(table) {
        const wrap=node('div','sign-table-wrap'), t=node('table','sign-table');
        const row=node('tr');const header=node('th');header.append(math('x'));row.append(header);
        table.columns.forEach(column=>{const th=node('th');th.append(math(column.x));row.append(th);});t.append(row);
        const signs=node('tr'), label=node('th');label.append(math(table.signLabel));signs.append(label);
        table.columns.forEach(column=>signs.append(node('td','',column.sign)));t.append(signs);
        if(table.meaningLabel){const meanings=node('tr'),label=node('th');label.append(math(table.meaningLabel));meanings.append(label);table.columns.forEach(column=>{const td=node('td');if(column.meaning)td.append(math(column.meaning));meanings.append(td);});t.append(meanings);}
        const tests=node('tr','test-row');tests.append(node('th','','Kontroll'));
        table.columns.forEach(column=>{const td=node('td');if(column.check)td.append(math(column.check));tests.append(td);});t.append(tests);
        wrap.append(t);return wrap;
    }
    async function loadSolution() {
        const values=current(), key=JSON.stringify(values);
        if(key===solutionKey && solutionContent.children.length)return;
        solutionAbort?.abort(); solutionAbort=new AbortController();
        const version=++solutionVersion;
        solutionContent.textContent='Koostan lahenduskäiku…';
        try {
            const result=await read(`/Functions/Solution?${params(values)}`,solutionAbort.signal);
            if(version!==solutionVersion)return;
            solutionContent.replaceChildren();
            result.parts.forEach((part,i)=>{
                const details=node('details','solution-part');details.open=true;
                const summary=node('summary');summary.append(node('span','step-number',i+1),node('span','',part.title));
                if(part.isNumerical)summary.append(node('span','tag','Numbriliselt'));
                details.append(summary);
                const steps=node('div','solution-steps');
                part.steps.forEach(step=>{
                    const block=node('div','solution-step');
                    if(step.text)block.append(rich(step.text));
                    if(step.formula){const div=node('div','math-block');div.dataset.latex=step.formula;block.append(div);}
                    if(step.table)block.append(signTable(step.table));
                    steps.append(block);
                });
                if(part.answers?.length)steps.append(node('div','answer-line',part.answers.join('; ')));
                details.append(steps);solutionContent.append(details);
            });
            solutionKey=key;renderMath(solutionContent);
        } catch(error){if(error.name!=='AbortError'&&version===solutionVersion)solutionContent.textContent=error.message;}
    }
    solutionToggle.addEventListener('click',()=>{
        solutionPanel.hidden=!solutionPanel.hidden;solutionToggle.setAttribute('aria-expanded',String(!solutionPanel.hidden));
        solutionToggle.firstChild.textContent=solutionPanel.hidden?'Näita lahenduskäiku ':'Peida lahenduskäik ';
        if(!solutionPanel.hidden){loadSolution();solutionPanel.scrollIntoView({behavior:'smooth',block:'start'});}
    });
    workspace.querySelector('.solution-close').addEventListener('click',()=>{solutionPanel.hidden=true;solutionToggle.setAttribute('aria-expanded','false');solutionToggle.firstChild.textContent='Näita lahenduskäiku ';solutionToggle.focus();});
    graph.querySelector('.graph-range').addEventListener('submit',event=>{event.preventDefault();clearTimeout(zoomTimer);loadGraph();});
    graph.querySelectorAll('[data-series],.show-points').forEach(input=>input.addEventListener('change',render));
    graph.querySelector('.graph-reset').addEventListener('click',()=>{const values=current();graphFrom.value=values.from;graphTo.value=values.to;graphStep.value=.05;loadGraph();});
    graph.querySelector('.graph-export').addEventListener('click',()=>{
        if(!data){showGraphError('Sisesta kõigepealt funktsioon.');return;}
        const link=document.createElement('a');link.download='funktsioon.png';link.href=canvas.toDataURL('image/png');link.click();
    });
    canvas.addEventListener('wheel',event=>{
        if(!data||!view||!geometry)return;
        event.preventDefault();
        const plot=geometry.plot;
        const ratio=Math.max(0,Math.min(1,(event.offsetX-plot.left)/(plot.right-plot.left)));
        const center=view.min+ratio*(view.max-view.min);
        const size=Math.max(.02,Math.min(2000,(view.max-view.min)*(event.deltaY>0?1.2:.8)));
        let min=center-size*ratio,max=min+size;
        if(min < -1000){min=-1000;max=min+size;}if(max>1000){max=1000;min=max-size;}
        view={min,max};syncRange();render();
        const neededStep = (view.max-view.min)/19000;
        if (Number(graphStep.value)<neededStep) graphStep.value=Number(neededStep.toPrecision(3));
        clearTimeout(zoomTimer);zoomTimer=setTimeout(loadGraph,250);
    },{passive:false});
    canvas.addEventListener('pointerdown',event=>{if(!data||!view)return;dragging={x:event.clientX,min:view.min,max:view.max};canvas.setPointerCapture(event.pointerId);tooltip.hidden=true;});
    canvas.addEventListener('pointermove',event=>{
        if(!data||!view||!geometry)return;
        if(dragging){const shift=(event.clientX-dragging.x)/(geometry.plot.right-geometry.plot.left)*(dragging.max-dragging.min);const size=dragging.max-dragging.min;const min=Math.max(-1000,Math.min(1000-size,dragging.min-shift));view={min,max:min+size};syncRange();render();return;}
        const px=event.offsetX,py=event.offsetY,plot=geometry.plot;
        if(px<plot.left||px>plot.right||py<plot.top||py>plot.bottom){tooltip.hidden=true;return;}
        const target=view.min+(px-plot.left)/(plot.right-plot.left)*(view.max-view.min);
        let lo=0,hi=data.x.length-1;while(lo<hi){const m=Math.floor((lo+hi)/2);if(data.x[m]<target)lo=m+1;else hi=m;}
        let i=lo;if(i>0&&Math.abs(data.x[i-1]-target)<Math.abs(data.x[i]-target))i--;
        const enabled=[...graph.querySelectorAll('[data-series]')].map(input=>input.checked);
        let text=`x = ${number(data.x[i])}`;
        [data.y,data.derivativeY,data.secondDerivativeY].forEach((s,j)=>{if(enabled[j])text+=`\n${['f(x)','f′(x)','f″(x)'][j]} = ${s[i]==null?'∄':number(s[i])}`;});
        if(graph.querySelector('.show-points').checked)[...data.zeros.map(p=>({...p,label:'Nullkoht'})),...data.extrema.map(p=>({...p,label:p.kind==='max'?'Maksimum':'Miinimum'})),...data.inflections.map(p=>({...p,label:'Käänupunkt'}))].forEach(p=>{if(Math.hypot(geometry.X(p.x)-px,geometry.Y(p.y)-py)<12)text+=`\n${p.label} (${number(p.x)}; ${number(p.y)})`;});
        data.asymptotes.forEach(x=>{if(Math.abs(geometry.X(x)-px)<8)text+=`\nAsümptoot x = ${number(x)}`;});
        tooltip.textContent=text;tooltip.hidden=false;
        tooltip.style.left=`${Math.max(2,Math.min(width-tooltip.offsetWidth-5,px+12))}px`;
        tooltip.style.top=`${Math.max(2,Math.min(height-tooltip.offsetHeight-5,py+12))}px`;
    });
    canvas.addEventListener('pointerup',()=>{if(dragging){dragging=null;loadGraph();}});
    canvas.addEventListener('pointercancel',()=>{dragging=null;});
    canvas.addEventListener('pointerleave',()=>{tooltip.hidden=true;});
    if (preview) {
        [formulaInput, inputFrom, inputTo].forEach(input => input.addEventListener('input', schedulePreview));
        formulaInput.addEventListener('calculate', () => {
            clearTimeout(previewTimer);
            if (!current().formula) { setStatus('Koosta kõigepealt funktsioon.', true); return; }
            loadPreview();
        });
        if (current().formula) loadPreview(); else render();
    }
    else{graphFrom.value=workspace.dataset.from;graphTo.value=workspace.dataset.to;loadGraph();}
    new ResizeObserver(render).observe(canvas.parentElement);
    window.addEventListener('themechange',render);
})();
