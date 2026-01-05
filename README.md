# Clio Cards

A single-player roguelike deckbuilder card game built in Unity for Desktop/Mobile deployment

## Introduction

The big picture idea is to introduce players to Peter Turchin, whose ideas about why civilizations collapse is the core inspiration.

This is a Spirelike designed to win in 1-2 hours per run. The cards are all about choices between concentrating wealth or spending it for the public good.
 
The demo card pool is probably somewhere between ¼ and ½ the size of what the final card pool should be to be fully playable.

## Overall Structure

The game starts in a decade between 1900 and 2000. Once playtesting is complete, decades will be locked until you complete the decade before it (the difficulty ascension system.) 

In each game, each year has a wealth goal. You can play only a certain number of cards per quarter. 

Fail to build increasing (by a growth percentage requirement) amounts of wealth per year (Capitalism in a nushell) and you immediately lose. Drive stability to zero and you immediately lose.

Each year has an extra benefit or challenge added. These are partly randomized, but get progressively more difficult as years progress. Every 5 years is an extra-difficult milestone challenge (boss fight, in game parlance) until reaching the final challenge, navigating the Crash.

After each year, there is a card store where you can purchase new cards for your deck. There are no free card rewards. That’s not how the world works. Also there’s inflation.

## Gameplay

When you start on year 1900, you will see a one-page tutorial overlay and a “Play” button. I’m going to hope that’s enough. The design is extremely forgiving and rewards just clicking on stuff to see what it does.

## Resources

Everything is public domain. Creative Commons music from YouTube Audio Library, free sound effects, and art created by digitally manipulating images downloaded from the New York Public Library public domain collection website. No AI. Everything is SVG vector graphics and font resources, so it scales to any resolution without quality loss. Pixels schmixels. The style is crunchy and minimalistic on purpose, and the process of grayscaling/layering/vector tracing means that I get a consistent result despite the image sources being wildly different.

The system uses the Unity colored sprite system, meaning that much of the SVG shapes are reusable and re-colorable. I have chosen a muted complementary color palette of 10 colors with three brightness levels each which can be swapped out and tuned easily without rebuilding any of the resources. The color combinations are currently semi-randomized for each card for testing purposes to check for possible collisions (insisting only on one each from a bright/mid/dark category for the appropriate layers; each image is two layers over a plain background, requiring three colors.) Once the palette is finalized, the permanent card system can specify hand-selected color combinations for each card.

I am open to any refinement suggestions or contributions for any and all resources including sounds, music, card art and prefab templates.

## Modularity

All of the card designs, challenges, starting stat values, deck composition, etc., are all stored in a file called “cards.json.” in the install files. That means that playtesters can be actively involved in tuning the difficulty and even designing new cards or deleting others. Getting the card pool tuned up is going to take a lot of time and experimentation, and no one should have to wait to try an idea by sending me a request and waiting for a new version. Just edit cards.json and run it again. Zero-code modding is basically built-in.

Under users/appdata/LocalLow/SingularitySoftware/ClioCards is a set of ClioData.json and 9 rotating backups. These are the last ten game states. It always loads ClioData.json when you run. It saves the game state after every deal, and after every quarter, making a rotating backup of previous states. Unlike some games, You can’t cheese the system by rewinding and retrying challenges from the start by quitting and restarting. But you CAN copy previous state history to go backwards to retry something, in particular to load the game state before a game-breaking bug and try to both get past it and possibly reproduce it (sending me the game state file and telling me how to reproduce the issue.)

## Gameplay Design Space

The demo card pool has many thematic elements built in, from something as simple as a Donation card raising stability by one point, to something as complex as a “Social Mobility” card raising stability, but adding two “Tycoon” cards, which raise both wealth and stability, but the stability component goes down every time you play them, eventually making them more harmful the longer they stick around, and ultimately playable, illustrating Peter Turchin’s “Elite Overproduction” tent-pole theory. 

Bottom line: Generating and concentrating wealth lowers stability. Spending resources to serve the public good raises stability. Most cards should serve this theme. The general approach, though, should be to build cards that are playable first, and then backfill the thematic justification.

## JSON documentation

This is subject to change, but here is the current system for the cards.json file. That said, just reading the current file will show you most of what you need to know by example:

```
	{
		"name":"Card Title",
		"art":"artname000", (the 000 is a placeholder for the three color values for later.)
		"desc":"human-readable description",
		"rules":"action1,action2,action3,etc.(see below)",
		"type":"" (leave blank for ordinary playing card)
	},
```	

“action” items in a rule consist of up to three components:

```
action|triggered-by|ends-when
```

