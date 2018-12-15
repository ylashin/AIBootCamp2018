import os
import numpy as np
import pandas as pd 
import matplotlib.pyplot as plt

import sklearn
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score
from sklearn.metrics import f1_score
from sklearn.externals import joblib

from azureml.core.run import Run
from azureml.core.runconfig import RunConfiguration

from yellowbrick.classifier import ROCAUC

run = Run.get_context()
os.makedirs('./outputs', exist_ok=True)


df = pd.read_csv("./Breast_cancer_data.csv", delimiter=",")
print(df.head())


count = df.diagnosis.value_counts()
count.plot(kind='bar')
plt.title("Distribution of malignant(1) and benign(0) tumor")
plt.xlabel("Diagnosis")
plt.ylabel("count")
plt.savefig("./outputs/class-distribution.png")
plt.clf()

y_target = df['diagnosis']
X_feature = df.drop(['diagnosis'], axis=1)

X_train, X_test, y_train, y_test= train_test_split(X_feature, y_target, test_size=0.3, random_state = 42)

from sklearn.ensemble import GradientBoostingClassifier

model = GradientBoostingClassifier()
visualizer = ROCAUC(model)

visualizer.fit(X_train, y_train) 

y_pred = model.predict(X_test)

acc = accuracy_score(y_test, y_pred)
f1 = f1_score(y_test, y_pred)

run.log('Accuracy', acc)
run.log('F1', f1)

visualizer.score(X_test, y_test)  # Evaluate the model on the test data
visualizer.poof('./outputs/AUC.png')    

model_file_name = 'breast-cancer-model.pkl'
model_file_path = os.path.join('./outputs/', model_file_name)
# save model in the outputs folder so it automatically get uploaded
with open(model_file_name, "wb") as file:
    joblib.dump(value=model, filename=model_file_path)

# when running in offline mode, model cannot be registered
register_model = getattr(run, "register_model", None)
if callable(register_model):
    # supply a model name, and the full path to the serialized model file.
    model = run.register_model(model_name='breast-cancer-model', model_path=model_file_path)
    model.add_tags({"run_id": run.id })

    is_model_ready = False
    run_details = run.get_details()
    arguments = run_details["runDefinition"]["Arguments"]
    if (len(arguments) > 0):
        is_model_ready = arguments[0]

    model.add_tags({"is_model_ready": str(is_model_ready).lower()})

