# Code Complexity and the Bin Packing Problem

*A mental tool for software system design — originally published May 25, 2022 on Medium ([Better Programming](https://medium.com/better-programming/code-complexity-and-the-bin-packing-problem-928538d72a29))*

![Generated at Hotpot.ai](content/blog-archive/images/bin-packing-01-header.jpg)
*Generated at Hotpot.ai and licensed for use by me. For all the hype around AI making art, this doesn't REALLY look like the prompt "Ten Bins in a row," does it?*

We all know that the Binning Problem is NP — NP-Hard, to be specific. What if this problem also describes an underlying issue with creating software? I want to explore the idea that bin packing is a good model for studying some of the problems that cause code complexity. This will require a mixture of hard math and soft analogy (Hard as in "definite," not difficult). This is dangerous, and I welcome any objections in the comment section.

First, a quick summary of the bin packing problem from geeksforgeeks.org:

> "Given n items of different weights and bins each of capacity c, assign each item to a bin such that number of total used bins is minimized. It may be assumed that all items have weights smaller than bin capacity."

OK, so we have a couple of variables to shoehorn our analogies into.

1. `W`: The set of weights that need to be bin packed from 1…N. Such that ∑ W = w₁+w₂ + … + wₙ.
2. `c`: The capacity of each bin.

So, let's start by assuming there is a minimum amount of code necessary to create our system. This is represented in the problem as ∑ W. Let's call bins modules. This can be kept very generic. A module may be a service in a micro-service architecture, an `Actor` (which is kind of the same thing as a micro-service), a Class in an OOP system, or maybe a Big Class (one that brings together a lot of smaller classes to accomplish a larger goal).

As any module grows, its internal complexity grows, so we want to limit its size. This limit on the size of a module, since we have identified modules with bins, is, of course, our variable `c`.

In principle, one module can communicate with any other. As a result, the complexity of these potential interactions is `O(n!)`.

This leaves the smaller pieces of code that modules are made of. These are our little `w`'s (w₁ through wₙ). So now we have the following model.

1. A program is the sum of all of its modules which are limited in size by `c` before they become too large and complex.
2. The subcomponents of a module (little `w`'s) add complexity to a module.
3. If a module's complexity is greater than `c`, it must be broken up into smaller modules.
4. The more modules you have, the more potential interactions between modules. Growing the architectural complexity factorially.

If we accept these assumptions, we have mapped the problem of code complexity onto the bin packing problem. We must then conclude that designing an optimally simple software system is NP.

To state the obvious, software design is difficult. You suspected that, but now you have math to back up your suspicions.

![](content/blog-archive/images/bin-packing-02-gvz42.png)
*Photo by [GVZ 42](https://unsplash.com/@gvz42) on [Unsplash](https://unsplash.com) | Image cropped*

## So What?

We can describe the problem of software design in terms of the binning problem. And? We certainly cannot use binning algorithms to design our software, so what gives?

Look again at the work we did above. We have split the complexity into inter-module and intra-module complexity. We have also shown that these two types of complexity are in tension with one another.

To reduce intra-module complexity, we could make our modules simpler by making them smaller (decrease `c`). However, the number of modules then increase. If we increase our module size (increase `c`), we get the opposite effect.

Looking at system design through this lens, we can evaluate our design decisions based on how well they balance these two constraints. While this does not cover all considerations that go into system design, it is an essential component.

![](content/blog-archive/images/bin-packing-03-kraakmo.jpg)
*Photo by Stephen Kraakmo on Unsplash*

This lens also informs us that we should be wary of things like the SOLID Principles. It's unlikely that mechanically applying five rules to our design will result in a satisfactorily optimum solution to complexity balancing. That's not to say they can't be decent guides, but filter everything between your ears.

## Applying the Model

For simplicity's sake, let's refer to everything above as "The Model." We can look at different software development practices and see how they try and deal with the tension between intra-module and inter-module complexity. This will be a healthy final exercise for exploring the Model.

### Monoliths and the model

Monolithic programs usually bring to mind old, poorly designed programs. This stereotype may even be true. If there is a function or class that people refer to by name when talking about software changes, your company may have a monolith.

When we examine monoliths in terms of the model, we see they are a method of reducing certain types of complexity. Monoliths trade intra-module complexity in exchange for reducing the number of modules. To put it in terms of The Model's variables, Monoliths make the value of `c` really large. In principle, if `c > ∑ W`, there is only one module, and the bin packing problem disappears. You don't have to worry about how modules interact with each other, but understanding anything about a module's inner workings can approach impossibility.

You probably actually write monoliths. Most of my internal utility programs are written with no consideration of software architecture because the total amount of code is small enough that the complexity doesn't matter.

### Microservices and the model

Microservices take the opposite strategy to Monoliths. They keep your value c small to cut down on intra-module complexity while letting the number of modules get very large. This seems like a disaster. As we stated in the description of The Model, the complexity of your module communication grows in factorial time with the number of modules. Microservices usually have rules set up around them that limit this damage. For example:

> A micorservice may either answer internal or external requests.
>
> A microservice that answers internal requests may not make requests to other microservices.
>
> A microservice that answers external requests may make requests only to microservices that handle internal requests.

If you can follow this constraint, it does limit your module-level complexity. Given `N` modules, you have modules of type internal or external. If we call these two groups `i` and `e`, then `N = i+e`. The number of interactions is limited to `i` multiplied by `e`. Since `i` and `e` must be less than `N`, then your module-level complexity grows at the rate `N²` instead of `N!`.

![](content/blog-archive/images/bin-packing-04-khalfalli.jpg)
*Photo by Yassine Khalfalli on Unsplash*

## Conclusion

This article was written experimentally. I wanted to try and prove that software design is fundamentally hard using discrete mathematics. You can decide if I succeeded in that or not. I am confident, however, that I've demonstrated that discrete mathematics can be used to gain insights into software design and architecture. It's a topic I hope to approach again in the future.

If you want to hear more of my thoughts on software design, please check out my criminally under-read piece on [legacy code](#/blog-archive/legacy-code).

Thanks again for reading.

---

[Back to Blog Archive](#/blog-archive/blog-archive)
