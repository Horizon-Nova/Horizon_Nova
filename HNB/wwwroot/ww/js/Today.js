const aiText = "Light rain today — picked your water-resistant navy jacket + slim trousers. Skipped your linen shirt: absorbs moisture and takes hours to dry. This combo keeps you sharp and dry all day.";
let selOcc = null, typeT = null;

const pastData = {
  'Apr 17':{occ:'Casual',name:'Weekend Easy',temp:'25°',ico:'☀️',reason:'Sunny and warm — light linen shirt + white shorts. Breathable and easy for a weekend afternoon.'},
  'Apr 18':{occ:'Date',name:'Evening Chic',temp:'21°',ico:'🌙',reason:'Clear and mild — your silk blouse + tailored trousers. Elegant without being overdressed for dinner.'},
  'Apr 19':{occ:'Work',name:'Smart Casual',temp:'23°',ico:'⛅',reason:'Mild and partly cloudy — beige chino + white button-down. Light enough for the temperature, smart enough for the office.'}
};

function show(id){ document.getElementById(id).classList.remove('hidden'); }
function hide(id){ document.getElementById(id).classList.add('hidden'); }
function fadeIn(id){ const e=document.getElementById(id); e.classList.remove('hidden'); e.classList.add('fade'); setTimeout(()=>e.classList.remove('fade'),250); }
function hideAllRight(){ ['r-empty','r-gen','r-items','r-result','r-record','r-future'].forEach(id=>hide(id)); }

function setEx(txt){
  const inp = document.getElementById('free-occ');
  inp.value = txt;
  handleFree(inp);
  inp.focus();
}

function clickDay(el){
  document.querySelectorAll('.wday').forEach(d=>d.classList.remove('sel'));
  el.classList.add('sel');
  const type = el.dataset.type;
  if(type==='today'){
    document.getElementById('tb-title').innerHTML = "Whatever the <em>Weather.</em>";
    show('occ-section');
    document.getElementById('occ-title').textContent = "What's today about?";
    document.getElementById('gbtn-label').textContent = "Style me today";
    resetOcc(); hideAllRight(); fadeIn('r-empty');
  } else if(type==='past'){
    hide('occ-section');
    document.getElementById('gbtn').disabled = true;
    const d = pastData[el.dataset.date];
    document.getElementById('tb-title').innerHTML = el.dataset.date;
    document.getElementById('rec-date').textContent = el.dataset.date;
    document.getElementById('rec-meta').textContent = d.occ+' · '+d.temp+' '+el.dataset.ico;
    document.getElementById('rec-badge').textContent = d.occ;
    document.getElementById('rec-weather').textContent = d.temp+' '+el.dataset.ico;
    document.getElementById('rec-name').textContent = d.name;
    document.getElementById('rec-reason').textContent = d.reason;
    hideAllRight(); fadeIn('r-record');
  } else {
    show('occ-section');
    document.getElementById('occ-title').textContent = 'What\'s the occasion for '+el.dataset.date+'?';
    document.getElementById('gbtn-label').textContent = 'Save to calendar';
    resetOcc();
    document.getElementById('fut-date').textContent = el.dataset.date;
    document.getElementById('fut-ico').textContent = el.dataset.ico;
    document.getElementById('fut-desc').textContent = el.dataset.temp+' · '+el.dataset.desc;
    hide('fut-saved'); hide('fut-gbtn');
    document.querySelectorAll('#r-future .ocard').forEach(c=>c.classList.remove('on'));
    hideAllRight(); fadeIn('r-future');
  }
}

