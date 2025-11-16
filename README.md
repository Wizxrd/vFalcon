# vFALCON
![](https://img.shields.io/badge/version-v1.0.0-limegreen)  
[DOWNLOAD](https://github.com/Wizxrd/vFalcon/releases)

## OVERVIEW  
vFalcon is a VATSIM replay and analysis platform modeled after the FAA's Falcon program used to track the manipulation of datablocks, aircraft positions, and other sector-specific data within the National Airspace System (NAS). Its interface and functions are configured to align with CRC profile standards, providing a consistent and familiar environment for operational training and review.

The primary use of vFalcon is for reviewing busy traffic scenarios, debriefing close calls, evaluating controller performance, and assisting in student training from live network situations.

## DEFINITIONS

* ARTCC  
  * Air Route Traffic Control Center. A facility that manages and separates aircraft flying under instrument flight rules in high-altitude enroute airspace between airports. ARTCC facilities usually utilize the ERAM system to control aircraft.  
* CAB  
  * CRC window used for simulating visual identification of aircraft from a control tower.  
* CRC  
  * Consolidated Radar Client. The primary application used by controllers to connect to the VATSIM network and control an ATC position in the USA region of VATSIM (VATUSA).  
* ERAM  
  * [En Route Automation Modernization](https://www.faa.gov/air_traffic/technology/eram). The FAA�s primary air traffic control system for managing high-altitude flights in en route airspace. It processes radar, flight plan, and weather data to track aircraft, provide conflict detection, and coordinate handoffs between sectors and facilities. It replaced the older Host Computer System and is used at all U.S. ARTCCs.  
* FDB  
  * Full Data Block. Usually consisting of all the data ATC can see about an aircraft that is paired with a flight plan such as current and assigned altitudes, ground speed, vector lines, callsign, Computer ID, etc�  
* FILTERS (ERAM GEOMAP Filter)  
  * Up to 40 buttons that display the selected features from the current GeoMap. Most may refer to these as Video Maps.  
* GEOMAP  
  * A collection of features (lines, symbols and/or text) to be displayed on an ERAM Situation Display (scope).  
* LDB  
  * Limited Data Block. Usually consisting of only the aircraft' s current transponder code and Mode-C altitude returns. At times you may see the aircraft callsign rather than the transponder code.  
* STARS  
  * Standard Terminal Automation Replacement System. The air traffic control automation platform used in TRACONs and some towers to process and display radar data, assist with sequencing, and provide controllers with flight plan and track information for managing aircraft in terminal airspace. It replaced older systems like ARTS (Automated Radar Terminal System).  
* TRACON  
  * Terminal Radar Approach Control. A facility that manages and separates aircraft arriving, departing, and transitioning through the airspace around one or more airports, typically within about a 30�50 mile radius and up to around 10,000 feet. A TRACON usually receives flight data from the parent �ARTCC� computer.  
* VATSIM  
  * [Virtual Air Traffic Simulation](https://vatsim.net/). A free online network where pilots and air traffic controllers connect in a shared virtual world for realistic flight simulation.  
* vNAS  
  * [Virtual National Airspace System](https://vnas.vatsim.net/). The core system supporting advanced ATC clients for controlling U.S. airspace on VATSIM, integrating with VATSIM to share traffic data, coordinate internationally, store flight plans, manage ARTCC virtual systems, and sync data across all U.S. ATC facilities.

## MINIMUM REQUIREMENTS

* Windows OS 10 or higher  
* Mouse or track pad with Center-Click abilities  
* 2GB RAM  
* Stable internet connection (for some features)  
* CRC Installed  
* The desired facility to be reviewed must be installed via CRC

## DATA BLOCKS  
The data block simulation will replicate, as closely as possible, the format and behavior used in the CRC program for the applicable facility type (ARTCC/TRACON/CAB/ASDEX). At present, due to limited available data, many fields are not displayed such as assigned headings, speeds, interim/temporary altitudes, procedural altitudes, fourth-line data, scratchpad entries, VCI, CID matching, and other similar elements. Once the necessary APIs (data exchange ports) are created by VATSIM and vNAS, these features are expected to be implemented in vFalcon to the fullest extent possible.

Until the appropriate APIs are created, vFalcon determines whether to display a LDB or a FDB by checking for an active CRC ATC whose frequency matches the sector frequency currently active in vFalcon. If no matching controller is found, the data block remains an LDB, even if the pilot is tuned to that frequency. If a matching controller is detected, the data block is displayed as an FDB. The user may also center-click on a LBD displaying a callsign to manually toggle it to an FDB.

## FEATURES
### GENERAL  
Prior to vFalcon displaying video map data (lines, symbols, text, etc�), you must first have CRC installed on your local computer. Furthermore, you must have the facility you wish to review installed via the CRC Facility manager menu. Refer to [CRC documentation](https://crc.virtualnas.net/docs/#/) for more information concerning CRC installation and setup.

### LIVE MONITOR MODE

* Monitor current traffic situations as they are happening now on the network.  
* Requires internet connection

### REPLAY MODE

* Review past recorded data.  
* Requires data to have been recorded on the user's local computer (no current internet connection required) or via a server that records the data for later review (internet connection required to retrieve data from server).

### CLIENT SIMULATION
vFalcon will simulate the CRC equivalent window for the user-selected facility to the furthest extent nessessary. Available windows: ERAM, STARS, CAB, and ASDEX. 

## DATA USAGE  
vFalcon integrates the following VATSIM and vNAS data feeds to generate its display environment, including aircraft position, altitude, route assignments, and other operational details.

* [Aircraft Data Feed](https://data.vatsim.net/v3/vatsim-data.json)  
  * Provides aircraft position, flight plan, and data block information.  
  * Refresh rate: 15 seconds (comparable to ERAM�s 12-second cycle).  
  * vFalcon synchronizes updates to occur one second after the expected refresh interval.  
* [Transceivers](https://data.vatsim.net/v3/transceivers-data.json)  
  * Provides frequency data used to pair pilots with controller-used frequencies for data block population.  
  * vFalcon refreshes in sync with the Aircraft Data Feed.  
* [vNAS Controllers](https://live.env.vnas.vatsim.net/data-feed/controllers.json)  
  * Provides real-time updates on ATC connection status.  
  * vFalcon refreshes in sync with the Aircraft Data and Transceivers feeds.

vFalcon allows users to record the live network data while vFalcon is open via the �Record� menu and this data may be stored on the users computer and reviewed later using the vFalcon Replay Mode. vFalcon also allows users to set up a server to monitor/record the data from VATSIM without vFalcon being launched, so that the user may point the Replay Mode to the server and review archived data; This is more ideal to be setup on an ARTCC level, at minimum.

## KEYBINDS
**Keybind** | **Description**
---|---
**Esc** | Clear find-by square
**Alt+F** | Toggle fullscreen
**Alt+T** | Toggle titlebar
**Alt+B** | Toggle resize border
**Alt+R** | Start/stop recording
**Alt+Shift+S** | Capture application window
**Ctrl+L** | Load recording
**Ctrl+E** | Exit replay mode
**Ctrl+T** | Toggle Top-Down mode
**Ctrl+S** | Save profile
**Ctrl+Shift+S** | Save profile as
**Ctrl+Shift+P** | Switch profile
**Ctrl+G** | Open general settings window
**Ctrl+A** | Open appearance settings window
**Ctrl+M** | Open maps window
**Ctrl+P** | Open positions window
**Ctrl+F** | Open filters window
**Shift+F** | Open find-by window
**LeftArrow** | Rewind replay
**RightArrow** | Fast forward replay
**Space** | Play/pause replay
**PageUp** | Increase velocity vectors
**PageDown** | Decrease velocity vectors
**Middle Click** | Middle click on target to toggle FDB/LDB
**Right Click** | Right click on target to bring up context menu
**Ctrl+Left Click** | Left click on target to cycle through FDB positions (counter-clockwise)
**Ctrl+Right Click** | Right click on target to toggle leader line length
**Shift+Left Click** | Left click on target to toggle DRI
**Alt+Left Click** | Left click on target to toggle Dwell lock
**J+Left Click** | Left click on target to cycle DRI size

## License

Copyright (c) 2025+ vFalcon.

Licensed under the [MIT License](https://mit-license.org/).