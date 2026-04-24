let pwVisible = false;
let liPwVisible = false;

function setCtx(from) {
  const pill = document.getElementById('ctx-pill');
  const suTitle = document.getElementById('su-title');
  const suSub = document.getElementById('su-sub');

  if (from === 'trial') {
    pill.classList.remove('hidden');
    document.getElementById('ctx-text').textContent = 'Save your look — sign up free';
    switchTab('signup');
    suTitle.innerHTML = 'Save your <em>look.</em>';
    suSub.textContent = 'Create a free account to use your own wardrobe and save your outfits.';
  } else {
    pill.classList.add('hidden');
    switchTab('login');
    suTitle.innerHTML = 'Start for <em>free.</em>';
    suSub.textContent = 'No credit card required. Cancel anytime.';
  }

  ['btn-1', 'btn-2'].forEach((id, i) => {
    document.getElementById(id).classList.toggle('active', ['landing', 'trial'][i] === from);
  });
}

function switchTab(tab) {
  document.getElementById('tab-su').className = 'tab' + (tab === 'signup' ? ' active' : '');
  document.getElementById('tab-li').className = 'tab' + (tab === 'login' ? ' active' : '');
  document.getElementById('view-signup').classList.toggle('hidden', tab !== 'signup');
  document.getElementById('view-login').classList.toggle('hidden', tab !== 'login');
}

function checkPw(pw) {
  const bars = ['pb1', 'pb2', 'pb3'];
  bars.forEach((b) => (document.getElementById(b).className = 'pw-bar'));
  const lbl = document.getElementById('pw-label');
  if (!pw) {
    lbl.textContent = '';
    return;
  }
  if (pw.length >= 1) document.getElementById('pb1').classList.add('w');
  if (pw.length >= 7) {
    document.getElementById('pb1').classList.add('m');
    document.getElementById('pb2').classList.add('m');
    lbl.textContent = 'Good';
    lbl.style.color = 'var(--orange)';
  }
  if (pw.length >= 10 && /[A-Z]/.test(pw) && /[0-9!@#$]/.test(pw)) {
    bars.forEach((b) => (document.getElementById(b).className = 'pw-bar s'));
    lbl.textContent = 'Strong [OK]';
    lbl.style.color = '#6db86b';
  } else if (pw.length < 7) {
    lbl.textContent = 'Too short';
    lbl.style.color = '#e05555';
  }
}

function togglePw() {
  pwVisible = !pwVisible;
  const inp = document.getElementById('pw-input');
  inp.type = pwVisible ? 'text' : 'password';
  inp.nextElementSibling.textContent = pwVisible ? 'Hide' : 'Show';
}

function toggleLiPw() {
  liPwVisible = !liPwVisible;
  const inp = document.getElementById('li-pw');
  inp.type = liPwVisible ? 'text' : 'password';
  inp.nextElementSibling.textContent = liPwVisible ? 'Hide' : 'Show';
}

function goToOnboarding() {
  alert('→ Onboarding (upload clothes)');
}

function goToApp() {
  alert('→ Today page (direct, no onboarding)');
}

document.addEventListener('DOMContentLoaded', function () {
  setCtx('landing');
});
