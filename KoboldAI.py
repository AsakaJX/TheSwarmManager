import json
import requests
import sys

if __name__ == "__main__":
    print(f"\nArguments count: {len(sys.argv)}\n")
    for i, arg in enumerate(sys.argv):
        print(f"Argument {i:>6}: {arg}\n")

url = "https://giant-clouds-happen-34-142-231-208.loca.lt/api/v1"

payload = {
    "prompt": sys.argv[1],
    "temperature": 0.5,
    "top_p": 0.9
}

response = requests.post(url=f'{url}/generate', json=payload)

r = response.json()

for i in r["results"]:
    print(f"{i}")