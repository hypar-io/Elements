# Getting Started with Hypar
Here's a short video that explains some of the Hypar concepts that we'll be using in this getting started guide.

<video width="100%" controls>
  <source src="https://hypar.io/videos/elements+functions+and+workflows.mp4" type="video/mp4">
</video>

Hypar is a cloud platform for generating buildings that makes it easy to publish, distribute, and maintain your building design logic. Rather than requiring web development skills to scale and deliver your design logic, Hypar lets you concentrate on what you want to get done while the platform creates the interface for your audience and provides computation, visualization, delivery, interoperability, and access control for your functions.

<div style="page-break-after: always;"></div>

## Signing up for Hypar

To use the Hypar platform, you're going to need an account.
Point your browser to <a href="https://hypar.io" target="_blank">https://hypar.io</a>, and you should see a page that looks like this.

![](./images/HyparLanding2020.11.16.png)

<div style="page-break-after: always;"></div>

If you don't have an account already, click on the **Create a free account** button below the logo. That should take you to the Sign Up screen.

![](./images/HyparSignup2020.11.16.png)

<div style="page-break-after: always;"></div>

Fill in a username, an email address you can access, and the password you'd like to use. Then click on the **Sign up** button.

![](./images/HyparSignUpComplete2020.11.16.png)

<div style="page-break-after: always;"></div>

Then we do some explaining:

![](./images/HyparSignUpNotice2020.11.16.png)

Now check the email account you supplied for a message that looks something like this:

![](./images/HyparSignUpEmail2019.06.16.png)

<div style="page-break-after: always;"></div>

Click on the <u>Verify Email</u> link in the message and you should see a confirmation page:

![](./images/HyparSignUpConfirm2019.06.16.png)

## Where to go next

That's it! You now have a Hypar account. Time to start building some workflows or [authoring some functions](./Functions.md)! You might also explore these resources to get ideas for building a function, or learn more about the platform:
- [Hypar's Discord live chat](https://discord.gg/Ts6mzXg). Lots of people should be there to answer questions or share ideas.
- [Hypar's YouTube channel](https://www.youtube.com/c/hypar) has walkthrough videos and livestreams.

### Access Plugins
- [Hypar Plug-in for Rhino](https://www.notion.so/hyparaec/Hypar-Plug-in-for-Rhino-b0962866892b4e6aa3be249a01a31f79)


<!--
TODO: Labels can't be trivially added to masses right now.

What if we wanted to see these results in the graphic display as well?
We'll handle that by adding **properties** to the generated masses, and display them using the **Label** dropdown. For the moment the **Label** dropdown only has **None** as an entry:

![](./images/HyparLabels.png)

<div style="page-break-after: always;"></div>

To add labels for our **volume** and **area** values open **StarterFunction.cs** in VSC. Move the two value calculations up under the **height** input, because we're going to need those values a little earlier so we can add them as properties to our **mass**:

![](./images/HyparStartFunctionLabels1.PNG)

<div style="page-break-after: always;"></div>

Before we add the **mass** to our Hypar **model**, we need to add a couple of **NumericProperty** entries to the **mass**. Add these lines before **model.AddElement(mass)**:

![](./images/HyparAddProperties.PNG)

There's a lot happening in these two lines, so let's look at each part, using our new **Volume** property as an example of how both lines work. Since we're adding a property to our **mass**, we use its method **AddProperty**:

![](./images/HyparAddProperty.PNG)

The first argument this method needs is a name for the new property, which in this case is **"Volume"**:

![](./images/HyparAddPropertyName.PNG)

Next the method needs to know what kind of property you're going to add. Eligible values for this argument are **StringProperty** (if we were adding a string value), or in this case **NumericProperty**:

![](./images/HyparAddNumericProperty.PNG)

<div style="page-break-after: always;"></div>

The new **NumericProperty** needs two arguments: the value, which in this case is the **volume** variable calculated above, and then the **UnitType**, which for this output is **UnitType.Volume**:

![](./images/HyparNewNumericProperty.PNG)

Save the file and publish **StarterFunction** again:

![](./images/HyparPublish2019.06.16.png)

<div style="page-break-after: always;"></div>

When you return to Hypar, click on the **Sample 20 Options** button in **StarterFunction** to create 20 new options, then click on one of the options to open it in the interactive display. In the **Label** dropdown you should see the two new properties you just added:

![](./images/HyparLabels2.PNG)

Select **volume** and you should see the value appear in the graphic display on the mass:

![](./images/HyparLabels3.png)

Now you see why we specified the **UnitType** for the new property. Specifying the **UnitType** as **UnitType.Volume** tells Hypar to add **m3** to the displayed value to denote cubic meters. Internally Hypar maintains all values in metric units. Functions can calculate the conversion to Imperial units if required.

<div style="page-break-after: always;"></div>

If you select **area** in the **Label** dropdown, you should see the value notated in square meters:

![](./images/HyparLabels4.png)

You're not limited to simple values like these. You can add any static or calculated value you'd like to any Hypar Element (like Mass).  -->
