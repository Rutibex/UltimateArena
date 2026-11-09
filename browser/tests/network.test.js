// Node 22+ integration test: two independent WebSocket clients, real TCP host.
import test from 'node:test';import assert from 'node:assert/strict';import {spawn} from 'node:child_process';import {setTimeout as delay} from 'node:timers/promises';import {fileURLToPath} from 'node:url';import {pathfind,los} from '../shared/sim.js';
const ROOT=fileURLToPath(new URL('../',import.meta.url));
async function waitFor(fn,timeout=8000){let start=Date.now();while(Date.now()-start<timeout){let r=fn();if(r)return r;await delay(20);}throw Error('Timed out waiting for network condition');}
test('two-client authoritative match: join, movement, rejected position, kill replication and disconnect',{timeout:30000},async()=>{
 let host=spawn(process.execPath,['server.js','--port','18081','--bots','0'],{cwd:ROOT,stdio:['ignore','pipe','pipe']});let output='';host.stdout.on('data',d=>output+=d);host.stderr.on('data',d=>output+=d);let sockets=[];try{
 await waitFor(()=>output.includes('Open http'));
 assert.equal((await fetch('http://127.0.0.1:18081/')).status,200);assert.equal((await fetch('http://127.0.0.1:18081/public/%2e%2e%2fserver.js')).status,403);
 function client(){let c={id:null,state:null,events:new Map(),seq:0},ws=new WebSocket('ws://127.0.0.1:18081/match');c.ws=ws;sockets.push(ws);ws.onmessage=e=>{let m=JSON.parse(e.data);if(m.type==='welcome')c.id=m.id;if(m.type==='state'){c.state=m;for(let e of m.events)c.events.set(e.id,e);}};return c;}
 let a=client(),b=client();await waitFor(()=>a.id&&b.id&&a.state?.players.length===2&&b.state?.players.length===2);a.ws.send(JSON.stringify({type:'join',name:'ALPHA'}));b.ws.send(JSON.stringify({type:'join',name:'BRAVO'}));await waitFor(()=>a.state.players.some(p=>p.name==='BRAVO'));
 a.ws.send('null');a.ws.send('[]');a.ws.send('{bad json');await delay(30);
 let me=()=>a.state.players.find(p=>p.id===a.id),target=()=>a.state.players.find(p=>p.id===b.id);let before={...me()};
 a.ws.send(JSON.stringify({type:'input',seq:++a.seq,yaw:0,pitch:0,f:0,s:0,weapon:3,x:99999,hp:9999,score:9999,ammo:[99999]}));await delay(180);assert.ok(Math.abs(me().x-before.x)<.05);assert.equal(me().score,0);assert.ok(me().hp<=100);assert.ok(me().ammo[0]<99999);
 let timer=setInterval(()=>{if(!a.state)return;let p=me(),t=target();if(!p||!t)return;let dx=t.x-p.x,dz=t.z-p.z,dist=Math.hypot(dx,dz),visible=los(p,t),f=0,yaw=Math.atan2(dx,-dz),pitch=Math.atan2(t.y-p.y-.45,dist),fire=false;
 if(visible){fire=true;}else{let route=pathfind(p,t),next=route.find(n=>Math.hypot(n.x-p.x,n.z-p.z)>.7);if(next){yaw=Math.atan2(next.x-p.x,-(next.z-p.z));pitch=0;f=1;}}
 a.ws.send(JSON.stringify({type:'input',seq:++a.seq,yaw,pitch,f,s:0,weapon:3,fire}));},16);
 try{await waitFor(()=>me().score>=1,18000);}finally{clearInterval(timer);}
 await waitFor(()=>b.state.players.find(p=>p.id===a.id)?.score>=1);assert.ok(a.events.size>0);let kill=[...a.events.values()].find(e=>e.type==='kill'&&e.owner===a.id&&e.target===b.id);assert.ok(kill);assert.ok(b.events.has(kill.id));assert.ok(Math.hypot(me().x-before.x,me().z-before.z)>1,'human traversed arena');
 let pA=me(),pB=b.state.players.find(p=>p.id===a.id);assert.equal(pA.score,pB.score);assert.ok(pA.ammo[3]<24);console.log('NETWORK EVIDENCE',JSON.stringify({players:2,scorer:pA.name,score:pA.score,replicatedKillId:kill.id,serverTime:a.state.time,transport:'WebSocket over loopback TCP',positionInjection:'ignored'}));
 b.ws.close();await waitFor(()=>a.state.players.length===1);assert.equal(a.state.players[0].id,a.id);
 }finally{for(let ws of sockets)ws.close();host.kill('SIGTERM');await delay(100);}
});