actions are in the form {stat}{operation}{amount} where stat is a value you’re adding to or removing from, operation is + or – or =, and amount is what to add/remove/set.

### Examples:

```
	{
		"name":"Construct Office",
		"art":"construction000",
		"desc":"-5 stability\n+1 wealth when\ninvestment increases\nthis year",
		"rules":"stability-5,wealth+1,stat+Office Building",
		"type":""
	},

	{
		"name":"Office Building",
		"art":"tower000",
		"desc":"+1 wealth when\ninvestment increases\nthis quarter",
		"rules":"wealth+1|investment+|quarter=5",
		"type":"stat"
	},	
```

Triggers are only relevant for permanents, statuses and challenges. “ends” triggers are only relevant for permanents and statuses, since challenges are automatically only a year in duration. A trigger reading wealth+ means it happens whenever wealth increases. A trigger reading deals- triggers whenever the deals decreases (which happens whenever a hand is dealt for any reason.)

example:add 5 wealth immediately

```
wealth+5
```

example:add 10 wealth and subtract 2 stability immediately

```
wealth+10,stability-2
```

example:add 2 wealth each year, ending at year 20

```
wealth+2|year+|year=20
```

example: add 5 stability when quarter is 4

```
stability+5|quarter=4
```

amount can also be a stat: a rule of wealth+interest will add the value of interest to wealth

example: add education to stability each quarter, ending at the end of the year (quarter=5 is a special case for end-of-year, where year+ happens at the start of the year.)

```
stability+education|quarter+|quarter=5
```

### Base Stat Names

Rules can contain these special stat keywords to modify game behavior:

**remove** – removes the current card (no symbol or amount)

**delete** – removes the first card currently held in hand

**prevent** – removes the next card added to the deck

**deck** – adds or removes a named card (deck+Tycoon,deck-Donation)

**stat** – adds a permanent or status effect (stat+Collaboration)

**maxdeals** – how many deals you start with at the start of the year

**deals** – the current number of deals you start with each quarter this year 

(use deals+1 to increase deals temporarily this year, use maxdeals+1 to increase it permanently.)

**maxhand** – hand size permanently

**hand** – hand size this year

**maxplays** – number of play slots permanently

**plays** – number of play slots this year

(note that reducing these values can be used as a cost on powerful cards)

**wealth** – starts at 0 each year. Must reach goal by end of year

**goal** – the wealth gained last year plus the required growth rate. Can be modified mid-year by cards

**capital** – permanent savings. Adds wealth each year, is spent in the store. Cards can modify it directly

**stability** – can only go up to 100%, but can go below 0 in order to avoid clamping since the check for zero only happens at the end of the year

**growth** – the percentage by which the wealth goal will bet set over the previous year’s ending wealth value (important: The goal isn’t the previous goal + 10%, it’s the final wealth + 10%. If you overperform, you have to keep overperforming.)

**inflation** – the percentage by which the store prices increase every year.

###Extended stats

These are all introduced by cards.json and aren’t fundamental to the core mechanics, illustrating how new ideas can be added just by modifying the .json.

**debt** – a mechanism introduced for some harmful starter Repayment cards

**investment** – a mechanism introduced for cards that scale over time

**elite** – a single-card mechanism for Tycoons

**unrest** – a single-card mechanism to measure the cost of removing a Strike. The longer you leave it, the worse it gets

**grind** - a single-card mechanism for the Grind card

**education** – a permanent mechanism for scaling stability increases

**energy** – a permanent mechanism for storing future stability increases, but it’s spent and reset to zero on each card that uses it

###Card Types

**"" (BLANK)** – if there is no type, it’s a regular card that you can buy in the store and put in your deck

**temp** – a type of card intended to be generated automatically. These will NOT be randomly generated in the store. Note that the start cards are of this type so they don’t show up in the store. This is also used for placeholders like the starting year rules.

**perm** – a permanent effect. These ARE available in the store and are often quite powerful.

**stat** – a temporary effect. These are not available in the store and are created by other cards if they need to have effects that last longer than the current quarter. Note that some cards like “Collaboration” create a status named “ Collaboration “ with the extra spaces placed to make it look identical, but with a distinct name so it can show a reduced rule description, since the playable card and the status it creates are operationally different objects.

**yearX** – a set of annual challenges in four groups, with escalating difficulty, chosen randomly during the game.

**bossX** – the 5-year milestone challenges, which are not randomized and appear in a specific order. Variable milestones could be added later, after a timeline with a preview is added so that players can see what the major challenges are that are coming (a core mechanic of deckbuilders. The biggest obstacles can’t just be a massive unfair surprise.)

      
      