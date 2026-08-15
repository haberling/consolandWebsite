# Legacy Code

*Originally published April 1, 2022 on Medium ([CodeX](https://medium.com/codex/legacy-code-2cb94a33dcd0))*

![Punch card, lifted from Wikipedia](content/blog-archive/images/legacy-01-punchcard.jpg)
*Punch Card lifted from [Wikipedia](https://en.wikipedia.org/wiki/Punched_card#/media/File:Used_Punchcard_(5151286161).jpg)*

If you have worked in software engineering for any amount of time you complained about your company's legacy code. It's hard to understand, hard to maintain, and hard to change. I certainly have. Of course, this is not to either of our credits. Complaining about legacy code is pathetic and ungrateful. I try my best to be neither, with varying levels of success.

After all, what is Legacy Code? It's code that works, and code that actually works. Not that it's passed your suite of unit tests, or follows your styling guidelines, or has a modern and consistent architecture. I mean "works" in a more fundamental way: It makes your company money. This should be obvious, if running it was actually costing you money you would just get rid of it. But we never want to get rid of legacy code, we only ever want to rewrite it. This is proof that it is performing some vital function. Unlike the code you merged into the development branch last week, legacy code is paying your salary. Think about that. Legacy code is the software that is generating the revenue necessary for you to pay your mortgage, and despite that you deride it. It supports you and you sneer at it.

Even worse, you may actually lie to your boss about it. Have you, dear reader, ever included Legacy Code in discussions about Technical Debt? It is possible that you have, I'm sure you agree that it happens quite often in our Industry. So, we have established that Legacy Code generates money. Debt, alternatively, does not. That's what makes it a liability. Things that make the company money are not Liabilities, they are Assets. The code you wrote last week, still waiting for the next release? That's also a liability¹. After all, it cost the company a lot of money to produce (software engineers are not cheap) and has yet to make the company any money. Hopefully, it will become an asset very soon. I think it will, I have the utmost confidence in you.

The point is, when we look at Legacy code what we are looking at is SUCCESS. How much time have you spent looking for what successful code looks like? Maybe you watched conference talks, attended conferences, or bought books about it. Maybe you even read the books. But you have examples of success at your company. Real examples. Examples our industry is far too eager to dismiss.

So far, I've only really identified Legacy Code as code that is making money for the company. I have then concluded that the code is good based on that fact. I do intend to go further. I want to argue that Legacy Code should inform new development.

Let's start with making some assumptions. Why was the Legacy Code written that way? Simple, It was written quickly by a smaller team who didn't have access to the tools we have today. We know this because the company has grown since the early days when this code was written, also the company was poorer, so moving quickly was a necessity. The company needed income, refactoring wasn't even a discussion. This is a generalization to be sure, but it's a generalization that should give you pause. Let's say for example you are in a book store and there are two books right beside each other:

> Clean Code: A Handbook of Agile Software Craftsmanship by Robert C. Martin²
>
> Good Code: Delivering Your Product Faster with Smaller Teams. by Made-up Invented Name

Certainly the second book is at least comparably compelling to the first. But, the most compelling part of book two isn't reflected in the title. This book is specific to your software's domain. It doesn't try and teach you how to make an enterprise billing system through the analog of a fake online boutique chocolate store. It teaches you to make an enterprise billing system with examples of an enterprise billing system; A working one, with customers. How many chocolate bars has FooBar Sweet Shop actually shipped anyways?

This book (the Legacy Code if you've missed the metaphor) also shows you an example of bug-free code. I understand your objections. Maybe you even stumbled across this article because you were trying to fix a bug in legacy code. Legacy code is free of big bugs, I can prove this by pointing out that it's been used successfully for many years. Sure it may have small bugs, but nothing stopping it from being deployed and used. Nothing stopping it from making the company money. In this way, it is an example of balancing the tradeoffs of delivery and product quality. Remember, good enough code released a month ago is an Asset. Perfect code released in six months is a liability (If you can actually meet that deadline).

Yes, Legacy Code has problems. That's why we have a special name for it. It can be hard to understand, and hard to change or extend. The point of this article wasn't to deny that whole cloth. The point was to argue that we need to view Legacy Code as an asset that we carry with us as we move forward, instead of simply a problem to be overcome.

**Footnotes**

1. This interpretation of unpublished code as a liability is taken from the book "The Goal by Eliyahu M. Goldratt and Jeff Cox." That book is about manufacturing and refers to inventory as a liability. Unfinished/undeployed code is inventory if I've ever seen it.
2. This example was chosen because it's a well-known book. Don't read too much into it.

---

[Back to Blog Archive](#/blog-archive/blog-archive)