function resetOcc(){
  document.querySelectorAll('#occ-section .ocard').forEach(c=>c.classList.remove('on'));
  document.getElementById('free-occ').value = '';
  selOcc = null;
  document.getElementById('gbtn').disabled = true;
}
function pickOcc(el, occ){
  document.querySelectorAll('#occ-section .ocard').forEach(c=>c.classList.remove('on'));
  el.classList.add('on'); selOcc = occ;
  document.getElementById('free-occ').value = '';
  document.getElementById('gbtn').disabled = false;
}
function handleFree(el){
  document.querySelectorAll('#occ-section .ocard').forEach(c=>c.classList.remove('on'));
  selOcc = el.value.trim();
  document.getElementById('gbtn').disabled = !selOcc;
}
function pickFutOcc(el, occ){
  document.querySelectorAll('#r-future .ocard').forEach(c=>c.classList.remove('on'));
  el.classList.add('on'); show('fut-gbtn');
}
function saveFuture(){
  hide('fut-gbtn'); fadeIn('fut-saved');
  const selDay = document.querySelector('.wday.sel');
  if(selDay){ const c=selDay.querySelector('.wd-circle'); if(c) c.classList.add('filled'); }
}
function startGen(){
  const occ = selOcc||'Work';
  document.getElementById('g-occ').textContent = occ;
  document.getElementById('r-occ-badge').textContent = occ;
  document.getElementById('tb-title').innerHTML = 'Generating...';
  hideAllRight(); fadeIn('r-gen');
  const steps=[{d:'sd3',t:'st3',dl:400},{d:'sd4',t:'st4',dl:920},{d:'sd5',t:'st5',dl:1400}];
  steps.forEach(({d,t,dl})=>{
    setTimeout(()=>{ document.getElementById(d).className='sd cur'; document.getElementById(t).className='st cur'; }, dl);
    setTimeout(()=>{ document.getElementById(d).className='sd done'; document.getElementById(t).className='st done'; }, dl+520);
  });
  setTimeout(()=>{
    hide('r-gen'); fadeIn('r-items');
    const ids=['ic0','ic1','ic2','ic3'], cks=['ck0','ck1','ck2','ck3'];
    const lbls=['Picking shirt...','Adding trousers...','Grabbing jacket...','Choosing shoes...'];
    ids.forEach((id,i)=>{
      setTimeout(()=>{
        document.getElementById('items-lbl').textContent = lbls[i];
        document.getElementById(id).classList.add('show');
        setTimeout(()=>document.getElementById(cks[i]).classList.add('show'), 270);
      }, i*520);
    });
    setTimeout(()=>showResult(), ids.length*520+500);
  }, 2300);
}
function showResult(){
  hide('r-items');
  document.getElementById('tb-title').innerHTML = "Whatever the <em>Weather.</em>";
  const todayEl = document.getElementById('day-today');
  const circle = todayEl.querySelector('.wd-circle');
  if(circle) circle.classList.add('filled');
  fadeIn('r-result');
  setTimeout(()=>document.getElementById('outfit-full').classList.add('show'), 170);
  setTimeout(()=>{ const n=document.getElementById('ai-name'); n.style.opacity='1'; n.style.transition='opacity .45s'; }, 620);
  setTimeout(()=>typeWriter(aiText,0), 840);
  const endT = 840+aiText.length*18;
  setTimeout(()=>{ document.getElementById('r-actions').style.opacity='1'; document.getElementById('r-tweak').style.opacity='1'; }, endT+120);
  setTimeout(()=>document.getElementById('shop-wrap').style.opacity='1', endT+600);
}
function typeWriter(text, i){
  const el = document.getElementById('ai-text');
  if(i < text.length){ el.innerHTML=text.slice(0,i+1)+'<span class="cursor"></span>'; typeT=setTimeout(()=>typeWriter(text,i+1),18); }
  else { el.innerHTML=text; }
}
function backToEmpty(){
  if(typeT) clearTimeout(typeT);
  document.getElementById('outfit-full').classList.remove('show');
  document.getElementById('ai-name').style.opacity='0';
  document.getElementById('ai-text').innerHTML='';
  document.getElementById('r-actions').style.opacity='0';
  document.getElementById('r-tweak').style.opacity='0';
  document.getElementById('shop-wrap').style.opacity='0';
  ['ic0','ic1','ic2','ic3'].forEach(id=>document.getElementById(id).classList.remove('show'));
  ['ck0','ck1','ck2','ck3'].forEach(id=>document.getElementById(id).classList.remove('show'));
  ['sd3','sd4','sd5'].forEach(id=>document.getElementById(id).className='sd');
  ['st3','st4','st5'].forEach(id=>document.getElementById(id).className='st');
  document.getElementById('tb-title').innerHTML="Whatever the <em>Weather.</em>";
  hideAllRight(); fadeIn('r-empty');
}
