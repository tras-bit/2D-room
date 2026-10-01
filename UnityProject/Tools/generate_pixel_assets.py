#!/usr/bin/env python3
"""Prepare an editable Piskel sprite sheet and crisp Unity raster asset.

Individual models use a fixed side-view, retro-industrial palette and hard pixel
clusters. This is a starter sheet for touch-up in Piskel, not a painted scene.
"""
from __future__ import annotations
import base64, json, struct, zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets" / "Resources" / "PixelArt"
SOURCE = ROOT / "ArtSource"
ASSETS.mkdir(parents=True, exist_ok=True)
SOURCE.mkdir(parents=True, exist_ok=True)

# Limited, deliberately stepped palette: soot, oxidized green, concrete and sodium light.
INK=(8,14,13,255); DEEP=(13,22,20,255); STEEL=(26,40,37,255); TILE_A=(39,58,50,255)
TILE_B=(47,65,55,255); TILE_C=(56,69,56,255); GROUT=(17,29,27,255)
RUST=(102,48,35,255); RUST_L=(153,66,43,255); AMBER=(194,135,61,255)
GLOW=(240,196,109,255); CONCRETE=(58,63,53,255); CONCRETE_L=(78,79,63,255)
WET=(35,62,60,255); CREAM=(180,165,120,255); PALE=(208,193,143,255)

class Canvas:
    def __init__(self,w,h,color=INK): self.w,self.h=w,h; self.p=[color]*(w*h)
    def rect(self,x,y,w,h,c):
        for yy in range(max(0,y),min(self.h,y+h)):
            a=yy*self.w+max(0,x); b=yy*self.w+min(self.w,x+w)
            if b>a:self.p[a:b]=[c]*(b-a)
    def px(self,x,y,c):
        if 0<=x<self.w and 0<=y<self.h:self.p[y*self.w+x]=c
    def hline(self,x,y,w,c):self.rect(x,y,w,1,c)
    def vline(self,x,y,h,c):self.rect(x,y,1,h,c)
    def tile(self,x,y,w,h,base,light,dark,grout):
        self.rect(x,y,w,h,base);self.rect(x,y,w,1,grout);self.rect(x,y,1,h,grout)
        self.rect(x+2,y+2,max(1,w-5),1,light);self.rect(x+2,y+3,1,max(1,h-6),dark)
        self.rect(x+w-2,y+2,1,max(1,h-4),dark)
    def png(self,path):
        def chunk(kind,data):return struct.pack('>I',len(data))+kind+data+struct.pack('>I',zlib.crc32(kind+data)&0xffffffff)
        raw=bytearray()
        for y in range(self.h):
            raw.append(0)
            for r,g,b,a in self.p[y*self.w:(y+1)*self.w]:raw.extend((r,g,b,a))
        data=b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>2I5B',self.w,self.h,8,6,0,0,0))+chunk(b'IDAT',zlib.compress(bytes(raw),9))+chunk(b'IEND',b'')
        Path(path).write_bytes(data)
    def piskel(self,path,name):
        self.png(Path(path).with_suffix('.frame.png'))
        image=Path(path).with_suffix('.frame.png').read_bytes()
        layer={'name':'Painted pixels','opacity':1,'frameCount':1,'chunks':[{'layout':[[0]],'base64PNG':'data:image/png;base64,'+base64.b64encode(image).decode()}]}
        project={'modelVersion':2,'piskel':{'name':name,'description':'Editable individual-model sprite sheet · bunker-inspired 2D side-view style','fps':8,'height':self.h,'width':self.w,'layers':[json.dumps(layer,separators=(',',':'))],'hiddenFrames':[]}}
        Path(path).write_text(json.dumps(project,separators=(',',':')),encoding='utf-8')
        Path(path).with_suffix('.frame.png').unlink()


