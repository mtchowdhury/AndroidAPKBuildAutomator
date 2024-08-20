# AndroidAPKBuildAutomator
The purpose of this repository was to automate apk building for different users(mostly for testers), from a central machine where the environment for the specific application was set up. This platform, behind the scene, pulls the latest build from a configurable git branch
then build the apk using gradle commandline, based on the selected environment config ( conn string, entity/country etc.) then delivers to web users maintaining a queue. Once the build process is finished it notifies specific user with SignalR that output is download ready.
(Of course all this process took place when the app couldnt be published in app store for internal compliance restrictions).

There are two applications:
* APKBuilder -- the web app from where the users request different builds
* RabbitConsumer -- This service keeps listening to different requests coming from the web portal and process them maintaining a queue.
and 
* A RabbitMQ server running on docker container

## External Packages/Libaries used:
* RabbitMQ.Client
* Serilog
* Microsoft.AspNetCore.SignalR