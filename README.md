# Pihole2Influx
This tool, inside of the docker-container gets data from a Pihole-DNS-Resolver and put them into a influxdb timeseries database.
The special for this project is, that the data is catched via the telnet-interface of pihole. With this interface, we collect much more data as of using the web-api.


## Prerequisites
If you try this tool, please notice that you enable the telnet interface for all devices if you plan to run the container outside of the pihole server.

## Specs
The tool is written with C# but the is not so important, because i deliver the tool in a docker-container and you can run it everywhere where docker runs.
The following image demonstrates the dataflow and the "position" of the application inside your technical landscape.

<img src="./working_dataflow.png"  alt="current dataflow"/>

### State
Currently, the tool ist in absolutely alpha stadium.

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
</ul>
</li>
</ul>

What is missing:
<ul>
<li>Write the data to influxdb</li>
<li>Create a default dashboard for influx</li>
<li>Get all possible data from pihole<br />
<ul>
<li>List of top permitted domains</li>
<li>List of top blocked domains</li>
<li>List of top clients</li>
<li>List of forward destinations (cache, dns, block)</li>
<li>List of query-types (e.g. A, AAAA, DS)</li>
<li>Version information</li>
<li>database statistics</li>
<li>overtime</li>
</ul>
</li>
<li>many tests</li>
</ul>

