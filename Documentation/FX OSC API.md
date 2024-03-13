# FX API Documentation

## Overview

The FX API offers functionalities that include:

- Loading OSC node settings from a JSON configuration file for flexible setup.
- Sending messages at specified intervals with a controlled rate to manage traffic efficiently.
- Processing "/GET" requests and other messages to update or fetch FX parameters.
- Optionally sending real-time FX parameter changes to configured OSC nodes.

## OSC Messages

### General Messages

- **/GET**  
  Fetches the current value of a specified FX parameter.  
  **Format**: `/[parameter_address]/GET`  
  **Example**: `/FXLight/intensity/GET`
  
- **/RESET**  
Sets the current value of a specified FX parameter to its default value  
**Format**: `/[parameter_address]/RESET`  
**Example**: `/FXLight/intensity/RESET`

### Scene Management

- **/SCENE/LOAD**  
  Loads a preset scene by name.  
  **Format**: `/SCENE/LOAD [scene_name]`  
  **Example**: `/SCENE/LOAD MyPresetScene`

- **/SCENE/SAVE**  
  Saves the current scene. If a scene name is provided, saves with the specified name.  
  **Format**: `/SCENE/SAVE [optional_scene_name]`  
  **Example**: `/SCENE/SAVE MySavedScene`

- **/SCENE/REMOVE**  
  Removes a preset scene by name.  
  **Format**: `/SCENE/REMOVE [scene_name]`  
  **Example**: `/SCENE/REMOVE OldPresetScene`

- **/SCENE/NEW**  
  Creates a new scene, resetting all parameters to default.

### Group Management

- **/GROUP/GET**  
  Fetches group data in json format
  **Format**: `/GROUP/GET [group_address] `  
  **Example**: `/GROUP/GET Group/1`

- **/GROUP/PARAM/ADD**  
  Adds an FX parameter to a specified group.  
  **Format**: `/GROUP/PARAM/ADD [group_address] [param_address]`  
  **Example**: `/GROUP/PARAM/ADD Group/1 /FXLight/intensity`

- **/GROUP/PARAM/REMOVE**  
  Removes an FX parameter from a specified group.  
  **Format**: `/GROUP/PARAM/REMOVE [group_address] [param_address]`  
  **Example**: `/GROUP/PARAM/REMOVE Group/1 /FXLight/intensity`

- **/GROUP/TRIGGER/ADD**  
  Adds a trigger to a specified group.  
  **Format**: `/GROUP/TRIGGER/ADD [group_address] [trigger_address]`  
  **Example**: `/GROUP/TRIGGER/ADD Group/1 /FXLight/FXTrigger`

- **/GROUP/TRIGGER/REMOVE**  
  Removes a trigger from a specified group.  
  **Format**: `/GROUP/TRIGGER/REMOVE [group_address] [trigger_address]`  
  **Example**: `/GROUP/TRIGGER/REMOVE Group/1 /FXLight/FXTrigger`

### Parameter and Trigger Management

All other messages not specifically mentioned are presumed to manage individual FX parameters or triggers directly. The API dynamically processes these messages based on the address pattern and the provided values.


### Message Sending 

Messages are sent at intervals specified by the `sendInterval` property, with a maximum number of messages per interval defined by `maxMessagesPerInterval`. 


## JSON Configuration

The OSC node settings are loaded from a JSON file (`OSCConfig.json`) which includes settings such as local and remote ports, send intervals, and whether to send parameter changes in real-time.

