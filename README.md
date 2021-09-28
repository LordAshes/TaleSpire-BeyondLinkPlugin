# Beyond Link Pack

This unofficial TaleSpire asset pack links your D&D Beyond character sheets with TaleSpire.
Currently copies current HD, max HD, AC, Shield AC, used HD, max HD.

## Change Log

1.0.0: Initial release

## Install

Use R2ModMan or similar installer to install this asset pack.

Update the BeyondLinks.json configuration with the desired character names and BeyondIds.
The name in the configuration needs to match the TS mini name.
The beyondId is the 8 digit number at the end of the URL when looking at a character.

The R2ModMan configuration for this plugin can be used to change the interval at which TaleSpire is syneced with the
D&D Beyond character sheets. Default 10 seconds.

## Usage

Everything happens automatically. When any of the supported properties are changed in D&D Beyond, Tale Spire will be
updated with the interval number of seconds.

The current HP and max HP is transferred to the Tale Spire asset HP.

The current AC and the shield modified AC are transferred to Stat 1. Expressed as AC without shield of AC with shield.

The used HD of and max HD is transferred to Stat 2. Expressed as used HD of max HD.

### Operating Status

In the bottom right side of the screen the word "Link" is visile when this plugin is running. The color of this word
indicates the success of syncing activity:

Blue = In-Between syncs.

Green = Last sync was successful.

Red = Last sync had at least one failure.

### Centralized vs Distributed Configuration

There are two ways in which this plugin can be used: Centralized and Distributed.

Centralized: The GM configures all D&D Beyond characters in his/her BeyondLinks.json. All other players do not have
             the plugin active. Disadvantages: Larger stress on single device. Advantages: Other players don't needs
			 to have this plugin at all, so other players could even be using an unmodded version of TaleSpire.
			 
Distributed: Each player configures his/her BeyondLinks.json with their own characters. Disadvantage: Each player needs
			 to have the plugin. Advantage: The processing is distributed between many devices so les powerful devices
			 are needed.

## Limitations

Currently the stat names are updated by this plugin and unused stats are set to Unused.

