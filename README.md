[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0) 
[![Build Status](https://travis-ci.com/LaszloLueck/Pihole2Influx.svg?branch=master)](https://travis-ci.com/LaszloLueck/Pihole2Influx) 
![GitHub top language](https://img.shields.io/github/languages/top/LaszloLueck/Pihole2Influx) 

[![Lines of Code](http://sonar.gretzki.ddns.net/api/project_badges/measure?project=3d075ac072aaf2b2b721e086939347b29728c396&metric=ncloc)](https://sonar.gretzki.ddns.net/dashboard?id=3d075ac072aaf2b2b721e086939347b29728c396) 
[![Quality Gate Status](http://sonar.gretzki.ddns.net/api/project_badges/measure?project=3d075ac072aaf2b2b721e086939347b29728c396&metric=alert_status)](https://sonar.gretzki.ddns.net/dashboard?id=3d075ac072aaf2b2b721e086939347b29728c396)
[![Security Rating](http://sonar.gretzki.ddns.net/api/project_badges/measure?project=3d075ac072aaf2b2b721e086939347b29728c396&metric=security_rating)](https://sonar.gretzki.ddns.net/dashboard?id=3d075ac072aaf2b2b721e086939347b29728c396)
[![Reliability Rating](http://sonar.gretzki.ddns.net/api/project_badges/measure?project=3d075ac072aaf2b2b721e086939347b29728c396&metric=reliability_rating)](https://sonar.gretzki.ddns.net/dashboard?id=3d075ac072aaf2b2b721e086939347b29728c396)
[![Coverage](http://sonar.gretzki.ddns.net/api/project_badges/measure?project=3d075ac072aaf2b2b721e086939347b29728c396&metric=coverage)](https://sonar.gretzki.ddns.net/dashboard?id=3d075ac072aaf2b2b721e086939347b29728c396)


# Pihole2Influx
This tool, inside of the docker-container, gets data from a Pihole-DNS-Resolver, especially the FTLDNS (Faster than light dns), convert and put them into a influxdb timeseries database.

## What makes this tool different from other tools? 
The data will catched via the telnet-interface of pihole. 
With this interface, it collects much more data as of using the web-Restful-API.

## Prerequisites
If you try this tool, please notice that you enable the telnet interface, on the pihole device, for all devices if you plan to run the container outside of the pihole server.
Please follow the link for a description how you enable telnet for any network device.
<a href="https://docs.pi-hole.net/ftldns/configfile/#socket_listening" target="blank">Link to ftl dns documentation</a>

It would be very helpful if you have knowledge with the following technologies. There is a big chance that all things runs out of the box, but I assume that you are familiar with the basic things below
- <a href="https://www.docker.com/" target="_blank">Docker</a> - installation and run the docker system
- <a href="https://www.influxdata.com/" target="_blank">InfluxDb</a> - installation and runnable system. Create and manage time series databases
- <a href="https://grafana.com/" target="_blank">Grafana</a> - installation and runnable system. Create and manage dashboards.

So that everything works as described and can fulfill its purpose, the following services must be available, set up and accessible for use in the network. In my environment i also use the following versions of the tools:

- Docker - Docker CE version 19.03.13, build 4484c46d9d
- Docker Compose - docker-compose version 1.25.0, build unknown
- InfluxDb - InfluxDB shell version: 1.8.3
- Grafana - Grafana v 7.3.4
- PiHole - PiHole v 5.2
- FTL - FTL v 5.3.1

Docker is no need. You can build and run the complete system, please follow the instructions as described in the <a href="install.md">installation documentation</a>.

## Specs
The tool is written with C# but the used language is not so important, because the tool runs completely in a docker-container and you can run it everywhere where docker runs.
The following image demonstrates the dataflow and the "position" of the application inside your technical landscape.

<img src="./images/working_dataflow.png"  alt="current dataflow"/>

## Insights
The app is originally written in C# 8 with Dontnet Core 3.1

On 2020/11/16 the code moves to C# 9 (thank god with immutable records) and dotnet 5.0

As a friend of functional programming with scala, i use a library called <a href="https://github.com/nlkl/Optional" target="_blank">Optional.Option</a>, so we become some syntactic sugar in the code and no more nullable poop.

## Installation
Please look in the <a href="install.md">installation document</a> and check what you need to run the container..

## Current Release
### 2020-12-03
#### Adding more errorhandling
If the write of data to influxdb is not successfully finished in 500ms, an exception is thrown and the process goes to the next worker. If the application container will unexpected stop the work, i've set the configuration parameter

`restart: always`

to my docker-compose.yaml.

I also create in the root of this project my docker-compose.yaml as default yaml, if anybody need this. You can find the file <a href="https://github.com/LaszloLueck/Pihole2Influx/blob/master/docker-compose.yaml">here</a>. If you plan to use this file 1:1 please be aware, that you create a docker static network with the range of 192.168.19.0/24. Also the ip-addresses of the services (pihole, influxdb) or database name would changed.

### 2020-12-02
#### Removing obsolete code to track telnet-exceptions
The issue with the read exception is solved, since yesterday, last code-commit (e.g. 17:xx EST), the tool runs in my docker environment without any error with a parallelity of 6. That means, i've fixed the issue and rollback to the prior code without logging exceptions.
The second change is the removing of the, now obsolete panels in grafana.

### 2020-12-01
#### Find and fix the issue with probably "read-exceptions" from telnet
As described yesterday, i was able to monitor some read timeouts on some different calls to pihole via telnet. I asked in the pihole forum if this could be a problem with the parallelity of the calls.
There answer was: No, because, every call comes to a queue and will be evaluated in order. There is no difference if there was one or ten calls.
If i parallel call the telnet interface of pihole via bash, i can't inspect this behavior, so a have a suspicion that the problem could be on my side.
And BANG! There was.
Whats the problem?
In easy steps the code do the following steps
1. Connect via telnet
2. Send the command
3. Wait until the result is complete loaded
4. Send the quit command
5. Disconnect

We will look at step 3. Here is the (old) code:
```c#
public Option<string> ReceiveDataSync(PiholeCommands message, string terminator)  
{  
  try  
  {  
	  var retValue = new StringBuilder();  
	  var received = new byte[256];  
	  while (_stream.Read(received, 0, received.Length) > 0)  
	  {  
		  var tmp = Encoding.UTF8.GetString(received);  
		  received = new byte[256];  
		  retValue.Append(tmp.Replace("\0", ""));  
		  if (tmp.Contains(terminator)) break;  
	 }  
	  return Option.Some(retValue.ToString());  
 }  catch (IOException exception)  
 {  
	 Log.Error(exception, "Read timeout while reading a network stream");  
	 return Option.None<string>();  
 }}
```
What does this mean?
While the stream (Networkstream) could be read (bytes in buffer > 0) do the following things.
- read the bytearray and encode this thing to an UTF8 string
- clear the array (for the next bunch of bytes from network)
- put the string to an stringBuilder (best way to concatenate strings together)
- and if the string from the bytearray contains ---EOM--- then break, so the end of the result from telnet is reached.

On many times this worked fine.
Except, if the Terminator-string (---EOM---) does no fit completely in one bytearray.
Example:
The content of the forelast array (for better read as string):

`[reply_CNAME 2988\nreply_IP 6759\nprivacy_level 0\nstatus enabled\n---E]`

The content of the last array:

`[OM---\0\0\0\0\0....]`

And oops! The comparison of ---EOM--- would be failed, because ---E is not ---EOM--- and OM--- is also not ---EOM---.

And now?

No problem, the world breaks into 2 different parts and we would all die!

No, here is what i have changed:

```c#
public Option<string> ReceiveDataSync(string terminator)  
{  
  var received = new byte[256];  
  try  
  {  
	  var sb = new StringBuilder();  
	  while (_stream.Read(received, 0, received.Length) > 0)  
	 {  
		 var toString = Encoding.UTF8.GetString(received);  
		 sb.Append(toString.Replace("\0", ""));  
	     received = new byte[256];  
		 if(sb.ToString().Contains(terminator)) break;  
	 }  
	 return Option.Some(sb.ToString());  
 }  catch (IOException exception)  
 {  
	 Log.Error(exception, "Read timeout while reading a network stream");  
	 return Option.None<string>();  
 }}
```
As you can see, i compare the complete result-string from strinbuilder with the terminator-string. And voila, everything works fine now.
I will inspect the behavior until tomorrow. I update my project on 17:30 and since then, i could not see any error in influxdb. If there are no exceptions tomorrow, i will remove the exception code from the project and the visualisation from grafana.

### 2020-11-30
#### Updated docker-compose file
If i call a `docker logs -f pihole2influx` after some days, it took some minutes to come to an end of the current logfile. I'm more interested in newer logs instead of showing all the old stuff, so i extend the docker-compose file for some parameters as described here (my current configuration):
```yaml
version: "3.7"  
  
services:  
  pihole2influx:  
    container_name: pihole2influx  
    networks:  
      default:  
        ipv4_address: 192.168.19.8  
    image: pihole2influx:latest  
    logging:  
      driver: "json-file"  
      options:  
        max-size: "10k"  
        max-file: "20"  
    environment:  
      - PIHOLEHOST=192.168.1.4  
      - PIHOLEPORT=4711  
      - INFLUXDBHOST=192.168.1.4  
      - INFLUXDBPORT=8086  
      - INFLUXDBNAME=pihole2influx  
      - INFLUXDBUSERNAME=  
      - INFLUXDBPASSWORD=  
      - RUNSEVERY=30  
      - CONCURRENTREQUESTSTOPIHOLE=4  
networks:  
  default:  
    external:  
      name: static-net  
...
```
As you can see, i've implemented a logrotation every 10KB filesize for 20 files. At least, it holds data for approximally 20 minutes. If you need more, increase the values.


#### Updated Grafana Dashboard
To make the telnet exceptions visible i released an extended Grafana dashboard.

#### Bump to pihole FTL 5.3.1
Today i updated my pi hole docker-container to the latest release of pihole (for docker).
All the things (pihole2influx, influxdb, grafana) running fine with that new version.
Currently my setup shows the following parameters:
- Pi Hole 5.2
- Pi Hole Web 5.2
- FTL 5.3.1
- Grafana 7.3.4
- InfluxDb 1.8.3
- Pihole2Influx from master branch

#### Current monitored read exceptions from telnet
As i inspect on my docker logs, i can observe various exceptions on different time slices from different called methods. The exception looks like:
```
11/30/2020 19:52:48 ERROR :: StandardTcpClientImpl : Read timeout while reading a network stream  
11/30/2020 19:52:48 ERROR :: StandardTcpClientImpl : Unable to read data from the transport connection: Connection timed out.  
11/30/2020 19:52:48 ERROR :: StandardTcpClientImpl : at System.Net.Sockets.NetworkStream.Read(Byte[] buffer, Int32 offset, Int32 size)  
at dck_pihole2influx.Transport.Telnet.StandardTcpClientImpl.ReceiveDataSync(PiholeCommands message, String terminator) in /app/Transport/Telnet/StandardTelnetClientImpl.cs:line 95  
11/30/2020 19:52:48 INFO :: SchedulerJob : Finished Task <4> for Worker <TopClientsConverter> in 5003 ms
```
OK, in my oppinion it looks like that sometimes pi hole cannot deliver an answer in the appropriate amount of (configured) time (currently 5 seconds). This problem i cannot reproduce with a parallelity of 1 (env-variable CONCURRENTREQUESTSTOPIHOLE). So if you really need every measurepoint, you shout set this variable to 1.
I've extend the measurepoints for an additional parameter (telnetError). If such an exception occurs, all needed informations goes to influxdb.
In grafana the result looks like:
![enter image description here](https://github.com/LaszloLueck/Pihole2Influx/blob/master/images/telnet_errors.png)

### 2020-11-27

Today night i see in the docker log that the app hung. I guess that a hickup in the network avoid that the app can connect to the pihole via telnet. The documentation of the appropriate methods
- read https://docs.microsoft.com/de-de/dotnet/api/system.net.sockets.tcpclient.receivetimeout?view=net-5.0
- write https://docs.microsoft.com/de-de/dotnet/api/system.net.sockets.tcpclient.sendtimeout?view=net-5.0

says, that there is no default timeout, so the app waits for ever to write or read data.

The other part is, if you give a wrong ip/port for pihole, the connection wait also too long to connect too.
But there is no default connection-timeout property that i can set.

In this case i call the connection async with a wait of 500ms.
After that time, an exception was thrown so we could see the problem in the logs.

Then, the handling of the results changes significant (there is not a result or void or else, there is for every stage a Option<>, with Some<> in good case or None<> in bad case, as you can see in <a href="https://github.com/LaszloLueck/Pihole2Influx/blob/master/dck_pihole2influx/Scheduler/SchedulerJob.cs">SchedulerJob.cs</a>.

In scala we have for this case the Either-function in that we can easily put the bad case to the left and the good case to the right.
https://www.scala-lang.org/api/2.9.3/scala/Either.html

Anyway, hopefully no app stops anymore.

### 2020-11-26

I've fixed an issue with the (until today) used telnet-client. The main problem was, that occasionally a warning appears like:

`the input string (telnet result) contains no data, please check your configuration.`

The reason for that? I don't know! The main problem is, sometimes the underlying code cannot read the result of the telnet command and my code avoid the further processing.

How am i resolved the issue?

I wrote my own Telnet-Component, based on dotnet core TcpClient feature. I wrapped the code, that we can (constructor-) overload the function without building the telnet object from the appropriate class (build to runtime as need).
When it will be used (in the parallel processing of getting data), i create the real object (one for each process) and send the commands.

The prior used telnet dependency wrote a mess of logging information to stdout (each result). I've never found a switch to avoid this.
The new code wrote nothing, so the log information looks like this:
```
11/26/2020 21:39:39 INFO :: SchedulerJob : Pihole host: 192.168.1.4
11/26/2020 21:39:39 INFO :: SchedulerJob : Pihole telnet port: 4711
11/26/2020 21:39:39 INFO :: SchedulerJob : InfluxDb host: 192.168.1.4
11/26/2020 21:39:39 INFO :: SchedulerJob : InfluxDb port: 8086
11/26/2020 21:39:39 INFO :: SchedulerJob : InfluxDb database name: pihole2influx
11/26/2020 21:39:39 INFO :: SchedulerJob : InfluxDb user name: 
11/26/2020 21:39:39 INFO :: SchedulerJob : InfluxDb password is not set
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect to Pihole and process data with 4 parallel process(es).
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect to pihole and get stats
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect Task <1> to Telnet-Host for worker DbStatsConverter
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect Task <2> to Telnet-Host for worker VersionInfoConverter
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect Task <3> to Telnet-Host for worker OvertimeConverter
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect Task <4> to Telnet-Host for worker TopClientsConverter
11/26/2020 21:39:39 INFO :: SchedulerJob : Finished Task <2> for Worker <VersionInfoConverter> in 6 ms
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect Task <5> to Telnet-Host for worker ForwardDestinationsConverter
11/26/2020 21:39:39 INFO :: SchedulerJob : Finished Task <4> for Worker <TopClientsConverter> in 6 ms
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect Task <6> to Telnet-Host for worker QueryTypesConverter
11/26/2020 21:39:39 INFO :: SchedulerJob : Finished Task <5> for Worker <ForwardDestinationsConverter> in 4 ms
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect Task <7> to Telnet-Host for worker TopAdsConverter
11/26/2020 21:39:39 INFO :: SchedulerJob : Finished Task <3> for Worker <OvertimeConverter> in 10 ms
11/26/2020 21:39:39 INFO :: SchedulerJob : Finished Task <6> for Worker <QueryTypesConverter> in 4 ms
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect Task <8> to Telnet-Host for worker TopDomainsConverter
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect Task <9> to Telnet-Host for worker StatsConverter
11/26/2020 21:39:39 INFO :: SchedulerJob : Finished Task <9> for Worker <StatsConverter> in 4 ms
11/26/2020 21:39:39 INFO :: SchedulerJob : Connect Task <10> to Telnet-Host for worker CacheInfoConverter
11/26/2020 21:39:39 INFO :: SchedulerJob : Finished Task <7> for Worker <TopAdsConverter> in 4 ms
11/26/2020 21:39:39 INFO :: SchedulerJob : Finished Task <8> for Worker <TopDomainsConverter> in 5 ms
11/26/2020 21:39:39 INFO :: SchedulerJob : Finished Task <10> for Worker <CacheInfoConverter> in 3 ms
11/26/2020 21:39:39 INFO :: SchedulerJob : Finished Task <1> for Worker <DbStatsConverter> in 122 ms

```

As you can see, with a parallelism of 4 there are 4 tasks on the beginning and if one is done, the next starts. Works like a charm. Also i put an extra information to the log output, how many time the processing of a worker need. The processing steps are:

- build the telnet object
- read the data from pihole with telnet
- parse the string and convert this into appropriate to
- convert the to to the appropriate influx measurement
- write the data to influx db
- close and dispose connection to telnet
- close and dispose connection to influxdb

### State
Currently, the tool ist a stable beta stadium. That means, the container runs on my system since 3 days without any problem.
I could'nt see any leaks or high memory consumption. The whole container needs 50Mb ram.

What works:
<ul>
<li>Basic Telnet-Connection</li>
<li>The Dockerfile with all the things that we need</li>
<li>The Base-Implementation</li>
<li>Get some statistics from pihole and print them to console<br />
Currently they are:
<ul>
<li>Standard Pihole Statistics</li>
<li>Informations about pihole's cache</li>
<li>List of top permitted domains</li>
<li>List of top blocked domains</li>
<li>List of query-types (e.g. A, AAAA, DS)</li>
<li>List of forward destinations (cache, dns, block)</li>
<li>List of top clients</li>
<li>overtime</li>
<li>Version information</li>
<li>database statistics</li>
</ul>
</li>
<li>Write the data to influxdb</li>
<li>Create a default dashboard for influx</li>
</ul>

What is missing:
<ul>
<li>better test coverage (currently not enough as you can see in the badge above)</li>
</ul>

## How it looks?
If all is up and running, you should checkoud the sample grafana dashboard from <a href="/Grafana-Dashboard/pihole2influx.json">here</a> and it shoulld looking like the following screenshot.
<img src="./images/grafana_screenshot.png"  alt="Grafana Screenshot"/>
<!--stackedit_data:
eyJoaXN0b3J5IjpbMzU4MDQ1NDcxLC0xOTE4NTM5MDk4LDEyNT
M0Nzc2NDIsMTgyNDM2MjY1NiwtNjEyMzkyOTkyLC01NDcyMTQ5
MjddfQ==
-->
