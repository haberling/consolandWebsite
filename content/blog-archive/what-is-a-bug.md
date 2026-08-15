# What is a "Bug" Anyways?

*Understanding the defects in your code — originally published April 27, 2022 on Medium ([Better Programming](https://medium.com/better-programming/what-is-a-bug-anyways-dd5700e89589))*

![Photo by Daniel K Cheung on Unsplash](content/blog-archive/images/bug-01-cheung.jpg)
*Photo by Daniel K Cheung on Unsplash*

In software, we talk a lot about bugs.

Where they come from, how to avoid them, how to deal with them, etc. But what actually is a bug?

I've seen projects that have over a hundred bugs, each one well documented and tracked, but the code base still ran. I wrote a bug yesterday that stopped an entire class of devices from communicating with the server. How is it that the entities in both of these scenarios get the same label?

How similar are they really? If you say there is a Reptile at the back door and you could mean an iguana sunbathing or a velociraptor fiddling with the door nob, have you truly said anything useful?

I mean sure, maybe taxonomically the creatures are simular¹. That doesn't tell us anything about how we should act in response. Software engineering is all about acting. We are trying to make products. Any definitions we use should be helping us achieve this goal.

![A robot with a plug, representing a reptile metaphor](content/blog-archive/images/bug-02-reptile.gif)
*I Got this image made on Fivvr by gandalfhardcore. I believe I own it so Copyright Marcus Haberling 2022.*

Of course, no one ever really talks about Reptiles. Biology provides much more specific strata of classification. Ladies and gentlemen, we have arrived at the thesis statement: We need a more specific classification of software bugs to handle them appropriately.

And you thought this article was not a listicle. Fool! I shall be listing things!

What follows is a proposal for breaking up bugs into three categories and how to deal with them.

If you think I've got it wrong, please tell me in the comments. If not, still leave a comment, it's good for engagement. (That supposedly matters)

## Catastrophes

Catastrophes are the simplest bugs to talk about and identify. They are defined thusly: "A bug that prevents you from shipping the next release of your software."

If you find this bug deployed in production, it usually means a special release to get it fixed. This is still a broad category and is highly dependent on the sort of system you are creating.

It could be a security vulnerability, a broken core feature, buttons that are halfway off the screen on the new iPhone, A UI problem that makes your customers suicidal², etc, etc.

Catastrophes must be dealt with now. If it will only take a few hours to fix and you cannot ship without fixing it, then there is no reason not to take care of it now when it's fresh in your mind.

If you do not know how long it will take to fix, You must work on it now. If you cannot say how long a bug will take to fix and it must be fixed before release, then you are incapable of even making any time estimates as to when you will be able to ship the next version.

You are dead in the water. The dead do not work on new features, they must first claw their way back to the land of the living.

## Quibbles

Quibbles can be defined as "Small imperfections in your software that you could easily ship with."

Maybe a button on the web front end doesn't line up quite right on the smallest supported screen size, maybe you can enter a negative number in a field that is supposed to be range checked (assuming its harmless), a number displays with two decimal places when the second decimal can only ever be zero, perhaps an animation fires late in weird and rare circumstances, etc, etc.

Quibbles harm the overall look and feel of your product. If there are too many, Quibbles can even prevent you from shipping. People don't have confidence in software that "feels" broken. But this class of bug can only prevent delivery in aggregate.

You should try and keep the number of quibbles low. If they are easy to fix, fix them when you are working in that area of the code for other reasons. If working on an issue without creating a unique branch is against your company's policy, break that policy until somebody tells you to stop. They probably never will.

If your company has a QA team, your backlog is probably full of quibbles. QA teams believe it is their job to find bugs. That isn't their job. Their job is the same as yours, to sell a product.

You can forgive them for their confusion seeing how you were under the impression that your job was to code. But QA finding Quibbles isn't an actual problem. You want to know about them, when convenient you should be looking to fix them. The problem is signal-to-noise ratio.

If QA is filling up your issue tracking software with multitudes of quibbles and a few catastrophic bugs, soon both classes start to get ignored. Quibbles should be identified separately from bugs, or they can impede development more than support it.

## Fungus

As we move into the third category, you may find cause to accuse me of reinventing technical debt. And sure, there is a lot of overlap between Technical Debt and Fungus.

I am attempting to create a new Taxonomy of software bugs. Some retreading is necessary. Also, I don't want to recycle the term Technical Debt. It has too much baggage and too many syllables. The terms Quibble and Fungus MUST have fewer syllables than Catastrophe³.

Fungus is any defect in your code that will grow over time. You probably have heard, "The longer we wait, the harder it will be to fix." You have probably even said it. Note that Fungus can never be a Catastrophe. Fungus may or may not be a quibble. This leaves us with the following chart:

![Fungus/Quibble/Catastrophe Venn-style chart](content/blog-archive/images/bug-03-chart.png)
*Stare longingly into the brilliance of MS Excel "Smart Art" and be humbled by its glory.*

Fungus cannot be a Catastrophe because Catastrophes must be dealt with now. They will have no chance to grow as problems. Quibbles of course can be fungus, because they may be easier to fix now than later. In some circumstances, Fungus will not point to any visible defects in your code.

If a module of code is difficult to change, and you expect it to need to change through your software's iterations: its Fungus. One of the worst types of Fungus is architectural violations.

Architecture violations pop up whenever you can deliver a feature improperly in ten minutes, but can deliver it properly in one week. Take the short path often enough and you can expect so many strange cases and communication crisscrosses that certain areas of your codebase are unworkable.

Architecture violations are one of the real reasons companies get bogged down over time. I'll write more about them in the future. In general, Fungus should take a higher priority than Quibbles.

Unless Quibbles are so numerous that you cannot ship, it's more economical to solve growing rather than static problems. Like Quibbles, you can allow for a little Fungus in your codebase.

If it means meeting deadlines that result in otherwise unobtainable sales, it can make sense in the short term. But Fungus is always risky, even if you think your software is at end of its life. This world is littered with code people thought would be retired twenty years ago.

## In Review

In my reclassification, I have now decomposed "bug" into three more useful categories. The title of this article asks: What is a Bug? My answer: Not worth talking about. Here is a summary of the new categories that ARE worth talking about.

1. Catastrophes stop you from delivering your software. They must be handled immediately.
2. Fungus is any problem that grows over time. Fungus should be dealt with as soon as possible, it is a medium to long-term risk to your codebase.
3. Quibbles are small imperfections that affect your software quality. You should try and keep their number down and deal with them opportunistically. If you ever find you or your team setting aside a specific sprint or development cycle to deal with Quibbles, you are being too passive with them.

This article had two parts: reclassification and prescriptions for the new classes. If you have objections to either, please let me know. My previous piece was on [Legacy Code](#/blog-archive/legacy-code), check it out if you're interested.

**Footnotes**

1. If your reaction to this is to say "Actually scientists believe that velociraptors were more birdlike than reptilian." Just know that I hold you in contempt.
2. Yes, it was Robinhood. Forbes did a decent write-up on it here.
3. No, I'm not going to explain this claim.

---

[Back to Blog Archive](#/blog-archive/blog-archive)
