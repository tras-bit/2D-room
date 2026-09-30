(() => {
  const STORAGE_KEY = 'subsistence-brief-v1';
  const fields = [...document.querySelectorAll('[data-question]')];
  const countEl = document.getElementById('answeredCount');
  const fillEl = document.getElementById('progressFill');
  const percentEl = document.getElementById('progressPercent');
  const toast = document.getElementById('toast');
  const feedback = document.getElementById('feedback');
  let toastTimer;

  function answers() {
    return fields.map((field, index) => ({
      number: index + 1,
      question: field.dataset.question,
      answer: field.value.trim()
    }));
  }

  function updateProgress() {
    const filled = fields.filter(field => field.value.trim()).length;
    const percent = Math.round((filled / fields.length) * 100);
    countEl.textContent = String(filled).padStart(2, '0');
    fillEl.style.width = `${percent}%`;
    percentEl.textContent = `${percent}%`;
    const status = document.getElementById('saveStatus');
    status.querySelector('span:last-child').textContent = 'Сохранено';
  }

  function save() {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify({ version: 1, updatedAt: new Date().toISOString(), answers: answers() }));
      updateProgress();
    } catch (error) {
      showToast('Не удалось сохранить в браузере');
    }
  }

  function load() {
    try {
      const data = JSON.parse(localStorage.getItem(STORAGE_KEY) || 'null');
      if (!data || !Array.isArray(data.answers)) return;
      data.answers.forEach(item => {
        const field = fields[item.number - 1];
        if (!field || typeof item.answer !== 'string') return;
        // Carry forward the reference-derived defaults if an older draft had the original placeholders.
        if (item.number === 3 && item.answer === 'Игра будет 2D. Точный ракурс ещё нужно выбрать.') return;
        if (item.number === 12 && !item.answer.trim()) return;
        field.value = item.answer;
      });
    } catch (error) {
      localStorage.removeItem(STORAGE_KEY);
    }
    updateProgress();
  }

  function makeBrief() {
    const list = answers();
    return [
      'SUBSISTENCE — БРИФ ИГРЫ',
      '2D survival horror · Unity 2022.3.62f2',
      'Идея: смесь выживания в духе Rust и тревожной атмосферы Backrooms.',
      '',
      ...list.map(item => `${String(item.number).padStart(2, '0')}. ${item.question}\n${item.answer || '— Пока без ответа —'}`),
      '',
      'Референс-изображение: будет прислано отдельно.'
    ].join('\n');
  }

  function showToast(message) {
    toast.textContent = message;
    toast.classList.add('show');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove('show'), 2300);
  }

  async function copyText(text, successMessage = 'Скопировано в буфер обмена') {
    try {
      await navigator.clipboard.writeText(text);
      showToast(successMessage);
      feedback.textContent = successMessage;
    } catch (error) {
      const temporary = document.createElement('textarea');
      temporary.value = text;
      temporary.style.cssText = 'position:fixed;opacity:0;left:-9999px;top:0';
      document.body.appendChild(temporary);
      temporary.select();
      const copied = document.execCommand('copy');
      temporary.remove();
      const message = copied ? successMessage : 'Не вышло скопировать — скачай .txt';
      showToast(message);
      feedback.textContent = message;
    }
  }

  function download(name, content, type) {
    const blob = new Blob([content], { type });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = name;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }

  fields.forEach(field => field.addEventListener('input', save));
  document.getElementById('copyAll').addEventListener('click', () => copyText(makeBrief(), 'Весь бриф скопирован'));
  document.getElementById('downloadTxt').addEventListener('click', () => {
    download('subsistence-brief.txt', makeBrief(), 'text/plain;charset=utf-8');
    showToast('Бриф сохранён в .txt');
  });
  document.getElementById('downloadJson').addEventListener('click', () => {
    download('subsistence-brief-backup.json', JSON.stringify({ version: 1, updatedAt: new Date().toISOString(), answers: answers() }, null, 2), 'application/json;charset=utf-8');
    showToast('Резервная копия скачана');
  });
  document.getElementById('importJson').addEventListener('change', event => {
    const file = event.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      try {
        const parsed = JSON.parse(reader.result);
        if (!Array.isArray(parsed.answers)) throw new Error('Invalid format');
        parsed.answers.forEach(item => {
          const field = fields[item.number - 1];
          if (field && typeof item.answer === 'string') field.value = item.answer;
        });
        save();
        showToast('Ответы восстановлены');
      } catch (error) {
        showToast('Не удалось прочитать этот файл');
      }
      event.target.value = '';
    };
    reader.readAsText(file);
  });
  document.getElementById('resetForm').addEventListener('click', () => {
    if (!window.confirm('Очистить все ответы? Это действие нельзя отменить.')) return;
    fields.forEach(field => { field.value = ''; });
    save();
    feedback.textContent = '';
    showToast('Ответы очищены');
  });
  document.querySelectorAll('.copy-one').forEach(button => {
    button.addEventListener('click', () => {
      const field = document.getElementById(button.dataset.copy);
      copyText(`${field.dataset.question}:\n${field.value.trim() || '— Пока без ответа —'}`, `Ответ ${field.id.slice(1)} скопирован`);
    });
  });

  document.querySelectorAll('.section-tab').forEach(button => {
    button.addEventListener('click', () => {
      document.getElementById(button.dataset.jump).scrollIntoView({ behavior: 'smooth', block: 'start' });
      document.querySelectorAll('.section-tab').forEach(tab => tab.classList.toggle('active', tab === button));
    });
  });

  const sections = [...document.querySelectorAll('.question-section')];
  if ('IntersectionObserver' in window) {
    const observer = new IntersectionObserver(entries => {
      const visible = entries.filter(entry => entry.isIntersecting).sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];
      if (!visible) return;
      const current = visible.target.dataset.section;
      document.querySelectorAll('.section-tab').forEach((tab, index) => tab.classList.toggle('active', String(index + 1) === current));
    }, { rootMargin: '-18% 0px -65% 0px', threshold: [0, .15, .4] });
    sections.forEach(section => observer.observe(section));
  }

  load();
})();
