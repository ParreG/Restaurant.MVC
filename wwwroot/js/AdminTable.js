const serverData = window.tableData || [];

const tableList = Array.isArray(serverData) ? serverData.map(t => ({
    id: t.id ?? t.Id,
    Number: t.number ?? t.Number,
    capacity: t.capacity ?? t.Capacity,
    x: (t.x ?? t.X ?? null),
    y: (t.y ?? t.Y ?? null),
    isAvailable: t.isAvailable ?? t.IsAvailable ?? true
})) : [];

const svgRoot = document.getElementById('floor');
const viewBox = svgRoot.viewBox.baseVal;
const snapSize = 25;

const marginX = 150, marginY = 175, stepX = 225, stepY = 225;
const maxCols = Math.max(1, Math.floor((viewBox.width - marginX * 2) / stepX) + 1);
const maxRows = Math.max(1, Math.floor((viewBox.height - marginY * 2) / stepY) + 1);

const tableMapByNumber = new Map();
let selectedTableNumber = null;
let editingTableNumber = null;

const snapToGrid = v => Math.round(v / snapSize) * snapSize;
const clamp = (v, min, max) => Math.max(min, Math.min(max, v));

function getSvgPoint(evt) {
    const pt = svgRoot.createSVGPoint();
    pt.x = evt.clientX; pt.y = evt.clientY;
    const p = pt.matrixTransform(svgRoot.getScreenCTM().inverse());
    return { x: p.x, y: p.y };
}

function polarToCartesian(cx, cy, r, a) {
    return { x: cx + r * Math.cos(a), y: cy + r * Math.sin(a) };
}

function getAutoPosition(index) {
    const col = index % maxCols;
    const row = Math.floor(index / maxCols) % maxRows;
    return { x: marginX + col * stepX, y: marginY + row * stepY };
}

function ensurePosition(table, index) {
    if (table.x == null || table.y == null) {
        const pos = getAutoPosition(index);
        table.x = pos.x; table.y = pos.y;
    }
}

function firstFreeNumber() {
    const used = new Set([...tableMapByNumber.keys()].map(Number));
    let n = 1; while (used.has(n)) n++; return n;
}

function getDimensionsForCapacity(capacity) {
    if (capacity <= 4) {
        const side = 130;
        return { shape: 'square', w: side, h: side, ring: side / 2 + 28, chairR: 14, boundX: side / 2, boundY: side / 2 };
    }
    if (capacity <= 6) {
        const w = 160, h = 100;
        return { shape: 'rect', w, h, ring: Math.max(w, h) / 2 + 28, chairR: 14, boundX: w / 2, boundY: h / 2 };
    }
    const r = 85;
    return { shape: 'circle', r, ring: r + 28, chairR: Math.max(10, Math.min(18, r * 0.22)), boundX: r, boundY: r };
}

function renderChairsCircle(group, table, dims) {
    const step = (2 * Math.PI) / Math.max(1, table.capacity);
    for (let i = 0; i < table.capacity; i++) {
        const angle = i * step - Math.PI / 2;
        const { x, y } = polarToCartesian(table.x, table.y, dims.ring, angle);
        const chair = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        chair.setAttribute('class', 'chair'); chair.setAttribute('cx', x); chair.setAttribute('cy', y); chair.setAttribute('r', dims.chairR);
        group.appendChild(chair);
    }
}

function renderChairsRect(group, table, dims) {
    const halfW = dims.w / 2 + 20;
    const halfH = dims.h / 2 + 20;
    const perSide = Math.floor(table.capacity / 2);
    const remainder = table.capacity - perSide * 2;

    for (let i = 0; i < perSide; i++) {
        const xTop = table.x - dims.w / 2 + (i + 1) * (dims.w / (perSide + 1));
        const yTop = table.y - halfH;
        const chairTop = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        chairTop.setAttribute('class', 'chair'); chairTop.setAttribute('cx', xTop); chairTop.setAttribute('cy', yTop); chairTop.setAttribute('r', dims.chairR);
        group.appendChild(chairTop);
    }
    for (let i = 0; i < perSide; i++) {
        const xBottom = table.x - dims.w / 2 + (i + 1) * (dims.w / (perSide + 1));
        const yBottom = table.y + halfH;
        const chairBottom = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        chairBottom.setAttribute('class', 'chair'); chairBottom.setAttribute('cx', xBottom); chairBottom.setAttribute('cy', yBottom); chairBottom.setAttribute('r', dims.chairR);
        group.appendChild(chairBottom);
    }
    if (remainder === 1) {
        const chairSide = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        chairSide.setAttribute('class', 'chair'); chairSide.setAttribute('cx', table.x + halfW); chairSide.setAttribute('cy', table.y); chairSide.setAttribute('r', dims.chairR);
        group.appendChild(chairSide);
    }
}

