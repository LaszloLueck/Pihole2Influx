# Installation and usage

## Build and run as Docker container

### Build the docker container from the sources
The easiest way to run the app is as Docker container. 
These brings up all the dependencies and runtimes you need to run the application. The other advantage is, that the
container technology is platform independent, so you can run this on windows, mac or linux... everywhere where docker
is running.
The applications docker container is not available on Docker Hub, but you can build the container anytime and simple easy.
All you have to do is run the following line in a shell:

`docker build https://github.com/LaszloLueck/pihole2influx.git\#master:dck_pihole2influx -t pihole2influx:latest`
This will download the dockerfile in the project, download all the resources, build, and test the application and finally
create the docker container.

If all is done, you can check if the container exists with

`docker images`

you will see the current line:

`pihole2influx                          latest              83f876cdea69        10 seconds ago        236MB`

All right, thats done, we have build the docker container!

### Run the docker container
The easiest and best way to run the docker container is, to create a docker-compose.yaml file and start / Stop the
container with such a file. The reason is, the much more easier readable and understandable configuration.
Lets go, here is a sample configuration-file. You can copy the code and paste this in an file named

`docker-compose.yaml`

```
version: "3" 
 
 services:
   pihole2influx:
     container_name: pihole2influx
     image: pihole2influx:latest
     environment:
       - PIHOLEHOST=192.168.1.4
       - PIHOLEPORT=4711
       - INFLUXDBHOST=192.168.1.4
       - INFLUXDBPORT=8086
       - INFLUXDBNAME=pihole2influx
       - CONCURRENTREQUESTSTOPIHOLE=4
       - RUNSEVERY=30
       - INFLUXDBUSERNAME=""
       - INFLUXDBPASSWORD=""
       - PIHOLEUSER=""
       - PIHOLEPASSWORD=""

 ...
```
Here is a short description of the parameters:

- PIHOLEHOST - That is the ip-address or hostname where the pihole is reachable
- PIHOLEPORT - The default pihole telnet port is 4711. Here you will enter a port if yours is different from default.
- INFLUXDBHOST - The ip-address or hostname where influx is reachable
- INFLUXDBPORT - The default influx port is 8086. Enter the port if yours is different from default
- INFLUXDBNAME - The database name where influx db will store the data
- CONCURRENTREQUESTSTOPIHOLE - It depends from your docker host and the pihole hardware with how much parallelism you will work. For an example, on my 24 core docker host it runs with parallelism of 12 to the docker-pihole host on the same machine. If you increase the amount, the faster is the app, but could leave the system unstable.
- RUNSEVERY - time in seconds where the scheduler is triggered
- INFLUXDBUSERNAME - If you secure the Influx-installation, here is the place for the user
- INFLUXDBPASSWORD - If you secure the Influx-installation, here is the place for the password
- PIHOLEUSER - If you secure the pihole-installation, here is the place for the user
- PIHOLEPASSWORD - If you secure the pihole-installation, here is the place for the password

Thats it, not less and not more to configure.
If you create and configure the file you can start the container now.
Start container from the location where the docker-compose.yaml is stored:

`docker-compose up -d`

Easy ey!

Stopping the container is same easy as starting:

`docker-compose down`

Wow!

Let's check if docker runs properly.
Type

`docker logs -f pihole2influx`

And you would see the standard output of the containers application. It would be look like:

