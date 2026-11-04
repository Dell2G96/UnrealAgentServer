// 사용법: node ime-log.mjs install  → 채팅 textarea 에 IME 이벤트 기록기 설치
//         node ime-log.mjs read     → 기록 출력
const Mode = process.argv[2];
const T = (await (await fetch("http://localhost:9222/json")).json()).find(t => t.type === "page" && t.url.includes("55558"));
const Ws = new WebSocket(T.webSocketDebuggerUrl); await new Promise(r => Ws.onopen = r);
const Eval = E => new Promise(r => { Ws.onmessage = M => { const D = JSON.parse(M.data); if (D.id === 1) r(D.result.result.value ?? D.result); }; Ws.send(JSON.stringify({ id: 1, method: "Runtime.evaluate", params: { expression: E, returnByValue: true } })); });
const Install = `(() => {
  window.__imeLog = [];
  const Ta = document.querySelector('textarea');
  if (!Ta) return 'no textarea';
  const Log = (k, e) => window.__imeLog.push([performance.now().toFixed(0), k, e.data ?? e.key ?? '', e.isComposing ?? '', JSON.stringify(Ta.value)].join(' | '));
  for (const k of ['keydown','compositionstart','compositionupdate','compositionend','beforeinput','input']) Ta.addEventListener(k, e => Log(k + (e.inputType ? '(' + e.inputType + ')' : ''), e), true);
  const D = Object.getOwnPropertyDescriptor(HTMLTextAreaElement.prototype, 'value');
  if (!window.__valuePatched) {
    Object.defineProperty(HTMLTextAreaElement.prototype, 'value', { get() { return D.get.call(this); }, set(v) { window.__imeLog && window.__imeLog.push(performance.now().toFixed(0) + ' | JS-SET value | ' + JSON.stringify(v) + ' | was ' + JSON.stringify(D.get.call(this))); D.set.call(this, v); }, configurable: true });
    window.__valuePatched = true;
  }
  return 'installed';
})()`;
console.log(Mode === "install" ? await Eval(Install) : (await Eval("window.__imeLog || ['no log']")).join(String.fromCharCode(10)));
Ws.close();
