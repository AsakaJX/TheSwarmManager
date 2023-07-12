import json
import requests
import io
import base64
import sys
import os

if __name__ == "__main__":
    print(f"\nArguments count: {len(sys.argv)}\n")
    for i, arg in enumerate(sys.argv):
        print(f"Argument {i:>6}: {arg}\n")

url = "http://127.0.0.1:7860"

getConfig = requests.get(url=f'{url}/sdapi/v1/options').json()
getConfigModel = getConfig["sd_model_checkpoint"]

getSDModels = requests.get(url=f'{url}/sdapi/v1/sd-models').json()
getSDModelsArray = [None] * len(getSDModels)

file = open("SDModelsList", "w")

for i in range(len(getSDModels)):
    modelStr = ""
    for j in getSDModels[i]['title']:
        modelStr+=j
    getSDModelsArray[i] = modelStr
    file.write(modelStr + "\n")
    print(getConfig)

file.close()

updatefile = open("newSDModel", "r+")
if os.stat("newSDModel").st_size == 0:
    print("newSDModel file is empty!")
    exit()

updatefile.close()
    


# print(getConfig["sd_model_checkpoint"])

# for i in r['images']:
#     image = Image.open(io.BytesIO(base64.b64decode(i.split(",",1)[0])))

#     png_payload = {
#         "image": "data:image/png;base64," + i
#     }
#     response2 = requests.post(url=f'{url}/sdapi/v1/png-info', json=png_payload)

#     pnginfo = PngImagePlugin.PngInfo()
#     pnginfo.add_text("parameters", response2.json().get("info"))
#     image.save(f"Resources/StableDiffusionOutput/{sys.argv[1]}.png", pnginfo=pnginfo)