```
11/14/2020 22:12:04 :: SchedulerJob : Use the following parameter for connections:
11/14/2020 22:12:04 :: SchedulerJob : Pihole host: 192.168.1.4
11/14/2020 22:12:04 :: SchedulerJob : Pihole telnet port: 4711
11/14/2020 22:12:04 :: SchedulerJob : InfluxDb host: 192.168.1.4
11/14/2020 22:12:04 :: SchedulerJob : InfluxDb port: 8086
11/14/2020 22:12:04 :: SchedulerJob : InfluxDb database name: pihole2influx
11/14/2020 22:12:04 :: SchedulerJob : InfluxDb user name: 
11/14/2020 22:12:04 :: SchedulerJob : InfluxDb password is not set
11/14/2020 22:12:04 :: SchedulerJob : Connect to Pihole and process data with 1 parallel process(es).
11/14/2020 22:12:04 :: SchedulerJob : Connect to pihole and get stats
11/14/2020 22:12:04 :: ConnectedTelnetClient : Connect to Telnet-Host at 192.168.1.4:4711
queries in database: 472767
database filesize: 35.79 MB
SQLite version: 3.31.1
---EOM---

11/14/2020 22:12:05 :: SchedulerJob : Finished Worker <DbStatsConverter>
11/14/2020 22:12:05 :: ConnectedTelnetClient : Connect to Telnet-Host at 192.168.1.4:4711
version v5.2
tag v5.2
branch master
hash dbd4a69
date 2020-08-09 22:09:43 +0100
---EOM---

11/14/2020 22:12:05 :: VersionInfoConverter : No implementation for CalculateMeasurementData found!
11/14/2020 22:12:05 :: SchedulerJob : Finished Worker <VersionInfoConverter>
11/14/2020 22:12:05 :: ConnectedTelnetClient : Connect to Telnet-Host at 192.168.1.4:4711
1605308700 490 143
1605309300 424 144
1605309900 478 166
1605310500 505 153
1605311100 450 164
1605311700 825 74
1605312300 203 65
1605312900 217 110
1605313500 223 109
1605314100 232 141
1605314700 131 53
1605315300 179 71
.
.
.
.

```
 Fine.
 Now it is time to prepare Grafana to make all the things visible!
 The project holds a sample dashboard file in the directory `/Grafana-Dashboard/pihole2influx.json`
 That is a good startpoint to create your own fancy looking dashboards.
 
 And now have fun with that!
 
 ## Build and run from sources without docker
 There is no need to run the app as docker-container. The application is written as a c# console app. After the main task is started, an inner task runs forever
 `Task.Delay(-1)` until you pressed Strg+c or reboot or whatever.
 Before you compile an run the app, you must install the .NET Core Components at least in version 3.1. Please use the following link:
 <a href="https://dotnet.microsoft.com/download/dotnet-core/3.1" target="_blank">Microsoft .Net Core SDK</a> (at least 3.1) for build and test
 
 - Change to the directory where the code (best case where the dck_pihole2influx.sln) is.
 - Please use the aboved described ENV-vars and add them to your current session.
 - Run `dotnet build`
 - If that was finished ...
 - Run `dotnet run` and voila, hopefully the app started. If not, there are some helpful error-messages to check where the problem is.
 In the dependencies there is all what you need to approve the application against the static code analysis tool called <a href="https://www.sonarqube.org/" target="_blank">Sonarqube</a>.
 Please setup an up and running SonarQube instance.
 Next step is to install the global dotnet-sonarscanner
 
 `dotnet tool install --global dotnet-sonarscanner`
 
 if you run
 
 `dotnet sonarscanner`

and you receive the message that nothing found, you will add the sonarscanner directory to you env-vars:

`export PATH="$PATH:/root/.dotnet/tools" --> where root is your home directory`

Then create a token of your sonar-user and a token for the application in Sonarqubes web-interface.
With the tokens type the following in the console:
```
dotnet sonarscanner begin /name:"Pihole2Influx" /d:sonar.host.url="http://IPORHOSTOFSONARQUBE:PORT" /k:"TOKENOFTHEAPPLICATION" /d:sonar.cs.opencover.reportsPaths="TestResults/coverage.opencover.xml" /d:sonar.login="TOKENOFTHEUSER"
dotnet build /t:rebuild
dotnet test dck_pihole2influx.test  /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="../TestResults/"
dotnet sonarscanner end /d:sonar.login="TOKENOFTHEUSER"
```