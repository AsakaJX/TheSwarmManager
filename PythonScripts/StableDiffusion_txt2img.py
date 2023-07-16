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

payload = {
    "prompt": sys.argv[2],
    "negative_prompt": "EasyNegative, " + sys.argv[3],
    "sampler_name": "DPM++ 2M Karras",
    "steps": 20,
    "cfg_scale": 10,
    "width": sys.argv[4],
    "height": sys.argv[5],
    "seed": sys.argv[6],
    "hr_scale": 2,
    "hr_upscaler": "R-ESRGAN 4x+ Anime6B",
    "hr_second_pass_steps": 20,
    "denoising_strength": 0.7,
}

response = requests.post(url=f'{url}/sdapi/v1/txt2img', json=payload)

r = response.json()

for i in r['images']:
    image = Image.open(io.BytesIO(base64.b64decode(i.split(",",1)[0])))

    png_payload = {
        "image": "data:image/png;base64," + i
    }
    response2 = requests.post(url=f'{url}/sdapi/v1/png-info', json=png_payload)

    pnginfo = PngImagePlugin.PngInfo()
    pnginfo.add_text("parameters", response2.json().get("info"))
    image.save(f"Resources/StableDiffusionOutput/{sys.argv[1]}.png", pnginfo=pnginfo)