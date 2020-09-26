# MaterialTools

XivLauncher plugin for functions modifying how the game loads materials for models.

## Features

* Fix Game Behavior

  Fixes the game's behavior to be more consistent when loading body and skin materials. Specifically, the connector/seam models don't load the correct body/face textures by default. This is most noticeable at the neck seam, which will load face 1's textures (sometimes even from a different race) regardless of your race/face selection.

* Skin Material Override 
  
  Overrides the default handling of skin materials so that all races and clans are allowed unique skin materials and textures. When the plugin is loaded, it will scan for race materials (cXXXXb0001/cXXXXb0101) in your dat files, and enable variants that are found. You can use the "Show Skin Material List" button to see a list of all materials that are being used for the skin resolver.