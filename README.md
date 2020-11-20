[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0) 
[![Build Status](https://travis-ci.com/LaszloLueck/Pihole2Influx.svg?branch=master)](https://travis-ci.com/LaszloLueck/Pihole2Influx) 
![GitHub top language](https://img.shields.io/github/languages/top/LaszloLueck/Pihole2Influx) 

[![Lines of Code](http://gretzki.ddns.net:9000/api/project_badges/measure?project=3d075ac072aaf2b2b721e086939347b29728c396&metric=ncloc)](http://gretzki.ddns.net:9000/dashboard?id=3d075ac072aaf2b2b721e086939347b29728c396) 
[![Quality Gate Status](http://gretzki.ddns.net:9000/api/project_badges/measure?project=3d075ac072aaf2b2b721e086939347b29728c396&metric=alert_status)](http://gretzki.ddns.net:9000/dashboard?id=3d075ac072aaf2b2b721e086939347b29728c396)
[![Security Rating](http://gretzki.ddns.net:9000/api/project_badges/measure?project=3d075ac072aaf2b2b721e086939347b29728c396&metric=security_rating)](http://gretzki.ddns.net:9000/dashboard?id=3d075ac072aaf2b2b721e086939347b29728c396)
[![Reliability Rating](http://gretzki.ddns.net:9000/api/project_badges/measure?project=3d075ac072aaf2b2b721e086939347b29728c396&metric=reliability_rating)](http://gretzki.ddns.net:9000/dashboard?id=3d075ac072aaf2b2b721e086939347b29728c396)

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
- Grafana - Grafana v 7.3.1
- PiHole - PiHole v 5.1.2
- FTL - FTL v 5.2

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
Please look in the <a href="install.md">installation document</a> and check what you need to run the container.

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
<li>better test coverage (currently only 70%)</li>
</ul>

## How it looks?
If all is up and running, you should checkoud the sample grafana dashboard from <a href="/Grafana-Dashboard/pihole2influx.json">here</a> and it shoulld looking like the following screenshot.
<img src="./images/grafana_screenshot.png"  alt="Grafana Screenshot"/>
