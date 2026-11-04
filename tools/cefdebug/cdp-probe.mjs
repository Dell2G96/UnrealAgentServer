// UE 5.6 CEF(-cefdebug=9222)에 붙어 채팅 페이지를 새로고침하고, 에러와 호환성 상태를 수집한다
const Targets = await (await fetch("http://localhost:9222/json")).json();
const Page = Targets.find(t => t.type === "page" && t.url.includes("55558"));
if (!Page) { console.log("chat page not found", Targets); process.exit(1); }

const Ws = new WebSocket(Page.webSocketDebuggerUrl);
let NextId = 1;
const Pending = new Map();
const Events = [];
const Send = (method, params = {}) => new Promise(res => { const id = NextId++; Pending.set(id, res); Ws.send(JSON.stringify({ id, method, params })); });

Ws.onmessage = M => {
    const D = JSON.parse(M.data);
    if (D.id && Pending.has(D.id)) { Pending.get(D.id)(D.result ?? D); Pending.delete(D.id); return; }
    if (D.method === "Runtime.exceptionThrown") Events.push("EXCEPTION: " + (D.params.exceptionDetails.exception?.description ?? D.params.exceptionDetails.text) + " @ " + D.params.exceptionDetails.url);
    if (D.method === "Runtime.consoleAPICalled" && ["error", "warning"].includes(D.params.type)) Events.push("CONSOLE." + D.params.type + ": " + D.params.args.map(a => a.value ?? a.description).join(" "));
    if (D.method === "Log.entryAdded" && ["error", "warning"].includes(D.params.entry.level)) Events.push("LOG." + D.params.entry.level + ": " + D.params.entry.text + " " + (D.params.entry.url ?? ""));
};
await new Promise(r => Ws.onopen = r);
await Send("Runtime.enable"); await Send("Log.enable"); await Send("Page.enable");
await Send("Page.reload", { ignoreCache: true });
await new Promise(r => setTimeout(r, 8000));

const Expr = `(async () => {
  const TryParse = src => { try { new Function(src); return "OK"; } catch (e) { return e.name + ": " + e.message; } };
  const Orig = await (await fetch("/_framework/blazor.web.js")).text();
  const Compat = await (await fetch("/compat/blazor.web.chrome90.js")).text();
  return {
    userAgent: navigator.userAgent.match(/Chrome\\/[\\d.]+/)[0],
    parseStaticBlock: TryParse("class A { static { this.x = 1 } }"),
    parseOriginalBlazor: TryParse(Orig),
    parseCompatBlazor: TryParse(Compat),
    nativeWithResolvers: Promise.withResolvers.toString().includes("[native code]"),
    hasArrayAt: typeof [].at,
    blazor: typeof window.Blazor,
    tailwind: typeof window.tailwind,
    hljs: typeof window.hljs,
    reconnectUi: !!document.querySelector("#components-reconnect-modal, .components-reconnect-show, .components-reconnect-failed"),
    blazorComments: (document.documentElement.innerHTML.match(/<!--Blazor:/g) || []).length,
    title: document.title
  };
})()`;
const R = await Send("Runtime.evaluate", { expression: Expr, awaitPromise: true, returnByValue: true });
console.log(JSON.stringify(R.result?.value ?? R, null, 2));
console.log("--- events after reload ---");
console.log(Events.length ? Events.join("\n") : "(none)");
Ws.close();
