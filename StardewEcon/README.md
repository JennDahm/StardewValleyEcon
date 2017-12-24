# Stardew Valley Economy

This mod adds economic events to Stardew Valley to encourage players to diversify
their crops and produce. There are always three events going at once: A monthly
event affecting crops, a biweekly event affecting artisan goods, and a weekly
event affecting minerals and fish. These events will affect the sell and purchase
prices of items in the game.

## How to play with this mod

Outside Pierre's shop, where there used to be a sign simply listing his hours,
there is now a news bulletin posted. This newspaper will tell you what the current
events are, listed from the monthly event at the top to the weekly event at the
bottom. Hovering over the event with your mouse will give you a concise
description of how the event affects pricing.

All you have to do is keep those events in mind when farming!

## Configuration

This mod is fairly easily configured. Each of the events that can occur are
listed one-per-line in files in the config folder of the mod, organized by
frequency of occurance. To add events, add a line with the headline to the
appropriate file. To remove events, remove the specific line from the file.

Each headline has blanks that can be filled in with locations or items. The
following list describes what each means:

* `%%` - Literal % character
* `%1` - Location
* `%2` - Crop

Currently, the mod only supports one item per headline. It will pick the first
item blank in the headline and modify its price for the duration of the event,
ignoring any others listed. The others will be generated for the headline's sake,
but their prices won't be affected.

The file format is fairly hardy - the mod will ignore empty lines and lines
filled with only whitespace (i.e. spaces), and any event without an item blank
won't affect the price of any item. A loose % not followed by a number or % is
interpreted as a literal % character.

Keep in mind that headlines can only be so long before they're cut off in the
news window.
