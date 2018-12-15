import os
import azureml.core
from azureml.core import Workspace
from azureml.core.authentication import ServicePrincipalAuthentication
from azureml.core.runconfig import RunConfiguration
from azureml.core.model import Model
from operator import attrgetter


spAuth = ServicePrincipalAuthentication(
    tenant_id="TENANT_ID",
    username="SERVICE_PRINCIPAL_ID",
    password="SERVICE_PRINCIPAL_PASSWORD")

print("SDK Version: ", azureml.core.VERSION)

subscription_id = "SUBSCRIPTION_ID"
resource_group = "RESOURCE_GROUP_NAME"
workspace_name = "WORKSPACE_NAME"

ws = Workspace(auth=spAuth, subscription_id=subscription_id,
               resource_group=resource_group, workspace_name=workspace_name)
print("Loaded workspace: " + ws.name)


ready_models = Model.list(workspace=ws, tags=[['is_model_ready', 'true']])
model = max([model for model in ready_models if model.name == "breast-cancer-model"], key=attrgetter('version'))

from azureml.core.conda_dependencies import CondaDependencies

myenv = CondaDependencies()
myenv.add_pip_package("numpy")
myenv.add_pip_package("sklearn")
#myenv.add_pip_package("azureml-core")
with open("myenv.yml","w") as f:
    f.write(myenv.serialize_to_string())

print("Created conda dependencies file.")

from azureml.core.image import ContainerImage

build_version = os.environ["BUILD_BUILDNUMBER"]

image_config = ContainerImage.image_configuration(execution_script = "score.py",
                                                  runtime = "python",
                                                  conda_file = "myenv.yml",
                                                  description = "A container image with breast cancer detection model",
                                                  tags = {"BuildVersion": build_version})

image = ContainerImage.create(name = "breast-cancer-image",
                              models = [model],
                              image_config = image_config,
                              workspace = ws)

image.wait_for_creation(show_output = True)

print(image.image_build_log_uri)