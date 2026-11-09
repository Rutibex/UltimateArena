import ctypes as C,json,os
from pathlib import Path
os.chdir(Path(__file__).resolve().parents[2])
os.environ['EGL_PLATFORM']='surfaceless'
e=C.CDLL('libEGL.so.1');e.eglGetProcAddress.restype=C.c_void_p;e.eglGetProcAddress.argtypes=[C.c_char_p]
def ef(n,rest,args):
 f=getattr(e,n);f.restype=rest;f.argtypes=args;return f
ptr=C.c_void_p;I=C.c_int;U=C.c_uint;F=C.c_float;S=C.c_char_p
getdisplay=ef('eglGetDisplay',ptr,[ptr]);d=getdisplay(None)
init=ef('eglInitialize',U,[ptr,C.POINTER(I),C.POINTER(I)]);ma=I();mi=I();assert init(d,C.byref(ma),C.byref(mi))
ef('eglBindAPI',U,[U])(0x30A0)
attrs=(I*15)(0x3024,8,0x3023,8,0x3022,8,0x3025,24,0x3033,1,0x3040,0x40,0x3038,0,0)
config=ptr();num=I();assert ef('eglChooseConfig',U,[ptr,C.POINTER(I),C.POINTER(ptr),I,C.POINTER(I)])(d,attrs,C.byref(config),1,C.byref(num)) and num.value
pa=(I*5)(0x3057,1440,0x3056,900,0x3038);surf=ef('eglCreatePbufferSurface',ptr,[ptr,ptr,C.POINTER(I)])(d,config,pa);ca=(I*3)(0x3098,3,0x3038);ctx=ef('eglCreateContext',ptr,[ptr,ptr,ptr,C.POINTER(I)])(d,config,None,ca);assert ef('eglMakeCurrent',U,[ptr,ptr,ptr,ptr])(d,surf,surf,ctx)
def gl(n,rest,args):return C.CFUNCTYPE(rest,*args)(e.eglGetProcAddress(('gl'+n).encode()))
fns={}
def call(n,args,types,rest=None):
 k=n+str(types)
 if k not in fns:fns[k]=gl(n,rest,types)
 return fns[k](*args)
objects={};program=0;texindex=-1
for item in json.load(open('build/render-calls.json')):
 n=item['fn'];a=item['args'];ret=item.get('ret')
 if n=='createShader':objects[ret]=call('CreateShader',a,[U],U)
 elif n=='shaderSource':
  src=S(a[1].encode());call('ShaderSource',[objects[a[0]],1,C.byref(src),None],[U,I,C.POINTER(S),ptr])
 elif n=='compileShader':
  sh=objects[a[0]];call('CompileShader',[sh],[U]);status=I();call('GetShaderiv',[sh,0x8B81,C.byref(status)],[U,U,C.POINTER(I)])
  if not status.value:
   log=C.create_string_buffer(8192);call('GetShaderInfoLog',[sh,8192,None,log],[U,I,ptr,ptr]);raise Exception(log.value.decode())
 elif n=='createProgram':objects[ret]=call('CreateProgram',[],[],U)
 elif n=='attachShader':call('AttachShader',[objects[a[0]],objects[a[1]]],[U,U])
 elif n=='linkProgram':
  program=objects[a[0]]
  for i,name in enumerate(['aPos','aNormal','aUV']):call('BindAttribLocation',[program,i,name.encode()],[U,U,S])
  call('LinkProgram',[program],[U]);status=I();call('GetProgramiv',[program,0x8B82,C.byref(status)],[U,U,C.POINTER(I)]);assert status.value,'link failed'
 elif n=='useProgram':call('UseProgram',[objects[a[0]]],[U])
 elif n in ['createVertexArray','createBuffer','createTexture']:
  val=U();func={'createVertexArray':'GenVertexArrays','createBuffer':'GenBuffers','createTexture':'GenTextures'}[n];call(func,[1,C.byref(val)],[I,C.POINTER(U)]);objects[ret]=val.value
 elif n=='bindVertexArray':call('BindVertexArray',[objects[a[0]]],[U])
 elif n in ['bindBuffer','bindTexture']:call(n[0].upper()+n[1:],[a[0],objects[a[1]]],[U,U])
 elif n=='bufferData':
  v=(F*len(a[1]))(*a[1]);call('BufferData',[a[0],C.sizeof(v),v,a[2]],[U,C.c_ssize_t,ptr,U])
 elif n=='enableVertexAttribArray':call('EnableVertexAttribArray',a,[U])
 elif n=='vertexAttribPointer':call('VertexAttribPointer',[*a[:5],ptr(a[5])],[U,I,U,U,I,ptr])
 elif n=='activeTexture':texindex=a[0]-33984;call('ActiveTexture',a,[U])
 elif n=='texImage2D':
  from PIL import Image
  image=Image.open('assets/'+['alloy','floor','ceramic','rust'][texindex]+'.png').convert('RGBA');b=image.tobytes();buf=C.create_string_buffer(b);call('TexImage2D',[3553,0,6408,256,256,0,6408,5121,buf],[U,I,I,I,I,I,U,U,ptr]);call('GenerateMipmap',[3553],[U])
 elif n=='texParameteri':call('TexParameteri',a,[U,U,I])
 elif n in ['uniform1i','uniform1f','uniform3f','uniform3fv','uniformMatrix4fv']:
  loc=call('GetUniformLocation',[program,a[0].encode()],[U,S],I)
  if n=='uniform1i':call('Uniform1i',[loc,a[1]],[I,I])
  elif n=='uniform1f':call('Uniform1f',[loc,a[1]],[I,F])
  elif n=='uniform3f':call('Uniform3f',[loc,*a[1:]],[I,F,F,F])
  elif n=='uniform3fv':call('Uniform3fv',[loc,1,(F*3)(*a[1])],[I,I,C.POINTER(F)])
  else:call('UniformMatrix4fv',[loc,1,0,(F*16)(*a[2])],[I,I,U,C.POINTER(F)])
 elif n in ['enable','clear']:call(n[0].upper()+n[1:],a,[U])
 elif n=='viewport':call('Viewport',a,[I,I,I,I])
 elif n=='clearColor':call('ClearColor',a,[F,F,F,F])
 elif n=='drawArrays':call('DrawArrays',a,[U,I,I])
 else:print('UNHANDLED',n)
err=call('GetError',[],[],U);print('GL error',err);assert err==0
buf=(C.c_ubyte*(1440*900*4))();call('ReadPixels',[0,0,1440,900,6408,5121,buf],[I,I,I,I,U,U,ptr]);from PIL import Image
im=Image.frombytes('RGBA',(1440,900),bytes(buf)).transpose(Image.Transpose.FLIP_TOP_BOTTOM);im.save('build/render-check.png');print('GLSL compiled, rendered 1440x900 without GL errors')
