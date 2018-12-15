import os
import azureml.core
import json
from azureml.core import Workspace
from azureml.core.authentication import ServicePrincipalAuthentication
from azureml.core.webservice import AciWebservice
from azureml.core.webservice import Webservice
from azureml.core import Image

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


build_version = os.environ["BUILD_BUILDNUMBER"]

aciconfig = AciWebservice.deploy_configuration(cpu_cores = 2, 
                                               memory_gb = 4, 
                                               tags = {'BuildVersion': build_version }, 
                                               description = 'Container instance hosting web service to  consume ABC Bricks routing solution')
											   
											   
image = Image(ws, name = "breast-cancer-image", tags= [['BuildVersion', build_version ]])

print("Picked image with version: " + str(image.version) + " which was built on : " + str(image.created_time))

aci_service = Webservice.deploy_from_image(deployment_config = aciconfig,
                                           image = image,
                                           name = 'breast-cancer-instance',
                                           workspace = ws)
aci_service.wait_for_deployment(True)
print(aci_service.state)
print(aci_service.scoring_uri)