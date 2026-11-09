import {PD_DATA} from './pd-data.js';
export const DT=1/60, TAU=Math.PI*2;
export const clamp=(x,a,b)=>Math.max(a,Math.min(b,x));
export const WEAPONS=[
 {name:'KESTREL',role:'Precision sidearm',alt:'Focused shot',kind:'bullet',damage:PD_DATA[0].damage*28,delay:PD_DATA[0].recoverytime60/60,spread:PD_DATA[0].spread,pellets:1,ammo:90,color:'#78dfde'},
 {name:'TEMPEST',role:'Rotary carbine',alt:'Magazine discharge',kind:'bullet',damage:PD_DATA[1].damage*12.5,delay:60/PD_DATA[1].initialrpm,spread:PD_DATA[1].spread,pellets:1,ammo:240,color:'#f7c565'},
 {name:'BREACHER',role:'Scatter cannon',alt:'Tight double volley',kind:'bullet',damage:PD_DATA[2].damage*(50/3),delay:.78,spread:PD_DATA[2].spread,pellets:9,ammo:36,color:'#f69d70'},
 {name:'LONGBOW',role:'Precision rail',alt:'Hold to zoom',kind:'rail',damage:PD_DATA[3].damage*(205/3),delay:1.05,spread:PD_DATA[3].spread,pellets:1,ammo:24,color:'#a5a3ff'},
 {name:'COMET',role:'Rocket launcher',alt:'Guided rocket',kind:'rocket',damage:PD_DATA[4].damage*92,delay:.85,spread:0,pellets:1,ammo:18,color:'#ef7969'},
 {name:'LATCH',role:'Grenade projector',alt:'Sticky proximity mine',kind:'grenade',damage:PD_DATA[5].damage*85,delay:.7,spread:0,pellets:1,ammo:24,color:'#a5df7b'}
].map((w,i)=>({...w,pd:PD_DATA[i]}));
// Original firing distribution and stance multipliers adapted from
// Perfect Dark bondgun.c:bgunCalculatePlayerShotSpread. Screen-space is
// converted to angular offsets here; balance/timing are arena-specific.
export function pdSpread(spread,rng,crouch=false,focused=false){
 if(crouch)spread*=.5;if(focused)spread*=.25;
 return (rng()-.5)*rng()*spread*Math.PI/180;
}
export const ARENA={name:'THE FOUNDRY',subtitle:'Orbital reclamation facility / Arena 01',size:48,boxes:[],decor:[],pads:[{x:0,z:11},{x:0,z:-11}],spawns:[[-19,0,-8],[19,0,8],[-18,0,10],[18,0,-10],[-10,0,19],[10,0,-19],[-16,3,-18],[16,3,18]]};
const box=(x,y,z,sx,sy,sz,mat=0,color=[1,1,1],glow=0)=>({x,y,z,sx,sy,sz,mat,color,glow});
const B=ARENA.boxes,D=ARENA.decor;
B.push(box(0,-.5,0,48,1,48,1));
B.push(box(-24,5,0,1,11,49,0),box(24,5,0,1,11,49,0),box(0,5,-24,48,11,1,0),box(0,5,24,48,11,1,0));
// Two upper galleries; stairs are actual collision geometry, no teleporting.
for(let s of [-1,1]){
 B.push(box(0,2.8,s*18.5,42,.4,8,2));
 for(let x of [-15,15])for(let i=0;i<8;i++)B.push(box(x,(i+1)*.375/2,s*(6.5+i),4,(i+1)*.375,1,0));
 for(let x=-20;x<=20;x+=4){D.push(box(x,5.5,s*23.35,.18,8,.2,0),box(x,7.9,s*23.2,2.8,.12,.15,0,s<0?[.1,.9,1]:[1,.4,.12],1));}
 D.push(box(0,3.08,s*14.52,42,.12,.12,0,[.15,.8,1],1));
}
B.push(box(0,2.5,0,5.5,5,5.5,3));
for(let x of [-9,9])for(let z of [-8,8]){B.push(box(x,3.2,z,1.8,6.4,1.8,0));D.push(box(x,3,z+1,.12,5,.08,0,[.15,.9,1],1));}
for(let s of [-1,1]){B.push(box(s*15,.8,0,4,1.6,3,3));B.push(box(s*6,.65,s*11,3,1.3,2,0));}
for(let s of [-1,1]){
 D.push(box(s*2.81,2.7,0,.08,3,3.5,0,[.1,1,.85],1),box(0,2.7,s*2.81,3.5,3,.08,0,[.1,1,.85],1));
 for(let x of [-22,22])D.push(box(x,9,s*13,3,.3,14,0),box(x,8.8,s*13,.12,.1,13,0,[.1,.5,.6],1));
}
export function rayBox(o,d,b,max=100){let lo=0,hi=max;for(let [k,sz] of [['x','sx'],['y','sy'],['z','sz']]){let mn=b[k]-b[sz]/2,mx=b[k]+b[sz]/2;if(Math.abs(d[k])<1e-8){if(o[k]<mn||o[k]>mx)return null;}else{let a=(mn-o[k])/d[k],c=(mx-o[k])/d[k];if(a>c)[a,c]=[c,a];lo=Math.max(lo,a);hi=Math.min(hi,c);if(lo>hi)return null;}}return lo;}
export function wallRay(o,d,max=100){let best=max;for(let b of B){let t=rayBox(o,d,b,best);if(t!==null)best=t;}return best;}
export function los(a,b){let dx=b.x-a.x,dy=(b.y+1)-(a.y+1),dz=b.z-a.z,l=Math.hypot(dx,dy,dz);return wallRay({x:a.x,y:a.y+1,z:a.z},{x:dx/l,y:dy/l,z:dz/l},l)>=l-.05;}
function overlap(p,b){return Math.abs(p.x-b.x)<b.sx/2+.36&&Math.abs(p.z-b.z)<b.sz/2+.36&&p.y<b.y+b.sy/2-.001&&p.y+1.65>b.y-b.sy/2+.001;}
export function move(p,input,dt){
 if(p.hp<=0)return;
 let f=(input.f||0),s=(input.s||0),len=Math.hypot(f,s);if(len>1){f/=len;s/=len;}
 let speed=input.crouch?4:9.4,wx=Math.sin(p.yaw)*f+Math.cos(p.yaw)*s,wz=-Math.cos(p.yaw)*f+Math.sin(p.yaw)*s;
 if(p.ground){let fr=Math.max(0,1-9*dt);p.vx*=fr;p.vz*=fr;}
 let acc=p.ground?75:23;
 let dot=p.vx*wx+p.vz*wz,add=Math.min(acc*dt,Math.max(0,speed-dot));p.vx+=wx*add;p.vz+=wz*add;
 let h=Math.hypot(p.vx,p.vz);if(h>15){p.vx*=15/h;p.vz*=15/h;}
 p.dodge=Math.max(0,p.dodge-dt);
 if(input.dodge&&p.ground&&p.dodge===0){let dl=Math.hypot(wx,wz)||1;p.vx=(wx||Math.sin(p.yaw))/dl*16;p.vz=(wz||-Math.cos(p.yaw))/dl*16;p.vy=3.6;p.ground=false;p.dodge=.9;}
 if(input.jump&&p.ground){p.vy=8;p.ground=false;}
 p.vy-=22*dt;
 let steps=Math.max(1,Math.ceil(Math.hypot(p.vx,p.vy,p.vz)*dt/.18)),ds=dt/steps;
 p.ground=false;
 for(let i=0;i<steps;i++){
  for(let axis of ['x','z']){let old=p[axis];p[axis]+=p['v'+axis]*ds;for(let b of B)if(overlap(p,b)){
   let top=b.y+b.sy/2;if(top-p.y<=.42&&top>=p.y-.01&&p.vy<=1){p.y=top;p.ground=true;}else{p[axis]=old;p['v'+axis]=0;break;}
  }}
  let old=p.y;p.y+=p.vy*ds;
  for(let b of B)if(overlap(p,b)){if(p.vy<=0&&old>=b.y+b.sy/2-.02){p.y=b.y+b.sy/2;p.vy=0;p.ground=true;}else if(p.vy>0){p.y=b.y-b.sy/2-1.65;p.vy=0;}}
 }
 if(p.y<.08&&p.vy<=0){p.y=0;p.vy=0;p.ground=true;}
 for(let pad of ARENA.pads)if(Math.hypot(p.x-pad.x,p.z-pad.z)<1.25&&p.y<.25&&p.vy<=0){p.vy=13;p.ground=false;}
}
export function makePlayer(id,name,bot=false){return {id,name,bot,x:0,y:0,z:0,vx:0,vy:0,vz:0,yaw:0,pitch:0,hp:100,armor:35,score:0,deaths:0,weapon:bot?1:0,ammo:WEAPONS.map(w=>w.ammo),ground:true,dodge:0,cd:0,respawn:0,invuln:0,input:{},lastSeq:0,path:[],nextThink:0,routeAge:0};}
function surface(x,z){let y=0;for(let b of B)if(Math.abs(x-b.x)<b.sx/2+.38&&Math.abs(z-b.z)<b.sz/2+.38)y=Math.max(y,b.y+b.sy/2);return y;}
// Navigation generated from the same collision solids. Steps/galleries are
// reachable only where the actual collision solver can traverse them.
const nodes=[];const nodeMap=new Map();
for(let ix=0;ix<45;ix++)for(let iz=0;iz<45;iz++){let x=ix-22,z=iz-22,y=surface(x,z);if(y>3.01)continue;let n={x,z,y,key:ix*45+iz,links:[]};nodes.push(n);nodeMap.set(n.key,n);}
for(let n of nodes)for(let off of [-45,45,-1,1]){let m=nodeMap.get(n.key+off);if(m&&Math.abs(n.x-m.x)+Math.abs(n.z-m.z)<1.1&&Math.abs(n.y-m.y)<=.42)n.links.push(m);}
const nearest=p=>{let best=null,d=Infinity;for(let n of nodes){let dd=(p.x-n.x)**2+(p.z-n.z)**2+4*(p.y-n.y)**2;if(dd<d){d=dd;best=n;}}return best;};
export function pathfind(a,b){let start=nearest(a),end=nearest(b);if(!start||!end)return [];let q=[start],prev=new Map([[start.key,null]]);for(let i=0;i<q.length;i++){let n=q[i];if(n===end)break;for(let m of n.links)if(!prev.has(m.key)){prev.set(m.key,n);q.push(m);}}if(!prev.has(end.key))return [];let out=[];for(let n=end;n&&n!==start;n=prev.get(n.key))out.push({x:n.x,y:n.y,z:n.z});return out.reverse();}
export class Game {
 constructor(seed=1701,bots=5){this.seed=seed>>>0;this.players=[];this.projectiles=[];this.events=[];this.eid=0;this.pid=0;this.time=0;this.round=480;this.fragLimit=20;this.winner=null;this.restart=0;this.pickups=[...[-18,18].flatMap(x=>[{x,y:0,z:0,type:'health'},{x,y:3,z:x<0?-18:18,type:'armor'}]),...WEAPONS.map((w,i)=>({x:Math.cos(i*TAU/6)*11,y:0,z:Math.sin(i*TAU/6)*11,type:'ammo',weapon:i}))].map((p,i)=>({...p,id:i,ready:0}));for(let i=0;i<bots;i++)this.add('bot'+i,['SABLE','VECTOR','HEX','CINDER','WRAITH','NOVA','FLINT','ONYX'][i%8],true);}
 rng=()=>{this.seed=(Math.imul(this.seed,1664525)+1013904223)>>>0;return this.seed/4294967296;};
 add(id,name,bot=false){if(this.players.some(p=>p.id===id))return null;let p=makePlayer(id,name,bot);this.players.push(p);this.spawn(p);return p;}
 spawn(p){let points=ARENA.spawns.map(a=>({x:a[0],y:a[1],z:a[2]}));points.sort((a,b)=>this.clearance(b,p)-this.clearance(a,p));let s=points[Math.floor(this.rng()*Math.min(3,points.length))];Object.assign(p,s,{hp:100,armor:35,vx:0,vy:0,vz:0,cd:.3,respawn:0,invuln:1,ground:true,path:[],ammo:WEAPONS.map(w=>w.ammo)});p.yaw=Math.atan2(-p.x,p.z);}
 clearance(s,me){return Math.min(999,...this.players.filter(p=>p!==me&&p.hp>0).map(p=>Math.hypot(s.x-p.x,s.z-p.z)));}
 emit(type,data){this.events.push({id:++this.eid,type,...data});if(this.events.length>180)this.events.shift();}
 damage(target,amount,owner,knock){if(target.hp<=0||target.invuln>0)return;if(knock){target.vx+=knock.x;target.vy+=knock.y;target.vz+=knock.z;}let absorbed=Math.min(target.armor,amount*.6);target.armor-=absorbed;target.hp-=amount-absorbed;this.emit('hit',{target:target.id,owner:owner.id,damage:amount});if(target.hp<=0){target.hp=0;target.deaths++;target.respawn=2;owner.score+=target===owner?-1:1;this.emit('kill',{owner:owner.id,target:target.id,weapon:owner.weapon});if(owner.score>=this.fragLimit)this.finish(owner);}}
 finish(p){if(this.winner)return;this.winner=p.name;this.restart=8;this.emit('round',{name:p.name});}
 explode(q){let owner=this.players.find(p=>p.id===q.owner);if(!owner)return;this.emit('boom',{x:q.x,y:q.y,z:q.z,r:5});for(let p of this.players){let dx=p.x-q.x,dy=p.y+.8-q.y,dz=p.z-q.z,d=Math.hypot(dx,dy,dz);if(d<5.3&&wallRay({x:q.x,y:q.y+.05,z:q.z},{x:dx/(d||1),y:dy/(d||1),z:dz/(d||1)},d)>=d-.15)this.damage(p,q.damage*(1-d/5.3)*(p===owner?.65:1),owner,{x:dx/(d||1)*9,y:5,z:dz/(d||1)*9});}}
 fire(p,alt=false){let w=WEAPONS[p.weapon];if(p.cd>0||p.hp<=0||this.winner)return;if(p.ammo[p.weapon]<=0){p.cd=.2;return;}let cost=alt&&(p.weapon===2)?2:1;if(p.ammo[p.weapon]<cost)return;p.ammo[p.weapon]-=cost;p.cd=w.delay*(alt&&p.weapon===1?.55:1);let origin={x:p.x,y:p.y+1.42,z:p.z};
 let spread=w.spread,pellets=w.pellets,damage=w.damage;if(alt&&p.weapon===0){spread*=.25;damage*=1.25;p.cd=.4;}if(alt&&p.weapon===1){spread=25;damage=12;}if(alt&&p.weapon===2){spread=16;pellets=16;p.cd=1.1;}
 let yaw=p.yaw,pitch=p.pitch;
 if(w.kind==='rocket'||w.kind==='grenade'){let d={x:Math.sin(yaw)*Math.cos(pitch),y:Math.sin(pitch),z:-Math.cos(yaw)*Math.cos(pitch)};let speed=w.kind==='rocket'?23:17;
 this.projectiles.push({id:++this.pid,owner:p.id,weapon:p.weapon,x:origin.x+d.x*.55,y:origin.y+d.y*.55,z:origin.z+d.z*.55,vx:d.x*speed,vy:d.y*speed+(w.kind==='grenade'?3:0),vz:d.z*speed,ttl:w.kind==='rocket'?5:2.5,age:0,damage,sticky:alt&&w.kind==='grenade',homing:alt&&w.kind==='rocket',stuck:false});this.emit('shot',{owner:p.id,weapon:p.weapon,alt,x:origin.x,y:origin.y,z:origin.z});return;}
 for(let i=0;i<pellets;i++){let ya=yaw+pdSpread(spread,this.rng,p.input.crouch,false),pi=pitch+pdSpread(spread,this.rng,p.input.crouch,false),d={x:Math.sin(ya)*Math.cos(pi),y:Math.sin(pi),z:-Math.cos(ya)*Math.cos(pi)},distance=wallRay(origin,d,100),hit=null;
 for(let target of this.players)if(target!==p&&target.hp>0){let t=rayBox(origin,d,{x:target.x,y:target.y+.85,z:target.z,sx:.8,sy:1.7,sz:.8},distance);if(t!==null){distance=t;hit=target;}}
 if(hit){let head=origin.y+d.y*distance>hit.y+1.32;this.damage(hit,damage*(head?1.4:1),p);}
 this.emit('trace',{owner:p.id,weapon:p.weapon,x:origin.x,y:origin.y,z:origin.z,ex:origin.x+d.x*distance,ey:origin.y+d.y*distance,ez:origin.z+d.z*distance});}
 this.emit('shot',{owner:p.id,weapon:p.weapon,alt,x:origin.x,y:origin.y,z:origin.z});}
 bot(p,dt){let targets=this.players.filter(q=>q!==p&&q.hp>0);if(!targets.length)return;targets.sort((a,b)=>Math.hypot(a.x-p.x,a.z-p.z)-Math.hypot(b.x-p.x,b.z-p.z));let t=targets[0],dist=Math.hypot(t.x-p.x,t.z-p.z),visible=los(p,t);p.nextThink-=dt;p.routeAge-=dt;
 if(p.nextThink<=0){p.nextThink=.2+this.rng()*.16;let want=dist<6?2:dist>19?3:this.rng()<.15?4:1;if(p.ammo[want]>0)p.weapon=want;}
 let input={f:0,s:0};if(visible&&dist<25){let desired=Math.atan2(t.x-p.x,-(t.z-p.z));let error=((desired-p.yaw+Math.PI*3)%TAU)-Math.PI;p.yaw+=clamp(error,-dt*3.8,dt*3.8);let aim=Math.atan2(t.y-p.y-.15,dist)+Math.sin(this.time*3+p.name.length)*.035;p.pitch+=clamp(aim-p.pitch,-dt*2,dt*2);input.s=Math.sin(this.time*.8+p.name.length)>0?.8:-.8;input.f=dist>13?.65:dist<5?-.6:0;input.fire=Math.abs(error)<.13&&p.invuln===0;input.alt=p.weapon===4&&dist>15;}
 else{if(p.routeAge<=0||!p.path.length){p.path=pathfind(p,t);p.routeAge=.8;}while(p.path.length&&Math.hypot(p.x-p.path[0].x,p.z-p.path[0].z)<.6)p.path.shift();let n=p.path[0];if(n){let ya=Math.atan2(n.x-p.x,-(n.z-p.z));p.yaw=ya;p.pitch=0;input.f=1;}}
 p.input=input;
 }
 tick(dt=DT){this.time+=dt;if(this.winner){this.restart-=dt;if(this.restart<=0){this.winner=null;this.round=480;for(let p of this.players){p.score=0;p.deaths=0;this.spawn(p);}}return;}this.round-=dt;if(this.round<=0)this.finish([...this.players].sort((a,b)=>b.score-a.score)[0]||{name:'Nobody'});
 for(let p of this.players){if(p.hp<=0){p.respawn-=dt;if(p.respawn<=0)this.spawn(p);continue;}p.cd=Math.max(0,p.cd-dt);p.invuln=Math.max(0,p.invuln-dt);if(p.bot)this.bot(p,dt);let input=p.input;p.yaw=Number.isFinite(input.yaw)?input.yaw:p.yaw;p.pitch=Number.isFinite(input.pitch)?clamp(input.pitch,-1.45,1.45):p.pitch;if(Number.isInteger(input.weapon))p.weapon=clamp(input.weapon,0,5);move(p,input,dt);if(input.fire||(input.alt&&p.weapon!==3))this.fire(p,!!input.alt);}
 for(let q of this.projectiles){q.ttl-=dt;q.age+=dt;let owner=this.players.find(p=>p.id===q.owner);if(!owner){q.ttl=-1;continue;}
 if(q.homing){let targets=this.players.filter(p=>p!==owner&&p.hp>0&&los(q,p)).sort((a,b)=>Math.hypot(a.x-q.x,a.z-q.z)-Math.hypot(b.x-q.x,b.z-q.z));let t=targets[0];if(t){let dx=t.x-q.x,dy=t.y+.8-q.y,dz=t.z-q.z,d=Math.hypot(dx,dy,dz);let k=Math.min(1,dt*2.2);q.vx+=(dx/d*20-q.vx)*k;q.vy+=(dy/d*20-q.vy)*k;q.vz+=(dz/d*20-q.vz)*k;}}
 if(!q.stuck){if(q.weapon===5)q.vy-=15*dt;let vel={x:q.vx,y:q.vy,z:q.vz},l=Math.hypot(vel.x,vel.y,vel.z),d={x:vel.x/l,y:vel.y/l,z:vel.z/l};let hit=wallRay(q,d,l*dt+.12);if(hit<l*dt+.12){if(q.weapon===5){if(q.sticky){q.stuck=true;q.ttl=12;}else{q.vy=Math.abs(q.vy)*.6;q.vx*=-.65;q.vz*=-.65;}}else q.ttl=0;}else{q.x+=q.vx*dt;q.y+=q.vy*dt;q.z+=q.vz*dt;}}
 for(let p of this.players)if(p.hp>0&&(p!==owner||q.age>.5)&&Math.hypot(p.x-q.x,p.y+.8-q.y,p.z-q.z)<(q.stuck?2.2:.8))q.ttl=0;
 if(q.ttl<=0)this.explode(q);}
 this.projectiles=this.projectiles.filter(q=>q.ttl>0);
 for(let pick of this.pickups){if(pick.ready>this.time)continue;for(let p of this.players)if(p.hp>0&&Math.hypot(p.x-pick.x,p.y-pick.y,p.z-pick.z)<1.2){let used=false;if(pick.type==='health'&&p.hp<100){p.hp=Math.min(100,p.hp+35);used=true;}if(pick.type==='armor'&&p.armor<100){p.armor=Math.min(100,p.armor+50);used=true;}if(pick.type==='ammo'&&p.ammo[pick.weapon]<WEAPONS[pick.weapon].ammo){p.ammo[pick.weapon]=Math.min(WEAPONS[pick.weapon].ammo,p.ammo[pick.weapon]+Math.ceil(WEAPONS[pick.weapon].ammo*.3));used=true;}if(used){pick.ready=this.time+12;this.emit('pickup',{owner:p.id,type:pick.type});break;}}}
 }
 snapshot(){return {time:this.time,round:this.round,winner:this.winner,restart:this.restart,players:this.players.map(({path,input,...p})=>p),projectiles:this.projectiles,pickups:this.pickups,events:this.events.slice(-100)};}
}
