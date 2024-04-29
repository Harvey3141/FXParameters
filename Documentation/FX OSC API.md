# FX API Documentation

## Overview

The FX API offers functionalities that include:

- Loading OSC node settings from a JSON configuration file
- Sending messages at specified intervals with a controlled rate to manage traffic efficiently.
- Optionally sending real-time FX parameter changes to configured OSC nodes.

## OSC Messages

### General Messages

- **/FX/GET**  
  Retrieves the current value of a specified FX parameter. This command requires the parameter's address as the first argument in the message.
  
  **Format**: `/FX/GET [parameter_address]`  
  **Example**: `/FX/GET /FXLight/intensity/GET`
  
- **/FX/SET**    
  Assigns a new value to a specified FX parameter. This command takes the parameter's address as the first argument and the new value as the second argument in the message.
  
  **Format**: `/FX/SET [parameter_address] [new_value]`  
  **Example**: `/FX/SET FXLight/intensity 0.5`
  
- **FX/RESET**
  Resets the current value of a specified FX parameter to its default value. This command requires the parameter's address as the first argument in the message.
  
  **Format**: `/FX/RESET [parameter_address]`
  **Example**: `/FX/RESET FXLight/intensity`

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
  **Example**: `/SCENE/REMOVE scene1`

- **/SCENE/NEW**  
  Creates a new scene, resetting all parameters to default, and clearing all groups. 
  
- **/SCENELIST/GET**  
  Requests the scene list, which is returned as /sceneList/get {scene1, scene2, etc}. This is also sent to subscribers whenever a scene is added or removed
  
  **Format**: `/SCENELIST/GET`  
  **Example**: `/SCENELIST/GET`

- **/SCENE/RESET**  
  Resets the current scene
  
  **Usage**: `/SCENE/RESET`

- **/SCENE/NAME/GET**  
  Retrieves the name of the current scene.

  **Usage**: `/SCENE/NAME/GET`

- **/SCENE/NAME/SET**  
  Sets the name current scene

  **Format**: `/SCENE/NAME/SET [new_scene_name]`  
  **Example**: `/SCENE/NAME/SET "NewSceneName"`

### Group Management

