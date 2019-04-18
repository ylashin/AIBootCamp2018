# Getting Started

1.	Install needed Python packages
    ```
    pip install --upgrade azureml-sdk[notebooks,automl]
    pip install azureml-explain-model


    pip install psutil
    jupyter nbextension install --py azureml.widgets --user
    jupyter nbextension enable --py azureml.widgets --user
    ```

2. In most of the source code files or notebooks you will find references to service principal Id & Password. All you need to do is to create an Azure Active Directory application and grant it owner access to you Azure Machine Leanring workspace. The creation can be done using the below PowerShell snippet if you have Azure CLI installed and you are logged in your account.

    ```
    az ad sp create-for-rbac --name "MyCoolServicePrincipal" --password "NoOneCouldEverGuessIt-:)"
    ```

3. You can get the Id (user name) of the above service application from Azure Portal --> Azure Active Directory section



## Notes:

* ONNX demo is further explained in my Medium post : https://medium.com/@ylashin/consume-onnx-models-using-azure-machine-learning-service-869b1029d107
