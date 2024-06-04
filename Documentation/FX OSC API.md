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

### Colour FX Parameter Messages

- **/FX/COLOUR/GLOBALCOLOURPALETTEINDEX/SET**
  Sets the global colour palette index for the specified color parameter.

  **Format**: `/FX/COLOUR/GLOBALCOLOURPALETTEINDEX/SET [parameter_address] [index]`  
  **Example**: `/FX/COLOUR/GLOBALCOLOURPALETTEINDEX/SET /FXLight/color 2`

- **/FX/COLOUR/USEGLOBALCOLOURPALETTE/SET**
  Sets whether a specified color parameter should use the global colour palette.

  **Format**: `/FX/COLOUR/USEGLOBALCOLOURPALETTE/SET [parameter_address] [true/false]`  
  **Example**: `/FX/COLOUR/USEGLOBALCOLOURPALETTE/SET /FXLight/color true`

- **/FX/COLOUR/GLOBALCOLOURPALETTEINDEX/GET**
  Retrieves the global colour palette index for the specified color parameter.

  **Format**: `/FX/COLOUR/GLOBALCOLOURPALETTEINDEX/GET [parameter_address]`  
  **Example**: `/FX/COLOUR/GLOBALCOLOURPALETTEINDEX/GET /FXLight/color`

- **/FX/COLOUR/USEGLOBALCOLOURPALETTE/GET**
  Retrieves whether the specified color parameter is using the global colour palette.

  **Format**: `/FX/COLOUR/USEGLOBALCOLOURPALETTE/GET [parameter_address]`  
  **Example**: `/FX/COLOUR/USEGLOBALCOLOURPALETTE/GET /FXLight/color`

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
  Requests the scene list, which is returned as /sceneList/get with the scenes including tags. This is also sent to subscribers whenever a scene is added or removed
  
  **Format**: `/SCENELIST/GET`  
  **Example**: `/SCENELIST/GET`

  **Example Response**:
  ```json
  [
      {
          "Name": "Core Lighting - LR Scan - Breakdown",
          "TagIds": ["tagID1", "tagID2"]
      },
  ] ```
  
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
  
- **/SCENE/TAG/ADD**  
  Adds a tag to the current scene.

  **Format**: `/SCENE/TAG/ADD [tag_id]`  
  **Example**: `/SCENE/TAG/ADD 76946cdc-9c65-42a2-9cd4-5f6040f4db39`

- **/SCENE/TAG/REMOVE**  
  Removes a tag from the current scene.

  **Format**: `/SCENE/TAG/REMOVE [tag_id]`  
  **Example**: `/SCENE/TAG/REMOVE 76946cdc-9c65-42a2-9cd4-5f6040f4db39`

- **/SCENE/TAGS/CLEAR**  
  Removes all tags from the current scene.

  **Format**: `/SCENE/TAGS/CLEAR`
  
### Aduio Management

- **/AUDIO/BPM/TAP**  
  Triggers a tap event to calculate BPM based on the timing of taps. No arguments are needed.

  **Usage**: `/AUDIO/BPM/TAP`

- **/AUDIO/BPM/RESETPHASE**  
  Resets the BPM phase. No arguments are needed.

  **Usage**: `/AUDIO/BPM/RESETPHASE`

- **/AUDIO/BPM/DOUBLEBPM**  
  Doubles the current BPM value. No arguments are needed.

  **Usage**: `/AUDIO/BPM/DOUBLEBPM`

- **/AUDIO/BPM/HALFBPM**  
  Halves the current BPM value. No arguments are needed.

  **Usage**: `/AUDIO/BPM/HALFBPM`

- **/AUDIO/BPM/VALUE/SET**  
  Sets the BPM value to a specified float value.

  **Usage**: `/AUDIO/BPM/VALUE/SET [float_value]`  
  **Example**: `/AUDIO/BPM/VALUE/SET 120.0`

- **/AUDIO/BPM/VALUE/GET**  
  Retrieves the current BPM value. The current BPM value will be sent in response.

  **Usage**: `/AUDIO/BPM/VALUE/GET`



### Tag Management

- **/TAG/NEW**  
  Adds a new tag to the tag configuration.

  **Format**: `/TAG/NEW [tag_type] [tag_value]`  
  **Example**: `/TAG/NEW scene-bucket NewTagValue`

- **/TAG/REMOVE**  
  Removes a tag from the tag configuration.

  **Format**: `/TAG/REMOVE [tag_id]`  
  **Example**: `/TAG/REMOVE 76946cdc-9c65-42a2-9cd4-5f6040f4db39`

- **/TAG/SET**  
  Updates a tag in the tag configuration.

  **Format**: `/TAG/SET [json_tag]`  
  **Example**: 
  ```json
      {
        "id": "1d13246e-f434-46aa-aa07-f4322240e4dd",
        "value": "1"
      }```

### Colour Palette Manager

- **/COLOURPALETTEMANAGER/ENABLED/GET**  
  Retrieves the current state of the colour palette manager (enabled/disabled). 
  
  **Format**: `/COLOURPALETTEMANAGER/ENABLED/GET`

- **/COLOURPALETTEMANAGER/ENABLED/SET**  
  Sets the enabled state of the colour palette manager.  
  
  **Format**: `/COLOURPALETTEMANAGER/ENABLED/SET [true/false]`  
  **Example**: `/COLOURPALETTEMANAGER/ENABLED/SET true`

- **/COLOURPALETTEMANAGER/FORCE/GET**  
  Retrieves the current state of the force update setting.  
  
  **Format**: `/COLOURPALETTEMANAGER/FORCE/GET`

- **/COLOURPALETTEMANAGER/FORCE/SET**  
  Sets the state of the force update setting.  
  
  **Format**: `/COLOURPALETTEMANAGER/FORCE/SET [true/false]`  
  **Example**: `/COLOURPALETTEMANAGER/FORCE/SET true`

- **/COLOURPALETTEMANAGER/ACTIVEPALETTE/GET**  
  Retrieves the ID of the currently active colour palette.  
  
  **Format**: `/COLOURPALETTEMANAGER/ACTIVEPALETTE/GET`

- **/COLOURPALETTEMANAGER/ACTIVEPALETTE/SET**  
  Sets the active colour palette by ID.  
  
  **Format**: `/COLOURPALETTEMANAGER/ACTIVEPALETTE/SET [palette_id]`  
  **Example**: `/COLOURPALETTEMANAGER/ACTIVEPALETTE/SET 123e4567-e89b-12d3-a456-426614174000`

- **/COLOURPALETTE/NEW**  
  Creates a new colour palette from a JSON string.  
  
  **Format**: `/COLOURPALETTE/NEW [palette_json]`  
  **Example**: `/COLOURPALETTE/NEW`

- **/COLOURPALETTE/REMOVE**  
  Removes an existing colour palette by ID.  
  
  **Format**: `/COLOURPALETTE/REMOVE [palette_id]`  
  **Example**: `/COLOURPALETTE/REMOVE 123e4567-e89b-12d3-a456-426614174000`

- **/COLOURPALETTE/SET**  
  Updates an existing colour palette with new data from a JSON string.  
  
  **Format**: `/COLOURPALETTE/SET [palette_json]`  
  **Example**: `/COLOURPALETTE/SET `
  
- **/COLOURPALETTELIST/GET**  
  Retrieves the all palette data as json.
  
  **Format**: `/COLOURPALETTELIST/GET`  


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