function rerenderChairs(table) {
    const ref = tableMapByNumber.get(table.Number); if (!ref) return;
    [...ref.group.querySelectorAll('.chair')].forEach(c => c.remove());
    const dims = ref.dims;
    (dims.shape === 'rect' || dims.shape === 'square')
        ? renderChairsRect(ref.group, table, dims)
        : renderChairsCircle(ref.group, table, dims);
}

function notify(kind, message, timeout = 2800) {
    const container = document.getElementById('notify-root');
    if (!container) { console.log(`[${kind}]`, message); return; }

    const icons = { success: '✔', error: '✖', warning: '⚠', info: 'ℹ' };
    const roles = { success: 'status', info: 'status', warning: 'alert', error: 'alert' };

    const card = document.createElement('div');
    card.className = `toast-card toast-${kind}`;
    card.setAttribute('role', roles[kind] || 'status');
    card.innerHTML = `
    <span class="toast-icon">${icons[kind] || 'ℹ'}</span>
    <div class="toast-text">${message}</div>
    <button class="toast-close" aria-label="Stäng">&times;</button>
  `;
    container.appendChild(card);

    card.animate(
        [{ opacity: 0, transform: 'translateY(8px) scale(.98)' },
        { opacity: 1, transform: 'translateY(0)  scale(1)' }],
        { duration: 180, easing: 'ease-out' }
    );

    const close = () => {
        card.animate([{ opacity: 1 }, { opacity: 0 }], { duration: 160, easing: 'ease-in' })
            .onfinish = () => card.remove();
    };
    const timerId = setTimeout(close, timeout);
    card.querySelector('.toast-close').addEventListener('click', () => { clearTimeout(timerId); close(); });

    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        card.getAnimations().forEach(a => a.finish());
    }
}

function setSelectedTable(number) {
    if (selectedTableNumber != null && tableMapByNumber.get(selectedTableNumber)) {
        tableMapByNumber.get(selectedTableNumber).group.classList.remove('table--selected');
    }
    selectedTableNumber = number ?? null;
    if (selectedTableNumber != null && tableMapByNumber.get(selectedTableNumber)) {
        tableMapByNumber.get(selectedTableNumber).group.classList.add('table--selected');
    }
}

function toggleSelectedTable(number) {
    setSelectedTable(selectedTableNumber === number ? null : number);
}

function wireTableEvents(group, table) {
    group.addEventListener('click', () => {
        if (dragExceededTolerance) return;
        toggleSelectedTable(table.Number);
    });
}

function renderTable(table, index) {
    ensurePosition(table, index);
    const dims = getDimensionsForCapacity(table.capacity);

    const group = document.createElementNS('http://www.w3.org/2000/svg', 'g');
    group.classList.add('table');
    if (!table.isAvailable) group.classList.add('table--busy');
    group.dataset.number = String(table.Number);
    group.style.cursor = 'pointer';

    let topShape;
    if (dims.shape === 'circle') {
        topShape = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
        topShape.setAttribute('cx', table.x); topShape.setAttribute('cy', table.y); topShape.setAttribute('r', dims.r);
    } else {
        topShape = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
        topShape.setAttribute('x', table.x - dims.w / 2); topShape.setAttribute('y', table.y - dims.h / 2);
        topShape.setAttribute('width', dims.w); topShape.setAttribute('height', dims.h);
        topShape.setAttribute('rx', 12); topShape.setAttribute('ry', 12);
    }
    topShape.setAttribute('class', 'table__top');

    const label = document.createElementNS('http://www.w3.org/2000/svg', 'text');
    label.setAttribute('class', 'table__label');
    label.setAttribute('x', table.x); label.setAttribute('y', table.y);
    label.style.pointerEvents = 'none';
    label.setAttribute('font-size', dims.shape === 'circle' ? dims.r * 0.8 : Math.min(dims.w, dims.h) * 0.5);
    label.textContent = table.Number;

    (dims.shape === 'rect' || dims.shape === 'square') ? renderChairsRect(group, table, dims) : renderChairsCircle(group, table, dims);

    group.appendChild(topShape);
    group.appendChild(label);
    svgRoot.appendChild(group);

    tableMapByNumber.set(table.Number, { group, top: topShape, label, dims, data: table });
    wireTableEvents(group, table);
}

function rerenderTable(table) {
    const number = table.Number;
    const oldRef = tableMapByNumber.get(number);
    if (oldRef) oldRef.group.remove();
    renderTable(table, 0);
    if (selectedTableNumber === number) {
        const ref = tableMapByNumber.get(number);
        if (ref) ref.group.classList.add('table--selected');
    }
}

tableList.forEach(renderTable);

