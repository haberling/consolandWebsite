# On 9 and 9ish Numbers

*A brief jaunt into amateur number theory — originally published June 16, 2022 on Medium ([Intuition](https://medium.com/intuition/on-9-and-9ish-numbers-f77e285c19f4))*

![Wait… it's all nines?](content/blog-archive/images/9ish-01-header.png)
*Wait… It's all nines?*

As a kid, when you were learning your multiplication tables, you probably noticed something about the number nine:

```
1*9=9  and 0+9=9
2*9=18 and 1+8=9
3*9=27 and 2+7=9
.
.
.
8*9=72 and 7+2=9
9*9=81 and 8+1=9
10*9=90 and 9+0=9
```

In other words, for any integer n in the range 1 through 10: the tens place of the result is n-1 and the tens place and the ones place add up to 9. I've always found this fact mildly interesting. However, the state space is too small for this pattern to even demand an explanation.

But what if this held true for an infinite class of numbers? Maybe that would be more interesting. Let's say there is a class of numbers called 9ish numbers. A 9ish number is defined as follows:

> Given Base n, that base's 9ish number is n-1.

So in Base 10, the 9ish number is 9. In Base 8 it's 7. In Base 16 it's f. In base(7) it's 6 and so on. All positive integers are 9ish numbers for a given base. So now we have a sufficiently large set of numbers, all of ℕ, that our rule may apply to. Next, we need to generalize our rule:

> Given a 9ish number n in base n+1. When n is multiplied by any number x from the range 1 through 10 in that base; the result can be written as x-1 in the tens place and n-(x-1) in the ones place. Or [x-1][n-(x-1)] which can be simplified to [x-1][n-x+1].

Author's Note: This holds for the set ℕ, assuming you do not include 0 as a natural number.

## Proving the Rule

I'm sure you get a sense of what is going on here. The cases for 1 and 10 are trivial. For those numbers in-between, it seems like this has something to do with nine's proximity to ten. Let's look at it from that angle. Since Medium isn't super great with math notation, I'm going to break out the engineering pad.

Given Base n+1, and integer x such that 1 < x < 10:

![Handwritten proof of the 9ish number rule](content/blog-archive/images/9ish-02-proof.jpg)

And there it is. I won't claim to have used proper notation. Nor will I claim that this took any real mathematical rigor. I don't have a degree in math and certainly am not a trained mathematician. However, I think this demonstrates that our rule holds for any 9ish number (A term I made up). Here are a few examples.

```
Base 4:  3 * 3 = 21
Base 7:  5 * 6 = 42
Base 8:  2 * 7 = 16
Base 11: 8 * A = 73
Base 16: 5 * F = 4B
```

## The Most Ridiculous Example I Can Think Of

Ok, so we have proved this multiply by 9 pattern holds for the last numeral of any number system. But there aren't that many number systems in use. Base 8 and Base 16 get trotted out in software every once in a while, but for the most part, we live in a decimal world. Time is Technically tracked in Base 60, but I'm not super interested in figuring out how to demonstrate this with a bunch of clocks. There is also Base 64 Encoding, but that uses capital and lower case as separate numerals which looks ugly. I guess we're done then?

> "No, there is another." -Yoda, small green space wizard.

![Photo by Kelly Sikkema on Unsplash](content/blog-archive/images/9ish-03-sikkema.jpg)
*Photo by Kelly Sikkema on Unsplash*

There is one more numeral system we could use, Cistercian Numerals:

![Cistercian numerals chart](content/blog-archive/images/9ish-04-cistercian-wiki.png)
*Taken from the Wikipedia Article on Cistercian Numerals, [here](https://en.wikipedia.org/wiki/Cistercian_numerals).*

To tell the story in the small. In the late middle ages, an order of Christian Monks came up with a system of numerals to represent any number between 1 and 9999. They did this by attaching four symbols to a central stave. They typically used a horizontal stave as opposed to the vertical ones in the image above, however, there are records of this style being used. The numerals were not used for arithmetic but instead used for denoting dates, page numbers, and the like.

That all changes today. But first, we need to complete the good Monks' work and turn these nine thousand nine hundred and ninety-nine symbols into a proper Base 10,000 number system. What's missing is obvious: zero. The most natural way to make a zero is simply not attaching anything to the central stave. That leaves us with a vertical bar. this means 10 (equivalent to 10,000 in decimal) can be written as:

![The Cistercian numeral for zero/ten](content/blog-archive/images/9ish-05-zero.jpg)

Because the Unicode standard doesn't contain Cistercian numerals (a grave crime), we are back to the engineering pad. Here are some examples of multiplying our 9ish number, 9999, with a few other Cistercian numerals.

![Handwritten Cistercian numeral multiplication examples](content/blog-archive/images/9ish-06-examples.jpg)

If you look carefully at the Base 10 numbers in the picture above, You see that this pattern also holds for repeating 9s, and repeating 9ish numbers in other bases.

```
Examples: 127 * 999 = 126,873 and 126 + 873 = 999
           34 * 999 =  33,966  and  33 + 966 = 999
```

This can be demonstrated with modifications to our original proof, but I will leave you to your own devices as far as that goes.

## That's All Folks

I first learned about Cistercian Numerals from a Numberphile Youtube video that is certainly worth a watch. As for the 9ish number theorem, it's just something that I noticed as a kid that I thought I could shamelessly stretch into a blog post. I hope you found this interesting, It was certainly enjoyable to write.

I usually write about software development. If you like this article, please give it a 👏, so I know if I should take more sojourns in math.

---

[Back to Blog Archive](#/blog-archive/blog-archive)
