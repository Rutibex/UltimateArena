// Original material recipes using Fabian Giesen's unmodified OpenKTG.
#include "gentexture.hpp"
#include <cstdio>
#include <cmath>
#include <string>
int main(int argc,char**argv) {
 InitTexgen();
 const char* names[]={"alloy","floor","ceramic","rust"};
 unsigned colors[][2]={{0xff253a43,0xff8ba5a7},{0xff101c28,0xff425369},{0xff535966,0xffbac3bf},{0xff331b14,0xffa87543}};
 for(int k=0;k<4;k++) {
  GenTexture grad(2,1); grad.Data[0].Init(colors[k][0]);grad.Data[1].Init(colors[k][1]);
  GenTexture tex(256,256); tex.Noise(grad,4,4,6,.57f,1701+k,GenTexture::NoiseBandlimit|GenTexture::NoiseNormalize);
  std::string path=std::string(argc>1?argv[1]:"assets")+"/"+names[k]+".ppm";
  FILE*f=fopen(path.c_str(),"wb"); if(!f)return 1;fprintf(f,"P6\n256 256\n255\n");
  for(int y=0;y<256;y++)for(int x=0;x<256;x++) {
   Pixel p=tex.Data[y*256+x]; float fct=1;
   bool seam=(x%128<3 || y%128<3);
   if(k==1) {seam=(x%64<2||y%64<2); if((x+y)%16<3)fct=.78f;}
   if(seam)fct=.32f;
   if((x%128==7||x%128==120)&&(y%128==7||y%128==120))fct=1.7f;
   unsigned char rgb[3];unsigned v[]={p.r,p.g,p.b};for(int c=0;c<3;c++)rgb[c]=(unsigned char)fmin(255,v[c]/256*fct);fwrite(rgb,1,3,f);
  }fclose(f);
 }
}