def text(c,x,y,value,color,scale=1):
    font={
      'A':['010','101','111','101','101'],'B':['110','101','110','101','110'],'C':['011','100','100','100','011'],
      'D':['110','101','101','101','110'],'E':['111','100','110','100','111'],'F':['111','100','110','100','100'],
      'G':['011','100','101','101','011'],'H':['101','101','111','101','101'],'I':['111','010','010','010','111'],
      'K':['101','101','110','101','101'],'L':['100','100','100','100','111'],'M':['101','111','111','101','101'],
      'N':['101','111','111','111','101'],'O':['010','101','101','101','010'],'P':['110','101','110','100','100'],
      'R':['110','101','110','101','101'],'S':['011','100','010','001','110'],'T':['111','010','010','010','010'],
      'U':['101','101','101','101','111'],'V':['101','101','101','101','010'],'W':['101','101','111','111','101'],
      'Y':['101','101','010','010','010'],'0':['111','101','101','101','111'],'1':['010','110','010','010','111'],
      '2':['110','001','010','100','111'],'3':['110','001','010','001','110'],'4':['101','101','111','001','001'],
      '5':['111','100','110','001','110'],'6':['011','100','110','101','010'],'7':['111','001','010','010','010'],
      '8':['010','101','010','101','010'],'9':['010','101','011','001','110'],':':['000','010','000','010','000'],'-':['000','000','111','000','000']}
    cx=x
    for ch in value.upper():
        glyph=font.get(ch)
        if glyph:
            for gy,row in enumerate(glyph):
                for gx,v in enumerate(row):
                    if v=='1':c.rect(cx+gx*scale,y+gy*scale,scale,scale,color)
        cx+=4*scale


