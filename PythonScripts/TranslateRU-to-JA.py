import os; os.environ['TF_CPP_MIN_LOG_LEVEL'] = '1'
from transformers import AutoTokenizer, AutoModelForSeq2SeqLM
import sys
import json
import pysbd

if __name__ == "__main__":
    print(f"\nArguments count: {len(sys.argv)}\n")
    for i, arg in enumerate(sys.argv):
        print(f"Argument {i:>6}: {arg}\n")

tokenizer = AutoTokenizer.from_pretrained("Helsinki-NLP/opus-mt-ru-en")

model = AutoModelForSeq2SeqLM.from_pretrained("Helsinki-NLP/opus-mt-ru-en")

seg_ru = pysbd.Segmenter(language="ru", clean=False)

input = sys.argv[1]
inputSegmented = seg_ru.segment(input)
translatedFinal = ""

counter = 0
for i in range(len(inputSegmented)):
    currentSentence = inputSegmented[i]
    input_ids = tokenizer.encode(currentSentence, return_tensors="pt")
    outputs = model.generate(input_ids)
    decoded = tokenizer.decode(outputs[0], skip_special_tokens=True)
    translatedFinal += decoded + " "
    counter += 1

seg_en = pysbd.Segmenter(language="en", clean=False)

from transformers import pipeline
fugu_translator = pipeline('translation', model='staka/fugumt-en-ja')

txt = translatedFinal
fuguFinal = fugu_translator(seg_en.segment(txt))

print(translatedFinal)

counter = 0
for i in fuguFinal:
    print('TRNSLTDTXT: ' + i['translation_text'])
    counter += 1