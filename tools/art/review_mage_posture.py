"""Plansza diagnostyczna postawy: niezmienione kadry Unity przed i po korekcie."""
from pathlib import Path
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[2]
DIRECTORY=ROOT/'TestResults/mage-posture'
views=[('idle','side'),('idle','front'),('run','side')]
sheet=Image.new('RGB',(1200,len(views)*526),(24,27,34))
draw=ImageDraw.Draw(sheet)
for row,(pose,view) in enumerate(views):
    for column,state in enumerate(['before','after']):
        image=Image.open(DIRECTORY/f'wand-{pose}-{view}-{state}.png').convert('RGB')
        image.thumbnail((600,500),Image.Resampling.LANCZOS)
        sheet.paste(image,(column*600,row*526+26))
        draw.text((12+column*600,row*526+7),f'{pose} / {view} / '+('PRZED' if state=='before' else 'PO KOREKCIE'),fill=(232,220,197))
sheet.save(DIRECTORY/'comparison.jpg',quality=95)
print(DIRECTORY/'comparison.jpg')
