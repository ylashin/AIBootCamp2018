import azureml.core
from azureml.core import Experiment
from azureml.core import Workspace
from azureml.core.authentication import ServicePrincipalAuthentication
from azureml.core.runconfig import RunConfiguration
from azureml.core import ScriptRunConfig
import socket
import argparse

parser = argparse.ArgumentParser(description='Submit ML experiment.')
parser.add_argument('--tagModelReady', help='Tag the model as ready for consumption', action="store_true")
args = parser.parse_args()

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


exp = Experiment(workspace=ws, name='BreastCancer')
print("Loaded experiment: " + exp.name)

# Editing a run configuration property on-fly.
run_config_user_managed = RunConfiguration()
run_config_user_managed.environment.python.user_managed_dependencies = True

src = ScriptRunConfig(source_directory='./',
                      # first argument controls whether to tag model as ready for production
                      arguments=[args.tagModelReady],
                      script='train.py',
                      run_config=run_config_user_managed)

run = exp.submit(src, tags={"host": socket.gethostname()})
run.wait_for_completion(show_output=True)

# get all metris logged in the run
metrics = run.get_metrics()
print(metrics)