def models_sheet():
    """Individual sprite models for editing in Piskel; the reference stays style-only."""
    c=Canvas(384,96,(0,0,0,0))

    # 01 · stranded maintenance survivor — layered boots, field jacket, pack and face.
    x=4;y=10
    c.rect(x+5,y+44,17,2,INK);c.rect(x+6,y+45,14,1,CONCRETE)
    c.rect(x+6,y+38,7,7,DEEP);c.rect(x+15,y+38,7,7,DEEP)
    c.rect(x+7,y+42,7,2,RUST);c.rect(x+16,y+42,7,2,RUST)
    c.rect(x+7,y+26,6,13,(66,78,65,255));c.rect(x+15,y+26,6,13,(59,71,61,255))
    c.rect(x+8,y+28,2,7,CONCRETE_L);c.rect(x+17,y+29,2,5,CONCRETE_L)
    c.rect(x+6,y+24,17,3,DEEP);c.rect(x+8,y+24,13,2,RUST)
    c.rect(x+5,y+13,19,12,DEEP);c.rect(x+7,y+14,15,10,(78,91,68,255))
    c.rect(x+4,y+15,4,9,DEEP);c.rect(x+5,y+16,3,7,(78,91,68,255))
    c.rect(x+21,y+15,4,9,DEEP);c.rect(x+22,y+16,3,7,(68,81,64,255))
    c.rect(x+10,y+15,2,9,(131,103,67,255));c.rect(x+8,y+18,4,4,RUST_L)
    c.rect(x+18,y+18,3,4,AMBER);c.px(x+19,y+19,DEEP)
    c.rect(x+11,y+10,6,4,(170,128,91,255));c.rect(x+8,y+3,13,9,DEEP)
    c.rect(x+9,y+4,11,7,(176,130,91,255));c.rect(x+8,y+3,13,3,(48,43,35,255))
    c.rect(x+7,y+4,3,6,(48,43,35,255));c.rect(x+18,y+7,3,2,INK);c.px(x+20,y+7,PALE)
    c.rect(x+6,y+2,6,2,RUST_L);c.rect(x+20,y+12,2,2,AMBER)

    # 02 · sealed survey worker, separate silhouette and high-contrast visor.
    x=37;y=10
    c.rect(x+5,y+44,18,2,INK);c.rect(x+7,y+45,14,1,CONCRETE)
    c.rect(x+7,y+37,6,8,DEEP);c.rect(x+16,y+37,6,8,DEEP)
    c.rect(x+7,y+26,15,13,DEEP);c.rect(x+8,y+27,13,10,(133,113,49,255))
    c.rect(x+7,y+28,3,7,AMBER);c.rect(x+20,y+28,2,7,AMBER)
    c.rect(x+5,y+15,20,13,DEEP);c.rect(x+7,y+16,16,10,(167,139,55,255))
    c.rect(x+4,y+16,4,10,DEEP);c.rect(x+5,y+18,3,6,(156,128,51,255))
    c.rect(x+23,y+16,4,10,DEEP);c.rect(x+23,y+18,3,6,(139,113,47,255))
    c.rect(x+9,y+19,12,3,(205,177,83,255));c.rect(x+12,y+20,7,1,INK)
    c.rect(x+11,y+11,8,6,DEEP);c.rect(x+8,y+5,14,9,DEEP)
    c.rect(x+10,y+7,11,6,(152,125,50,255));c.rect(x+9,y+5,14,3,(192,157,57,255))
    c.rect(x+10,y+8,11,3,(42,58,54,255));c.rect(x+12,y+9,7,1,WET)
    c.rect(x+7,y+2,18,3,DEEP);c.rect(x+9,y+1,14,2,AMBER);c.px(x+24,y+6,RUST_L)

    # 03 · the watcher: asymmetric, slumped human outline with impossible eyes.
    x=73;y=10
    c.rect(x+5,y+44,19,2,INK);c.rect(x+8,y+45,14,1,STEEL)
    c.rect(x+7,y+34,7,11,DEEP);c.rect(x+16,y+35,7,10,DEEP)
    c.rect(x+5,y+17,21,19,DEEP);c.rect(x+7,y+18,17,15,(72,84,64,255))
    c.rect(x+4,y+19,4,13,DEEP);c.rect(x+3,y+27,4,7,(83,95,69,255))
    c.rect(x+23,y+19,4,12,DEEP);c.rect(x+24,y+27,4,7,(80,91,66,255))
    c.rect(x+8,y+21,12,2,(104,112,78,255));c.rect(x+11,y+25,11,2,(47,57,45,255))
    c.rect(x+8,y+8,16,12,DEEP);c.rect(x+10,y+10,13,9,(105,115,79,255))
    c.rect(x+8,y+6,17,5,DEEP);c.rect(x+10,y+5,12,3,STEEL)
    c.rect(x+12,y+13,3,3,AMBER);c.rect(x+19,y+14,3,3,RUST_L)
    c.rect(x+14,y+18,8,2,INK);c.rect(x+12,y+20,3,3,DEEP)
    c.rect(x+5,y+23,3,2,RUST_L);c.rect(x+24,y+22,3,3,RUST_L)

    # 04 · security door asset, isolated with its own outline, bolts and warm lock strip.
    x=111;y=8
    c.rect(x+2,y,40,66,INK);c.rect(x+5,y+3,34,60,STEEL)
    c.rect(x+7,y+5,30,56,RUST);c.rect(x+9,y+7,26,52,(79,49,37,255))
    c.rect(x+12,y+11,20,43,DEEP);c.rect(x+14,y+14,16,38,(26,41,36,255))
    c.rect(x+16,y+18,12,31,(17,31,29,255));c.rect(x+18,y+22,8,22,(26,42,36,255))
    c.rect(x+8,y+6,28,2,AMBER);c.rect(x+8,y+56,28,3,AMBER);c.rect(x+10,y+58,24,1,GLOW)
    c.rect(x+33,y+25,2,10,AMBER);c.rect(x+34,y+27,1,6,GLOW)
    for yy in (9,20,33,46,58):
        c.px(x+6,yy,CREAM);c.px(x+36,yy,CREAM)
    c.rect(x+18,y+10,8,2,RUST_L);c.px(x+21,y+11,GLOW)

    # 05 · crate and caged wall lamp as individual inventory/environment models.
    x=163;y=47
    c.rect(x,y,23,24,INK);c.rect(x+2,y+2,19,20,(81,70,49,255))
    c.rect(x+4,y+4,3,16,(111,87,54,255));c.rect(x+16,y+4,3,16,(105,82,52,255))
    c.rect(x+4,y+10,15,3,STEEL);c.rect(x+9,y+8,5,6,RUST_L);c.rect(x+10,y+9,3,3,AMBER)
    c.rect(x+2,y+20,19,2,STEEL);c.px(x+3,y+21,CREAM);c.px(x+19,y+21,CREAM)
    x=163;y=15
    c.rect(x+2,y,19,3,INK);c.rect(x+4,y+2,15,4,STEEL)
    c.rect(x+6,y+6,11,2,AMBER);c.rect(x+8,y+8,7,2,GLOW)
    c.rect(x+10,y+10,3,5,RUST);c.rect(x+8,y+14,7,2,STEEL)

    # 06 · corridor trader — layered coat, hood, pack and a green deal marker.
    x=200;y=10
    c.rect(x+5,y+44,22,2,INK);c.rect(x+8,y+42,6,3,STEEL);c.rect(x+19,y+42,6,3,STEEL)
    c.rect(x+7,y+26,7,17,DEEP);c.rect(x+17,y+26,7,17,DEEP)
    c.rect(x+8,y+28,5,13,(72,84,66,255));c.rect(x+18,y+28,4,13,(65,78,61,255))
    c.rect(x+5,y+14,22,16,DEEP);c.rect(x+7,y+16,18,12,(67,83,63,255))
    c.rect(x+4,y+16,5,13,INK);c.rect(x+24,y+16,5,13,INK)
    c.rect(x+9,y+17,3,10,(133,91,59,255));c.rect(x+21,y+17,3,10,(110,76,51,255))
    c.rect(x+12,y+11,8,5,(170,128,91,255));c.rect(x+9,y+4,15,10,DEEP)
    c.rect(x+10,y+5,13,7,(178,137,99,255));c.rect(x+8,y+3,17,4,STEEL)
    c.rect(x+11,y+8,10,3,(57,43,34,255));c.rect(x+19,y+8,2,2,CREAM)
    c.rect(x+7,y+21,19,3,RUST);c.rect(x+22,y+23,4,7,(105,77,46,255))
    c.rect(x+23,y+19,4,4,(57,128,74,255));c.rect(x+24,y+20,2,2,(142,199,106,255))

    # 07 · paired side-view elevator-door states: closed steel leaves, then the open bay.
    x=236;y=8
    c.rect(x+2,y,38,64,INK);c.rect(x+5,y+3,32,58,STEEL);c.rect(x+7,y+5,28,54,RUST)
    c.rect(x+9,y+7,24,50,(34,44,38,255));c.rect(x+10,y+9,11,46,(79,91,72,255))
    c.rect(x+22,y+9,10,46,(65,79,65,255));c.rect(x+20,y+9,2,46,DEEP)
    c.rect(x+12,y+14,6,2,CONCRETE_L);c.rect(x+24,y+14,5,2,CONCRETE_L)
    c.rect(x+12,y+47,6,2,STEEL);c.rect(x+24,y+47,5,2,STEEL)
    c.rect(x+34,y+26,2,10,(86,177,91,255));c.rect(x+35,y+27,1,4,(177,222,122,255))
    for yy in (12,25,39,53,58):c.px(x+6,yy,CREAM);c.px(x+36,yy,CREAM)

    x=280;y=8
    c.rect(x+2,y,38,64,INK);c.rect(x+5,y+3,32,58,STEEL);c.rect(x+7,y+5,28,54,RUST)
    c.rect(x+13,y+8,16,48,INK);c.rect(x+15,y+10,12,43,DEEP)
    c.rect(x+8,y+8,5,48,(76,89,71,255));c.rect(x+29,y+8,5,48,(68,82,67,255))
    c.rect(x+9,y+11,2,40,CONCRETE_L);c.rect(x+31,y+11,2,40,CONCRETE_L)
    c.rect(x+16,y+15,10,1,STEEL);c.rect(x+16,y+50,10,2,AMBER)
    c.rect(x+34,y+26,2,10,(86,177,91,255));c.rect(x+35,y+27,1,4,(177,222,122,255))
    for yy in (12,25,39,53,58):c.px(x+6,yy,CREAM);c.px(x+36,yy,CREAM)

    # 08 · green access card, shown separately for readable UI/world pickup art.
    x=333;y=27
    c.rect(x,y,19,27,INK);c.rect(x+2,y+2,15,23,(47,115,69,255))
    c.rect(x+4,y+4,11,3,(118,173,92,255));c.rect(x+4,y+10,10,2,(36,76,51,255))
    c.rect(x+4,y+15,8,2,(36,76,51,255));c.rect(x+13,y+19,2,3,(193,179,117,255))

    return c



if __name__=='__main__':
    artwork=models_sheet()
    artwork.png(ASSETS/'individual_models.png')
    artwork.piskel(SOURCE/'individual_models.piskel','SUBSISTENCE · Individual pixel models')
    print(f'Created individual sprite sheet {ASSETS / "individual_models.png"} and editable Piskel project {SOURCE / "individual_models.piskel"}')