let dragTableNumber = null, dragStartPoint = null, dragStartPos = null;
let dragExceededTolerance = false;
const DRAG_TOLERANCE_PX = 3;

svgRoot.addEventListener('pointerdown', e => {
    const group = e.target.closest('.table');
    dragExceededTolerance = false;

    if (!group) {
        setSelectedTable(null);
        return;
    }

    dragTableNumber = Number(group.dataset.number);
    setSelectedTable(dragTableNumber);

    dragStartPoint = getSvgPoint(e);
    const { data } = tableMapByNumber.get(dragTableNumber);
    dragStartPos = { x: data.x, y: data.y };
    svgRoot.setPointerCapture(e.pointerId);
});

svgRoot.addEventListener('pointermove', e => {
    if (dragTableNumber == null) return;
    const ref = tableMapByNumber.get(dragTableNumber);
    const dims = ref.dims, table = ref.data;
    const cur = getSvgPoint(e), dx = cur.x - dragStartPoint.x, dy = cur.y - dragStartPoint.y;

    if (!dragExceededTolerance) {
        if (Math.abs(dx) > DRAG_TOLERANCE_PX || Math.abs(dy) > DRAG_TOLERANCE_PX) dragExceededTolerance = true;
    }

    let nextX = clamp(dragStartPos.x + dx, dims.boundX + 5, viewBox.width - dims.boundX - 5);
    let nextY = clamp(dragStartPos.y + dy, dims.boundY + 5, viewBox.height - dims.boundY - 5);

    if (dims.shape === 'circle') { ref.top.setAttribute('cx', nextX); ref.top.setAttribute('cy', nextY); }
    else { ref.top.setAttribute('x', nextX - dims.w / 2); ref.top.setAttribute('y', nextY - dims.h / 2); }

    ref.label.setAttribute('x', nextX); ref.label.setAttribute('y', nextY);
    table.x = nextX; table.y = nextY;
    rerenderChairs(table);
});

svgRoot.addEventListener('pointerup', e => {
    if (dragTableNumber == null) return;
    const ref = tableMapByNumber.get(dragTableNumber);
    const dims = ref.dims, table = ref.data;

    table.x = snapToGrid(table.x); table.y = snapToGrid(table.y);

    if (dims.shape === 'circle') { ref.top.setAttribute('cx', table.x); ref.top.setAttribute('cy', table.y); }
    else { ref.top.setAttribute('x', table.x - dims.w / 2); ref.top.setAttribute('y', table.y - dims.h / 2); }

    ref.label.setAttribute('x', table.x); ref.label.setAttribute('y', table.y);
    rerenderChairs(table);

    svgRoot.releasePointerCapture(e.pointerId);
    dragTableNumber = null; dragStartPoint = null; dragStartPos = null;
});

document.addEventListener('keydown', e => {
    if (e.key === 'Escape') setSelectedTable(null);
});

svgRoot.addEventListener('click', e => {
    if (!e.target.closest('.table') && !dragExceededTolerance) setSelectedTable(null);
});

document.getElementById("saveLayoutBtn").addEventListener("click", async () => {
    const payload = {
        updates: tableList.map(t => ({
            tableNumber: t.Number,
            x: Math.round(t.x ?? 0),
            y: Math.round(t.y ?? 0)
        }))
    };
    try {
        const response = await fetch("/AdminTables/SaveLayout", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });
        const responseText = await response.text();
        if (!response.ok) throw new Error(`API ${response.status}: ${responseText}`);
        notify('success', 'Placeringar sparade!');
    } catch (error) {
        console.error(error);
        notify('error', 'Kunde inte spara placeringar.');
    }
});

if (typeof firstFreeNumber === 'function') {
    document.getElementById('createTableModal').addEventListener('show.bs.modal', () => {
        document.getElementById('createNumber').value = firstFreeNumber();
        document.getElementById('createCapacity').value = "";
    });
}

document.getElementById("createSubmit").addEventListener("click", async () => {
    const number = Number(document.getElementById("createNumber").value);
    const capacity = Number(document.getElementById("createCapacity").value);
    if (!number || !capacity) { notify('warning', 'Fyll i nummer och kapacitet.'); return; }
    if (tableMapByNumber.has(number)) { notify('warning', `Bord nummer ${number} finns redan. Välj ett annat nummer.`); return; }

    try {
        const response = await fetch("/AdminTables/Create", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ number, capacity })
        });
        const responseText = await response.text();
        if (!response.ok) throw new Error(`API ${response.status}: ${responseText}`);

        const newTable = { id: 0, Number: number, capacity, x: null, y: null, isAvailable: true };
        tableList.push(newTable);
        renderTable(newTable, tableList.length - 1);
        setSelectedTable(number);

        bootstrap.Modal.getInstance(document.getElementById('createTableModal')).hide();
        document.getElementById("createNumber").value = "";
        document.getElementById("createCapacity").value = "";
        notify('success', `Bord ${number} skapades.`);
    } catch (error) {
        console.error(error);
        notify('error', 'Kunde inte skapa bord.');
    }
});

