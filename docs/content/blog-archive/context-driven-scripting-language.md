# Why You Should Write a Context-Driven Scripting Language

*Sometimes the best way out of code complexity is to go deeper — originally published May 11, 2022 on Medium ([Better Programming](https://medium.com/better-programming/why-you-should-write-a-context-driven-scripting-language-6714581e98b4))*

![Photo by Arnold Francisca on Unsplash](content/blog-archive/images/cdsl-01-francisca.jpg)
*Photo by Arnold Francisca on Unsplash*

Have you, Dear Reader, ever gotten caught in a customization trap?

Example 0: You add custom range checking to input fields with ranges defined in the database. People are delighted. Until they want the range of inputs conditionally defined by another field.

Example 1: You create a settings page that loads in different settings for free and premium accounts. "Perfect, this is exactly what we wanted." Until, of course, some settings need to be shown based on the type or types of devices the user has active sessions on.

I could go on ad infinitum. The point is the more flexible you or your team make your front end, the more flexibility is demanded. Features beget more features. Maybe the Children's Book: "If you Give a Mouse a Cookie" was actually about feature creep and software bloat.

![If You Give a Mouse a Cookie book cover](content/blog-archive/images/cdsl-02-publisher.jpg)
*This image was yanked from the publisher's website and put here. (haha Vim joke). Anyways link to its owners is [here](https://www.harpercollins.com/).*

And that's what is summoned into your once simple, elegant codebase: bloat. You could maybe write a bunch of different UI components and switch between them. Or, you could fill a single UI with such an unholy amount of special cases, that it becomes unworkable. Whatever poison you choose, it's yours to swallow.

## Context-Driven Scripting Languages

If you read the title, I'm assuming you've deduced the thesis: A Context-Driven Scripting Language can make your code more flexible while cutting down on code bloat and complexity.

But what is a Context-Driven Scripting Language (CDSL)? Well, it's a term I made up (It is the sacred right of all software engineers to make up terms). It is defined thusly:

> A Context-Driven Scripting Language (CDSL) is a small language, not usually Turing complete, that is given access to a predefined set of values (context) in order to generate an output.

![A pixel-art robot with a plug](content/blog-archive/images/cdsl-03-robot.gif)
*Image by gandalfhardcore. Copyright Marcus Haberling 2022. The robot is the scripting engine, the context that drives and binds it is the plug. My metaphors transcend the written word.*

## Example 0 with a CDSL

So a CDSL may have access to the values of every field on a settings page, which it uses to output a range checking error message, or an empty string if the current field is valid. A CDSL takes this formatting code, which was previously meshed into the rest of the User Interface, and puts it somewhere else. Probably a database. Let's take a look at the advantages this buys you.

1. Code Readability: By taking this validation logic out of your front-end code, you can remove a lot of the special cases that plague front-end development.
2. Flexibility: Remember, in Example 0 we had previously added code to do a range check validation, when our new requirement came, we then would have had to either rewrite the old system or create a parallel system. With a CDSL, the only code that changes corresponds directly to the field behavior you're changing.
3. Code Reusability: Once the interpreter for your CDSL is in place, the code that it executes can be used on the multiple platforms you support. Changing the validation code for our field changes it across the different front ends we support: Mobile, Web, Etc. We can also use the same code for our secondary validation on the backend. It's a common requirement to provide validation on the UI and API layers.
4. Fast Deployment: Because the validation logic is no longer part of your codebase, you can update the behavior of applications outside of a versioned deployment. For Phone Apps, this means sidestepping Apple's and Android's approval processes.

## Writing a Context-Driven Scripting Language

It's almost impossible to talk about writing scripting languages in general. That's more appropriate for a Textbook than an article. Let's then discuss one CDSL in particular. To write this CDSL, we need to adhere to a few principles.

## Principles of our CDSL

Prefix Notation: Sometimes known as Polish Notation, Prefix notation is very simple to process. In prefix notation, operators come before the operands.

```
Eg: 3+4/5 turns into +3/45.
```

Note that prefix notation does not have an order of operations. This is one of the great simplifications it affords us. Prefix notation can also be thought of as function notation.

![A compilers textbook](content/blog-archive/images/cdsl-04-compiler-book.png)
*No, we're not writing a Compiler, this is a scripting language after all. But this text is useful all the same. You can still get a used copy of the classic Compilers textbook for $11 on Amazon at the time of writing.*

No Whitespace: Our script should be able to be written without whitespace and a minimum number of control characters. This will help us to write small scripts, especially when only one conditional statement is necessary.

Pure Functionality: Our scripts should not be modifying any state. Therefore, we should make the language a pure functional language.

## Syntax of our CDSL

We need to establish a syntax for our language, later we will need to parse this syntax and tokenize our script. People have smeared the term tokenization into other parts of computer science, so it may be better to call this lexical analysis. Either way, we'll be accomplishing both tasks of defining and tokenizing using regular expressions. Warning: if you have trouble with regular expressions, (Stop lying to yourself, you do have trouble with regular expressions) then please check out Regex101. I wrote a piece about it [here](#/blog-archive/meet-regex101).

![Photo by Shubham Dhage on Unsplash](content/blog-archive/images/cdsl-05-dhage.jpg)
*Photo by Shubham Dhage on Unsplash*

### Operators

```
? <condition> <true return> <false return> | If-Else Statement
= <left hand side> <right hand side>       | Equality
! <argument>                               | Not
> <left hand side> <right hand side>       | Greater Than
< <left hand side> <right hand side>       | Less Than
+ <left hand side> <right hand side>       | Addition
- <left hand side> <right hand side>       | Subtraction
* <left hand side> <right hand side>       | Multiplication
/ <left hand side> <right hand side>       | Division
% <left hand side> <right hand side>       | Modulus (Remainder)
| <left hand side> <right hand side>       | Logical Or
& <left hand side> <right hand side>       | Logical And
```

Note: Logical operators will treat any non-zero number as zero, there is no boolean number type.

### Data Types

Integers: This CDSL will only have integer values. If you need floats when implementing your own, then so be it. For now, integers will do. We'll precede all of our integers with a `#` character to denote them.

```
#[-]?[0-9]+
```

Strings: If we are going to solve problems like stated in example zero, we are going to need to output string values. Simple double-quotes notation will do fine. We can escape double quotes with a backslash.

```
"((\\[\\,"])|[^",\\])*"
```

Interpolated Strings: If we want to use our scripts to make strings, interpolation can make the code to do so more concise. Therefore we will do string interpolation using the following syntax.

```
"String with arguments {1}, {2}, {1} and {3}" #1 "insertMe" #-1
```

```
Produces: String with arguments 1, insertMe, 1 and -1
```

The numbers won't have to follow a strict natural number sequence but will be taken in as arguments from least to greatest. If we add that to our regex we get this:

```
"((\\[\\,",{,}])|[^",{,},\\]|({[0-9]+}))*"
```

### Functions

We don't need the ability to define functions inside of a CDSL, but that doesn't mean we won't ever want predefined functions outside of our operators. Let's denote them with a '~' in front of alphabetic characters.

```
~[A-Za-z]+
```

### Context Values

We did say this was a context-driven language, didn't we? Of course, we need a way of accessing this context inside of our script code. This is also the most custom piece of the language.

Say you have a settings object you want to access fields in. I would access it with the notation, s.fieldName. The specifics will depend on what your CDSL is trying to accomplish.

We'll keep our definition flexible. One lower case character, followed by a dot and the alphabetic name of the "context" we are trying to access.

```
[a-z]\.[a-zA-Z]+
```

There is a problem with the regex above. It both greedily captures alphabetic characters and starts with an alphabetic character. This means if we put two of them next to each other, we will capture the prefix of the second context with the name of the first. This violates our no whitespace principle.

We need to add a negative lookahead to not capture a character followed by a period. This must be added to our function regex for the same purpose.

```
([a-z]\.[a-zA-Z]+(?!\.)) - Context Values
(~[A-Za-z]+(?!\.))       - Functions
```

### Bringing it all Together

Now we just need to combine our regex statements from operators, value types, functions, and contexts. We end up with a terribly long regular expression, but one that can split our script into token strings in one pass.

```
([\?,\=,!,<,>,\+,\-,\*,\/,%,\|,\&])|(#[-]?[0-9]+)|("((\\[\\,",{,}])|[^",{,},\\]|({[0-9]+}))*")|(~[A-Za-z]+(?!\.))|([a-z]\.[a-zA-Z]+(?!\.))
```

## Making the Engine

Now that we have a well-defined syntax, we need a means of turning that syntax into output. Let's call that the Scripting Engine. I don't know if anyone else calls it that, but they should.

Take note that this will be a very bare-bones description of an engine. The very minimum to get our CDSL off the ground. I think you'll see how some of the decisions we've made so far make this process far easier than it could have been.

![Photo by lee attwood on Unsplash](content/blog-archive/images/cdsl-06-attwood.jpg)
*Photo by lee attwood on Unsplash*

### Parsing

First, we need to transform our script into a list of token strings. This is simple enough, just extract them using the great and terrible regex we created above.

### Tokenization

Once we have token strings, we have to turn them into tokens (Shocking, I know). Notice that each token string actually starts with a different character! I may have planned this in advance. Using this character, we'll turn each token string into a token object. Here is a little pseudocode.

```
function Tokenize(string tokenString) returns token {
    if tokenString[0] in listOfOperators:
        return CreateOperatorToken(tokenString)
    if tokenString[0] == " :
        return CreateStringToken(tokenString)
    if tokenString[0] == # :
        return CreateIntegerToken(tokenString)
    if tokenString[0] == ~ :
        return CreateFunctionToken(tokenString)
    if tokenString[0] in lowerCaseLetters :
        return CreateContextToken(tokenString)
    throw TokenizationException
}
```

Things certainly don't have to be broken up precisely this way. But whatever your method, we need to create token objects with a few pieces of data.

```
Token {
    TokenType,
    IntegerValue,
    StringValue,
    NumberOfArguments
}
```

NumberOfArguments reveals the structure of our CDSL's execution. Think of every token as a function, if it takes zero arguments then it returns its value, if it takes one or more arguments, some execution must be done before you can get said value.

### Stack it Up

We have gone from a string to a list of token strings, to a list of tokens. The next step is to prepare them for execution. To that, we need to turn our list into a stack with the 0th element on top. This doesn't even mean that we necessarily have to shift the tokens into another data structure. It only means that from now on we'll be at minimum using the list in this manner. Here is an inefficient method, just to demonstrate.

```
function StackTokens(listOfTokens) returns Stack<Tokens> {
    let tempStack = new Stack<Tokens>
    let returnStack = new Stack<Tokens>

    for each token in listOfTokens:
        tempStack.Push(token)

    while tempStack is not empty:
        returnStack.Push(tempStack.Pop())
}
```

It's now obvious why sticking to prefix notation was important. By putting our program into a stack with the leftmost token on top, we put the tokens in perfect order for recursive execution.

### Recursive Execution

I really am flying in the dark on the level of explanation I should be giving. Too specific and your eyes will glaze over. Too little, and a may fail to communicate to some members of the audience. But either way, we have reached the final step; running the script and generating an output. This can be done iteratively but is easier to demonstrate (and code) recursively.

```
recursiveExec(tokenStack){
    if tokenStack is empty:
        throw EmptyStackException

    let token = tokenStack.Pop()
    if token.NumberOfArguments is 0:
        return executeWithNoArguments(token)

    let args = list of type Token size token.NumberOfArguments
    for i in range 0 to token.NumberOfArguments:
        let arg = recursiveExec(tokenStack)
        args[i] = arg

    return executeWithArguments(token, args)
}
```

If this looks a little bit too simple, it's because I'm cheating. The functions executeWithNoArguments and executeWithArguments are doing some heavy lifting here. Obviously, things like string interpolation and function calls will take a bit of code.

This is also a very basic execution. We do no error checking, besides throwing an exception when we can't execute the script. We could do things like making sure the stack is empty at the end of execution, or provide a stack trace when there is a failure.

I'll leave those niceties to your own implementation.

### Example Code

Let's say you allow a user to send themselves an alert once they use a certain amount of data on their plan. They are not allowed to set this alert at a higher amount of data than their plan's MaxMb. If their plan is unlimited, the MaxMb is set to zero, and they can set the warning at any level. We want our script to output an error message, or an empty string if the input is valid. Our context plan is denoted with a p prefix inside our CDSL. Our context settings are denoted with the prefix s.

```
?|=p.MaxMb#0<s.MaxMbWarnp.MaxMb"""You cannot set your warning level greater than your data limit of {1}Gb(s)"/p.MaxMb#1000
```

Its a bit more readable with whitespace between our tokens:

```
? | = p.MaxMb #0 < s.MaxMbWarn p.MaxMb
  ""
  "You cannot set your warning level greater than your data limit of {1}Gb(s)" / p.MaxMb #1000
```

If you put both of these into Regex101 with the regular expression we created for tokenization, you'll see they create the same matches. Prefix notation can be a bit weird when you're not adjusted to it, but it's easy to pick up.

## Conclusion

I think I've laid out a good case as to why a CDSL could be good for your project, as well as an excellent description of how to go about making one. If you think I got anything wrong, let me know in the comments. This turned into a rather long piece, but I hope you found it interesting.

---

[Back to Blog Archive](#/blog-archive/blog-archive)