- **/GROUPLIST/GET**  
  Requests the group list, which is returned as /groupList/get {group/1, group/2, group/3}. This is also sent to subscribers whenever a scene is added or removed.

  **Format**: `/GROUPLIST/GET'   
  **Example**: `/GROUPLIST/GET`
  
  
- **/GROUP/NEW**  
  Creates a new group, optionally provide group data, otherwise default properties will be used
  
  **Format**: `/GROUP/NEW [group data]`   
  **Example**: `/GROUP/NEW {group JSON data}`
  
- **/GROUP/REMOVE**  
  Removes specified group, unless group is pinned
  
  **Format**: `/GROUP/REMOVE [group_address]`  
  **Example**: `/GROUP/REMOVE /Group/10`
  
- **/GROUP/CLEAR**  
  Removes all parameters and triggers from specified group
  
  **Format**: `/GROUP/CLEAR [group_address]`  
  **Example**: `/GROUP/CLEAR /Group/10`

- **/GROUP/GET**  
  Fetches group data in JSON format
  
  **Format**: `/GROUP/GET [group_address] `  
  **Example**: `/GROUP/GET /Group/1`
  
- **/GROUP/SET**  
  Sets the data of a group
  
  **Format**: `/GROUP/SET [group address] [group data]`  
  **Example**: `/GROUP/SET /Group/1 {group JSON data}`  
  
- **/GROUP/ENABLED/SET**  
  Sets the enabled state of a group
  
  **Format**: `/GROUP/ENABLED/SET [group address] [state]`  
  **Example**: `/GROUP/ENABLED/SET /Group/1 true`  
  
- **/GROUP/ENABLED/GET**  
  Fetches the enabled state of a group. This is sent to subscribers automatically
  
  **Format**: `/GROUP/ENABLED/GET [group address]`  
  **Example**: `/GROUP/ENABLED/GET /Group/1 true` 

- **/GROUP/PARAMS/REMOVE**  
  Removes all FX parameters from a specified group.
  
  **Format**: `/GROUP/PARAM/REMOVE [group_address]`  
  **Example**: `/GROUP/PARAM/REMOVE /Group/1`  

- **/GROUP/PARAM/ADD**  
  Adds an FX parameter to a specified group.
  
  **Format**: `/GROUP/PARAM/ADD [group_address] [param_address]`  
  **Example**: `/GROUP/PARAM/ADD /Group/1 /FXLight/intensity`

- **/GROUP/PARAM/REMOVE**  
  Removes an FX parameter from a specified group.
  
  **Format**: `/GROUP/PARAM/REMOVE [group_address] [param_address]`  
  **Example**: `/GROUP/PARAM/REMOVE /Group/1 /FXLight/intensity`
  
- **/GROUP/PARAM/GET**  
  Retrieve parameter within a group in JSON format.

  **Format**: `/GROUP/PARAM/GET [group_address] [param_address]`  
  **Example**: `/GROUP/PARAM/GET /Group/1 /FXLight/intensity`

- **/GROUP/PARAM/SET**  
  Sets the parameter values within a specified group, creates a new param if not found 

  **Format**: `/GROUP/PARAM/SET [group_address] [param_address] [json]`  
  **Example**: `/GROUP/PARAM/SET /Group/1 /FXLight/intensity json data`

- **/GROUP/PARAM/ENABLED/GET**  
  Retrieves the enabled state of a specific parameter within a group.

  **Format**: `/GROUP/PARAM/ENABLED/GET [group_address] [param_address]`  
  **Example**: `/GROUP/PARAM/ENABLED/GET /Group/1 /FXLight/intensity`

- **/GROUP/PARAM/ENABLED/SET**  
  Enables or disables a specific parameter within a group.

  **Format**: `/GROUP/PARAM/ENABLED/SET [group_address] [param_address] [state]`  
  **Example**: `/GROUP/PARAM/ENABLED/SET /Group/1 /FXLight/intensity true`

- **/GROUP/TRIGGER/ADD**  
  Adds a trigger to a specified group.
  
  **Format**: `/GROUP/TRIGGER/ADD [group_address] [trigger_address]`  
  **Example**: `/GROUP/TRIGGER/ADD /Group/1 /FXLight/FXTrigger`

- **/GROUP/TRIGGER/REMOVE**  
  Removes a trigger from a specified group.
  
  **Format**: `/GROUP/TRIGGER/REMOVE [group_address] [trigger_address]`  
  **Example**: `/GROUP/TRIGGER/REMOVE /Group/1 /FXLight/FXTrigger`
  
 - **/GROUP/PATTERN/NUMBEATS**  
  Sets the number of beats in a pattern for a specified group. 

  **Format**: `/GROUP/PATTERN/NUMBEATS [group_address] [num_beats]`  
  **Example**: `/GROUP/PATTERN/NUMBEATS /Group/1 16`

- **/GROUP/TAP/ADDTRIGGERATCURRENTTIME**  
  Adds a trigger to a tap pattern at the current time.

  **Format**: `/GROUP/TAP/ADDTRIGGERATCURRENTTIME [group_address]`  
  **Example**: `/GROUP/TAP/ADDTRIGGERATCURRENTTIME /Group/1`

- **/GROUP/TAP/NUMBEROFTRIGGERS/SET**  
  Sets the number of triggers in a tap pattern

  **Format**: `/GROUP/TAP/NUMBEROFTRIGGERS/SET [group_address] [num_triggers]`  
  **Example**: `/GROUP/TAP/NUMBEROFTRIGGERS/SET /Group/1 5`

- **/GROUP/TAP/CLEARTRIGGERS**  
  Clears all triggers in a tap pattern within a group.

  **Format**: `/GROUP/TAP/CLEARTRIGGERS [group_address]`  
  **Example**: `/GROUP/TAP/CLEARTRIGGERS /Group/1`
  


### Parameter and Trigger Management

All other messages not specifically mentioned are presumed to manage individual FX parameters or triggers directly. The API dynamically processes these messages based on the address pattern and the provided values.


### Message Sending 

Messages are sent at intervals specified by the `sendInterval` property, with a maximum number of messages per interval defined by `maxMessagesPerInterval`. 


## JSON Configuration

The OSC node settings are loaded from a JSON file (`OSCConfig.json`) which includes settings such as local and remote ports, send intervals, and whether to send parameter changes in real-time.