document.getElementById("openPickEditBtn").addEventListener("click", () => {
    document.getElementById("pickEditNumber").value = selectedTableNumber ?? "";
    new bootstrap.Modal(document.getElementById('pickEditModal')).show();
});

document.getElementById("pickEditSubmit").addEventListener("click", () => {
    const pickedNumber = Number(document.getElementById("pickEditNumber").value);
    if (!pickedNumber) { notify('warning', 'Ange bordsnummer.'); return; }
    const ref = tableMapByNumber.get(pickedNumber);
    if (!ref) { notify('warning', `Bord ${pickedNumber} finns inte.`); return; }
    editingTableNumber = pickedNumber;
    document.getElementById("editCapacity").value = ref.data.capacity;

    const pickModal = bootstrap.Modal.getInstance(document.getElementById('pickEditModal'));
    if (pickModal) pickModal.hide();
    new bootstrap.Modal(document.getElementById('editTableModal')).show();
});

document.getElementById("editSubmit").addEventListener("click", async () => {
    if (editingTableNumber == null) { notify('warning', 'Inget bord valt.'); return; }
    const ref = tableMapByNumber.get(editingTableNumber);
    const table = ref?.data;
    if (!table) { notify('error', 'Kunde inte hitta bordet.'); return; }

    const newCapacity = Number(document.getElementById("editCapacity").value);
    if (!newCapacity || newCapacity < 1) { notify('warning', 'Ogiltig kapacitet.'); return; }

    try {
        const response = await fetch(`/AdminTables/Update/${table.Number}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ number: table.Number, capacity: newCapacity })
        });
        const responseText = await response.text();
        if (!response.ok) throw new Error(`API ${response.status}: ${responseText}`);

        table.capacity = newCapacity;
        rerenderTable(table);
        setSelectedTable(table.Number);

        bootstrap.Modal.getInstance(document.getElementById('editTableModal')).hide();
        notify('success', `Bord ${table.Number} uppdaterat.`);
    } catch (error) {
        console.error(error);
        notify('error', 'Kunde inte uppdatera bord.');
    }
});

document.getElementById("openPickDeleteBtn").addEventListener("click", () => {
    document.getElementById("pickDeleteNumber").value = selectedTableNumber ?? "";
    new bootstrap.Modal(document.getElementById('pickDeleteModal')).show();
});

document.getElementById("pickDeleteSubmit").addEventListener("click", () => {
    const pickedNumber = Number(document.getElementById("pickDeleteNumber").value);
    if (!pickedNumber) { notify('warning', 'Ange bordsnummer.'); return; }
    if (!tableMapByNumber.get(pickedNumber)) { notify('warning', `Bord ${pickedNumber} finns inte.`); return; }

    document.getElementById('confirmDeleteText').textContent = `Är du säker på att du vill ta bort bord ${pickedNumber}?`;
    document.getElementById('confirmDeleteBtn').dataset.number = String(pickedNumber);

    const pickModal = bootstrap.Modal.getInstance(document.getElementById('pickDeleteModal'));
    if (pickModal) pickModal.hide();
    new bootstrap.Modal(document.getElementById('confirmDeleteModal')).show();
});

document.getElementById("confirmDeleteBtn").addEventListener("click", async (e) => {
    const numberToDelete = Number(e.currentTarget.dataset.number);
    try {
        const response = await fetch(`/AdminTables/Delete/${numberToDelete}`, { method: "DELETE" });
        const responseText = await response.text();

        if (!response.ok) {
            if (response.status === 409 || /TABLE_HAS_BOOKINGS|FK_Bookings|REFERENCE constraint|foreign key/i.test(responseText)) {
                notify('warning', 'Bordet har bokningar och kan inte tas bort. Flytta/ta bort bokningarna först.');
                return;
            }
            throw new Error(`API ${response.status}: ${responseText}`);
        }

        const index = tableList.findIndex(t => t.Number === numberToDelete);
        if (index >= 0) tableList.splice(index, 1);
        const ref = tableMapByNumber.get(numberToDelete);
        if (ref) ref.group.remove();
        tableMapByNumber.delete(numberToDelete);
        if (selectedTableNumber === numberToDelete) selectedTableNumber = null;

        bootstrap.Modal.getInstance(document.getElementById('confirmDeleteModal')).hide();
        notify('success', `Bord ${numberToDelete} togs bort.`);
    } catch (error) {
        console.error(error);
        notify('error', 'Kunde inte ta bort bord.');
    }
});
