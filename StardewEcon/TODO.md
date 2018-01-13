# Basic Functionality
* Determine whether we want to deal with config file versioning.
  That is, if the version in the config file doesn't match the version of the
  mod, what do we do?

# Current Issues
* Shop items have twice the price change as they should.
* If the user deletes or changes the file that saves their current events and
  then loads a save on a day when new events would be created, existing items
  that were affected by the replaced events would keep their modified price.
  We can only really combat this by resetting the price of every item on load.

# Extended Functionality
* Do we allow events affecting multiple items at once? Maybe necessary - crops and their seeds, for example.
* Do we allow multiple events of the same type? That is, two weekly events at once?
* Do we add a news broadcast to the TV as flavor? (See Daily News mod)
* Do we try to factor in Supply & Demand or other more complicated economic models?
