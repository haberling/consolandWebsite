# Meet Regex101: The Vital Software Tool You're Not Using

*Breaking out of the regular expressions cycle — originally published May 3, 2022 on Medium ([Better Programming](https://medium.com/better-programming/meet-regex101-the-vital-software-tool-youre-not-using-24aa93518db4))*

![Photo by Ante Hamersmit on Unsplash](content/blog-archive/images/regex101-01-hamersmit.jpg)
*Photo by Ante Hamersmit on Unsplash*

Let's talk about the Regular Expressions Cycle:

1. You want to parse a string.
2. You realize that you could turn twenty lines of spaghetti code into two readable lines with regular expressions.
3. You haven't used regular expressions in six months.
4. You learn enough regular expressions for your current use case.
5. You make your regular expression. It takes three times as long as you planned, but it works. (huzzah!)
6. Six months pass and you forget what you learned. Repeat.

I, like many of you, used to be caught in this cycle. I kept a regular expressions cheat sheet in my desk drawer so I could periodically dust it off and begin the process of learning anew. This cycle burdened me with guilt. Yes, without regular expressions my code would be a lot messier. But, it probably would be done quicker. Is this a waste of my time? Is this a waste of my company's time?

All of that changed when talking to a college friend one night. I was logged in to work remotely after hours, trying to knock out a particularly complicated regular expression. After I vented my frustration, he said "Why aren't you using Regex101?"

He made it sound like I was an idiot. This was not inaccurate. Using Regex101, you can write and test regular expressions quickly and easily. (Thesis Statement)

## Basic Features of Regex101

The best way to learn a software tool is to use it. I do implore you to check it out at https://regex101.com.

1. Main Window: The main window allows you to write a regular expression and then see if it matches the test string you provide.

![The Regex101 main window](content/blog-archive/images/regex101-02-mainwindow.png)
*For this article, I wrote a regular expression that matches lower camelcase strings. I had a few errors along the way but it still took under 5 minutes.*

2. Explanation: Regex101 provides a written explanation of what your expression is doing in the top-right pane. This is a powerful tool for learning regular expressions. I've found since using Regex101, I've managed to retain more knowledge about regular expressions.

3. Match Information: A List of your matches and each match's subgroups are available in the middle pane on the right. This is generated from your test string. Personally, this cleared up some of the properties in C#'s Match class.

4. Quick References: A searchable dictionary of regular expressions syntax is available in the bottom right pane.

## Advanced Features of Regex101

1. Flavor Selection: Regex101 provides a variety of implementations to choose from, so you know that the regular expression you are generating will work in your codebase.

![The Regex101 explanation pane](content/blog-archive/images/regex101-03-explanation.jpg)

2. Function: Regex101 has a variety of functions outside of matching. These include replacement (an important use case of regular expressions), and Unit Tests!

3. Code Generation: Regex101 will generate an example of how to implement your regular expression in a variety of languages. Languages have different ways of declaring and using regular expressions, this can save you added Googling.

4. Library: Perhaps someone has already written the expression that you're looking for! You can always search the library of user-submitted regular expressions before re-inventing the wheel.

## In Conclusion

Regular expressions are a powerful tool in Software Engineering. They can make your code shorter, more readable, and easier to test. Regex101 allows us to utilize this tool quickly with clarity and confidence.

It was never my intention to write this post. I was writing an article on a software pattern I call Context-Driven Scripting Languages. But regular expressions are a big part of that discussion, so I wanted another article about Regex101 that I could link to. I've also written more high-level articles on [Legacy Code](#/blog-archive/legacy-code) and [Classifying Bugs](#/blog-archive/what-is-a-bug). Click through if you're interested.

---

[Back to Blog Archive](#/blog-archive/blog-archive)
