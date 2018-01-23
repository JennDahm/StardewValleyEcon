# Stardew Valley Economy

This mod adds economic events to Stardew Valley to encourage players to diversify
their crops and produce. There are always three events going at once: A monthly
event affecting crops, a biweekly event affecting artisan goods or cooked items,
and a weekly event affecting minerals, fish, and foraged goods. These events will
affect the sell and purchase prices of items in the game.

Please note that this mod is currently in alpha. This means there may be
game-breaking bugs, and it is not recommended for valuable playthroughs. We would
be happy for you to playtest this mod for us, however, and give us your feedback!

## How to play with this mod

Outside Pierre's shop, where there used to be a sign simply listing his hours,
there is now a news bulletin posted. This newspaper will tell you what the current
events are, listed from the monthly event at the top to the weekly event at the
bottom. Hovering over the event with your mouse will give you a concise
description of how the event affects pricing.

All you have to do is keep those events in mind when playing!

## Configuration

This mod is fairly easily configured. Each of the events that can occur are
listed one per line in files in the config folder of the mod, organized by
frequency of occurance. To add events, add a line with the headline to the
appropriate file. To remove events, remove the specific line from the file.

Additionally, each item substitution category can be configured. Each item that
can be substituted for a given category is listed one per line in appropriately
named files in the config fodler of the mod.

### Event Configuration

Each headline has substitution locations that can be filled in with items
and other content. You specify these locations with substitution sequences.
The following list describes each supported sequence and what it means:

* `%%` - Literal % character
* `%0` - Earthquake Magnitude
* `%1` - Location
* `%2` - Crop
* `%3` - Mineral
* `%4` - Number of Fatalities
* `%5` - Foraged Good
* `%6` - River/Lake Fish
* `%7` - Ocean Fish
* `%8` - Artisan Good
* `%9` - Cooked Good

If you want an event to pick from one of several categories, you can combine
them with `+`. The combined categories must be of the same type - that is,
they must all be item categories or they must all be other categories - but
it really only makes sense for item categories anyway. The following are a
couple of examples:

* `%6+7` - River/Lake Fish or Ocean Fish
* `%3+5+9` - Mineral, Foraged Good, or Cooked Good

Currently, the mod only supports one affected item per headline. It will pick
the first item substitution in the headline and modify that item's price for the
duration of the event, ignoring any others listed. The others will be generated
and substituted for the headline's sake, but their prices won't be affected.

The event file format is fairly hardy - the mod will ignore empty lines and lines
filled with only whitespace (i.e. spaces), and any event without an item blank
won't affect the price of any item. A loose % not followed by a number or another
% is interpreted as a literal % character. If you mess up a substitution
sequence, it will just be written straight to the headline and skipped. Extra
whitespace on either end of the line will be trimmed before display.

You can also insert comments into the files. Any line starting with # will be
ignored by the mod and is strictly for human information and readability.

Keep in mind that headlines can only be so long before they're cut off in the
news window.

### Item Category Configuration

The number used on each line of these files is the "parentSheetIndex" of the
item. You can look up the id for an item using this
[StardewValleyWiki page](https://stardewvalleywiki.com/Modding:Object_data).
When an event is generated and an item from this category is selected, it will
be selected from this file.

Like with the event files, you can insert a comment into the file. Any line
starting with # will be ignored by the mod and is strictly for human information
and readability.

This file format may change in the future to accomodate more information about
each item, such as what season it's available in.

