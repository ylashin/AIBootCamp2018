import pickle
import json
import numpy
from sklearn.externals import joblib
from azureml.core.model import Model

def init():
    global model

    model_path = Model.get_model_path('breast-cancer-model')
    print("Model path : " + model_path)

    model = joblib.load(model_path)

# note you can pass in multiple rows for scoring
def run(raw_data):
    try:
        data = json.loads(raw_data)['data']
        data = numpy.array(data)
        result = model.predict(data).tolist()
    except Exception as e:
        result = str(e)

    return json.dumps({"result": result})