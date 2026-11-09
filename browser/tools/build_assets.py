"""Regenerate cached PNGs with C++ OpenKTG; Python standard library only."""
import pathlib,subprocess,zlib,struct
r=pathlib.Path(__file__).resolve().parents[1]
(r/'build').mkdir(exist_ok=True)
subprocess.run(['g++','-O2','-std=c++11','-I'+str(r/'vendor/openktg'),str(r/'tools/materials.cpp'),str(r/'vendor/openktg/gentexture.cpp'),'-o',str(r/'build/materials')],check=True)
subprocess.run([str(r/'build/materials'),str(r/'assets')],check=True)
def chunk(t,d):return struct.pack('!I',len(d))+t+d+struct.pack('!I',zlib.crc32(t+d)&0xffffffff)
for p in (r/'assets').glob('*.ppm'):
 data=p.read_bytes().split(b'\n',3)[3]; raw=b''.join(b'\0'+data[y*768:(y+1)*768] for y in range(256))
 png=b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('!2I5B',256,256,8,2,0,0,0))+chunk(b'IDAT',zlib.compress(raw))+chunk(b'IEND',b'')
 p.with_suffix('.png').write_bytes(png);p.unlink()
print('Generated four OpenKTG materials')
