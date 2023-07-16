import json
import requests
import io
import base64
import sys

from PIL import Image, PngImagePlugin

if __name__ == "__main__":
    print(f"\nArguments count: {len(sys.argv)}\n")
    for i, arg in enumerate(sys.argv):
        print(f"Argument {i:>6}: {arg}\n")

url = "http://127.0.0.1:7860"

imageName = sys.argv[1].replace('.png', '')

with open(sys.argv[1], "rb") as imageFile:
    inputImage = base64.b64encode(imageFile.read()).decode("utf-8") 
    
payload = {
    "image": inputImage,
    "upscaler_1": "R-ESRGAN 4x+ Anime6B",
    "resize_mode": 0,
    "upscaling_resize": sys.argv[2],
}

response = requests.post(url=f'{url}/sdapi/v1/extra-single-image', json=payload)

r = response.json()

i = r['image']
image = Image.open(io.BytesIO(base64.b64decode(i.split(",",1)[0])))

png_payload = {
    "image": "data:image/png;base64," + i
}
response2 = requests.post(url=f'{url}/sdapi/v1/png-info', json=png_payload)

pnginfo = PngImagePlugin.PngInfo()
pnginfo.add_text("parameters", response2.json().get("info"))
image.save(f"{imageName}_upscaled.png", pnginfo=pnginfo)