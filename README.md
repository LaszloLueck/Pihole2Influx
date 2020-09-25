# Pihole2Influx
This tool, inside of the docker-container gets data from a Pihole-DNS-Resolver and put them into a influxdb timeseries database.
The special for this project is, that the data is catched via the telnet-interface of pihole. With this interface, we collect much more data as of using the web-api.


## Prerequisites
If you try this tool, please notice that you enable the telnet interface for all devices if you plan to run the container outside of the pihole server.

## Specs
The tool is written with C# but the is not so important, because i deliver the tool in a docker-container and you can run it everywhere where docker runs.

### State
Currently, the tool ist in absolutely alpha stadium.

What works:
- Basic Telnet-Connection
- The Dockerfile with all the things that we need
- The Base-Implementation 
- Get some statistics from pihole and print them to console


What is missing:
- Write the data to influxdb
- Create a default dashboard for influx
- Get all possible data from pihole
- many tests


