# Basic Functionality

* Fix bounding box on NewsBulletinObject.
* Fix display of items. Parsnips in particular display incorrectly.
* Actually implement price changes.
* Figure out how to get item names.
* Figure out whether we store and retrieve current events from player save.
  With entirely deterministic RNG, the only thing that could prevent us from
  recreating a player's events from scratch is if the config files were updated.
  Is that a large enough concern? Maybe - we would be pushing updates every so
  often.

# Extended Functionality
* Do we allow events affecting multiple items at once? Maybe necessary - crops and their seeds, for example.
* Do we allow multiple events of the same type? That is, two weekly events at once?
* Do we add a news broadcast to the TV as flavor? (See Daily News mod)
* Do we try to factor in Supply & Demand or other more complicated economic models?
