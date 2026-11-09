// Dependency-free HTTP/WebSocket host. Server owns movement, ammunition,
// damage, projectiles, pickups and match scores. Clients send intent only.
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
import {Game,DT,clamp} from './shared/sim.js';
const ROOT=path.dirname(fileURLToPath(import.meta.url));
const args=process.argv.slice(2),opt=(key,fallback)=>{let i=args.indexOf(key);return i<0?fallback:args[i+1];};
const port=Number(opt('--port',8080)),bind=opt('--bind','127.0.0.1'),botCount=clamp(Number(opt('--bots',5)),0,10);
const game=new Game(1701,botCount),clients=new Map();let id=0;
function frame(data,opcode=1){let p=Buffer.from(data),h;if(p.length<126)h=Buffer.from([128|opcode,p.length]);else if(p.length<65536){h=Buffer.alloc(4);h[0]=128|opcode;h[1]=126;h.writeUInt16BE(p.length,2);}else{h=Buffer.alloc(10);h[0]=128|opcode;h[1]=127;h.writeBigUInt64BE(BigInt(p.length),2);}return Buffer.concat([h,p]);}
function send(c,obj){if(!c.socket.destroyed&&c.socket.writableLength<262144)c.socket.write(frame(JSON.stringify(obj)));}
const server=http.createServer((req,res)=>{
 let pathname;try{pathname=decodeURIComponent(new URL(req.url,'http://localhost').pathname);}catch{res.writeHead(400).end();return;}
 if(pathname==='/health'){res.writeHead(200,{'Content-Type':'application/json'}).end(JSON.stringify({ok:true,players:game.players.length,humans:clients.size,tick:game.time}));return;}
 if(pathname==='/')pathname='/public/index.html';
 if(!/^\/(public|shared|assets)\//.test(pathname)){res.writeHead(404).end();return;}
 const file=path.resolve(ROOT,'.'+pathname);if(!['public','shared','assets'].some(dir=>file.startsWith(path.join(ROOT,dir)+path.sep))){res.writeHead(403).end();return;}
 fs.readFile(file,(err,data)=>{if(err){res.writeHead(404).end();return;}res.writeHead(200,{'Content-Type':({'.html':'text/html','.js':'text/javascript','.css':'text/css','.png':'image/png'})[path.extname(file)]||'application/octet-stream','Cache-Control':'no-cache','X-Content-Type-Options':'nosniff'}).end(data);});
});
function remove(c){if(c.removed)return;c.removed=true;clients.delete(c.id);game.players=game.players.filter(p=>p.id!==c.id);game.projectiles=game.projectiles.filter(q=>q.owner!==c.id);}
function message(c,text){let m;try{m=JSON.parse(text);}catch{return;}
 if(!m||typeof m!=='object'||Array.isArray(m))return;
 if(m.type==='join'){c.player.name=typeof m.name==='string'?m.name.replace(/[^a-zA-Z0-9 _-]/g,'').slice(0,16)||'PILOT':'PILOT';return;}
 if(m.type==='ping'){send(c,{type:'pong',at:m.at});return;}
 if(m.type!=='input'||typeof m.seq!=='number'||!Number.isSafeInteger(m.seq)||m.seq<=c.player.lastSeq)return;
 if(!Number.isFinite(m.yaw)||!Number.isFinite(m.pitch))return;
 // Cap per-client traffic, sanitize every input, never accept position/hits.
 let now=Date.now();if(now-c.window>1000){c.window=now;c.count=0;}if(++c.count>180)return;c.lastInput=now;
 c.player.lastSeq=m.seq;c.player.input={f:clamp(Number(m.f)||0,-1,1),s:clamp(Number(m.s)||0,-1,1),yaw:m.yaw%(Math.PI*2),pitch:clamp(m.pitch,-1.45,1.45),jump:!!m.jump,dodge:!!m.dodge,crouch:!!m.crouch,fire:!!m.fire,alt:!!m.alt,weapon:Number.isInteger(m.weapon)?clamp(m.weapon,0,5):c.player.weapon};}
server.on('upgrade',(req,socket,head)=>{
 if(req.url!=='/match'||clients.size>=8||req.headers['sec-websocket-version']!=='13'||!req.headers['sec-websocket-key']){socket.end('HTTP/1.1 400 Bad Request\r\n\r\n');return;}
 let accept=crypto.createHash('sha1').update(req.headers['sec-websocket-key']+'258EAFA5-E914-47DA-95CA-C5AB0DC85B11').digest('base64');socket.write('HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: '+accept+'\r\n\r\n');socket.setNoDelay(true);
 let cid='human'+(++id),c={id:cid,socket,buf:Buffer.alloc(0),window:Date.now(),count:0,lastInput:Date.now(),player:game.add(cid,'PILOT '+id)};clients.set(cid,c);send(c,{type:'welcome',id:cid,protocol:1,tickRate:60,snapshotRate:20});send(c,{type:'state',...game.snapshot()});
 function receive(data){c.buf=Buffer.concat([c.buf,data]);if(c.buf.length>65536){socket.destroy();return;}
 while(c.buf.length>=2){let b=c.buf,op=b[0]&15,fin=b[0]&128,masked=b[1]&128,len=b[1]&127,off=2;if(!fin||!masked){socket.destroy();return;}if(len===126){if(b.length<4)return;len=b.readUInt16BE(2);off=4;}else if(len===127){socket.destroy();return;}if(len>8192){socket.destroy();return;}if(b.length<off+4+len)return;let mask=b.subarray(off,off+4),payload=Buffer.from(b.subarray(off+4,off+4+len));for(let i=0;i<len;i++)payload[i]^=mask[i%4];c.buf=b.subarray(off+4+len);if(op===8){socket.end(frame('',8));remove(c);return;}if(op===9){socket.write(frame(payload,10));continue;}if(op===1)message(c,payload.toString('utf8'));else if(op!==10){socket.destroy();return;}}
 }socket.on('data',receive);socket.on('close',()=>remove(c));socket.on('error',()=>remove(c));if(head.length)receive(head);
});
let previous=performance.now(),acc=0,ticks=0;
const timer=setInterval(()=>{let now=performance.now();acc+=Math.min(.2,(now-previous)/1000);previous=now;for(let c of clients.values())if(Date.now()-c.lastInput>300)c.player.input={};while(acc>=DT){game.tick(DT);acc-=DT;if(++ticks%3===0)for(let c of clients.values())send(c,{type:'state',...game.snapshot()});}},4);
server.listen(port,bind,()=>console.log(`ULTIMATE ARENA / THE FOUNDRY\nOpen http://localhost:${port}\nBind: ${bind}:${port} | ${botCount} bots | 60 Hz authority / 20 Hz snapshots\nFor LAN hosting: node server.js --bind 0.0.0.0\nCtrl+C to stop.`));
for(let signal of ['SIGINT','SIGTERM'])process.on(signal,()=>{clearInterval(timer);for(let c of clients.values())c.socket.destroy();server.close(()=>process.exit(0));});
